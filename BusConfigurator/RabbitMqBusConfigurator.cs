using System;
using System.Threading.Tasks;
using MassTransit;
using Messaging.Infrastructure.ServiceBus.Models;

namespace Messaging.Infrastructure.ServiceBus.BusConfigurator
{
    public class RabbitMqBusConfigurator : IBusConfigurator
    {
        // Properties

        public BusConfiguration Configuration { get; set; }

        // Ctor

        public RabbitMqBusConfigurator(BusConfiguration configuration)
        {
            Configuration = configuration;
            Configuration.ValidateConfigurationThrows();
        }

        // Implementation

        /// <summary>
        /// Create & configures a bus. Note: No more consumers can be added after the bus is created.
        /// </summary>
        public IBusControl CreateBus(Action<IBusFactoryConfigurator, IHost> registrationAction = null)
        {
            var connUri = new Uri(Configuration.ConnectionUri);
            var connectionName = $"{System.Reflection.Assembly.GetEntryAssembly().GetName().Name}-{Environment.MachineName}-Connection-{Guid.NewGuid().ToString().Substring(0, 4)}";
            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(connUri, connectionName, hst =>
                {
                    hst.Username(Configuration.Login);
                    hst.Password(Configuration.Password);
                });

                cfg.UseJsonSerializer();
                cfg.PurgeOnStartup = false;
                cfg.AutoDelete = true;

                registrationAction?.Invoke(cfg, host);
            });

            return bus;
        }

        public Task<IBusControl> CreateBusAsync(Action<IBusFactoryConfigurator, IHost> registrationAction = null)
        {
            return Task.Run(() => CreateBus(registrationAction));
        }
    }
}
