using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System.Linq;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Core
{
    public class ProductoConfigurationTests : IntegrationTestBase
    {
        public ProductoConfigurationTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Producto_Configuration_ShouldBeAppliedCorrectly()
        {
            // Arrange
            var entityType = DbContext.Model.FindEntityType(typeof(Producto));

            // Assert
            entityType.Should().NotBeNull();
            
            // Table and Schema
            entityType.GetTableName().Should().Be("Productos");
            entityType.GetSchema().Should().Be("Core");

            // Primary Key
            entityType.FindPrimaryKey().Properties.Should().ContainSingle(p => p.Name == "Id");
            entityType.FindProperty("Id").ValueGenerated.Should().Be(ValueGenerated.Never);

            // Properties
            var nombreProperty = entityType.FindProperty("Nombre");
            nombreProperty.IsNullable.Should().BeFalse();
            nombreProperty.GetMaxLength().Should().Be(100);

            var descripcionProperty = entityType.FindProperty("Descripcion");
            descripcionProperty.GetMaxLength().Should().Be(500);

            var precioNavigation = entityType.FindNavigation("Precio");
            precioNavigation.Should().NotBeNull();
            var precioOwnedType = precioNavigation.TargetEntityType;
            precioOwnedType.IsOwned().Should().BeTrue();
            var precioValorProperty = precioOwnedType.FindProperty("Valor");
            precioValorProperty.GetColumnName().Should().Be("Precio");
            precioValorProperty.GetPrecision().Should().Be(18);
            precioValorProperty.GetScale().Should().Be(2);

            var categoriaIdProperty = entityType.FindProperty("CategoriaId");
            categoriaIdProperty.IsNullable.Should().BeFalse();
            
            var categoriaNombreProperty = entityType.FindProperty("CategoriaNombre");
            categoriaNombreProperty.GetMaxLength().Should().Be(100);

            var popularidadProperty = entityType.FindProperty("Popularidad");
            popularidadProperty.GetDefaultValue().Should().Be(0);

            var estaActivoProperty = entityType.FindProperty("EstaActivo");
            estaActivoProperty.IsNullable.Should().BeFalse();
            estaActivoProperty.GetDefaultValue().Should().Be(true);

            // Relationships
            var recetasNavigation = entityType.GetNavigations()
                .SingleOrDefault(n => n.Name == "Recetas");
            recetasNavigation.Should().NotBeNull();
            recetasNavigation.IsCollection.Should().BeTrue();
            recetasNavigation.ForeignKey.PrincipalEntityType.ClrType.Should().Be(typeof(Producto));

            // Indexes
            var nombreIndex = entityType.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == "Nombre"));
            nombreIndex.Should().NotBeNull();

            var compositeIndex = entityType.GetIndexes().FirstOrDefault(i => i.Properties.Count == 2 && i.Properties.Any(p => p.Name == "CategoriaId") && i.Properties.Any(p => p.Name == "EstaActivo"));
            compositeIndex.Should().NotBeNull();
            
            // Query Filter
            entityType.GetQueryFilter().Should().NotBeNull();
            var queryFilter = entityType.GetQueryFilter();
            var productoEliminado = Producto.Crear("test", "test", new Domain.Core.Productos.ValueObjects.PrecioProducto(1), System.Guid.NewGuid());
            productoEliminado.MarkAsDeleted();
            var productoNoEliminado = Producto.Crear("test2", "test2", new Domain.Core.Productos.ValueObjects.PrecioProducto(1), System.Guid.NewGuid());
            
            ((System.Func<Producto, bool>)queryFilter.Compile()).Invoke(productoEliminado).Should().BeFalse();
            ((System.Func<Producto, bool>)queryFilter.Compile()).Invoke(productoNoEliminado).Should().BeTrue();
        }
    }
} 