using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace MassTransit
{
    public static class ContextExtensions
    {
        public static async Task SendAsync<TCommand>(this ConsumeContext context, TCommand command) where TCommand : class, new()
        {
            var sendPoint = await context.GetSendEndpointAsync<TCommand>();
            await sendPoint.Send(command);
        }

        public static async Task<ISendEndpoint> GetSendEndpointAsync<TCommand>(this ConsumeContext context) where TCommand : class, new()
        {
            var busBaseAddress = context.DestinationAddress;
            var className = typeof(TCommand).Name;

            ValidationHelpers.EnsureNamingConventionForCommand(className);

            var completeUri = BusAddressHelpers.CreateSendEndPointUri<TCommand>(busBaseAddress);
            var sender = await context.GetSendEndpoint(completeUri);

            return sender;
        }
    }
}
