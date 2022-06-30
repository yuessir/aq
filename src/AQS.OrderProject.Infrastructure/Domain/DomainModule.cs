using AQS.OrderProject.Application.Orders.DomainServices;
using AQS.OrderProject.Domain.Customers.Orders;
using Autofac;

namespace AQS.OrderProject.Infrastructure.Domain
{
    public class DomainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OrderDuplicateChecker>()
                .As<IOrderDuplicateChecker>()
                .InstancePerLifetimeScope();
        }
    }
}