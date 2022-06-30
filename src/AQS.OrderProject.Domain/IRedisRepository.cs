using System.Collections.Generic;
using System.Threading.Tasks;
using AQS.OrderProject.Domain.Customers.Orders;

namespace AQS.OrderProject.Domain
{
    public interface IRedisRepository
    {
        string getRedisKey(RedisCacheCategory category, string companyCode, string key);

        void put<T>(RedisCacheCategory category, string companyCode, string key, T value) where T : class, new();

        Task<T> get<T>(RedisCacheCategory category, string companyCode, string key) where T : class, new();

        Task<List<CrawlerDistributionModel>> getAllCrawlerDistribution();

        Task<List<CrawlerServer>> getAllCrawlerServers();

        Task<bool> exists(RedisCacheCategory category, string companyCode, string key);

        Task<string> remove(RedisCacheCategory category, string companyCode, string key);
    }
}