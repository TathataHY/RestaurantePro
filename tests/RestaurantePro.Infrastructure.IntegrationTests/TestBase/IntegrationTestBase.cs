using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using Scrutor;
using Xunit;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly TestDbContext DbContext;

        protected IntegrationTestBase()
        {
            var services = new ServiceCollection();

            // Mocks
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(m => m.UserId).Returns("test-user");

            var dateTimeServiceMock = new Mock<IDateTimeService>();
            dateTimeServiceMock.Setup(m => m.Now).Returns(DateTime.UtcNow);
            
            var domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();

            services.AddSingleton(currentUserServiceMock.Object);
            services.AddSingleton(dateTimeServiceMock.Object);
            services.AddSingleton(domainEventDispatcherMock.Object);

            // Logging
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            // Interceptors
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<SoftDeleteInterceptor>();
            services.AddScoped<DomainEventInterceptor>();
            
            // Configure DbContext
            services.AddScoped<TestDbContext>(sp =>
            {
                var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
                       .UseInMemoryDatabase(Guid.NewGuid().ToString())
                       .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                       .AddInterceptors(
                           sp.GetRequiredService<AuditableEntityInterceptor>(),
                           sp.GetRequiredService<SoftDeleteInterceptor>(),
                           sp.GetRequiredService<DomainEventInterceptor>()
                       ).Options;
                
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                return new TestDbContext(
                    options,
                    loggerFactory.CreateLogger<RestauranteProDbContext>()
                );
            });
            
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<TestDbContext>());
            services.AddScoped<RestauranteProDbContext>(provider => provider.GetRequiredService<TestDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Add repositories
            services.Scan(scan => scan
                .FromAssemblyOf<UnitOfWork>()
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            ServiceProvider = services.BuildServiceProvider();
            
            DbContext = ServiceProvider.GetRequiredService<TestDbContext>();

            // Inicializar y sembrar la base de datos para cada prueba
            ResetDatabaseAsync().GetAwaiter().GetResult();
        }

        protected virtual async Task SeedDataAsync()
        {
            // Este método puede ser sobreescrito por clases de prueba para sembrar datos específicos.
            await Task.CompletedTask;
        }

        public async Task ResetDatabaseAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            await SeedDataAsync();
        }

        public void Dispose()
        {
            DbContext.Dispose();
            (ServiceProvider as IDisposable)?.Dispose();
        }
    }
} 