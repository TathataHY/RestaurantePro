using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Infrastructure.Persistence.Configurations.Operaciones;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Operaciones
{
    public class ReservacionConfigurationTests
    {
        [Fact]
        public void ReservacionConfiguration_DebeConfigurarLaEntidadCorrectamente()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var configuration = new ReservacionConfiguration();

            // Act
            configuration.Configure(modelBuilder.Entity<Reservacion>());

            // Assert
            var entityType = modelBuilder.Model.FindEntityType(typeof(Reservacion));
            entityType.Should().NotBeNull();

            // Verificar el nombre de la tabla y el esquema
            entityType.GetTableName().Should().Be("Reservaciones");
            entityType.GetSchema().Should().Be("Operaciones");

            // Verificar la clave primaria
            entityType.FindPrimaryKey().Should().NotBeNull();
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Verificar propiedades requeridas y longitudes máximas
            entityType.FindProperty("ClienteId").IsNullable.Should().BeFalse();
            entityType.FindProperty("MesaId").IsNullable.Should().BeFalse();
            entityType.FindProperty("Fecha").IsNullable.Should().BeFalse();
            entityType.FindProperty("Hora").IsNullable.Should().BeFalse();
            entityType.FindProperty("CantidadPersonas").IsNullable.Should().BeFalse();
            entityType.FindProperty("Telefono").GetMaxLength().Should().Be(20);
            entityType.FindProperty("Email").GetMaxLength().Should().Be(150);
            entityType.FindProperty("Observaciones").GetMaxLength().Should().Be(500);
            entityType.FindProperty("Observaciones").IsNullable.Should().BeTrue();
            entityType.FindProperty("MotivoCancelacion").GetMaxLength().Should().Be(200);
            entityType.FindProperty("MotivoCancelacion").IsNullable.Should().BeTrue();
            entityType.FindProperty("Estado").IsNullable.Should().BeFalse();

            // Verificar índices
            entityType.GetIndexes().Should().Contain(i => i.Properties.Select(p => p.Name).Contains("Fecha") && i.Properties.Select(p => p.Name).Contains("Hora"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Select(p => p.Name).Contains("Estado"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Select(p => p.Name).Contains("ClienteId"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Select(p => p.Name).Contains("MesaId"));

            // Verificar AutoInclude en navegaciones (esto es más difícil de probar directamente aquí, pero la configuración lo invoca)
            // La presencia de las navegaciones se puede verificar
            var mesaNavigation = entityType.FindNavigation("Mesa");
            mesaNavigation.Should().NotBeNull();
            mesaNavigation.IsCollection.Should().BeFalse();

            var clienteNavigation = entityType.FindNavigation("Cliente");
            clienteNavigation.Should().NotBeNull();
            clienteNavigation.IsCollection.Should().BeFalse();
        }
    }
} 