using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using MassTransit.Turnout.Contracts;
using Messaging.Infrastructure.ServiceBus.Models;
using Microsoft.ServiceBus;

namespace Messaging.Infrastructure.ServiceBus.BusConfigurator
{
    public class AzureSbBusConfigurator : IBusConfigurator
    {
        // Properties
        public BusConfiguration Configuration { get; set; }
        public ServiceBusTimeoutConfiguration TimeoutConfig { get; set; }
        public string ConnectionString { get; set; }

        // Ctors

        /// <summary>
        /// User either BusConfiguration
        /// </summary>
        public AzureSbBusConfigurator(BusConfiguration configuration, ServiceBusTimeoutConfiguration timeoutConfig = null)
        {
            TimeoutConfig = timeoutConfig;
            Configuration = configuration;
            Configuration.ValidateConfigurationThrows();
        }

        /// <summary>
        /// Or use a connection string
        /// </summary>
        public AzureSbBusConfigurator(string connectionString, ServiceBusTimeoutConfiguration timeoutConfig = null)
        {
            TimeoutConfig = timeoutConfig;
            ConnectionString = connectionString;
        }

        // Implementation

        public IBusControl CreateBus(Action<IBusFactoryConfigurator, IHost> registrationAction = null)
        {
            var buz = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                IServiceBusHost host;

                if (!string.IsNullOrEmpty(ConnectionString))
                {
                    host = cfg.Host(ConnectionString, hst => { });
                }
                else
                {
                    var connUri = new Uri(Configuration.ConnectionUri);
                    host = cfg.Host(connUri, hst =>
                    {
                        hst.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(Configuration.Login,
                            Configuration.Password);
                    });
                }

                // Azure-specific configuration that isn't included in the interface of 'cfg'. Let these comments be, for reference!
                //var x = (ServiceBusBusFactoryConfigurator) cfg;
                //x.AutoDeleteOnIdle = TimeSpan.FromMinutes(5); // Minimum is 5 minutes, might change in the future.
                //x.EnableExpress = false;

                if (TimeoutConfig != null)
                {
                    // Renew lock to enable jobs to run longer than 5 minutes
                    cfg.LockDuration = TimeoutConfig.LockDuration;
                    if (TimeoutConfig.UseRenewLock != TimeSpan.Zero)
                        cfg.UseRenewLock(TimeoutConfig.UseRenewLock);
                }

                cfg.UseJsonSerializer();
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