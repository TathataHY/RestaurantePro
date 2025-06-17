using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Configurations.Operaciones;
using Xunit;
using System.Linq;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Operaciones
{
    public class ComandaConfigurationTests : IntegrationTestBase
    {
        public ComandaConfigurationTests()
        {
            // El constructor de IntegrationTestBase ya se encarga de inicializar el DbContext
        }

        [Fact]
        public void ComandaConfiguration_DebeConfigurarLaEntidadCorrectamente()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            var configuration = new ComandaConfiguration();

            // Act
            configuration.Configure(modelBuilder.Entity<Comanda>());
            var entityType = modelBuilder.Model.FindEntityType(typeof(Comanda));

            // Assert
            // Verificar nombre de la tabla y esquema
            entityType.Should().NotBeNull();
            entityType.GetTableName().Should().Be("Comandas");
            entityType.GetSchema().Should().Be("Operaciones");

            // Verificar clave primaria
            entityType.FindPrimaryKey().Properties.Should().Contain(p => p.Name == "Id");

            // Verificar propiedades requeridas y longitudes máximas
            entityType.FindProperty("NumeroComanda").IsNullable.Should().BeFalse();
            entityType.FindProperty("NumeroComanda").GetMaxLength().Should().Be(50);
            
            entityType.FindProperty("FechaCreacion").IsNullable.Should().BeFalse();
            
            entityType.FindProperty("Estado").IsNullable.Should().BeFalse();
            
            entityType.FindProperty("MesaId").IsNullable.Should().BeFalse();
            
            entityType.FindProperty("MeseroId").IsNullable.Should().BeFalse();

            entityType.FindProperty("Observaciones").GetMaxLength().Should().Be(500);

            entityType.FindProperty("EstaEliminado").IsNullable.Should().BeFalse();

            // Verificar precisión de propiedades decimales
            entityType.FindProperty("DescuentoFidelizacion").GetPrecision().Should().Be(18);
            entityType.FindProperty("DescuentoFidelizacion").GetScale().Should().Be(2);

            // Verificar owned types (Value Objects)
            var totalNavigation = entityType.FindNavigation("Total");
            totalNavigation.Should().NotBeNull();
            totalNavigation.IsCollection.Should().BeFalse();
            var totalOwnedType = totalNavigation.TargetEntityType;
            totalOwnedType.FindProperty("Subtotal").GetPrecision().Should().Be(18);
            totalOwnedType.FindProperty("Subtotal").GetScale().Should().Be(2);
            totalOwnedType.FindProperty("Impuestos").GetPrecision().Should().Be(18);
            totalOwnedType.FindProperty("Impuestos").GetScale().Should().Be(2);
            totalOwnedType.FindProperty("Descuento").GetPrecision().Should().Be(18);
            totalOwnedType.FindProperty("Descuento").GetScale().Should().Be(2);
            totalOwnedType.FindProperty("Total").GetPrecision().Should().Be(18);
            totalOwnedType.FindProperty("Total").GetScale().Should().Be(2);

            // Verificar índices
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "NumeroComanda") && i.IsUnique);
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "MesaId"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "ClienteId"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "MeseroId"));
            entityType.GetIndexes().Should().Contain(i => i.Properties.Any(p => p.Name == "Estado"));

            // Verificar relaciones
            var itemsNavigation = entityType.FindNavigation("Items");
            itemsNavigation.Should().NotBeNull();
            itemsNavigation.IsCollection.Should().BeTrue();
            itemsNavigation.TargetEntityType.GetForeignKeys().Should().Contain(fk => fk.Properties.Any(p => p.Name == "ComandaId") && fk.DeleteBehavior == DeleteBehavior.Cascade);
        }
    }
} 