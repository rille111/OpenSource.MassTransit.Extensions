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
        public ServiceBusTimeoutConfiguration TimeoutConfig { get; set; }
        public string ConnectionString { get; set; }

        // Ctors

        public RabbitMqBusConfigurator(BusConfiguration configuration)
        {
            Configuration = configuration;
            Configuration = configuration;
            Configuration.ValidateConfigurationThrows();
        }

        // Implementation

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
