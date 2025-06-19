using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Infrastructure.DependencyInjection;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using Xunit;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;

namespace RestaurantePro.Infrastructure.IntegrationTests.DependencyInjection
{
    public class InfrastructureSetupTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public InfrastructureSetupTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:DefaultConnection", "DataSource=file::memory:?cache=shared" },
                    { "JwtSettings:Secret", "super-secret-key-for-testing-purposes-only" },
                    { "JwtSettings:Issuer", "test-issuer" },
                    { "JwtSettings:Audience", "test-audience" },
                    { "JwtSettings:ExpirationInMinutes", "60" },
                    { "JwtSettings:RefreshTokenExpirationInDays", "7" }
                })
                .Build();

            services.AddSingleton(_configuration);

            services.AddInfrastructureServices(_configuration);
            
            services.AddSingleton(Substitute.For<IDomainEventDispatcher>());

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RestauranteProDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            services.AddScoped<DbContext>(provider => provider.GetRequiredService<RestauranteProDbContext>());

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void AddInfrastructureServices_ShouldRegisterCommonServices()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IDelayProvider>());
            Assert.NotNull(_serviceProvider.GetService<ITimeProvider>());
        }

        [Fact]
        public void AddInfrastructureServices_ShouldRegisterPersistenceServices()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IUnitOfWork>());
        }

        [Fact]
        public void AddInfrastructureServices_ShouldRegisterIdentityServices()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IIdentityService>());
            Assert.NotNull(_serviceProvider.GetService<IJwtTokenService>());
        }

        [Fact]
        public void AddInfrastructureServices_ShouldRegisterExternalServices()
        {
            // Assert
            // We check one of the registered services, e.g., IEmailService
            Assert.NotNull(_serviceProvider.GetService<IEmailService>());
        }

        [Fact]
        public void AddInfrastructureServices_ShouldRegisterCachingServices()
        {
            // Assert
            // For testing, MemoryCacheService is registered
            Assert.NotNull(_serviceProvider.GetService<ICacheService>());
        }

        [Fact]
        public void AddInfrastructureServices_ShouldRegisterLoggingServices()
        {
            // Assert
            // Check if a logger factory is available
            Assert.NotNull(_serviceProvider.GetService<ILoggerFactory>());
        }
    }
} 