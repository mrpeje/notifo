using MongoDB.Driver.Core.WireProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Notifo.Domain.Integrations.SMSGyteway;

public sealed partial class SMSGatewaysIntegration : ISmsSender // , IIntegrationHook
{
    public async Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message,
        CancellationToken ct)
    {
        using (var httpClient = httpClientFactory.CreateClient("SMSGateway"))
        {
            //var responseMessage = await httpClient.PostAsJsonAsync("http://localhost:1337/api/objects/2", request, ct);
            //var response = await httpClient.GetStringAsync("http://localhost:1337/api/objects/2");
            //responseMessage.EnsureSuccessStatusCode();
            //var responses = await responseMessage.Content.ReadFromJsonAsync<ResponseMessage[]>(cancellationToken: ct);
            DeliveryResult result2 = default(DeliveryResult);
            return result2;
        }
    }
}
