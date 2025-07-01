using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Infrastructure.Persistence.Configurations.Inventario;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Inventario
{
    public class OrdenCompraConfigurationTests
    {
        [Fact]
        public void OrdenCompraConfiguration_DebeConfigurarLaEntidadCorrectamente()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var configuration = new OrdenCompraConfiguration();

            // Act
            configuration.Configure(modelBuilder.Entity<OrdenCompra>());

            // Assert
            var entityType = modelBuilder.Model.FindEntityType(typeof(OrdenCompra));
            entityType.Should().NotBeNull();

            // Verificar tabla y esquema
            entityType.GetTableName().Should().Be("OrdenesCompra");
            entityType.GetSchema().Should().Be("Inventario");

            // Verificar clave primaria
            entityType.FindPrimaryKey().Should().NotBeNull();
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Verificar propiedades básicas
            entityType.FindProperty("FechaEmision").IsNullable.Should().BeFalse();
            entityType.FindProperty("Total").GetPrecision().Should().Be(18);
            entityType.FindProperty("Total").GetScale().Should().Be(2);
            
            // Verificar que Estado está configurado como requerido
            var estadoProperty = entityType.FindProperty("Estado");
            estadoProperty.Should().NotBeNull();
            estadoProperty.IsNullable.Should().BeFalse();
            
            entityType.FindProperty("Observaciones").GetMaxLength().Should().Be(500);

            // Verificar índices
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "FechaEmision"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Estado"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "ProveedorId"));

            // Verificar relación con Items
            var navigation = entityType.FindNavigation("Items");
            navigation.Should().NotBeNull();
            navigation.IsCollection.Should().BeTrue();
            navigation.TargetEntityType.GetForeignKeys().Should().Contain(fk => fk.Properties.Any(p => p.Name == "OrdenCompraId"));
        }
    }
} 