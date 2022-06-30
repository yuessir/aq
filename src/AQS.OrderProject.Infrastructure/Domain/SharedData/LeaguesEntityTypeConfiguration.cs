using AQS.OrderProject.Domain.SharedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AQS.OrderProject.Infrastructure.Domain.SharedData
{
    public class LeaguesEntityTypeConfiguration : IEntityTypeConfiguration<Leagues>
    {
        public void Configure(EntityTypeBuilder<Leagues> builder)
        {
            builder.ToTable("Leagues");

            builder.HasKey(x => x.Id);
            builder.Property(p => p.LeagueId).HasColumnName("league_id").HasColumnType("VARCHAR(100)");
            builder.Property(p => p.LeagueMapId).HasColumnName("league_map_id").HasColumnType("INT");
            builder.Property(p => p.LeagueName).HasColumnName("league_name").HasColumnType("VARCHAR(200)").IsRequired();
            builder.Property(p => p.Website).HasColumnName("web_site").HasColumnType("INT").IsRequired();
            builder.Property(p => p.Sport).HasColumnName("sport").HasColumnType("INT").IsRequired();
        }
    }
}