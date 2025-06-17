using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Infrastructure.Persistence.Configurations.Inventario;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Inventario
{
    public class IngredienteConfigurationTests
    {
        [Fact]
        public void IngredienteConfiguration_DebeConfigurarLaEntidadCorrectamente()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var configuration = new IngredienteConfiguration();

            // Act
            configuration.Configure(modelBuilder.Entity<Ingrediente>());

            // Assert
            var entityType = modelBuilder.Model.FindEntityType(typeof(Ingrediente));
            entityType.Should().NotBeNull();

            // Verificar tabla y esquema
            entityType.GetTableName().Should().Be("Ingredientes");
            entityType.GetSchema().Should().Be("Inventario");

            // Verificar clave primaria
            entityType.FindPrimaryKey().Should().NotBeNull();
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Verificar propiedades
            entityType.FindProperty("Nombre").IsNullable.Should().BeFalse();
            entityType.FindProperty("Nombre").GetMaxLength().Should().Be(100);
            entityType.FindProperty("Codigo").IsNullable.Should().BeFalse();
            entityType.FindProperty("Codigo").GetMaxLength().Should().Be(20);
            entityType.FindProperty("Descripcion").GetMaxLength().Should().Be(500);
            entityType.FindProperty("UnidadMedida").IsNullable.Should().BeFalse();
            entityType.FindProperty("Stock").IsNullable.Should().BeFalse();
            entityType.FindProperty("StockMinimo").IsNullable.Should().BeFalse();
            
            // Verificar índices
            var codigoIndex = entityType.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == "Codigo"));
            codigoIndex.Should().NotBeNull();
            codigoIndex.IsUnique.Should().BeTrue();
            
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Nombre"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Stock"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "ProveedorPrincipalId"));

            // Verificar la configuración de la entidad poseída (Owned Entity)
            var ownedNavigation = entityType.FindNavigation("Movimientos");
            ownedNavigation.Should().NotBeNull();
            var ownedEntityType = ownedNavigation.TargetEntityType;
            ownedEntityType.Should().NotBeNull();
            
            ownedEntityType.GetTableName().Should().Be("MovimientosInventario");
            ownedEntityType.GetSchema().Should().Be("Inventario");
            ownedEntityType.FindProperty("Cantidad").GetPrecision().Should().Be(10);
            ownedEntityType.FindProperty("Cantidad").GetScale().Should().Be(2);
            ownedEntityType.FindProperty("TipoMovimiento").GetMaxLength().Should().Be(50);
        }
    }
} 