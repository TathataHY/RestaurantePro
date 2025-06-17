using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Infrastructure.Persistence;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Interceptors
{
    public class AuditableEntityInterceptorTests
    {
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ILogger<AuditableEntityInterceptor>> _loggerInterceptorMock;
        private readonly Mock<ILogger<TestDbContext>> _loggerDbContextMock;

        public AuditableEntityInterceptorTests()
        {
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _loggerInterceptorMock = new Mock<ILogger<AuditableEntityInterceptor>>();
            _loggerDbContextMock = new Mock<ILogger<TestDbContext>>();
        }

        private TestDbContext CreateDbContext(AuditableEntityInterceptor interceptor)
        {
            var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;

            return new TestDbContext(options, _loggerDbContextMock.Object);
        }

        [Fact]
        public async Task SavingChanges_DebeEstablecerFechaCreacion_CuandoSeAgregaEntidad()
        {
            // Arrange
            var now = new DateTime(2024, 1, 1, 12, 0, 0);
            _dateTimeServiceMock.Setup(s => s.Now).Returns(now);
            _currentUserServiceMock.Setup(s => s.UserId).Returns("test-user");

            var interceptor = new AuditableEntityInterceptor(_currentUserServiceMock.Object, _dateTimeServiceMock.Object, _loggerInterceptorMock.Object);
            var dbContext = CreateDbContext(interceptor);

            var entity = AuditableTestEntity.Crear("Test Entity");
            entity.SetFechaCreacionForTesting(default);

            // Act
            dbContext.TestEntities.Add(entity);
            await dbContext.SaveChangesAsync();

            // Assert
            entity.FechaCreacion.Should().Be(now);
        }

        [Fact]
        public async Task SavingChanges_DebeEstablecerFechaActualizacion_CuandoSeModificaEntidad()
        {
            // Arrange
            var initialTime = new DateTime(2024, 1, 1, 12, 0, 0);
            var updateTime = new DateTime(2024, 1, 1, 13, 0, 0);

            _dateTimeServiceMock.SetupSequence(s => s.Now)
                .Returns(initialTime)
                .Returns(updateTime);
            _currentUserServiceMock.Setup(s => s.UserId).Returns("test-user");

            var interceptor = new AuditableEntityInterceptor(_currentUserServiceMock.Object, _dateTimeServiceMock.Object, _loggerInterceptorMock.Object);
            var dbContext = CreateDbContext(interceptor);

            var entity = AuditableTestEntity.Crear("Test Entity");
            dbContext.TestEntities.Add(entity);
            await dbContext.SaveChangesAsync(); // Guardado inicial

            // Act
            entity.ActualizarNombre("Nuevo Nombre");
            await dbContext.SaveChangesAsync(); // Segundo guardado (modificación)

            // Assert
            entity.FechaActualizacion.Should().Be(updateTime);
        }
    }
} 