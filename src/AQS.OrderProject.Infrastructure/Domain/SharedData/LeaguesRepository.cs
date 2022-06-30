using System.Collections.Generic;
using System.Threading.Tasks;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.SharedData;
using AQS.OrderProject.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AQS.OrderProject.Infrastructure.Domain.SharedData
{
    public class LeaguesRepository : ILeaguesRepository
    {
        private readonly SharedContext _sharedContext;

        public LeaguesRepository(SharedContext sharedContext)
        {
            this._sharedContext = sharedContext;
        }

        public async Task<Leagues> FindByWebsiteAndLeagueName(OrderMappingType website, string leagueName)
        {
            return await this._sharedContext.Leagues.FirstOrDefaultAsync(p =>
                p.Website == website && p.LeagueName == leagueName);
        }

        public async Task<List<Leagues>> FindAll()
        {
            return await this._sharedContext.Leagues.ToListAsync();
        }
    }
}