using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Comercial
{
    public class TarjetaFidelizacionRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private ITarjetaFidelizacionRepository _repository;
        private Guid _clienteId1;
        private string _codigoTarjeta1;

        public TarjetaFidelizacionRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public async Task InitializeAsync()
        {
            _repository = ServiceProvider.GetRequiredService<ITarjetaFidelizacionRepository>();
            await ResetDatabaseAsync();
            await SeedTarjetasAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedTarjetasAsync()
        {
            _clienteId1 = Guid.NewGuid();
            _codigoTarjeta1 = await _repository.GenerarCodigoUnicoAsync();

            var tarjeta1 = TarjetaFidelizacion.Crear(_clienteId1, _codigoTarjeta1);
            tarjeta1.Activar();
            tarjeta1.AgregarPuntos(100, "Compra inicial");

            var tarjeta2 = TarjetaFidelizacion.Crear(_clienteId1, await _repository.GenerarCodigoUnicoAsync());
            tarjeta2.Activar();
            tarjeta2.AgregarPuntos(50, "Bono");
            tarjeta2.Cancelar("Cancelada para prueba");

            await _repository.AgregarAsync(tarjeta1);
            await _repository.AgregarAsync(tarjeta2);
        }

        [Fact]
        public async Task ObtenerPorCodigoAsync_DebeRetornarTarjeta_CuandoExiste()
        {
            // Act
            var tarjeta = await _repository.ObtenerPorCodigoAsync(_codigoTarjeta1);

            // Assert
            tarjeta.Should().NotBeNull();
            tarjeta.Codigo.Should().Be(_codigoTarjeta1);
            tarjeta.PuntosAcumulados.Should().Be(100);
        }

        [Fact]
        public async Task ObtenerPorClienteIdAsync_DebeRetornarTodasLasTarjetasDelCliente()
        {
            // Act
            var tarjetas = await _repository.ObtenerPorClienteIdAsync(_clienteId1);

            // Assert
            tarjetas.Should().NotBeNull();
            tarjetas.Should().HaveCount(2);
        }

        [Fact]
        public async Task ObtenerTarjetaActivaPorClienteIdAsync_DebeRetornarSoloLaActiva()
        {
            // Act
            var tarjeta = await _repository.ObtenerTarjetaActivaPorClienteIdAsync(_clienteId1);

            // Assert
            tarjeta.Should().NotBeNull();
            tarjeta.Codigo.Should().Be(_codigoTarjeta1);
            tarjeta.Estado.Should().Be(EstadoTarjeta.Activa);
        }
    }
} 