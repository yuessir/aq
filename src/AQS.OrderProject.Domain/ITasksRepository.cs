using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AQS.OrderProject.Domain.Customers.Orders;

namespace AQS.OrderProject.Domain
{
    public interface ITasksRepository
    {
        Task<List<OrderTask>> FindAll(string userName);

        Task<int> GetNextSeq(string userName, DateTime createdTime);

        Task<OrderTask> InsertTask(string userName, int seq,
            int matchId,
            int? matchMapId,
            MarketType marketType,
            OrderTaskStatus taskStatus,
            bool notified,
            DateTime createdDate,
            string createdBy,
            BetType betType,
            DateTime finishTime,
            string finishBy);

        Task UpdateTaskMarketTypeAndBetType(string userName, int taskId, MarketType marketType, BetType betType);

        Task UpdateTaskToCancel(string userName, int taskId, OrderTaskStatus customerCanceled, DateTime finishTime, string hitterId);

        Task<bool> CheckTaskDuplicated(string userName, int? matchMapId, MarketType marketType);
    }
}