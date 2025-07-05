using FluentAssertions;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Contexts
{
    public class ComercialDbContextTests : IntegrationTestBase
    {
        public ComercialDbContextTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task ComercialDbContext_DebeGuardarClienteCorrectamente()
        {
            // Arrange
            var cliente = Cliente.Crear(
                Guid.NewGuid(),
                ClienteNombre.Crear("Juan", "Perez"),
                Email.Create("juan.perez@test.com"),
                PhoneNumber.Create("1234567890"),
                new DateTime(1990, 1, 1));

            // Act
            DbContext.Clientes.Add(cliente);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var clienteGuardado = await DbContext.Clientes.FindAsync(cliente.Id);
            clienteGuardado.Should().NotBeNull();
            clienteGuardado.Nombre.Nombre.Should().Be("Juan");
            clienteGuardado.Email.ToString().Should().Be("juan.perez@test.com");
        }
    }
} 