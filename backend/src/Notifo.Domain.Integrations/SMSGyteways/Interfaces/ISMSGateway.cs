using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifo.Domain.Integrations.SMSGyteways.Interfaces;
public interface ISMSGateway
{
    public Task<DeliveryResult> SendSMSAsync(SmsMessage message, CancellationToken ct);
}
