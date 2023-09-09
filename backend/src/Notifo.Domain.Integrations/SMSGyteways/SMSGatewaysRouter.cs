using Notifo.Domain.Integrations.SMSGyteways.Interfaces;

namespace Notifo.Domain.Integrations.SMSGyteways;
public sealed class SMSGatewaysRouter : ISMSGatewaysRouter
{
    public ISMSGateway RouteMessage(SmsMessage message, IEnumerable<ISMSGateway> gateways, CancellationToken ct)
    {
        return gateways.FirstOrDefault();
    }
}
