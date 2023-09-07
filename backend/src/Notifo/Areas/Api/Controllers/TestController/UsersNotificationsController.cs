using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Notifo.Areas.Api.Controllers.Events.Dtos;
using Notifo.Areas.Api.Controllers.Users.Dtos;
using Notifo.Domain.Events;
using Notifo.Infrastructure.Texts;

namespace Notifo.Areas.Api.Controllers.MyController;

[ApiExplorerSettings(GroupName = "CustomAPI")]
public sealed class UsersNotificationsController : BaseController
{
    private readonly IEventPublisher eventPublisher;

    public UsersNotificationsController(IEventPublisher eventPublisher)
    {
        this.eventPublisher = eventPublisher;
    }

    [HttpPost]
    [Route("api/usernotification")]
    public async Task<IActionResult> CreateUserNotification()
    {
        // --------------------------------------------- User Creation -----------------------------------
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:1337/api/objects/2");
        var dto = new UpsertUserDto();
        dto.EmailAddress = "auto@test";
        dto.PhoneNumber = "1234567890";
        dto.Id = dto.EmailAddress + "|" + dto.PhoneNumber;
        var command = dto.ToUpsert();
        command.AppId = "044ea419-9250-49c9-84da-d9c6fa4f489a";

        var clock = SystemClock.Instance;
        command.Timestamp = clock.GetCurrentInstant();

        var userId = "64e89ed5dc616dae3ed5a323";
        command.Principal = BackendUser(userId);
        command.PrincipalId = userId;

        var user = await Mediator.SendAsync(command, HttpContext.RequestAborted);
        // --------------------------------------------- Notification send -----------------------------------
        PublishDto notificationRquest = new PublishDto();

        notificationRquest.Topic = "users/" + user.Id;

        notificationRquest.Preformatted = new NotificationFormattingDto();
        notificationRquest.Preformatted.Subject = new LocalizedText
        {
            ["en"] = "Subjec str 1"
        };
        notificationRquest.Preformatted.Body = new LocalizedText
        {
            ["en"] = "Body str 1"
        };

        var dtoSetting = new ChannelSettingDto();
        dtoSetting.Send = Domain.ChannelSend.Send;
        dtoSetting.Condition = Domain.ChannelCondition.Always;
        dtoSetting.Required = Domain.ChannelRequired.Inherit;

        var settings = new Dictionary<string, ChannelSettingDto>
        {
            { "sms", dtoSetting }
        };
        notificationRquest.Settings = settings;
        notificationRquest.Silent = false;
        notificationRquest.Test = false;
        notificationRquest.Timestamp = clock.GetCurrentInstant();

        var @event = notificationRquest.ToEvent("044ea419-9250-49c9-84da-d9c6fa4f489a");
        await eventPublisher.PublishAsync(@event, HttpContext.RequestAborted);

        return NoContent();
    }

    private static ClaimsPrincipal BackendUser(string userId)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim("sub", userId));

        return claimsPrincipal;
    }
}
