﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AQS.OrderProject.Application.Configuration.Commands;
using AQS.OrderProject.Domain.SeedWork;
using AQS.OrderProject.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AQS.OrderProject.Infrastructure.Processing
{
    public class UnitOfWorkCommandHandlerWithResultDecorator<T, TResult> : ICommandHandler<T, TResult> where T : ICommand<TResult>
    {
        private readonly ICommandHandler<T, TResult> _decorated;

        private readonly IUnitOfWork _unitOfWork;

        private readonly OrdersContext _ordersContext;

        public UnitOfWorkCommandHandlerWithResultDecorator(
            ICommandHandler<T, TResult> decorated, 
            IUnitOfWork unitOfWork, 
            OrdersContext ordersContext)
        {
            _decorated = decorated;
            _unitOfWork = unitOfWork;
            _ordersContext = ordersContext;
        }

        public async Task<TResult> Handle(T command, CancellationToken cancellationToken)
        {
            var result = await this._decorated.Handle(command, cancellationToken);

            if (command is InternalCommandBase<TResult>)
            {
                var internalCommand = await _ordersContext.InternalCommands.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken: cancellationToken);

                if (internalCommand != null)
                {
                    internalCommand.ProcessedDate = DateTime.UtcNow;
                }
            }

            await this._unitOfWork.CommitAsync(cancellationToken);

            return result;
        }
    }
}