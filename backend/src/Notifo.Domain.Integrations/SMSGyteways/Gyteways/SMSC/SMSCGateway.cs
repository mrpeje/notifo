using Newtonsoft.Json;
using Notifo.Domain.Integrations.SMSGyteways.Gyteways.SMSC;
using Notifo.Domain.Integrations.SMSGyteways.Interfaces;
using Notifo.Infrastructure;
using System.Globalization;
using System.Text;

namespace Notifo.Domain.Integrations.SMSGyteway;
public sealed class SMSCGateway : ISMSGateway
{

    private readonly IHttpClientFactory httpClientFactory;
    public string Login { get; set; }
    public string Password { get; set; }
    public string From { get; set; }
    public SMSCGateway(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<DeliveryResult> SendSMSAsync(SmsMessage message)
    {
        using (var httpClient = httpClientFactory.CreateClient("SMSCGateway"))
        {
            var postUrl = "https://smsc.ru/rest/send/";

            var request = new RequestModel
            {
                Login = Login,
                Password = Password,
                To = message.To,
                Message = message.Text,
            };

            var jsonContent = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var postResponse = await httpClient.PostAsync(postUrl, stringContent);
            if (postResponse.IsSuccessStatusCode)
            {
                var contents = await postResponse.Content.ReadAsStringAsync();
                try
                {
                    var responseBody = JsonConvert.DeserializeObject<Response>(contents);
                    if (responseBody != null)
                    {
                        if (responseBody.NumberError != null)
                        {
                            var errorString = string.Empty;
                            foreach (var error in responseBody.NumberError)
                            {
                                errorString += error.Key + ":" + error.Value;
                            }

                            var errorMessage = string.Format(CultureInfo.CurrentCulture, this.GetType().Name + " failed to send sms {1}", errorString);

                            throw new DomainException(errorMessage);
                        }

                        if (!string.IsNullOrEmpty(responseBody.ErrorCode))
                        {
                            var errorMessage = string.Format(CultureInfo.CurrentCulture, this.GetType().Name + " failed to send sms to '{0}': {1}", message.To, responseBody.ErrorCode);

                            throw new DomainException(errorMessage);
                        }

                        // If no errors found and count of sended sms > 0 take it as success
                        if (responseBody.Count > 0)
                        {
                            return DeliveryResult.Sent;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format(CultureInfo.CurrentCulture, this.GetType().Name + " error unknown ", message.To, ex.Message);

                    throw new DomainException(errorMessage);
                }
            }

            return DeliveryResult.Attempt;
        }
    }
}
