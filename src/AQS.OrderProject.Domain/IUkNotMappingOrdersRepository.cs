using System.Threading.Tasks;
using AQS.OrderProject.Domain.Customers.Orders;

namespace AQS.OrderProject.Domain;

public interface IUkNotMappingOrdersRepository
{
    Task InsertUkNotMappingOrder(string currentUser, UkNotMappingOrder notMappingOrder);
}