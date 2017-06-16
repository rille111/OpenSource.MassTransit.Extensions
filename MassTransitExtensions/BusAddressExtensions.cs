// ReSharper disable once CheckNamespace
namespace MassTransit
{
    public static class BusAddressExtensions
    {
        public static string ExtractBusAddress(this IBusControl bus)
        {
            return bus.Address.ExtractBusAddress();
        }
    }
}
