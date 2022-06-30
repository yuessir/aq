using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.Customers.Orders;
using Dapper;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace AQS.OrderProject.Application.Orders.DomainServices;

public class OrderDuplicateChecker: IOrderDuplicateChecker
{
    private readonly WebsiteConfig _webSiteConfig;

    public OrderDuplicateChecker(IOptions<WebsiteConfig> webSiteConfig)
    {
        _webSiteConfig = webSiteConfig.Value;
    }

    public bool IsDuplicated(string orderId, string userName)
    {
        using var connection = new MySqlConnection(_webSiteConfig.GetDataSource(userName).ConnectionString);

        const string sql = "SELECT 1 FROM ORDERS WHERE ORDER_ID = @OrderId LIMIT 1; ";
        var customersNumber = connection.QuerySingleOrDefault<int?>(sql,
            new
            {
                OrderId = orderId
            });

        return !customersNumber.HasValue;
    }
}