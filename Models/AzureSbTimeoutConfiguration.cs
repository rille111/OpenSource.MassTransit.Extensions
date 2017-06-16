using System;

namespace Messaging.Infrastructure.ServiceBus.Models
{
    public class AzureSbTimeoutConfiguration
    {
        public TimeSpan LockDuration { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan UseRenewLock { get; set; }
    }
}