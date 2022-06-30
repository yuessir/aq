using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Infrastructure.Processing.Outbox;
using MediatR;

namespace AQS.OrderProject.Infrastructure.Processing.InternalCommands
{
    internal class ProcessInternalCommandsCommand : CommandBase<Unit>, IRecurringCommand
    {

    }
}