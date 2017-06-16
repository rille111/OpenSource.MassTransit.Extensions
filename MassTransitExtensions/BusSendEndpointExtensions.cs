using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace MassTransit
{
    public static class BusSendEndpointExtensions
    {
        /// <summary>
        /// Uses the class name of TCommand to determine the name of the queue (Queue name should be unique per any Command)
        /// TCommand MUST end with 'Command'
        /// </summary>
        public static ISendEndpoint GetSendEndpoint<TCommand>(this IBusControl bus) where TCommand : class
        {
            var task = Task.Run(async () => await GetSendEndpointAsync<TCommand>(bus));
            return task.Result;
        }

        /// <summary>
        /// Uses the class name of TCommand to determine the name of the queue (Queue name should be unique per any Command)
        /// TCommand MUST end with 'Command'
        /// </summary>
        public static async Task<ISendEndpoint> GetSendEndpointAsync<TCommand>(this IBusControl bus) where TCommand : class
        {
            var commandType = typeof(TCommand);
            if (!commandType.IsInterface)
                throw new InvalidCastException($"Bus Send Endpoints must be configured to interfaces!");

            ValidationHelpers.EnsureNamingConventionForCommand(commandType.Name);

            return await GetSendEndpointAsync(bus, commandType.Name);
        }

        /// <summary>
        /// Please use the other overloads!
        /// </summary>
        public static async Task<ISendEndpoint> GetSendEndpointAsync(this IBusControl bus, string deliverOnQueue)
        {
            var newConn = bus.ExtractBusAddress();

            if (deliverOnQueue == null)
                throw new ArgumentNullException($"{nameof(deliverOnQueue)}");
            var newUri = new Uri(newConn + "/" + deliverOnQueue);
            var sender = await bus.GetSendEndpoint(newUri);

            return sender;
        }

    }
}
