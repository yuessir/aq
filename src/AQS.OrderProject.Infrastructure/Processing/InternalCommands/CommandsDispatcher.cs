using System;
using System.Reflection;
using System.Threading.Tasks;
using AQS.OrderProject.Application.Orders.V2.PlaceOrder;
using AQS.OrderProject.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AQS.OrderProject.Infrastructure.Processing.InternalCommands
{
    public class CommandsDispatcher : ICommandsDispatcher
    {
        private readonly IMediator _mediator;
        private readonly OrdersContext _ordersContext;

        public CommandsDispatcher(
            IMediator mediator, 
            OrdersContext ordersContext)
        {
            this._mediator = mediator;
            this._ordersContext = ordersContext;
        }

        public async Task DispatchCommandAsync(Guid id)
        {
            var internalCommand = await this._ordersContext.InternalCommands.SingleOrDefaultAsync(x => x.Id == id);

            Type type = Assembly.GetAssembly(typeof(PlaceCustomerOrderCommand)).GetType(internalCommand.Type);
            dynamic command = JsonConvert.DeserializeObject(internalCommand.Data, type);

            internalCommand.ProcessedDate = DateTime.UtcNow;

            await this._mediator.Send(command);
        }
    }
}