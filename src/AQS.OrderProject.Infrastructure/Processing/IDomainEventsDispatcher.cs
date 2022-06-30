using System.Threading.Tasks;

namespace AQS.OrderProject.Infrastructure.Processing
{
    public interface IDomainEventsDispatcher
    {
        Task DispatchEventsAsync();
    }
}