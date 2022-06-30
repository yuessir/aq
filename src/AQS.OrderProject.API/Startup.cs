using System;
using System.Linq;
using AQS.OrderProject.API.Configuration;
using AQS.OrderProject.API.SeedWork;
using AQS.OrderProject.Application.Configuration;
using AQS.OrderProject.Application.Configuration.Validation;
using AQS.OrderProject.Domain.Configs;
using AQS.OrderProject.Domain.SeedWork;
using AQS.OrderProject.Infrastructure;
using AQS.OrderProject.Infrastructure.Caching;
using AQS.OrderProject.Infrastructure.Database;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;
using ILogger = Serilog.ILogger;

namespace AQS.OrderProject.API
{  
    public class Startup
    {
        private readonly IConfiguration _configuration;

        private const string SharedConnectionString = "DefaultConnection";

        private static ILogger _logger;

        public Startup(IWebHostEnvironment env)
        {
            _logger = ConfigureLogger();
            _logger.Information("Logger configured");

            this._configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Startup>()
                .Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<WebsiteConfig>(_configuration.GetSection("AQS"));
            services.Configure<GameConfig>(_configuration.GetSection("Game"));

            services.AddControllers();
            services.AddMemoryCache();
            services.AddSwaggerDocumentation();
            services.AddProblemDetails(x =>
            {
                x.Map<InvalidCommandException>(ex => new InvalidCommandProblemDetails(ex));
                x.Map<BusinessRuleValidationException>(ex => new BusinessRuleValidationExceptionProblemDetails(ex));
            });

            services.AddDbContext<SharedContext>(dbContextOptions => dbContextOptions.UseMySQL(_configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpContextAccessor();
            var serviceProvider = services.BuildServiceProvider();

            IExecutionContextAccessor executionContextAccessor = new ExecutionContextAccessor(serviceProvider.GetService<IHttpContextAccessor>());

            var children = this._configuration.GetSection("Caching").GetChildren();
            var cachingConfiguration = children.ToDictionary(child => child.Key, child => TimeSpan.Parse(child.Value));
            var memoryCache = serviceProvider.GetService<IMemoryCache>();

            return ApplicationStartup.Initialize(
                services, 
                this._configuration[SharedConnectionString],
                new MemoryCacheStore(memoryCache, cachingConfiguration),
                _logger,
                executionContextAccessor);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<CorrelationMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseProblemDetails();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSwaggerDocumentation();
        }

        private static ILogger ConfigureLogger()
        {
            return new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Context}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.RollingFile(new CompactJsonFormatter(), "logs/logs")
                .CreateLogger();
        }
    }
}
