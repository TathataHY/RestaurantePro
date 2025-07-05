using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Operaciones
{
    public class ReservacionRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IReservacionRepository _repository = null!;
        private Guid _clienteId;
        private Guid _mesaId1;
        private Guid _mesaId2;
        private Guid _reservacionPendienteId;

        public ReservacionRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IReservacionRepository>();
            await SeedReservacionesAsync();
        }

        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        private async Task SeedReservacionesAsync()
        {
            // Crear Cliente
            var nombreCliente = ClienteNombre.Crear("Juan", "Perez");
            var cliente = Cliente.Crear(nombreCliente, "juan.perez@test.com", "123456789", new DateTime(1990, 5, 20));
            _clienteId = cliente.Id;
            
            // Crear Mesas
            var mesa1 = Mesa.Crear(1, 4, "Ventana");
            _mesaId1 = mesa1.Id;
            var mesa2 = Mesa.Crear(2, 2, "Centro");
            _mesaId2 = mesa2.Id;

            // Crear Reservaciones
            var reservacionPendiente = Reservacion.Crear(_mesaId1, _clienteId, DateTime.Now.AddDays(1), TimeSpan.FromHours(2), 4, "987654321", "reserva@test.com");
            _reservacionPendienteId = reservacionPendiente.Id;

            var reservacionConfirmada = Reservacion.Crear(_mesaId2, _clienteId, DateTime.Now.AddDays(2), TimeSpan.FromHours(2), 2, "987654321", "reserva@test.com");
            reservacionConfirmada.Confirmar();
            
            var reservacionCancelada = Reservacion.Crear(_mesaId1, _clienteId, DateTime.Now.AddDays(3), TimeSpan.FromHours(1), 3, "987654321", "reserva@test.com");
            reservacionCancelada.Cancelar("Cambio de planes");

            await DbContext.AddRangeAsync(cliente, mesa1, mesa2, reservacionPendiente, reservacionConfirmada, reservacionCancelada);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarReservacionCorrecta()
        {
            // Act
            var reservacion = await _repository.ObtenerPorIdAsync(_reservacionPendienteId);

            // Assert
            reservacion.Should().NotBeNull();
            reservacion!.Id.Should().Be(_reservacionPendienteId);
            reservacion.Estado.Should().Be(EstadoReservacion.Pendiente);
            reservacion.Mesa.Should().NotBeNull();
            reservacion.Cliente.Should().NotBeNull();
        }

        [Fact]
        public async Task ObtenerReservacionesPorFechaAsync_DebeRetornarReservacionesDelDia()
        {
            // Act
            var fechaBusqueda = DateTime.Now.AddDays(1).Date;
            var reservaciones = await _repository.ObtenerPorFechaAsync(fechaBusqueda);

            // Assert
            reservaciones.Should().NotBeNull();
            reservaciones.Should().HaveCount(1);
            reservaciones.First().Id.Should().Be(_reservacionPendienteId);
        }

        [Fact]
        public async Task ObtenerReservacionesPorClienteAsync_DebeRetornarTodasLasReservacionesDelCliente()
        {
            // Act
            var reservaciones = await _repository.ObtenerPorClienteAsync(_clienteId);

            // Assert
            reservaciones.Should().NotBeNull();
            reservaciones.Should().HaveCount(3);
        }

        [Fact]
        public async Task ObtenerReservacionesActivasAsync_DebeRetornarPendientesYConfirmadas()
        {
            // Act
            var reservaciones = await _repository.ObtenerTodasAsync();

            // Assert
            var reservacionesActivas = reservaciones.Where(r => r.Estado == EstadoReservacion.Pendiente || r.Estado == EstadoReservacion.Confirmada);

            reservacionesActivas.Should().NotBeNull();
            reservacionesActivas.Should().HaveCount(2);
            reservacionesActivas.Should().NotContain(r => r.Estado == EstadoReservacion.Cancelada);
        }
        
        [Fact]
        public async Task ObtenerDisponibilidadMesasAsync_DebeRetornarMesasSinReservacionParaEsaHora()
        {
            // Arrange
            var fechaBusqueda = DateTime.Now.AddDays(1);

            // Act
            var idsMesasDisponibles = await _repository.ObtenerMesasDisponiblesAsync(fechaBusqueda, fechaBusqueda.TimeOfDay, 2);

            // Assert
            idsMesasDisponibles.Should().NotBeNull();
            idsMesasDisponibles.Should().NotContain(_mesaId1); // La mesa 1 ya está reservada a esa hora
            idsMesasDisponibles.Should().Contain(_mesaId2); // La mesa 2 debería estar disponible
        }
    }
} 