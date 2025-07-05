using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System.Linq;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Configurations.Core
{
    public class RecetaConfigurationTests : IntegrationTestBase
    {
        public RecetaConfigurationTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void Receta_Configuration_ShouldBeAppliedCorrectly()
        {
            // Arrange
            var entityType = DbContext.Model.FindEntityType(typeof(Receta));

            // Assert
            entityType.Should().NotBeNull();

            // Table and Schema
            entityType.GetTableName().Should().Be("Recetas");
            entityType.GetSchema().Should().Be("Core");

            // Primary Key
            entityType.FindPrimaryKey().Properties.Should().ContainSingle(p => p.Name == "Id");
            entityType.FindProperty("Id").ValueGenerated.Should().Be(ValueGenerated.Never);

            // Properties
            entityType.FindProperty("ProductoId").IsNullable.Should().BeFalse();
            entityType.FindProperty("Preparacion").IsNullable.Should().BeFalse();
            entityType.FindProperty("Preparacion").GetMaxLength().Should().Be(2000);
            entityType.FindProperty("TiempoPreparacionMinutos").IsNullable.Should().BeFalse();
            
            // Owned Collection
            var ingredientesNavigation = entityType.FindNavigation("Ingredientes");
            ingredientesNavigation.Should().NotBeNull();
            var ownedEntityType = ingredientesNavigation.TargetEntityType;
            ownedEntityType.IsOwned().Should().BeTrue();
            ownedEntityType.GetTableName().Should().Be("IngredientesRecetas");
            ownedEntityType.GetSchema().Should().Be("Core");
            ownedEntityType.FindPrimaryKey().Properties.Select(p => p.Name).Should().BeEquivalentTo(new[] { "RecetaId", "IngredienteId" });
            ownedEntityType.FindProperty("Nombre").GetMaxLength().Should().Be(100);
            
            if (DbContext.Database.IsRelational())
            {
                ownedEntityType.FindProperty("Cantidad").GetColumnType().Should().Be("decimal(18,2)");
            }
            
            ownedEntityType.FindProperty("UnidadMedida").GetMaxLength().Should().Be(50);
            
            // Indexes
            var productoIdIndex = entityType.GetIndexes().SingleOrDefault(i => i.Properties.Any(p => p.Name == "ProductoId"));
            productoIdIndex.Should().NotBeNull();
            productoIdIndex.IsUnique.Should().BeFalse();

            // Query Filter
            entityType.GetQueryFilter().Should().NotBeNull();
        }
    }
} 