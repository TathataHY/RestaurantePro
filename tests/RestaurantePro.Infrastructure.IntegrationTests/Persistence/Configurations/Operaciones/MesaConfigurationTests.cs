using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Infrastructure.Persistence.Configurations.Operaciones;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Operaciones
{
    public class MesaConfigurationTests
    {
        [Fact]
        public void MesaConfiguration_DebeConfigurarLaEntidadCorrectamente()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var configuration = new MesaConfiguration();

            // Act
            configuration.Configure(modelBuilder.Entity<Mesa>());

            // Assert
            var entityType = modelBuilder.Model.FindEntityType(typeof(Mesa));
            entityType.Should().NotBeNull();

            // Verificar el nombre de la tabla y el esquema
            entityType.GetTableName().Should().Be("Mesas");
            entityType.GetSchema().Should().Be("Operaciones");

            // Verificar la clave primaria
            entityType.FindPrimaryKey().Should().NotBeNull();
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Verificar propiedades requeridas y longitudes máximas
            entityType.FindProperty("Numero").IsNullable.Should().BeFalse();
            entityType.FindProperty("Capacidad").IsNullable.Should().BeFalse();
            entityType.FindProperty("Ubicacion").GetMaxLength().Should().Be(50);
            entityType.FindProperty("Estado").IsNullable.Should().BeFalse();

            // Verificar índices
            var numeroIndex = entityType.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == "Numero"));
            numeroIndex.Should().NotBeNull();
            numeroIndex.IsUnique.Should().BeTrue();

            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Estado"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Ubicacion"));
        }
    }
} 