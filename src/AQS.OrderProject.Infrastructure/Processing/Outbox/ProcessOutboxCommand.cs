using AQS.OrderProject.Application.Configuration.Commands;
using MediatR;

namespace AQS.OrderProject.Infrastructure.Processing.Outbox
{
    public class ProcessOutboxCommand : CommandBase<Unit>, IRecurringCommand
    {

    }
}