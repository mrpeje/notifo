using Notifo.Domain.Integrations.SMSGyteways.Gyteways.Intel_tele;
using Notifo.Domain.Integrations.SMSGyteways.Interfaces;

namespace Notifo.Domain.Integrations.SMSGyteway;
public class IntelTeleGateway : ISMSGateway
{
    private readonly IHttpClientFactory httpClientFactory;
    public string Login { get; set; }
    public string Password { get; set; }
    public string From { get; set; }

    public IntelTeleGateway(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<DeliveryResult> SendSMSAsync(SmsMessage message)
    {
        using (var httpClient = httpClientFactory.CreateClient("IntelTeleGateway"))
        {
            httpClient.BaseAddress = new Uri("http://api.sms.intel-tele.com/");
            var url = "message/send/?";
            var parameters = new System.Collections.Specialized.NameValueCollection
            {
                { "username", Login },
                { "api_key", Password },
                { "from", From },
                { "to", message.To },
                { "message", message.Text },
            };

            var urlWithParams = url + ToQueryString(parameters);

            var response = await httpClient.GetAsync(urlWithParams);
            var responseContent = await response.Content.ReadAsStringAsync();

            var responseBody = Newtonsoft.Json.JsonConvert.DeserializeObject<ReplyModel>(responseContent);
            if (responseBody != null && responseBody.Reply.Count > 0)
            {
                // Process the first response, since there is only 1 number in the request
                var reply = responseBody.Reply.FirstOrDefault();

                if (reply != null && reply.Status.Contains("OK", StringComparison.Ordinal))
                {
                    return DeliveryResult.Sent;
                }

                if (reply != null && reply.Status.Contains("error", StringComparison.Ordinal))
                {
                    return DeliveryResult.Failed(responseContent);
                }
            }

            return DeliveryResult.Failed("Empty response");
        }
    }

    private static string ToQueryString(System.Collections.Specialized.NameValueCollection nvc)
    {
        return string.Join("&", Array.ConvertAll(nvc.AllKeys, key => $"{key}={nvc[key]}"));
    }
}
