using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Comercial
{
    public class FacturaConfigurationTests : IntegrationTestBase
    {
        public FacturaConfigurationTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Configure_ShouldSetCorrectTableNameAndSchema()
        {
            var entityType = DbContext.Model.FindEntityType(typeof(Factura));

            entityType.Should().NotBeNull();
            entityType.GetTableName().Should().Be("Facturas");
            entityType.GetSchema().Should().Be("Comercial");
        }
    }
} 