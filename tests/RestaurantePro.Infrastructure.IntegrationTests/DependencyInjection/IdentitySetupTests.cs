using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Infrastructure.DependencyInjection;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Collections.Generic;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.DependencyInjection
{
    public class IdentitySetupTests
    {
        private readonly IServiceProvider _serviceProvider;

        public IdentitySetupTests()
        {
            var services = new ServiceCollection();

            // Configuración en memoria para JWT e Identity
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "TestSecretKeyForJwt-1234567890"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"},
                {"JwtSettings:ExpirationInMinutes", "60"},
                {"JwtSettings:RefreshTokenExpirationInDays", "7"},
                {"IdentitySettings:PasswordSettings:RequiredLength", "8"},
                // Agrega otras configuraciones de identity si son necesarias para que el setup no falle
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            services.AddSingleton(configuration);

            // Mocks de dependencias de persistencia (similares a PersistenceSetupTests)
            services.AddSingleton(Substitute.For<IDomainEventDispatcher>());
            services.AddSingleton(Substitute.For<ICurrentUserService>());
            services.AddSingleton(Substitute.For<IDateTimeService>());

            // DbContext en memoria
            services.AddDbContext<RestauranteProDbContext>(options =>
                options.UseInMemoryDatabase("TestIdentityDb"));
            
            services.AddLogging();

            // Llamar al método de configuración de identidad
            services.AddIdentityServices(configuration);

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void AddIdentityServices_ShouldRegisterIdentityServices()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<IJwtTokenService>());
            Assert.NotNull(_serviceProvider.GetService<IIdentityService>());
            Assert.NotNull(_serviceProvider.GetService<IUserPermissionService>());
        }

        [Fact]
        public void AddIdentityServices_ShouldRegisterCoreIdentityServices()
        {
            // Assert
            Assert.NotNull(_serviceProvider.GetService<UserManager<IdentityApplicationUser>>());
            Assert.NotNull(_serviceProvider.GetService<RoleManager<ApplicationRole>>());
            Assert.NotNull(_serviceProvider.GetService<SignInManager<IdentityApplicationUser>>());
        }
    }
} 