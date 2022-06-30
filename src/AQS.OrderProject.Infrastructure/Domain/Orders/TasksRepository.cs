using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Infrastructure.Domain.SharedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderTaskStatus = AQS.OrderProject.Domain.OrderTaskStatus;

namespace AQS.OrderProject.Infrastructure.Domain.Orders
{
    public class TasksRepository : BaseRepository, ITasksRepository
    {
        public TasksRepository(IOptions<WebsiteConfig> websiteConfig) : base(websiteConfig)
        {
        }

        public async Task<List<OrderTask>> FindAll(string userName)
        {
            await using var context = GetContext(userName);
            return await context.Tasks.ToListAsync();
        }

        public async Task<int> GetNextSeq(string userName, DateTime createdTime)
        {
            await using var context = GetContext(userName);
            var startTime = createdTime.Date;
            var endTime = startTime.AddDays(1);

            return await context.Tasks.Where(p =>
                p.CreatedDate >= startTime 
                && p.CreatedDate < endTime)
                .MaxAsync(p => p.Seq + 1);
        }

        public async Task<OrderTask> InsertTask(string userName, int seq,
            int matchId,
            int? matchMapId,
            MarketType marketType,
            OrderTaskStatus taskStatus,
            bool notified,
            DateTime createdDate,
            string createdBy,
            BetType betType,
            DateTime finishTime,
            string finishBy)
        {
            await using var context = GetContext(userName);
            var tasks = new OrderTask(seq,
                matchId,
                matchMapId,
                marketType,
                taskStatus,
                notified,
                createdDate,
                createdBy,
                betType,
                finishTime,
                finishBy);

            context.Tasks.Add(tasks);

            return tasks;
        }

        public async Task UpdateTaskMarketTypeAndBetType(string userName, int taskId, MarketType marketType, BetType betType)
        {
            await using var context = GetContext(userName);
            var result = await context.Tasks.FirstOrDefaultAsync(p => p.Id == taskId);
            result?.UpdateTaskMarketTypeAndBetType(marketType, betType);
        }

        public async Task UpdateTaskToCancel(string userName, int taskId, OrderTaskStatus customerCanceled, DateTime finishTime, string hitterId)
        {
            await using var context = GetContext(userName);
            var result = await context.Tasks.FirstOrDefaultAsync(p => p.Id == taskId);
            result?.UpdateTaskToCancel(customerCanceled, finishTime, hitterId);
        }

        public async Task<bool> CheckTaskDuplicated(string userName, int? matchMapId, MarketType marketType)
        {
            await using var context = GetContext(userName);
            return await context.Tasks.AnyAsync(p =>
                p.MatchId == matchMapId 
                && p.MarketType == marketType 
                && p.TaskStatus == OrderTaskStatus.Ship);
        }
    }
}