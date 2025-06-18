using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RestaurantePro.Infrastructure.Monitoring.HealthChecks;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Net.Sockets;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Data.Common;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;

namespace RestaurantePro.Infrastructure.IntegrationTests.Monitoring.HealthChecks
{
    public sealed class DatabaseHealthCheckTests : IntegrationTestBase
    {
        public DatabaseHealthCheckTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task CheckHealthAsync_WhenDbIsAvailable_ShouldReturnHealthy()
        {
            // Arrange
            await ResetDatabaseAsync(); // Asegurar un estado limpio
            var healthCheck = ServiceProvider.GetRequiredService<DatabaseHealthCheck>();

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Healthy);
            result.Description.Should().Be("Conexión a la base de datos establecida correctamente");
        }

        [Fact]
        public async Task CheckHealthAsync_WhenDbIsUnavailable_ShouldReturnUnhealthy()
        {
            // Arrange
            var mockConnection = new Mock<DbConnection>();
            mockConnection.Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new SocketException());
            mockConnection.Setup(c => c.Database).Returns("TestDatabase");
            mockConnection.Setup(c => c.State).Returns(System.Data.ConnectionState.Closed);

            var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
                .UseSqlite(mockConnection.Object)
                .Options;

            var faultyDbContext = new RestauranteProDbContext(options, 
                new Mock<ILogger<RestauranteProDbContext>>().Object,
                new Mock<IDomainEventDispatcher>().Object);
            
            var services = new ServiceCollection();
            services.AddSingleton<RestauranteProDbContext>(faultyDbContext);
            services.AddSingleton<ILogger<DatabaseHealthCheck>>(new Mock<ILogger<DatabaseHealthCheck>>().Object);
            services.AddSingleton<DatabaseHealthCheck>();
            
            var serviceProvider = services.BuildServiceProvider();
            var healthCheck = serviceProvider.GetRequiredService<DatabaseHealthCheck>();

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Description.Should().Be("No se pudo establecer conexión con la base de datos");
        }
    }
} 