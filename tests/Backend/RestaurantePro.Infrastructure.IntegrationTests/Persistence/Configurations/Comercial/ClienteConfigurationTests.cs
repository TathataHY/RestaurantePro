using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Comercial
{
    public class ClienteConfigurationTests : IntegrationTestBase
    {
        public ClienteConfigurationTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Configure_ShouldSetCorrectTableNameAndSchema()
        {
            var entityType = DbContext.Model.FindEntityType(typeof(Cliente));

            entityType.Should().NotBeNull();
            entityType.GetTableName().Should().Be("Clientes");
            entityType.GetSchema().Should().Be("Comercial");
        }
    }
} 