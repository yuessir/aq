using AQS.OrderProject.Domain.SharedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AQS.OrderProject.Infrastructure.Domain.SharedData
{
    public class MatchMapEntityTypeConfiguration : IEntityTypeConfiguration<MatchMap>
    {
        public void Configure(EntityTypeBuilder<MatchMap> builder)
        {
            builder.ToTable("match_map");

            builder.HasKey(x => x.Id);
            builder.Property(p => p.HomeTeamId).HasColumnName("home_team_id").HasColumnType("INT").HasDefaultValue(0).IsRequired();
            builder.Property(p => p.HomeTeam).HasColumnName("home_team").HasColumnType("VARCHAR(200)").IsRequired();
            builder.Property(p => p.AwayTeamId).HasColumnName("away_team_id").HasColumnType("INT").IsRequired();
            builder.Property(p => p.AwayTeam).HasColumnName("away_team").HasColumnType("VARCHAR(200)").HasDefaultValue(0).IsRequired();
            builder.Property(p => p.Time).HasColumnName("time").HasColumnType("DATETIME");
            builder.Property(p => p.Website).HasColumnName("web_site").HasColumnType("INT").IsRequired();
        }
    }
}