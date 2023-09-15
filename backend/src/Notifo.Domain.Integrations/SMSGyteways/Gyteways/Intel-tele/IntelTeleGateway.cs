using Notifo.Domain.Integrations.SMSGyteways.Gyteways.Intel_tele;
using Notifo.Domain.Integrations.SMSGyteways.Interfaces;
using Notifo.Infrastructure;
using System.Globalization;

namespace Notifo.Domain.Integrations.SMSGyteway;
public sealed class IntelTeleGateway : ISMSGateway
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
                { "to", message.To + " via IntelTeleGateway" },
                { "message", message.Text },
            };

            var urlWithParams = url + ToQueryString(parameters);

            var response = await httpClient.GetAsync(urlWithParams);
            var responseContent = await response.Content.ReadAsStringAsync();

            var responseBody = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseBody != null && responseBody.Reply.Count > 0)
            {
                // Process the first response, since there is only 1 number in the request
                var reply = responseBody.Reply.FirstOrDefault();
                if (reply != null)
                {
                    if (reply.Status.Contains("OK", StringComparison.Ordinal))
                    {
                        return DeliveryResult.Sent;
                    }

                    if (reply.Status.Contains("error", StringComparison.Ordinal))
                    {
                        var errorMessage = string.Format(CultureInfo.CurrentCulture, this.GetType().Name + " failed to send sms to '{0}': {1}", message.To, reply.Status);

                        throw new DomainException(errorMessage);
                    }
                }
            }

            return DeliveryResult.Attempt;
        }
    }

    private static string ToQueryString(System.Collections.Specialized.NameValueCollection nvc)
    {
        return string.Join("&", Array.ConvertAll(nvc.AllKeys, key => $"{key}={nvc[key]}"));
    }
}
