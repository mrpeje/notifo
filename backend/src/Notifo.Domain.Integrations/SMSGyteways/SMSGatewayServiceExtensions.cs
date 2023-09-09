using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Integrations.SMSGyteways;
using Notifo.Domain.Integrations.SMSGyteways.Interfaces;

namespace Notifo.Domain.Integrations.SMSGyteway;
public static class SMSGatewayServiceExtensions
{
    public static IServiceCollection AddIntegrationSMSGateways(this IServiceCollection services)
    {
        services.AddSingletonAs<SMSGatewaysIntegration>()
            .As<IIntegration>();
        services.AddSingletonAs<SMSGatewaysRouter>()
            .As<ISMSGatewaysRouter>();
        services.AddSingletonAs<XMLGateway>()
            .As<ISMSGateway>();
        services.AddSingletonAs<SMSCGateway>()
            .As<ISMSGateway>();
        services.AddSingletonAs<IntelTeleGateway>()
            .As<ISMSGateway>();
        return services;
    }
}
