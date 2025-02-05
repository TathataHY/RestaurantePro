using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using RestaurantePro.Infrastructure.Services;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Interfaces.Hubs;

namespace RestaurantePro.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<IComandaHub> _mockComandaHub;
        private readonly Mock<ILogger<SignalRNotificationService>> _mockLogger;
        private readonly SignalRNotificationService _sut;

        public NotificationServiceTests()
        {
            _mockComandaHub = new Mock<IComandaHub>();
            _mockLogger = new Mock<ILogger<SignalRNotificationService>>();
            _sut = new SignalRNotificationService(_mockComandaHub.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task NotifyComandaStatusChanged_DebeEnviarNotificacion()
        {
            // Arrange
            var comandaId = 1;
            var newStatus = EstadoComanda.EnPreparacion;

            // Act
            await _sut.NotifyComandaStatusChangedAsync(comandaId, newStatus);

            // Assert
            _mockComandaHub.Verify(
                x => x.NotifyComandaStatusChanged(comandaId, newStatus),
                Times.Once);
        }

        [Fact]
        public async Task NotifyRoleAsync_DeberiaEnviarNotificacionAlGrupoCorrecto()
        {
            // Arrange
            var role = "Cocinero";
            var message = "Nueva comanda";

            // Act
            await _sut.NotifyRoleAsync(role, message);

            // Assert
            _mockComandaHub.Verify(
                x => x.NotifyRole(role, message),
                Times.Once);
        }
    }
} 