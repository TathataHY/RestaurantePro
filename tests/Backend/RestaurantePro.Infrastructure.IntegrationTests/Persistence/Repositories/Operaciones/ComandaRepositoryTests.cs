using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Operaciones
{
    public class ComandaRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IComandaRepository _repository = null!;
        private IDateTimeService _dateTimeService = null!;
        private Guid _mesaId1;
        private Guid _meseroId1;
        private Guid _clienteId1;
        private Guid _productoId1;
        private Guid _comandaId1;

        public ComandaRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IComandaRepository>();
            _dateTimeService = ServiceProvider.GetRequiredService<IDateTimeService>();
            await SeedComandasAsync();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedComandasAsync()
        {
            // Dependencias
            var mesa = Mesa.Crear(1, 4, "Terraza");
            _mesaId1 = mesa.Id;

            var mesero = Usuario.Crear("mesero1", "Carlos Ruiz", "mesero1@test.com", RolUsuario.Mesero);
            _meseroId1 = mesero.Id;
            
            var cliente = Cliente.Crear(ClienteNombre.Crear("Ana", "Gomez"), "ana.gomez@test.com", "5551112233", new DateTime(1995, 1, 1));
            _clienteId1 = cliente.Id;

            var producto = Producto.Crear("Empanada", "Empanada de prueba", new PrecioProducto(25.0m), Guid.NewGuid(), "Empanadas");
            _productoId1 = producto.Id;

            await DbContext.AddRangeAsync(mesa, mesero, cliente, producto);
            await DbContext.SaveChangesAsync();

            var fechaCreacionPasada = _dateTimeService.UtcNow.AddDays(-1);

            // Comanda 1: Abierta
            var comanda1 = Comanda.Crear(_meseroId1, fechaCreacionPasada, _clienteId1, _mesaId1, numeroComanda: "C00001");
            comanda1.AgregarProducto(_productoId1, 2, 50, "Sin cebolla", fechaCreacionPasada);
            comanda1.ActualizarEstado(EstadoComanda.EnProceso, fechaCreacionPasada);
            _comandaId1 = comanda1.Id;

            // Comanda 2: Finalizada
            var comanda2 = Comanda.Crear(_meseroId1, fechaCreacionPasada, _clienteId1, _mesaId1, numeroComanda: "C00002");
            comanda2.AgregarProducto(_productoId1, 1, 100, fechaActualizacion: fechaCreacionPasada);
            comanda2.ActualizarEstado(EstadoComanda.Finalizada, fechaCreacionPasada);

            await _repository.AgregarAsync(comanda1);
            await _repository.AgregarAsync(comanda2);
            await _repository.GuardarCambiosAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeIncluirItems()
        {
            // Act
            var comanda = await _repository.ObtenerPorIdAsync(_comandaId1);

            // Assert
            comanda.Should().NotBeNull();
            comanda!.Id.Should().Be(_comandaId1);
            comanda.Items.Should().NotBeEmpty();
            comanda.Items.Should().HaveCount(1);
        }

        [Theory]
        [InlineData(EstadoComanda.EnProceso, 1)]
        [InlineData(EstadoComanda.Finalizada, 1)]
        [InlineData(EstadoComanda.Cancelada, 0)]
        [InlineData(EstadoComanda.Lista, 0)]
        public async Task ObtenerPorEstadoAsync_DebeRetornarComandasCorrectas(EstadoComanda estado, int cantidadEsperada)
        {
            // Act
            var comandas = await _repository.ObtenerComandasPorEstadoAsync(estado);

            // Assert
            comandas.Should().NotBeNull();
            comandas.Should().HaveCount(cantidadEsperada);
        }

        [Fact]
        public async Task ObtenerPorMesaAsync_DebeRetornarComandasDeLaMesa()
        {
            // Act
            var comandas = await _repository.ObtenerComandasPorMesaAsync(_mesaId1);

            // Assert
            comandas.Should().NotBeNull();
            comandas.Should().HaveCount(2); // Comanda 1 y 2 están en la misma mesa
            comandas.Should().OnlyContain(c => c.MesaId == _mesaId1);
        }
        
        [Fact]
        public async Task ObtenerComandasActivasAsync_DebeRetornarSoloActivas()
        {
            // Act
            var comandasActivas = await _repository.ObtenerComandasActivasAsync();

            // Assert
            comandasActivas.Should().NotBeNull();
            comandasActivas.Should().HaveCount(1);
            comandasActivas.First().NumeroComanda.Should().Be("C00001");
        }
        
        [Fact]
        public async Task ObtenerEstadisticasPorPeriodoAsync_DebeRetornarConteosCorrectos()
        {
            // Arrange
            var fechaInicio = _dateTimeService.UtcNow.AddDays(-2);
            var fechaFin = _dateTimeService.UtcNow.AddDays(1);

            // Act
            var estadisticas = await _repository.ObtenerEstadisticasPorPeriodoAsync(fechaInicio, fechaFin);

            // Assert
            estadisticas.Should().NotBeNull();
            estadisticas.Should().HaveCount(1);
            estadisticas.Keys.First().Date.Should().Be(_dateTimeService.UtcNow.AddDays(-1).Date);
        }
    }
} 