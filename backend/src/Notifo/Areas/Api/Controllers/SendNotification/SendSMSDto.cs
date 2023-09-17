using System.Runtime.Serialization;

namespace Notifo.Areas.Api.Controllers.TestController;

public class SendNotificationDto
{

    public string Id { get; set; }


    public string Address { get; set; }


    public string Body { get; set; }


    public DeliveryMethod Preferred_Method { get; set; }
}

public enum DeliveryMethod
{
    SMS = 0,
    Telegram = 1,
}
