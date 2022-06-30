using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.SharedData;

namespace AQS.OrderProject.Domain
{
    public interface IMatchRepository
    {
        Task<Matches> GetMatch(int matchId);

        Task<Matches> FindByWebsiteName(OrderMappingType website, DateTime scheduledKickOffTimeUtc, string homeTeam, string awayTeam, bool isLive);

        Task<Matches> FindByUkName(OrderMappingType website, DateTime scheduledKickOffTimeUtc, string homeTeam, string awayTeam, bool isLive);

        Task<Matches> FindByUkNameOrWebsiteName(OrderId orderId, OrderMappingType website, string leagueName,
            DateTime scheduledKickOffTimeUtc, string homeTeam, string awayTeam, bool isLive);

        Task<List<Matches>> FindLastTenMatches();

        Task UpdateMatchMapId(int matchId, int matchMapId);

        Task UpdateMatchMapIdAndTeamId(int matchId, int matchMapId, int homeTeamId, int awayTeamId);

        Task UpdateTeamMap(int matchId, int matchMapId, int homeTeamId, int awayTeamId);

        Task UpdateTeamMap(int teamId, OrderMappingType website, string teamName);

        Task<int> GetMatchMapIdByTeamName(string homeTeam, string awayTeam, DateTime matchTime);

        Task<int> GetMatchMapIdByTeamId(int homeTeamId, int awayTeamId, DateTime matchTime);

        Task InsertMatchMap(MatchMap matchMap);
    }
}