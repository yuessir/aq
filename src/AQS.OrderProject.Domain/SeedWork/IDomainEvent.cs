using System;
using MediatR;

namespace AQS.OrderProject.Domain.SeedWork
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}