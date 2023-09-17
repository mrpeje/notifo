using System.Security.Claims;
using Amazon.Auth.AccessControlPolicy;
using Google.Apis.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Notifo.Areas.Api.Controllers.Events.Dtos;
using Notifo.Areas.Api.Controllers.Notifications.Dtos;
using Notifo.Areas.Api.Controllers.TestController;
using Notifo.Areas.Api.Controllers.Users.Dtos;
using Notifo.Domain.Apps;
using Notifo.Domain.Events;
using Notifo.Domain.Identity;
using Notifo.Domain.Integrations;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Infrastructure;
using Notifo.Infrastructure.Texts;
using Notifo.Pipeline;
using OpenAI.ObjectModels;

namespace Notifo.Areas.Api.Controllers.MyController;

[ApiExplorerSettings(GroupName = "UsersNotifications")]
public sealed class UsersNotificationsController : BaseController
{
    private readonly IEventPublisher eventPublisher;
    private readonly IAppStore appStore;
    private readonly IUserStore userStore;
    private readonly IUserNotificationStore userNotificationsStore;
    public UsersNotificationsController(IEventPublisher eventPublisher, IAppStore appStore, IUserStore userStore, IUserNotificationStore userNotificationsStore)
    {
        this.eventPublisher = eventPublisher;
        this.appStore = appStore;
        this.userStore = userStore;
        this.userNotificationsStore = userNotificationsStore;
    }

    [HttpPost]
    [Route("api/usernotification")]
    public async Task<IActionResult> CreateUserAndSendNotification(SendNotificationDto notification)
    {
        var apiKey = HttpContext.Request.Headers["ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            return BadRequest();
        }

        var app = await appStore.GetByApiKeyAsync(apiKey, HttpContext.RequestAborted);
        if (app == null)
        {
            return Unauthorized();
        }

        try
        {
            User user = await UpsertUser(notification.Address, app);
            await SendNotification(notification, app, user);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while processing the request." + ex.Message);
        }
    }

    private async Task<User> UpsertUser(string address, App app)
    {
        var dto = new UpsertUserDto
        {
            PhoneNumber = address,
            Id = address.Substring(1),
            EmailAddress = string.Empty
        };

        var principalId = app.Contributors.Keys.FirstOrDefault();
        var command = dto.ToUpsert();
        command.AppId = app.Id;
        command.Timestamp = SystemClock.Instance.GetCurrentInstant();
        command.Principal = BackendUser(principalId);
        command.PrincipalId = principalId;

        var user = await Mediator.SendAsync(command, HttpContext.RequestAborted);
        return user;
    }

    private static ClaimsPrincipal BackendUser(string userId)
    {
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        claimsIdentity.AddClaim(new Claim("sub", userId));

        return claimsPrincipal;
    }
    private async Task SendNotification(SendNotificationDto notification, App? app, User? user)
    {
        PublishDto notificationRquest = new PublishDto();

        notificationRquest.Topic = "users/" + user.Id;
        notificationRquest.Data = "Id:" + notification.Id;
        notificationRquest.Preformatted = new NotificationFormattingDto();
        notificationRquest.Preformatted.Subject = new LocalizedText
        {
            ["en"] = notification.Body
        };

        var dtoSetting = new ChannelSettingDto();
        dtoSetting.Send = Domain.ChannelSend.Send;
        dtoSetting.Condition = Domain.ChannelCondition.Always;
        dtoSetting.Required = Domain.ChannelRequired.Inherit;

        var settings = new Dictionary<string, ChannelSettingDto>();
        switch (notification.Preferred_Method)
        {
            case DeliveryMethod.SMS:
                settings.Add("sms", dtoSetting);
                break;
            case DeliveryMethod.Telegram:
                settings.Add("messaging", dtoSetting);
                break;
        }

        notificationRquest.Settings = settings;
        notificationRquest.Silent = false;
        notificationRquest.Test = false;
        notificationRquest.Timestamp = SystemClock.Instance.GetCurrentInstant();

        var @event = notificationRquest.ToEvent(app.Id);
        await eventPublisher.PublishAsync(@event, HttpContext.RequestAborted);
    }

    [HttpPost]
    [Route("api/usernotification/{id}")]
    [ProducesResponseType(200, Type = typeof(IActionResult))]
    public async Task<IActionResult> GetNotificationStatus(string id)
    {
        var apiKey = HttpContext.Request.Headers["ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            return BadRequest();
        }

        var app = await appStore.GetByApiKeyAsync(apiKey, HttpContext.RequestAborted);
        if (app == null)
        {
            return Unauthorized();
        }

        IResultList<User> users = await GetAppUsers(app.Id);
        foreach (var user in users)
        {
            IResultList<UserNotification> notifications = await GetUserNotifications(app.Id, user.Id);
            foreach (var notification in notifications)
            {
                if (notification.Data != null && notification.Data.Contains(id))
                {
                    notification.Channels.TryGetValue("sms", out var smsChannel);
                    if (smsChannel != null)
                    {
                        var channelSendInfo = smsChannel.Status.FirstOrDefault();
                        if (channelSendInfo.Value != null)
                        {
                            return Ok(channelSendInfo.Value.Status);
                        }

                        return Ok(DeliveryStatus.Unknown);
                    }

                    notification.Channels.TryGetValue("messaging", out var messagingChannel);
                    if (messagingChannel != null)
                    {
                        var channelSendInfo = messagingChannel.Status.FirstOrDefault();
                        if (channelSendInfo.Value != null)
                        {
                            return Ok(channelSendInfo.Value.Status);
                        }

                        return Ok(DeliveryStatus.Unknown);
                    }
                }
            }
        }

        return NotFound();
    }

    private async Task<IResultList<UserNotification>> GetUserNotifications(string appId, string userId)
    {
        var userQueryDto = new UserNotificationQueryDto
        {
            Skip = 0,
            Take = 100
        };
        var notifications = await userNotificationsStore.QueryAsync(appId, userId, userQueryDto.ToQuery(true), HttpContext.RequestAborted);
        return notifications;
    }

    private async Task<IResultList<User>> GetAppUsers(string appId)
    {
        var q = new QueryDto
        {
            Skip = 0,
            Take = 100
        };
        var users = await userStore.QueryAsync(appId, q.ToQuery<UserQuery>(true), HttpContext.RequestAborted);
        return users;
    }
}
