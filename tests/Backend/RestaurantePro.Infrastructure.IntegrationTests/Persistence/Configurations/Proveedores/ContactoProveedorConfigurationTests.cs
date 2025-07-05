using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Infrastructure.Persistence.Configurations.Proveedores;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Proveedores
{
    public class ContactoProveedorConfigurationTests
    {
        [Fact]
        public void ContactoProveedorConfiguration_DebeConfigurarLaEntidadCorrectamente()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var configuration = new ContactoProveedorConfiguration();

            // Act
            configuration.Configure(modelBuilder.Entity<ContactoProveedor>());

            // Assert
            var entityType = modelBuilder.Model.FindEntityType(typeof(ContactoProveedor));
            entityType.Should().NotBeNull();

            // Tabla y esquema
            entityType.GetTableName().Should().Be("ContactosProveedores");
            entityType.GetSchema().Should().Be("Proveedores");

            // Clave primaria
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Propiedades
            entityType.FindProperty("Nombre").IsNullable.Should().BeFalse();
            entityType.FindProperty("Nombre").GetMaxLength().Should().Be(100);
            entityType.FindProperty("Cargo").IsNullable.Should().BeFalse();
            entityType.FindProperty("Cargo").GetMaxLength().Should().Be(100);

            // Value Objects
            entityType.FindNavigation("Email").Should().NotBeNull();
            entityType.FindNavigation("Telefono").Should().NotBeNull();

            // Índices
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Nombre"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "ProveedorId"));
        }
    }
} 