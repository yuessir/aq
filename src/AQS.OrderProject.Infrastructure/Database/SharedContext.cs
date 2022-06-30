using AQS.OrderProject.Domain.SharedData;
using AQS.OrderProject.Infrastructure.Domain.SharedData;
using Microsoft.EntityFrameworkCore;

namespace AQS.OrderProject.Infrastructure.Database
{
    public class SharedContext : DbContext
    {
        public SharedContext(DbContextOptions<SharedContext> options) : base(options)
        {

        }

        public DbSet<Leagues> Leagues { get; set; }

        public DbSet<Matches> Matches { get; set; }

        public DbSet<MatchMap> MatchMap { get; set; }

        public DbSet<TeamMap> TeamMap { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new LeaguesEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MatchesEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MatchMapEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TeamMapEntityTypeConfiguration());
        }
    }
}