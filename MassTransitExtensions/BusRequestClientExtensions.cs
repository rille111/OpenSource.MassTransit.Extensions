using System;

// ReSharper disable once CheckNamespace
namespace MassTransit
{
    public static class BusRequestClientExtensions
    {
        /// <summary>
        /// Using this extension method will base the queue name of TRequest.
        /// </summary>
        /// <typeparam name="TRequest">The query interface/class. Should end with `Query or `Command - otherwise exception is thrown when validating.</typeparam>
        /// <typeparam name="TResponse">Whatever is expected to be returned.</typeparam>
        /// <param name="bus">What bus to work with</param>
        /// <param name="timeout">The timeout before the request is cancelled</param>
        /// <param name="ttl">THe time to live for the request message</param>
        /// <param name="callback">Callback when the request is sent</param>
        /// <returns>The response.</returns>
        public static IRequestClient<TRequest, TResponse> CreateRequestClient<TRequest, TResponse>(this IBusControl bus,
            TimeSpan timeout, TimeSpan? ttl = null, Action<SendContext<TRequest>> callback = null)
            where TRequest : class where TResponse : class
        {
            var deliverOnQueue = typeof (TRequest).Name;
            ValidationHelpers.EnsureNamingConventionForCommand(deliverOnQueue);
            var requestUri = new Uri(bus.ExtractBusAddress() + "/" + deliverOnQueue);
            return (IRequestClient<TRequest, TResponse>)new MessageRequestClient<TRequest, TResponse>(bus, requestUri, timeout, ttl, callback);
        }
    }
}
