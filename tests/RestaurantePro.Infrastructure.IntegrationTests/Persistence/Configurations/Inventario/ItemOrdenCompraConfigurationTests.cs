using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Infrastructure.Persistence.Configurations.Inventario;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Inventario
{
    public class ItemOrdenCompraConfigurationTests
    {
        [Fact]
        public void ItemOrdenCompraConfiguration_DebeConfigurarLaEntidadCorrectamente()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var configuration = new ItemOrdenCompraConfiguration();

            // Act
            configuration.Configure(modelBuilder.Entity<ItemOrdenCompra>());

            // Assert
            var entityType = modelBuilder.Model.FindEntityType(typeof(ItemOrdenCompra));
            entityType.Should().NotBeNull();

            // Verificar tabla y esquema
            entityType!.GetTableName().Should().Be("ItemsOrdenCompra");
            entityType.GetSchema().Should().Be("Inventario");

            // Verificar clave primaria
            entityType.FindPrimaryKey().Should().NotBeNull();
            entityType.FindPrimaryKey()!.Properties.Should().Contain(p => p.Name == "Id");

            // Verificar propiedades requeridas
            entityType.FindProperty("OrdenCompraId")!.IsNullable.Should().BeFalse();
            entityType.FindProperty("IngredienteId")!.IsNullable.Should().BeFalse();
            entityType.FindProperty("NombreIngrediente")!.IsNullable.Should().BeFalse();
            entityType.FindProperty("Cantidad")!.IsNullable.Should().BeFalse();
            entityType.FindProperty("UnidadMedida")!.IsNullable.Should().BeFalse();
            entityType.FindProperty("PrecioUnitario")!.IsNullable.Should().BeFalse();
            entityType.FindProperty("Subtotal")!.IsNullable.Should().BeFalse();

            // Verificar longitudes máximas
            entityType.FindProperty("NombreIngrediente")!.GetMaxLength().Should().Be(100);
            entityType.FindProperty("UnidadMedida")!.GetMaxLength().Should().Be(50);
            
            // Verificar precisión y escala para decimales
            entityType.FindProperty("Cantidad")!.GetPrecision().Should().Be(10);
            entityType.FindProperty("Cantidad")!.GetScale().Should().Be(2);
            entityType.FindProperty("PrecioUnitario")!.GetPrecision().Should().Be(18);
            entityType.FindProperty("PrecioUnitario")!.GetScale().Should().Be(2);
            entityType.FindProperty("Subtotal")!.GetPrecision().Should().Be(18);
            entityType.FindProperty("Subtotal")!.GetScale().Should().Be(2);
            entityType.FindProperty("CantidadRecibida")!.GetPrecision().Should().Be(10);
            entityType.FindProperty("CantidadRecibida")!.GetScale().Should().Be(2);

            // Verificar índices
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "OrdenCompraId"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "IngredienteId"));
        }
    }
} 