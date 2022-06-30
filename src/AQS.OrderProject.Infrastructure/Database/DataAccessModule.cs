using System;
using AQS.OrderProject.Application.Configuration.Data;
using AQS.OrderProject.Domain;
using AQS.OrderProject.Domain.Customers.Orders;
using AQS.OrderProject.Domain.SeedWork;
using AQS.OrderProject.Infrastructure.Domain;
using AQS.OrderProject.Infrastructure.Domain.Orders;
using AQS.OrderProject.Infrastructure.Domain.SharedData;
using AQS.OrderProject.Infrastructure.SeedWork;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AQS.OrderProject.Infrastructure.Database
{
    public class DataAccessModule : Module
    {
        private readonly string _databaseConnectionString;

        public DataAccessModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SqlConnectionFactory>()
                .As<ISqlConnectionFactory>()
                .WithParameter("connectionString", _databaseConnectionString)
                .InstancePerLifetimeScope();

            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
            builder.RegisterType<OrdersRepository>().As<IOrdersRepository>().InstancePerLifetimeScope();
            builder.RegisterType<UkNotMappingOrdersRepository>().As<IUkNotMappingOrdersRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TasksRepository>().As<ITasksRepository>().InstancePerLifetimeScope();
            builder.RegisterType<LeaguesRepository>().As<ILeaguesRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MatchRepository>().As<IMatchRepository>().InstancePerLifetimeScope();
            builder.RegisterType<RedisRepository>().As<IRedisRepository>().InstancePerLifetimeScope();
            builder.RegisterType<TasksRepository>().As<ITasksRepository>().InstancePerLifetimeScope();
            builder.RegisterType<StronglyTypedIdValueConverterSelector>().As<IValueConverterSelector>().SingleInstance();

            builder
                .Register(c =>
                {
                    var dbContextOptionsBuilder = new DbContextOptionsBuilder<OrdersContext>();
                    dbContextOptionsBuilder.UseMySQL(_databaseConnectionString);
                    dbContextOptionsBuilder.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();

                    return new OrdersContext(dbContextOptionsBuilder.Options);
                })
                .AsSelf()
                .As<DbContext>()
                .InstancePerLifetimeScope();
        }
    }
}