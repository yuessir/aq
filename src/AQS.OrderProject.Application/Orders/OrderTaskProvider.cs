using System.Threading.Tasks;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Infrastructure;
using Autofac;
using Navyblue.BaseLibrary;
using Serilog;
using TFA.AQS.Order.Application;
using OrderTaskStatus = AQS.OrderProject.Domain.OrderTaskStatus;

namespace AQS.OrderProject.Application.Orders
{
    public class OrderTaskProvider
    {
        public static async Task<bool> CheckTaskDuplicated(string userName, int? matchMapId, MarketType marketType)
        {
            using var lifetimeScope = CompositionRoot.BeginLifetimeScope();

            var tasksRepository = lifetimeScope.Resolve<ITasksRepository>();

            return await tasksRepository.CheckTaskDuplicated(userName, matchMapId, marketType);
        }


        public static async Task<OrderTask> InsertTask(string userName, Order order, ILogger logger)
        {
            using var lifetimeScope = CompositionRoot.BeginLifetimeScope();

            var tasksRepository = lifetimeScope.Resolve<ITasksRepository>();

            var seq = await tasksRepository.GetNextSeq(userName, order.CreatedTime);

            var taskModel = BuildTasksModel(seq, order);

            var task = await tasksRepository.InsertTask(userName,
                taskModel.Seq,
                taskModel.MatchId,
                taskModel.MatchMapId,
                taskModel.MarketType,
                taskModel.TaskStatus,
                taskModel.Notified,
                taskModel.CreatedDate,
                taskModel.CreatedBy,
                taskModel.BetType,
                taskModel.FinishTime,
                taskModel.FinishBy);

            logger.Information(LogResource.V2TP01.FormatWith(order.OrderId, task?.ToJson()));

            if (task is not { Id: > 0, BetType: BetType.None })
            {
                //TODO Throw exception?????
            }

            order.SetTaskId(task.Id);

            return task;
        }

        private static TasksModel BuildTasksModel(int seq, Order order)
        {
            TasksModel task = new()
            {
                Seq = seq > 0 ? seq : 1,
                MatchMapId = order.MatchMapId,
                MarketType = order.MarketType,
                TaskStatus = OrderTaskStatus.Ship,
                Notified = false,
                CreatedDate = order.CreatedTime,
                CreatedBy = order.CreatedBy,
                BetType = order.BetType
            };
            return task;
        }
    }
}