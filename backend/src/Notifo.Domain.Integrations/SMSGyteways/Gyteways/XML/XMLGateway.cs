using System.Net;
using System.Text;
using System.Xml.Serialization;

using Notifo.Domain.Integrations.SMSGyteways.Gyteways.XML;
using Notifo.Domain.Integrations.SMSGyteways.Interfaces;

namespace Notifo.Domain.Integrations.SMSGyteway;
public class XMLGateway : ISMSGateway
{
    private readonly IHttpClientFactory httpClientFactory;
    private string SerializedXml { get; set; }

    public string Login { get; set; }
    public string Password { get; set; }
    public string From { get; set; }

    public XMLGateway(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<DeliveryResult> SendSMSAsync(SmsMessage message)
    {
        SerializeMessage(message);
        if (!string.IsNullOrEmpty(SerializedXml))
        {
            var response = PostXMLData("http://77.244.208.166/xml/");
            if (response != null)
            {
                if (response.Information.Status.Contains("sent", StringComparison.Ordinal))
                {
                    return DeliveryResult.Sent;
                }

                if (response.Error != null)
                {
                    return DeliveryResult.Failed(response.Error);
                }

                return DeliveryResult.Failed(response.Information.Status);
            }
        }

        return DeliveryResult.Failed("Empty request string");
    }

    private void SerializeMessage(SmsMessage message)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLRequest));

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

        using (MemoryStream memoryStream = new MemoryStream())
        {
            xmlSerializer.Serialize(memoryStream, request);
            memoryStream.Position = 0;
            SerializedXml = Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }

    public XMLResponse PostXMLData(string destinationUrl)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
        byte[] bytes;
        bytes = System.Text.Encoding.ASCII.GetBytes(SerializedXml);
        request.ContentType = "text/xml; encoding='utf-8'";
        request.ContentLength = bytes.Length;
        request.Method = "POST";

        Stream requestStream = request.GetRequestStream();
        requestStream.Write(bytes, 0, bytes.Length);
        requestStream.Close();

        HttpWebResponse response;
        response = (HttpWebResponse)request.GetResponse();
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Stream responseStream = response.GetResponseStream();
            XMLResponse parsedResponse = ParseResponse(responseStream);

            return parsedResponse;
        }

        return null;
    }

    private XMLResponse ParseResponse(Stream responseStream)
    {
        try
        {
            XmlSerializer serializerResponse = new XmlSerializer(typeof(XMLResponse));
            var response = (XMLResponse)serializerResponse.Deserialize(responseStream);

            if (response.Information != null)
            {
                response.Sent = true;
            }

            return response;
        }
        catch (InvalidOperationException execption)
        {
            return null;
        }
    }

   
}
