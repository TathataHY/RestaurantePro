using FluentAssertions;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Contexts
{
    public class ProveedoresDbContextTests : IntegrationTestBase
    {
        [Fact]
        public async Task ProveedoresDbContext_DebeGuardarProveedorCorrectamente()
        {
            // Arrange
            var proveedor = Proveedor.Crear(
                "Proveedor de Prueba",
                "Contacto Principal",
                "contacto@proveedor.com",
                "1234567890",
                "Calle Falsa 123",
                "Ciudad Prueba",
                "12345",
                "País Prueba",
                "PROV123456789",
                "Cuenta Bancaria 123",
                30
            );

            // Act
            DbContext.Proveedores.Add(proveedor);
            await DbContext.SaveChangesAsync();

            // Assert
            var proveedorGuardado = await DbContext.Proveedores.FindAsync(proveedor.Id);
            proveedorGuardado.Should().NotBeNull();
            proveedorGuardado.Nombre.Should().Be("Proveedor de Prueba");
        }
    }
} 