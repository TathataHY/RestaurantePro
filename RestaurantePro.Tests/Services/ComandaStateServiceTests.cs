using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces.Repositories;
using RestaurantePro.Core.Interfaces.Services;
using RestaurantePro.Core.Services;

namespace RestaurantePro.Tests.Services
{
    public class ComandaStateServiceTests
    {
        private readonly Mock<IComandaRepository> _comandaRepository;
        private readonly Mock<IMesaRepository> _mesaRepository;
        private readonly Mock<IPlatoRepository> _platoRepository;
        private readonly Mock<INotificationService> _notificationService;
        private readonly Mock<ILogger<ComandaStateService>> _logger;
        private readonly ComandaStateService _sut;

        public ComandaStateServiceTests()
        {
            _comandaRepository = new Mock<IComandaRepository>();
            _mesaRepository = new Mock<IMesaRepository>();
            _platoRepository = new Mock<IPlatoRepository>();
            _notificationService = new Mock<INotificationService>();
            _logger = new Mock<ILogger<ComandaStateService>>();

            _sut = new ComandaStateService(
                _comandaRepository.Object,
                _mesaRepository.Object,
                _platoRepository.Object,
                _notificationService.Object,
                _logger.Object
            );
        }

        [Theory]
        [InlineData(EstadoComanda.Pendiente, EstadoComanda.EnPreparacion, true)]
        [InlineData(EstadoComanda.EnPreparacion, EstadoComanda.Lista, true)]
        [InlineData(EstadoComanda.Lista, EstadoComanda.Cancelada, true)]
        [InlineData(EstadoComanda.Pendiente, EstadoComanda.Cancelada, false)]
        public async Task UpdateComandaState_DebeValidarTransicionesDeEstado(
            EstadoComanda estadoInicial,
            EstadoComanda nuevoEstado,
            bool debePermitir)
        {
            // Arrange
            var comandaId = 1;
            var comanda = new Comanda { Id = comandaId, Estado = estadoInicial };

            _comandaRepository.Setup(x => x.GetByIdAsync(comandaId))
                .ReturnsAsync(comanda);

            // Act
            var result = await _sut.UpdateComandaStateAsync(comandaId, nuevoEstado);

            // Assert
            result.IsSuccess.Should().Be(debePermitir);
            if (debePermitir)
            {
                _notificationService.Verify(
                    x => x.NotifyComandaStatusChangedAsync(comandaId, nuevoEstado),
                    Times.Once);
            }
        }

        [Fact]
        public async Task UpdateComandaState_CuandoSeCancela_DebeActualizarMesaYCalcularTotal()
        {
            // Arrange
            var comandaId = 1;
            var mesaId = 2;
            var comanda = new Comanda
            {
                Id = comandaId,
                Estado = EstadoComanda.Lista,
                MesaId = mesaId,
                Detalles = new List<ComandaDetalle>
                {
                    new() { Subtotal = 100 },
                    new() { Subtotal = 150 }
                }
            };

            var mesa = new Mesa { Id = mesaId, Estado = EstadoMesa.Ocupada };

            _comandaRepository.Setup(x => x.GetByIdAsync(comandaId))
                .ReturnsAsync(comanda);
            _mesaRepository.Setup(x => x.GetByIdAsync(mesaId))
                .ReturnsAsync(mesa);

            // Act
            var result = await _sut.UpdateComandaStateAsync(comandaId, EstadoComanda.Cancelada);

            // Assert
            result.IsSuccess.Should().BeTrue();
            comanda.Total.Should().Be(250);
            mesa.Estado.Should().Be(EstadoMesa.Disponible);
            _mesaRepository.Verify(x => x.UpdateAsync(mesa), Times.Once);
        }

        [Fact]
        public async Task UpdateComandaState_CambioNoPermitido_DeberiaRetornarError()
        {
            // Arrange
            var comandaId = 1;
            var comanda = new Comanda { Id = comandaId, Estado = EstadoComanda.Pendiente };

            _comandaRepository.Setup(x => x.GetByIdAsync(comandaId))
                .ReturnsAsync(comanda);

            // Act
            var result = await _sut.UpdateComandaStateAsync(comandaId, EstadoComanda.Cancelada);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Transición de estado inválida: Pendiente -> Cancelada");
        }
    }
}