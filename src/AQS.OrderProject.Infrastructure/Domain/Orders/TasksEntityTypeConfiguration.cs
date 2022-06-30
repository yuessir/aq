using AQS.OrderProject.Domain.Customers.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AQS.OrderProject.Infrastructure.Domain.Orders
{
    public class TasksEntityTypeConfiguration : IEntityTypeConfiguration<OrderTask>
    {
        public void Configure(EntityTypeBuilder<OrderTask> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(x => x.Id);
            builder.Property(p => p.Seq).HasColumnName("Seq").HasColumnType("INT").HasDefaultValue(0).IsRequired();
            builder.Property(p => p.MatchId).HasColumnName("Match_Id").HasColumnType("INT").IsRequired();
            builder.Property(p => p.MarketType).HasColumnName("MarketType").HasColumnType("INT").IsRequired();
            builder.Property(p => p.TaskStatus).HasColumnName("Status").HasColumnType("INT").IsRequired();
            builder.Property(p => p.Notified).HasColumnName("Notified").HasColumnType("TINYINT(1)");
            builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate").HasColumnType("DATETIME").IsRequired();
            builder.Property(p => p.CreatedBy).HasColumnName("CreatedBy").HasColumnType("VARCHAR(45)").IsRequired();
            builder.Property(p => p.BetType).HasColumnName("BetType").HasColumnType("INT");
            builder.Property(p => p.FinishTime).HasColumnName("FinishTime").HasColumnType("DATETIME");
            builder.Property(p => p.FinishBy).HasColumnName("FinishBy").HasColumnType("VARCHAR(45)");
        }
    }
}