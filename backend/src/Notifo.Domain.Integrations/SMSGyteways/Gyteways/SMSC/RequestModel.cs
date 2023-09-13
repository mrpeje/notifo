using Newtonsoft.Json;

namespace Notifo.Domain.Integrations.SMSGyteways.Gyteways.SMSC;
internal class RequestModel
{
    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("psw")]
    public string Password { get; set; }

    [JsonProperty("phones")]
    public string To { get; set; }
    [JsonProperty("mes")]
    public string Message { get; set; }

    [JsonProperty("err")]
    public int ErrorFormat { get; set; } = 1;

}
