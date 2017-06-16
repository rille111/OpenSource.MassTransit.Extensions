using System;
using System.Threading.Tasks;
using MassTransit;
using Messaging.Infrastructure.ServiceBus.Models;

namespace Messaging.Infrastructure.ServiceBus.BusConfigurator
{
    /// <summary>
    /// Helps you configure and create a Bus that you use for publishing, sending and consuming - messages.
    /// Note: For publishing (Events) you do it directly from the Bus. For sending (commands), you need to configure a Send Endpoint, see code examples in Messaging.Labs.
    /// </summary>
    public interface IBusConfigurator
    {
        BusConfiguration Configuration { get; set; }

        /// <summary>
        /// Async version (unstable). A Bus adds Consumers that react to Events/Commands, and can also be used to publish Events. NOTE: Do not send Commands/Queries by using Bus.Publish()!
        /// Remember to pass along RetryPolicy!
        /// </summary>
        /// <param name="registrationAction">/// Important: Since lambda-based retrypolicy creators dont work, use the old: cfg.UseRetry(Retry.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(5)));</param>
        /// <returns></returns>
        IBusControl CreateBus(Action<IBusFactoryConfigurator, IHost> registrationAction = null);

        /// <summary>
        /// A Bus adds Consumers that react to Events/Commands, and can also be used to publish Events. NOTE: Do not send Commands/Queries by using Bus.Publish()!
        /// Remember to pass along RetryPolicy!
        /// </summary>
        /// <param name="registrationAction">/// Important: Since lambda-based retrypolicy creators dont work, use the old: cfg.UseRetry(Retry.Exponential(3, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(5)));</param>
        /// <returns></returns>
        Task<IBusControl> CreateBusAsync(Action<IBusFactoryConfigurator, IHost> registrationAction = null);
    }
}
