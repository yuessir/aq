using System;
using System.Threading.Tasks;

namespace AQS.OrderProject.Infrastructure.Processing
{
    public interface ICommandsDispatcher
    {
        Task DispatchCommandAsync(Guid id);
    }
}
