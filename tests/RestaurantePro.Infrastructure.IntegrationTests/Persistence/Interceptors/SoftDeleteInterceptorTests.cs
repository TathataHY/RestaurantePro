using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Interceptors
{
    // Usaremos la misma entidad y DbContext de prueba que en AuditableEntityInterceptorTests
    // para evitar duplicar código.

    public class SoftDeleteInterceptorTests
    {
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ILogger<SoftDeleteInterceptor>> _loggerInterceptorMock;
        private readonly Mock<ILogger<TestDbContext>> _loggerDbContextMock;

        public SoftDeleteInterceptorTests()
        {
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _loggerInterceptorMock = new Mock<ILogger<SoftDeleteInterceptor>>();
            _loggerDbContextMock = new Mock<ILogger<TestDbContext>>();
        }

        private TestDbContext CreateDbContext(SoftDeleteInterceptor interceptor)
        {
            var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;

            return new TestDbContext(options, _loggerDbContextMock.Object);
        }

        [Fact]
        public async Task SavingChanges_DebeAplicarBorradoLogico_EnLugarDeFisico()
        {
            // Arrange
            var now = new DateTime(2024, 1, 1, 12, 0, 0);
            var userId = "test-user-delete";
            _dateTimeServiceMock.Setup(s => s.Now).Returns(now);
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

            var interceptor = new SoftDeleteInterceptor(_dateTimeServiceMock.Object, _currentUserServiceMock.Object, _loggerInterceptorMock.Object);
            var dbContext = CreateDbContext(interceptor);
            
            var entity = AuditableTestEntity.Crear("Entidad para borrar");
            dbContext.TestEntities.Add(entity);
            await dbContext.SaveChangesAsync(); // Guardar para que exista en la BD

            // Act
            dbContext.TestEntities.Remove(entity);
            await dbContext.SaveChangesAsync();

            // Assert
            // 1. La entidad todavía existe en la base de datos
            var entityFromDb = await dbContext.TestEntities.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == entity.Id);
            entityFromDb.Should().NotBeNull();

            // 2. La entidad está marcada como inactiva
            entityFromDb.IsActive.Should().BeFalse();
        }
    }
} 