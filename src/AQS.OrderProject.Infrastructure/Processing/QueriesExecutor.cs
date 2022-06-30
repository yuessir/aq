using System.Threading.Tasks;
using AQS.OrderProject.Application.Configuration.Queries;
using Autofac;
using MediatR;

namespace AQS.OrderProject.Infrastructure.Processing
{
    public static class QueriesExecutor
    {
        public static async Task<TResult> Execute<TResult>(IQuery<TResult> query)
        {
            using (var scope = CompositionRoot.BeginLifetimeScope())
            {
                var mediator = scope.Resolve<IMediator>();

                return await mediator.Send(query);
            }
        }
    }
}