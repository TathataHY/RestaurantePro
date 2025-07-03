using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Comandas.EventHandlers;
using RestaurantePro.Application.Operaciones.Comandas.Events;
using Xunit;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.EventHandlers.ComandaActualizada
{
    public class ComandaActualizadaEventHandlerTests
    {
        private readonly Mock<ISignalRService> _mockSignalRService;
        private readonly Mock<ILogger<ComandaActualizadaEventHandler>> _mockLogger;
        private readonly ComandaActualizadaEventHandler _handler;

        public ComandaActualizadaEventHandlerTests()
        {
            _mockSignalRService = new Mock<ISignalRService>();
            _mockLogger = new Mock<ILogger<ComandaActualizadaEventHandler>>();
            _handler = new ComandaActualizadaEventHandler(_mockSignalRService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_NotificaActualizacionComanda_Correctamente()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var estadoAnterior = EstadoComanda.Creada;
            var nuevoEstado = EstadoComanda.EnProceso;
            var notification = new ComandaActualizadaNotificationEvent(comandaId, estadoAnterior, nuevoEstado);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _mockSignalRService.Verify(x => x.NotificarActualizacionComandaAsync(
                comandaId, nuevoEstado.ToString(), null), Times.Once);
        }

        [Fact]
        public async Task Handle_LogueaInformacion_Correctamente()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var estadoAnterior = EstadoComanda.EnProceso;
            var nuevoEstado = EstadoComanda.Lista;
            var notification = new ComandaActualizadaNotificationEvent(comandaId, estadoAnterior, nuevoEstado);

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Procesando evento ComandaActualizadaNotificationEvent")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CuandoServicioFalla_LogueaErrorYLanzaExcepcion()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var estadoAnterior = EstadoComanda.Lista;
            var nuevoEstado = EstadoComanda.Cancelada;
            var notification = new ComandaActualizadaNotificationEvent(comandaId, estadoAnterior, nuevoEstado);
            _mockSignalRService.Setup(x => x.NotificarActualizacionComandaAsync(It.IsAny<Guid>(), It.IsAny<string>(), null))
                .ThrowsAsync(new Exception("Fallo de SignalR"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(notification, CancellationToken.None));
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al procesar evento ComandaActualizadaNotificationEvent")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
} 