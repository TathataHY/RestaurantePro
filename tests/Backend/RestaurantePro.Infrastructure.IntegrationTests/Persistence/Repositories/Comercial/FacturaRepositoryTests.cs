using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using FluentAssertions;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Comercial
{
    public class FacturaRepositoryTests : IntegrationTestBase
    {
        public FacturaRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Repositorio_DebePoderResolver_IFacturaRepository()
        {
            // Arrange
            await InitializeAsync();
            
            // Act
            var repository = ServiceProvider.GetService<IFacturaRepository>();

            // Assert
            repository.Should().NotBeNull();
        }
    }
} 