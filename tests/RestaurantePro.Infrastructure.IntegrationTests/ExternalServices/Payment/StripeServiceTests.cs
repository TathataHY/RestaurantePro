using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.ExternalServices.Payment;
using System.Threading.Tasks;
using FluentAssertions;
using RestaurantePro.Application.Common.Interfaces;
using System.Collections.Generic;

namespace RestaurantePro.Infrastructure.IntegrationTests.ExternalServices.Payment
{
    public class StripeServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<StripeService>> _mockLogger;
        private readonly StripeService _stripeService;

        public StripeServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<StripeService>>();

            // Simular la configuración de Stripe
            var mockConfSection = new Mock<IConfigurationSection>();
            mockConfSection.Setup(s => s.Value).Returns("dummy_api_key");
            _mockConfiguration.Setup(c => c.GetSection("Payment:Stripe:ApiKey")).Returns(mockConfSection.Object);

            mockConfSection = new Mock<IConfigurationSection>();
            mockConfSection.Setup(s => s.Value).Returns("dummy_webhook_secret");
            _mockConfiguration.Setup(c => c.GetSection("Payment:Stripe:WebhookSecret")).Returns(mockConfSection.Object);

            _stripeService = new StripeService(_mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ProcesarPagoAsync_DebeRetornarResultadoExitoso_EnImplementacionSimulada()
        {
            // Arrange
            var monto = 250.75m;
            var moneda = "CLP";
            var descripcion = "Pago de prueba Stripe";

            // Act
            var result = await _stripeService.ProcesarPagoAsync(monto, moneda, descripcion);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TransaccionId.Should().NotBeNullOrEmpty();
            result.Value.TransaccionId.Should().StartWith("pi_");
            result.Value.Estado.Should().Be(PaymentStatus.Completado);
            result.Value.Monto.Should().Be(monto);
            result.Value.Moneda.Should().Be(moneda);
            result.Value.Mensaje.Should().Contain("simulado");
        }

        [Fact]
        public async Task ReembolsarPagoAsync_DebeRetornarResultadoExitoso_EnImplementacionSimulada()
        {
            // Arrange
            var transaccionId = "pi_123456789";
            var montoReembolso = 100.00m;
            var motivo = "Reembolso de prueba Stripe";

            // Act
            var result = await _stripeService.ReembolsarPagoAsync(transaccionId, montoReembolso, motivo);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.ReembolsoId.Should().NotBeNullOrEmpty();
            result.Value.ReembolsoId.Should().StartWith("re_");
            result.Value.Estado.Should().Be(RefundStatus.Completado);
            result.Value.TransaccionOriginalId.Should().Be(transaccionId);
            result.Value.MontoReembolsado.Should().Be(montoReembolso);
        }

        [Fact]
        public async Task VerificarEstadoPagoAsync_DebeRetornarUnEstadoValido_EnImplementacionSimulada()
        {
            // Arrange
            var transaccionId = "pi_987654321";

            // Act
            var result = await _stripeService.VerificarEstadoPagoAsync(transaccionId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeOneOf(PaymentStatus.Completado, PaymentStatus.Pendiente, PaymentStatus.Fallido);
        }
    }
} 