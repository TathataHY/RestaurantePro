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

            // Verificar la configuración de la relación con entidad raíz
            var navigation = entityType.FindNavigation("Movimientos");
            navigation.Should().NotBeNull();
            navigation.IsCollection.Should().BeTrue();
            
            // Verificar que la relación apunta a la entidad raíz MovimientoInventario
            var relatedEntityType = navigation.TargetEntityType;
            relatedEntityType.Should().NotBeNull();
            relatedEntityType.Name.Should().Be("RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario");
            
            // Verificar que la tabla de la entidad relacionada está configurada correctamente
            relatedEntityType.GetTableName().Should().Be("MovimientoInventario");
            // (No se valida el esquema porque puede ser null en pruebas unitarias)
        }
    }
} 