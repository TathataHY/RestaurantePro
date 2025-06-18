using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Comercial
{
    public class TarjetaFidelizacionConfigurationTests : IntegrationTestBase
    {
        public TarjetaFidelizacionConfigurationTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Configure_ShouldSetCorrectTableNameAndSchema()
        {
            var entityType = DbContext.Model.FindEntityType(typeof(TarjetaFidelizacion));

            entityType.Should().NotBeNull();
            entityType.GetTableName().Should().Be("TarjetasFidelizacion");
            entityType.GetSchema().Should().Be("Comercial");
        }
    }
} 