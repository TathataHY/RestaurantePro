using FluentAssertions;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Contexts
{
    public class InventarioDbContextTests : IntegrationTestBase
    {
        [Fact]
        public async Task InventarioDbContext_DebeGuardarIngredienteCorrectamente()
        {
            // Arrange
            var ingrediente = Ingrediente.Crear(
                "Tomate",
                "TOM-001",
                "Tomate fresco para ensaladas",
                UnidadMedida.Kilogramo,
                5, // Stock Mínimo
                10 // Stock Actual
            );

            // Act
            DbContext.Ingredientes.Add(ingrediente);
            await DbContext.SaveChangesAsync();

            // Assert
            var ingredienteGuardado = await DbContext.Ingredientes.FindAsync(ingrediente.Id);
            ingredienteGuardado.Should().NotBeNull();
            ingredienteGuardado.Nombre.Should().Be("Tomate");
            ingredienteGuardado.Stock.Should().Be(10);
        }
    }
}
