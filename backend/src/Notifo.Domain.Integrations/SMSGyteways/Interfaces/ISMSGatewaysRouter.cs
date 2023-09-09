using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifo.Domain.Integrations.SMSGyteways.Interfaces;
public interface ISMSGatewaysRouter
{
    ISMSGateway RouteMessage(SmsMessage message, IEnumerable<ISMSGateway> gateways, CancellationToken ct);
}
