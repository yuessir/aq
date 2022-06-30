using AQS.OrderProject.Domain.Customers.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AQS.OrderProject.Infrastructure.Domain.Orders
{
    public class UkNotMappingOrderEntityTypeConfiguration : IEntityTypeConfiguration<UkNotMappingOrder>
    {
        public void Configure(EntityTypeBuilder<UkNotMappingOrder> builder)
        {
            builder.ToTable("uk_not_mapping_order");

            builder.HasKey(x => x.OrderId);
            builder.Property(p => p.OrderId).HasColumnName("order_id").HasColumnType("VARCHAR(36)").IsRequired().HasConversion(new OrderIdValueConverter());
            builder.Property(p => p.CreatedTimeUtc).HasColumnName("created_time_utc").HasColumnType("DATETIME").IsRequired();
            builder.Property(p => p.RequestJson).HasColumnName("request_json").HasColumnType("TEXT").IsRequired();
            builder.Property(p => p.HgScheduledKickOffTimeUtc).HasColumnName("hg_scheduled_kick_off_time_utc").HasColumnType("DATETIME");
            builder.Property(p => p.HgLeagueName).HasColumnName("hg_league_name").HasColumnType("VARCHAR(100)");
            builder.Property(p => p.HgHomeTeam).HasColumnName("hg_home_team").HasColumnType("VARCHAR(100)");
            builder.Property(p => p.HgAwayTeam).HasColumnName("hg_away_team").HasColumnType("VARCHAR(100)");
            builder.Property(p => p.SboScheduledKickOffTimeUtc).HasColumnName("sbo_scheduled_kick_off_time_utc").HasColumnType("DATETIME");
            builder.Property(p => p.SboLeagueName).HasColumnName("sbo_league_name").HasColumnType("VARCHAR(100)");
            builder.Property(p => p.SboHomeTeam).HasColumnName("sbo_home_team").HasColumnType("VARCHAR(100)");
            builder.Property(p => p.SboAwayTeam).HasColumnName("sbo_away_team").HasColumnType("VARCHAR(100)");
            builder.Property(p => p.IbcScheduledKickOffTimeUtc).HasColumnName("ibc_scheduled_kick_off_time_utc").HasColumnType("DATETIME");
            builder.Property(p => p.IbcLeagueName).HasColumnName("ibc_league_name").HasColumnType("VARCHAR(100)");
            builder.Property(p => p.IbcHomeTeam).HasColumnName("ibc_home_team").HasColumnType("VARCHAR(100)");
            builder.Property(p => p.IbcAwayTeam).HasColumnName("ibc_away_team").HasColumnType("VARCHAR(100)");
        }
    }
}