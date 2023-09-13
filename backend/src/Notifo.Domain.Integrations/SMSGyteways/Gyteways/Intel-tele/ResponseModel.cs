using Newtonsoft.Json;

namespace Notifo.Domain.Integrations.SMSGyteways.Gyteways.Intel_tele;
public class ResponseModel
{
    [JsonProperty("reply")]
    public List<Response> Reply { get; set; }
}
public class Response
{
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("parts")]
    public int Parts { get; set; }

    [JsonProperty("cost")]
    public double Cost { get; set; }

    [JsonProperty("number")]
    public string Number { get; set; }

    [JsonProperty("message_id")]
    public string MessageId { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("mccmnc")]
    public string MccMnc { get; set; }
}
