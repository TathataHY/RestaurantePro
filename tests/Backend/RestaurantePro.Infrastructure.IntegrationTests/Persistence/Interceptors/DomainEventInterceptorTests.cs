using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Interceptors
{
    public class DomainEventInterceptorTests
    {
        private readonly Mock<IDomainEventDispatcher> _dispatcherMock;
        private readonly Mock<ILogger<DomainEventInterceptor>> _loggerInterceptorMock;
        private readonly Mock<ILogger<RestauranteProDbContext>> _loggerDbContextMock;

        public DomainEventInterceptorTests()
        {
            _dispatcherMock = new Mock<IDomainEventDispatcher>();
            _loggerInterceptorMock = new Mock<ILogger<DomainEventInterceptor>>();
            _loggerDbContextMock = new Mock<ILogger<RestauranteProDbContext>>();
        }

        private TestInterceptorDbContext CreateDbContext(DomainEventInterceptor interceptor)
        {
            var options = new DbContextOptionsBuilder<TestInterceptorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;

            return new TestInterceptorDbContext(options);
        }

        [Fact]
        public async Task SavingChanges_DebeDespacharEventosDeDominio()
        {
            // Arrange
            var interceptor = new DomainEventInterceptor(_dispatcherMock.Object, _loggerInterceptorMock.Object);
            var dbContext = CreateDbContext(interceptor);

            var entity = AuditableTestEntity.Crear("Test");
            entity.RegistrarEventoPrueba();


            // Act
            dbContext.AuditableTestEntities.Add(entity);
            await dbContext.SaveChangesAsync();

            // Assert
            // Verificar que el dispatcher fue llamado una vez con el evento correcto
            _dispatcherMock.Verify(d => d.Dispatch(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            
            // Verificar que los eventos se limpiaron de la entidad
            entity.DomainEvents.Should().BeEmpty();
        }
    }
} 