using Newtonsoft.Json;

namespace Notifo.Domain.Integrations.SMSGyteways.Gyteways.SMSC;

public class Response
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("cnt")]
    public int Count { get; set; }

    [JsonProperty("error")]
    public string Error { get; set; }

    [JsonProperty("error_code")]
    public string ErrorCode { get; set; }

    [JsonProperty("errors")]
    public Dictionary<string, int> NumberError { get; set; }
}
