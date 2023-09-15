using System.Globalization;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Integrations.SMSGyteways.Gyteways.XML;
using Notifo.Domain.Integrations.SMSGyteways.Interfaces;
using Notifo.Infrastructure;

namespace Notifo.Domain.Integrations.SMSGyteway;
public sealed class XMLGateway : ISMSGateway
{
    private readonly IHttpClientFactory httpClientFactory;

    public string Login { get; set; }
    public string Password { get; set; }
    public string From { get; set; }

    public XMLGateway(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<DeliveryResult> SendSMSAsync(SmsMessage message)
    {
        using (var httpClient = httpClientFactory.CreateClient("XMLGateway"))
        {
            string url = "http://77.244.208.166/xml/";
            var serializedRequest = GetSerializedRequest(message);
            var content = new StringContent(serializedRequest, Encoding.UTF8, "application/xml");

            var response = await httpClient.PostAsync(url, content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var xmlResponse = DeserializeResponse(responseStream);

                if (xmlResponse != null)
                {
                    if (xmlResponse.Information != null)
                    {
                        if (xmlResponse.Information.Status.Contains("send", StringComparison.Ordinal))
                        {
                            return DeliveryResult.Sent;
                        }

                        var errorMessage = string.Format(CultureInfo.CurrentCulture, this.GetType().Name + " failed to send sms to '{0}': {1}", message.To, xmlResponse.Information.Status);

                        throw new DomainException(errorMessage);
                    }

                    if (xmlResponse.Error != null)
                    {
                        var errorMessage = string.Format(CultureInfo.CurrentCulture, this.GetType().Name + " failed to send sms to '{0}': {1}", message.To, xmlResponse.Error);

                        throw new DomainException(errorMessage);
                    }
                }
            }
        }

        return DeliveryResult.Attempt;
    }

    private string GetSerializedRequest(SmsMessage message)
    {
        var requestSubscriber = new XMLRequestSubscriber();
        requestSubscriber.Phone = message.To;
        requestSubscriber.NumberSMS = 1;

        var requestMessage = new XMLRequestMessage();
        requestMessage.Sender = From;
        requestMessage.Text = message.Text;
        requestMessage.Type = "sms";
        requestMessage.Subscriber = requestSubscriber;

        var requestSecurity = new XMLRequestSecurity
        {
            Login = new Login { Value = this.Login },
            Password = new Password { Value = this.Password }
        };

        var request = new XMLRequest
        {
            Message = requestMessage,
            Security = requestSecurity
        };

        return SerializeRequest(request);
    }

    private string SerializeRequest(XMLRequest request)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLRequest));
            xmlSerializer.Serialize(memoryStream, request);
            memoryStream.Position = 0;

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }

    private XMLResponse DeserializeResponse(Stream responseStream)
    {
        try
        {
            XmlSerializer serializerResponse = new XmlSerializer(typeof(XMLResponse));
            var response = (XMLResponse)serializerResponse.Deserialize(responseStream);

            return response;
        }
        catch (Exception ex)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, this.GetType().Name + " unknow error", ex.Message);

            throw new DomainException(errorMessage);
        }
    }
}
