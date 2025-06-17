using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.Repositories.Comercial
{
    public class TarjetaFidelizacionRepositoryTests : IntegrationTestBase, IAsyncLifetime
    {
        private ITarjetaFidelizacionRepository _repository = null!;
        private IUnitOfWork _unitOfWork = null!;
        private Guid _clienteId1;
        private string _codigoTarjeta1 = null!;

        public TarjetaFidelizacionRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = ServiceProvider.GetRequiredService<ITarjetaFidelizacionRepository>();
            _unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
            await SeedTarjetasAsync();
        }

        public override Task DisposeAsync() => Task.CompletedTask;

        private async Task SeedTarjetasAsync()
        {
            _clienteId1 = Guid.NewGuid();
            _codigoTarjeta1 = "TF-VALIDA-01";

            var tarjeta1 = TarjetaFidelizacion.Crear(_clienteId1, _codigoTarjeta1);
            tarjeta1.Activar();
            tarjeta1.AgregarPuntos(100, "Compra inicial");

            var clienteId2 = Guid.NewGuid();
            var tarjeta2 = TarjetaFidelizacion.Crear(clienteId2, "TF-CANCELADA-02");
            tarjeta2.Activar();
            tarjeta2.AgregarPuntos(50, "Bono");
            tarjeta2.Cancelar("Cancelada para prueba");
            
            var tarjeta3 = TarjetaFidelizacion.Crear(_clienteId1, "TF-PLATINO-03");
            tarjeta3.Activar();
            tarjeta3.AgregarPuntos(3000, "Bono Bienvenida Platino");

            await _repository.AgregarAsync(tarjeta1);
            await _repository.AgregarAsync(tarjeta2);
            await _repository.AgregarAsync(tarjeta3);
            await _unitOfWork.SaveChangesAsync();

            // DEBUG: Verificar que la tarjeta 3 se guardó como Platino
            _fixture.ClearTracker();
            var tarjetaPlatino = await _repository.ObtenerPorCodigoAsync("TF-PLATINO-03");
            tarjetaPlatino.Should().NotBeNull();
            tarjetaPlatino.NivelFidelizacion.Should().Be(NivelFidelizacion.Platino, "los 3000 puntos deberían haberla promovido a Platino");
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
        public async Task ObtenerPorNumeroAsync_DebeRetornarTarjeta_CuandoExiste()
        {
            // Act
            var tarjeta = await _repository.ObtenerPorNumeroAsync(_codigoTarjeta1);

            // Assert
            tarjeta.Should().NotBeNull();
            tarjeta!.Codigo.Should().Be(_codigoTarjeta1);
        }

        [Theory]
        [InlineData("TF-VALIDA-01", true)]
        [InlineData("TF-NO-EXISTE", false)]
        public async Task ExisteNumeroTarjetaAsync_DebeRetornarValorCorrecto(string codigo, bool expected)
        {
            // Act
            var existe = await _repository.ExisteNumeroTarjetaAsync(codigo);

            // Assert
            existe.Should().Be(expected);
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
            tarjeta.Estado.Should().Be(EstadoTarjeta.Activa);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_DebeIncluirHistorial()
        {
            // Arrange
            var tarjetaExistente = await _repository.ObtenerPorCodigoAsync(_codigoTarjeta1);

            // Act
            var tarjeta = await _repository.ObtenerPorIdAsync(tarjetaExistente.Id);

            // Assert
            tarjeta.Should().NotBeNull();
            tarjeta!.HistorialPuntos.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ObtenerPorEstadoAsync_DebeRetornarTarjetasCorrectas()
        {
            // Act
            var tarjetasCanceladas = await _repository.ObtenerPorEstadoAsync(EstadoTarjeta.Cancelada);

            // Assert
            tarjetasCanceladas.Should().ContainSingle();
            tarjetasCanceladas.First().Estado.Should().Be(EstadoTarjeta.Cancelada);
        }

        [Fact]
        public async Task ObtenerPorNivelAsync_DebeRetornarTarjetasCorrectas()
        {
            // Act
            var tarjetasNivelBasico = await _repository.ObtenerPorNivelAsync(NivelFidelizacion.Basico);

            // Assert
            tarjetasNivelBasico.Should().HaveCount(1); // Solo la tarjeta 1 es Basico
            tarjetasNivelBasico.First().Codigo.Should().Be(_codigoTarjeta1);
        }

        [Fact]
        public async Task ObtenerTarjetasPorNivelesAsync_DebeRetornarTarjetasCorrectas()
        {
            // Arrange
            var niveles = new[] { NivelFidelizacion.Basico, NivelFidelizacion.Platino };

            // Act
            var tarjetas = await _repository.ObtenerTarjetasPorNivelesAsync(niveles);

            // Assert
            tarjetas.Should().NotBeNull();
            tarjetas.Should().HaveCount(2);
            tarjetas.Should().Contain(t => t.NivelFidelizacion == NivelFidelizacion.Basico);
            tarjetas.Should().Contain(t => t.NivelFidelizacion == NivelFidelizacion.Platino);
        }

        [Fact]
        public async Task GenerarCodigoUnicoAsync_DebeSerUnico()
        {
            // Act
            var codigo1 = await _repository.GenerarCodigoUnicoAsync();
            var codigo2 = await _repository.GenerarCodigoUnicoAsync();

            // Assert
            codigo1.Should().NotBe(codigo2);
            codigo1.Should().StartWith("TF");
        }

        [Fact]
        public async Task AgregarAsync_DebeGuardarNuevaTarjeta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var codigo = await _repository.GenerarCodigoUnicoAsync();
            var nuevaTarjeta = TarjetaFidelizacion.Crear(clienteId, codigo);

            // Act
            await _repository.AgregarAsync(nuevaTarjeta);
            await _unitOfWork.SaveChangesAsync();

            // Assert
            var tarjetaGuardada = await _repository.ObtenerPorCodigoAsync(codigo);
            tarjetaGuardada.Should().NotBeNull();
            tarjetaGuardada.ClienteId.Should().Be(clienteId);
        }

        [Fact]
        public async Task ActualizarAsync_DebeModificarTarjeta()
        {
            // Arrange
            _fixture.ClearTracker(); 
            
            var tarjeta = await _repository.ObtenerPorCodigoAsync(_codigoTarjeta1);
            tarjeta.Should().NotBeNull();
            tarjeta.AgregarPuntos(50, "Test Update");
            
            // Act
            // El ChangeTracker de EF Core detecta los cambios en la entidad 'tarjeta'
            // y SaveChangesAsync los persiste. No es necesario llamar a ActualizarAsync.
            await _unitOfWork.SaveChangesAsync();

            // Assert
            _fixture.ClearTracker();
            var tarjetaActualizada = await _repository.ObtenerPorCodigoAsync(_codigoTarjeta1);
            tarjetaActualizada.Should().NotBeNull();
            tarjetaActualizada.PuntosAcumulados.Should().Be(150);
        }

        [Fact]
        public async Task EliminarAsync_DebeMarcarTarjetaComoEliminada()
        {
            // Arrange
            var tarjeta = await _repository.ObtenerPorCodigoAsync(_codigoTarjeta1);
            
            // Act
            await _repository.EliminarAsync(tarjeta.Id);
            await _unitOfWork.SaveChangesAsync();
            _fixture.ClearTracker();

            // Assert
            var tarjetaEliminada = await _repository.ObtenerPorIdAsync(tarjeta.Id);
            tarjetaEliminada.Should().BeNull(); // El filtro global de consulta debe excluirla
        }

        [Fact]
        public async Task ObtenerConPuntosProximosAExpirarAsync_DebeRetornarCorrectamente()
        {
            // Arrange
            var tarjetaActiva = await _repository.ObtenerPorCodigoAsync(_codigoTarjeta1);
            tarjetaActiva.ConfigurarFechaExpiracion(DateTime.UtcNow.AddDays(5));
            await _repository.ActualizarAsync(tarjetaActiva);
            await _unitOfWork.SaveChangesAsync();

            _fixture.ClearTracker(); // Limpiar tracker para asegurar que la consulta va a la "BD"

            // Act
            var tarjetasConExpiracion = await _repository.ObtenerConPuntosProximosAExpirarAsync(DateTime.UtcNow.AddDays(10));

            // Assert
            tarjetasConExpiracion.Should().NotBeNull();
            tarjetasConExpiracion.Should().ContainSingle();
            var tarjetaResultante = tarjetasConExpiracion.First();
            tarjetaResultante.Codigo.Should().Be(tarjetaActiva.Codigo);
        }
    }
} 