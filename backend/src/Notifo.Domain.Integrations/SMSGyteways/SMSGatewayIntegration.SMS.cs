using Notifo.Domain.Integrations.SMSGyteways.Interfaces;
using Notifo.Infrastructure;
using System.Globalization;

namespace Notifo.Domain.Integrations.SMSGyteway;

public sealed partial class SMSGatewaysIntegration : ISmsSender // , IIntegrationHook
{
    public async Task<DeliveryResult> SendAsync(IntegrationContext context, SmsMessage message,
        CancellationToken ct)
    {
        // Get avalible gateways
        IEnumerable<ISMSGateway> gateways = GetGateways(context);
        // Get gateway by rulles
        var gateway = smsGatewaysRouter.RouteMessage(message, gateways);
        if (gateway != null)
        {
            // Send SMS via gateway
            var response = await gateway.SendSMSAsync(message);
            return response;
        }

        var errorMessage = string.Format(CultureInfo.CurrentCulture, this.GetType().Name + " no active suitable gateways");

        throw new DomainException(errorMessage);
    }

    private IEnumerable<ISMSGateway> GetGateways(IntegrationContext context)
    {
        List<ISMSGateway> gatewayList = new List<ISMSGateway>();
        var isEnabledXML = EnabledXMLProperty.GetBoolean(context.Properties);
        if (isEnabledXML)
        {
            xmlGateway.Login = LoginXMLProperty.GetString(context.Properties);
            xmlGateway.Password = PasswordXMLProperty.GetString(context.Properties);
            xmlGateway.From = FromProperty.GetString(context.Properties);
            gatewayList.Add(xmlGateway);
        }

        var isEnabledSMSC = EnabledSMSCProperty.GetBoolean(context.Properties);
        if (isEnabledSMSC)
        {
            smscGateway.Login = LoginSMSCProperty.GetString(context.Properties);
            smscGateway.Password = PasswordSMSCProperty.GetString(context.Properties);
            smscGateway.From = FromProperty.GetString(context.Properties);
            gatewayList.Add(smscGateway);
        }

        var isEnabledIntelTele = EnabledIntelTeleProperty.GetBoolean(context.Properties);
        if (isEnabledIntelTele)
        {
            intelTeleGateway.Login = LoginIntelTeleProperty.GetString(context.Properties);
            intelTeleGateway.Password = PasswordIntelTeleProperty.GetString(context.Properties);
            intelTeleGateway.From = FromProperty.GetString(context.Properties);
            gatewayList.Add(intelTeleGateway);
        }

        return gatewayList.AsEnumerable();
    }
}
