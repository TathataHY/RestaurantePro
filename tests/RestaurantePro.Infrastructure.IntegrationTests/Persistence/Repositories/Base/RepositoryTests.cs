using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Base
{
    public class RepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private Repository<TestEntity> _repository = null!;
        private Repository<NonAuditableTestEntity> _nonAuditableRepository = null!;
        
        public RepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            var loggerMock = new Mock<ILogger<Repository<TestEntity>>>();
            _repository = new Repository<TestEntity>(DbContext, loggerMock.Object);

            var nonAuditableLoggerMock = new Mock<ILogger<Repository<NonAuditableTestEntity>>>();
            _nonAuditableRepository = new Repository<NonAuditableTestEntity>(DbContext, nonAuditableLoggerMock.Object);
        }

        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        [Fact]
        public async Task AgregarAsync_DebeAñadirEntidadAlDbContext()
        {
            // Arrange
            var entity = new TestEntity { Nombre = "Test 1" };

            // Act
            await _repository.AgregarAsync(entity);
            await DbContext.SaveChangesAsync();

            // Assert
            var result = await DbContext.Set<TestEntity>().FindAsync(entity.Id);
            result.Should().NotBeNull();
            result.Nombre.Should().Be("Test 1");
        }
        
        [Fact]
        public async Task EliminarAsync_DebeHacerBorradoLogico_ParaEntidadesAuditables()
        {
            // Arrange
            var entity = new TestEntity { Nombre = "Para Borrar" };
            DbContext.Set<TestEntity>().Add(entity);
            await DbContext.SaveChangesAsync();

            // Act
            await _repository.EliminarAsync(entity);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var result = await DbContext.Set<TestEntity>().FindAsync(entity.Id);
            result.Should().NotBeNull();
            result.EstaEliminado.Should().BeTrue();
        }

        [Fact]
        public async Task EliminarAsync_DebeHacerBorradoFisico_ParaEntidadesNoAuditables()
        {
            // Arrange
            var entity = new NonAuditableTestEntity { Valor = "Para Borrar Físicamente" };
            DbContext.Set<NonAuditableTestEntity>().Add(entity);
            await DbContext.SaveChangesAsync();

            // Act
            await _nonAuditableRepository.EliminarAsync(entity);
            await DbContext.SaveChangesAsync();

            // Assert
            var result = await DbContext.Set<NonAuditableTestEntity>().FindAsync(entity.Id);
            result.Should().BeNull();
        }
    }
} 