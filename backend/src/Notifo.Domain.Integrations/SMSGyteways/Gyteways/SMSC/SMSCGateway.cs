using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Notifo.Domain.Integrations.SMSGyteways.Gyteways.SMSC;
using Notifo.Domain.Integrations.SMSGyteways.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifo.Domain.Integrations.SMSGyteway;
public class SMSCGateway : ISMSGateway
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
                            foreach (var error in responseBody.NumberError)
                            {
                                return DeliveryResult.Failed(error.Key + ":" + error.Value);
                            }
                        }

                        if (!string.IsNullOrEmpty(responseBody.ErrorCode))
                        {
                            return DeliveryResult.Failed(responseBody.ErrorCode + " " + responseBody.Error);
                        }

                        // If no errors found and count of sended sms > 0 take it as success
                        if (responseBody.Count > 0)
                        {
                            return DeliveryResult.Sent;
                        }
                    }
                }
                catch
                {
                    return DeliveryResult.Failed("Failed parse response");
                }
            }

            return DeliveryResult.Failed("Empty response");
        }
    }
}
