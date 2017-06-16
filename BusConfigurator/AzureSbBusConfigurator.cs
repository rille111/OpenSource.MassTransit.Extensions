using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Messaging.Infrastructure.ServiceBus.Models;
using Microsoft.ServiceBus;

namespace Messaging.Infrastructure.ServiceBus.BusConfigurator
{
    public class AzureSbBusConfigurator : IBusConfigurator
    {
        private readonly AzureSbTimeoutConfiguration _azureSbTimeoutConfiguration;
        // Fields

        public readonly List<Type> Consumers = new List<Type>();

        // Properties

        public BusConfiguration Configuration { get; set; }

        // Ctor

        public AzureSbBusConfigurator(BusConfiguration configuration, AzureSbTimeoutConfiguration azureSbTimeoutConfiguration = null)
        {
            _azureSbTimeoutConfiguration = azureSbTimeoutConfiguration;
            Configuration = configuration;
            Configuration.ValidateConfigurationThrows();
        }

        // Implementation

        /// <summary>
        /// Create & configures a bus. Note: No more consumers can be added after the bus is created.
        /// </summary>
        public IBusControl CreateBus(Action<IBusFactoryConfigurator, IHost> registrationAction = null)
        {
            var buz = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                var connUri = new Uri(Configuration.ConnectionUri);
                var host = cfg.Host(connUri, hst =>
                {
                    hst.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(Configuration.Login,
                        Configuration.Password);
                });

                // Azure-specific configuration that isn't included in the interface of 'cfg'. Let these comments be, for reference!
                //var x = (ServiceBusBusFactoryConfigurator) cfg;
                //x.AutoDeleteOnIdle = TimeSpan.FromMinutes(5); // Minimum is 5 minutes, might change in the future.
                //x.EnableExpress = false;

                if (_azureSbTimeoutConfiguration != null)
                {
                    // Renew lock to enable jobs to run longer than 5 minutes
                    cfg.LockDuration = _azureSbTimeoutConfiguration.LockDuration;
                    if (_azureSbTimeoutConfiguration.UseRenewLock != TimeSpan.Zero)
                        RenewLockConfigurationExtensions.UseRenewLock(cfg, _azureSbTimeoutConfiguration.UseRenewLock);
                }

                SerializerConfigurationExtensions.UseJsonSerializer((IBusFactoryConfigurator) cfg);
                registrationAction?.Invoke(cfg, host);
            });
            return buz;
        }

        public Task<IBusControl> CreateBusAsync(Action<IBusFactoryConfigurator, IHost> registrationAction = null)
        {
            return Task.Run(() => CreateBus(registrationAction));
        }
    }
}