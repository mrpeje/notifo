using Notifo.Domain.Integrations.SMSGyteway;
using Notifo.Domain.Integrations.SMSGyteways.Interfaces;

namespace Notifo.Domain.Integrations.SMSGyteways;
public sealed class SMSGatewaysRouter : ISMSGatewaysRouter
{
    public ISMSGateway? RouteMessage(SmsMessage message, IEnumerable<ISMSGateway> gateways)
    {
        var number = message.To;
        var textSMS = message.Text;
        string ukraineCode = "380";

        if (number.StartsWith("+" + ukraineCode, StringComparison.OrdinalIgnoreCase) ||
            number.StartsWith(ukraineCode, StringComparison.OrdinalIgnoreCase))
        {
            return GetSpecifiedGateway<IntelTeleGateway>(gateways);
        }

        if (int.TryParse(textSMS, out int _))
        {
            return GetSpecifiedGateway<SMSCGateway>(gateways);
        }

        return GetSpecifiedGateway<XMLGateway>(gateways);
    }

    private static ISMSGateway? GetSpecifiedGateway<T>(IEnumerable<ISMSGateway> gateways)
    {
        var gateway = gateways.Where(e => e.GetType() == typeof(T)).FirstOrDefault();
        return gateway;
    }
}
