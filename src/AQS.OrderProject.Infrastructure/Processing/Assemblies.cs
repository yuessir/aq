using System.Reflection;
using AQS.OrderProject.Application.Orders.V2.PlaceOrder;

namespace AQS.OrderProject.Infrastructure.Processing
{
    internal static class Assemblies
    {
        public static readonly Assembly Application = typeof(PlaceCustomerOrderCommand).Assembly;
    }
}