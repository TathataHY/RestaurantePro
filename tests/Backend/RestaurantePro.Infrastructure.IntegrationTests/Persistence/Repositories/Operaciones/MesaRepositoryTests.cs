using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Operaciones
{
    public class MesaRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private IMesaRepository _repository = null!;
        private Guid _mesaDisponibleId;
        private Guid _mesaOcupadaId;

        public MesaRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<IMesaRepository>();
            await SeedMesasAsync();
        }

        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        private async Task SeedMesasAsync()
        {
            var mesa1 = Mesa.Crear(1, 4, "Terraza");
            _mesaDisponibleId = mesa1.Id;

            var mesa2 = Mesa.Crear(2, 2, "Interior");
            mesa2.MarcarComoOcupada();
            _mesaOcupadaId = mesa2.Id;

            var mesa3 = Mesa.Crear(3, 6, "Terraza");
            
            var mesa4 = Mesa.Crear(4, 4, "Barra");
            mesa4.MarcarComoFueraDeServicio("Pata coja");

            await DbContext.AddRangeAsync(mesa1, mesa2, mesa3, mesa4);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeRetornarMesaCorrecta()
        {
            // Act
            var mesa = await _repository.ObtenerPorIdAsync(_mesaDisponibleId);

            // Assert
            mesa.Should().NotBeNull();
            mesa.Id.Should().Be(_mesaDisponibleId);
            mesa.Numero.Should().Be(1);
        }

        [Fact]
        public async Task ObtenerMesasDisponiblesAsync_DebeRetornarSoloDisponibles()
        {
            // Act
            var mesas = await _repository.ObtenerMesasDisponiblesAsync();

            // Assert
            mesas.Should().NotBeNull();
            mesas.Should().HaveCount(2); // Mesa 1 y 3
            mesas.Should().OnlyContain(m => m.Estado == EstadoMesa.Disponible);
        }

        [Fact]
        public async Task ObtenerTotalComensalesActualesAsync_DebeRetornarSumaCapacidades()
        {
            // Act
            var totalComensales = await _repository.ObtenerTotalComensalesActualesAsync();

            // Assert
            totalComensales.Should().Be(2); // Solo la mesa 2 (capacidad 2) está ocupada
        }

        [Fact]
        public async Task BuscarMejorMesaAsync_DebeEncontrarLaMesaOptima()
        {
            // Act
            var mesaEncontrada = await _repository.BuscarMejorMesaAsync(3);

            // Assert
            mesaEncontrada.Should().NotBeNull();
            mesaEncontrada!.Numero.Should().Be(1); // La mesa 1 tiene capacidad 4, es la más ajustada
        }

        [Fact]
        public async Task ActualizarAsync_DebeCambiarEstadoDeMesa()
        {
            // Arrange
            var mesa = await _repository.ObtenerPorIdAsync(_mesaDisponibleId);
            mesa.MarcarComoReservada();

            // Act
            await _repository.ActualizarAsync(mesa);
            await _repository.GuardarCambiosAsync();

            var mesaActualizada = await _repository.ObtenerPorIdAsync(_mesaDisponibleId);

            // Assert
            mesaActualizada.Should().NotBeNull();
            mesaActualizada.Estado.Should().Be(EstadoMesa.Reservada);
        }
    }
} 