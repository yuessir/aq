using System.Threading.Tasks;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Infrastructure.Domain.SharedData;
using Microsoft.Extensions.Options;

namespace AQS.OrderProject.Infrastructure.Domain.Orders;

public class UkNotMappingOrdersRepository : BaseRepository, IUkNotMappingOrdersRepository
{
    public UkNotMappingOrdersRepository(IOptions<WebsiteConfig> websiteConfig) : base(websiteConfig)
    {
    }

    public async Task InsertUkNotMappingOrder(string currentUser, UkNotMappingOrder notMappingOrder)
    {
        await using var context = this.GetContext(currentUser);
        await context.UkNotMappingOrders.AddAsync(notMappingOrder);
    }
}