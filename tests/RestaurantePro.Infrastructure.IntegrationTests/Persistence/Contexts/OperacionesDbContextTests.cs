using FluentAssertions;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Contexts
{
    public class OperacionesDbContextTests : IntegrationTestBase, IAsyncLifetime
    {
        private Guid _mesaId;
        private Guid _clienteId;

        public OperacionesDbContextTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public async Task InitializeAsync()
        {
            await base.InitializeAsync();
            
            var mesa = Mesa.Crear(100, 4, "Area de Pruebas");
            _mesaId = mesa.Id;

            var cliente = Cliente.Crear(ClienteNombre.Crear("Test", "User"), "test@user.com", "1122334455", new DateTime(2000, 1, 1));
            _clienteId = cliente.Id;

            await DbContext.AddRangeAsync(mesa, cliente);
            await DbContext.SaveChangesAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;


        [Fact]
        public async Task OperacionesDbContext_DebeGuardarReservacionCorrectamente()
        {
            // Arrange
            var reservacion = Reservacion.Crear(
                _mesaId,
                _clienteId,
                DateTime.Now.AddDays(1),
                TimeSpan.FromHours(2),
                4,
                "123456789",
                "test@test.com",
                "Mesa cerca de la ventana"
            );
            
            // Act
            DbContext.Reservaciones.Add(reservacion);
            await DbContext.SaveChangesAsync();
            
            // Assert
            var reservacionGuardada = await DbContext.Reservaciones.FindAsync(reservacion.Id);
            reservacionGuardada.Should().NotBeNull();
            reservacionGuardada.CantidadPersonas.Should().Be(4);
            reservacionGuardada.Email.Should().Be("test@test.com");
        }
    }
} 