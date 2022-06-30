using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Domain.SeedWork;
using AQS.OrderProject.Infrastructure.Database;
using AQS.OrderProject.Infrastructure.Processing;

namespace AQS.OrderProject.Infrastructure.Domain
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrdersContext _ordersContext;
        private readonly IDomainEventsDispatcher _domainEventsDispatcher;

        public UnitOfWork(
            OrdersContext ordersContext, 
            IDomainEventsDispatcher domainEventsDispatcher)
        {
            this._ordersContext = ordersContext;
            this._domainEventsDispatcher = domainEventsDispatcher;
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await this._domainEventsDispatcher.DispatchEventsAsync();
            return await this._ordersContext.SaveChangesAsync(cancellationToken);
        }
    }
}