using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Infrastructure.Persistence.Configurations.Operaciones;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Operaciones
{
    public class PreparacionDiariaConfigurationTests
    {
        [Fact]
        public void PreparacionDiariaConfiguration_DebeConfigurarLaEntidadCorrectamente()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var configuration = new PreparacionDiariaConfiguration();

            // Act
            configuration.Configure(modelBuilder.Entity<PreparacionDiaria>());

            // Assert
            var entityType = modelBuilder.Model.FindEntityType(typeof(PreparacionDiaria));
            entityType.Should().NotBeNull();

            // Verificar el nombre de la tabla y el esquema
            entityType.GetTableName().Should().Be("PreparacionesDiarias");
            entityType.GetSchema().Should().Be("Operaciones");

            // Verificar la clave primaria
            entityType.FindPrimaryKey().Should().NotBeNull();
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Verificar propiedades
            entityType.FindProperty("FechaPreparacion").IsNullable.Should().BeFalse();
            entityType.FindProperty("Estado").IsNullable.Should().BeFalse();
            entityType.FindProperty("Observaciones").GetMaxLength().Should().Be(500);
            entityType.FindProperty("ChefId").IsNullable.Should().BeFalse();
            entityType.FindProperty("CantidadPreparada").IsNullable.Should().BeFalse();
            entityType.FindProperty("CantidadDisponible").IsNullable.Should().BeFalse();
            entityType.FindProperty("FechaVencimiento").IsNullable.Should().BeFalse();
            entityType.FindProperty("ProductoId").IsNullable.Should().BeFalse();

            // Verificar índices
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "FechaPreparacion"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Estado"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "ChefId"));
        }
    }
} 