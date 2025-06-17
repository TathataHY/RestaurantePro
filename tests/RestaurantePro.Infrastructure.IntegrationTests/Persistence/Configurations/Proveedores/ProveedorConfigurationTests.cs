using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Infrastructure.Persistence.Configurations.Proveedores;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Proveedores
{
    public class ProveedorConfigurationTests
    {
        [Fact]
        public void ProveedorConfiguration_DebeConfigurarLaEntidadCorrectamente()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var configuration = new ProveedorConfiguration();

            // Act
            configuration.Configure(modelBuilder.Entity<Proveedor>());

            // Assert
            var entityType = modelBuilder.Model.FindEntityType(typeof(Proveedor));
            entityType.Should().NotBeNull();

            // Tabla y esquema
            entityType.GetTableName().Should().Be("Proveedores");
            entityType.GetSchema().Should().Be("Proveedores");

            // Clave primaria
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Propiedades
            entityType.FindProperty("Nombre").IsNullable.Should().BeFalse();
            entityType.FindProperty("Nombre").GetMaxLength().Should().Be(100);
            entityType.FindProperty("RFC").IsNullable.Should().BeFalse();
            entityType.FindProperty("RFC").GetMaxLength().Should().Be(20);

            // Value Objects
            entityType.FindNavigation("Email").Should().NotBeNull();
            entityType.FindNavigation("Telefono").Should().NotBeNull();
            
            // Relaciones
            var contactosNavigation = entityType.FindNavigation("Contactos");
            contactosNavigation.Should().NotBeNull();
            contactosNavigation.IsCollection.Should().BeTrue();
            contactosNavigation.TargetEntityType.GetForeignKeys()
                .Should().Contain(fk => fk.Properties.Any(p => p.Name == "ProveedorId"));

            var categoriasNavigation = entityType.FindNavigation("Categorias");
            categoriasNavigation.Should().NotBeNull();
            categoriasNavigation.IsCollection.Should().BeTrue();
            categoriasNavigation.TargetEntityType.GetTableName().Should().Be("ProveedorCategorias");
            
            // Índices
            var rfcIndex = entityType.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == "RFC"));
            rfcIndex.Should().NotBeNull();
            rfcIndex.IsUnique.Should().BeTrue();
        }
    }
} 