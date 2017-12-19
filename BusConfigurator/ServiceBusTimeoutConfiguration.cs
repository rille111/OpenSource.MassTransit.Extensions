using System;

namespace Messaging.Infrastructure.ServiceBus.BusConfigurator
{
    public class ServiceBusTimeoutConfiguration
    {
        public TimeSpan LockDuration { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan UseRenewLock { get; set; }
    }
}
