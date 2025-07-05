using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.ExternalServices.SMS;
using System.Threading.Tasks;
using FluentAssertions;
using System.Collections.Generic;
using RestaurantePro.Application.Common.Interfaces;
using System;
using System.Threading;

namespace RestaurantePro.Infrastructure.IntegrationTests.ExternalServices.SMS
{
    public class TwilioServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<TwilioService>> _mockLogger;
        private readonly Mock<IDelayProvider> _mockDelayProvider;
        private readonly TwilioService _twilioService;

        public TwilioServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<TwilioService>>();
            _mockDelayProvider = new Mock<IDelayProvider>();

            // Simular configuración de Twilio
            _mockConfiguration.SetupGet(x => x["Twilio:AccountSid"]).Returns("AC_test_sid");
            _mockConfiguration.SetupGet(x => x["Twilio:AuthToken"]).Returns("test_auth_token");
            _mockConfiguration.SetupGet(x => x["Twilio:FromNumber"]).Returns("+1234567890");

            // Configurar el delay provider para que no espere en las pruebas
            _mockDelayProvider.Setup(x => x.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.CompletedTask);

            _twilioService = new TwilioService(
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockDelayProvider.Object
            );
        }

        [Fact]
        public async Task SendSMSAsync_DebeRetornarTrue_EnImplementacionSimulada()
        {
            // Arrange
            var phoneNumber = "+1987654321";
            var message = "Mensaje de prueba";

            // Act
            var result = await _twilioService.SendSMSAsync(phoneNumber, message);

            // Assert
            result.Should().BeTrue();
            _mockDelayProvider.Verify(x => x.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetDeliveryStatusAsync_DebeRetornarUnEstadoValido_EnImplementacionSimulada()
        {
            // Arrange
            var messageId = "SM_test_id";
            var posiblesEstados = new[] { "delivered", "sent", "queued", "failed" };

            // Act
            var result = await _twilioService.GetDeliveryStatusAsync(messageId);

            // Assert
            result.Should().NotBeNullOrEmpty();
            posiblesEstados.Should().Contain(result);
        }

        [Theory]
        [InlineData("+1234567890", true)]
        [InlineData("1234567890", true)]
        [InlineData("123-456-7890", true)]
        [InlineData("(123) 456-7890", true)]
        [InlineData("12345", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValidPhoneNumber_DebeValidarNumerosCorrectamente(string phoneNumber, bool expected)
        {
            // Act
            var result = _twilioService.IsValidPhoneNumber(phoneNumber);

            // Assert
            result.Should().Be(expected);
        }
    }
} 