using System;

namespace Messaging.Infrastructure.ServiceBus.Models
{
    public class BusConfiguration
    {
        /// <summary>
        /// Example: rabbitmq://rabbit.xxxxx.se/[VirtualHost]
        /// Example: sb://xxxxx.servicebus.windows.net
        /// AzureSb seems to use "Path", how does that work? Dunno.
        /// Now, how should we see VirtualHost for MassTransit? Is it the Domain for the system?
        /// Networks are segregated by vhosts (http://masstransit.readthedocs.io/en/master/configuration/transports/rabbitmq.html)
        /// </summary>
        public string ConnectionUri { get; set; }

        /// <summary>
        /// Either the login for Rabbit, or policyname (Shared Access Policy) in Azure.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Either the password for Rabbit, or the Shared Access Policy Key in Azure.
        /// </summary>
        public string Password { get; set; }
    }

    public static class BusConfigurationExtensions
    {
        public static void ValidateConfigurationThrows(this BusConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException($"{nameof(configuration)}");
            if (configuration.ConnectionUri == null)
                throw new ArgumentNullException($"{nameof(configuration.ConnectionUri)}");
            if (configuration.Login == null)
                throw new ArgumentNullException($"{nameof(configuration.Login)}");
            if (configuration.Password == null)
                throw new ArgumentNullException($"{nameof(configuration.Password)}");
            if (configuration.ConnectionUri.EndsWith("/"))
                throw new ArgumentException($"ConnectionUri can not end with slash!", $"{nameof(configuration.ConnectionUri)}");
        }
    }
}