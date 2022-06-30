using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Infrastructure.Processing.InternalCommands;
using AQS.OrderProject.Infrastructure.Processing.Outbox;
using Microsoft.EntityFrameworkCore;

namespace AQS.OrderProject.Infrastructure.Database
{
    public class OrdersContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderTask> Tasks { get; set; }

        public DbSet<UkNotMappingOrder> UkNotMappingOrders { get; set;}

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        public DbSet<InternalCommand> InternalCommands { get; set; }

        public OrdersContext(DbContextOptions options) : base(options)
        {

        }

        private readonly string _connectionString;

        public OrdersContext(string connectionString, DbContextOptions<OrdersContext> options) : base(options)
        {
            this._connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(this._connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersContext).Assembly);
        }
    }
}
