using System.Threading.Tasks;
using AQS.OrderProject.Application.Configuration.Commands;

namespace AQS.OrderProject.Application.Configuration.Processing
{
    public interface ICommandsScheduler
    {
        Task EnqueueAsync<T>(ICommand<T> command);
    }
}