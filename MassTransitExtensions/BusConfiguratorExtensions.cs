using System;

// ReSharper disable once CheckNamespace
namespace MassTransit
{
    public static class BusConfiguratorExtensions
    {
        /// <summary>
        /// Prefer using this method when configuring a receive endpoint, so that the consumers for a command/event will listen on the correct queue.
        /// </summary>
        /// <typeparam name="TMessage">Must end with either Command, Event or Query (convention)</typeparam>
        public static void ReceiveEndpoint<TMessage>(this IBusFactoryConfigurator cfg, Action<IReceiveEndpointConfigurator> conf)
        {
            var className = typeof(TMessage).Name;
            ValidationHelpers.EnsureNamingConventionForMessage(className);
            cfg.ReceiveEndpoint(className, conf);
        }
    }
}
