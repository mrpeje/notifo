using Microsoft.Extensions.DependencyInjection;

namespace Notifo.Domain.Integrations.SMSGyteway;
public static class SMSGatewayServiceExtensions
{
    public static IServiceCollection AddIntegrationSMSGateways(this IServiceCollection services)
    {
        services.AddSingletonAs<SMSGatewaysIntegration>()
            .As<IIntegration>();

        return services;
    }
}
