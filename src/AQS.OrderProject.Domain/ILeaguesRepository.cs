using System.Collections.Generic;
using System.Threading.Tasks;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.SharedData;

namespace AQS.OrderProject.Domain
{
    public interface ILeaguesRepository
    {
        Task<Leagues> FindByWebsiteAndLeagueName(OrderMappingType website, string leagueName);

        Task<List<Leagues>> FindAll();
    }
}