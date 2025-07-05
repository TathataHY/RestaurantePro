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
    public class PayPalServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<PayPalService>> _mockLogger;
        private readonly PayPalService _payPalService;

        public PayPalServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<PayPalService>>();

            // Simular la configuración de PayPal
            var mockConfSection = new Mock<IConfigurationSection>();
            mockConfSection.Setup(s => s.Value).Returns("dummy_client_id");
            _mockConfiguration.Setup(c => c.GetSection("Payment:PayPal:ClientId")).Returns(mockConfSection.Object);

            mockConfSection = new Mock<IConfigurationSection>();
            mockConfSection.Setup(s => s.Value).Returns("dummy_client_secret");
            _mockConfiguration.Setup(c => c.GetSection("Payment:PayPal:ClientSecret")).Returns(mockConfSection.Object);

            _payPalService = new PayPalService(_mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ProcesarPagoAsync_DebeRetornarResultadoExitoso_EnImplementacionSimulada()
        {
            // Arrange
            var monto = 100.50m;
            var moneda = "USD";
            var descripcion = "Pago de prueba";

            // Act
            var result = await _payPalService.ProcesarPagoAsync(monto, moneda, descripcion);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TransaccionId.Should().NotBeNullOrEmpty();
            result.Value.TransaccionId.Should().StartWith("PP-");
            result.Value.Estado.Should().Be(PaymentStatus.Completado);
            result.Value.Monto.Should().Be(monto);
            result.Value.Moneda.Should().Be(moneda);
            result.Value.Mensaje.Should().Contain("simulado");
        }

        [Fact]
        public async Task ReembolsarPagoAsync_DebeRetornarResultadoExitoso_EnImplementacionSimulada()
        {
            // Arrange
            var transaccionId = "PP-123456789";
            var montoReembolso = 50.25m;
            var motivo = "Reembolso de prueba";

            // Act
            var result = await _payPalService.ReembolsarPagoAsync(transaccionId, montoReembolso, motivo);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.ReembolsoId.Should().NotBeNullOrEmpty();
            result.Value.ReembolsoId.Should().StartWith("RF-");
            result.Value.Estado.Should().Be(RefundStatus.Completado);
            result.Value.TransaccionOriginalId.Should().Be(transaccionId);
            result.Value.MontoReembolsado.Should().Be(montoReembolso);
        }

        [Fact]
        public async Task VerificarEstadoPagoAsync_DebeRetornarUnEstadoValido_EnImplementacionSimulada()
        {
            // Arrange
            var transaccionId = "PP-987654321";

            // Act
            var result = await _payPalService.VerificarEstadoPagoAsync(transaccionId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeOneOf(PaymentStatus.Completado, PaymentStatus.Pendiente, PaymentStatus.Fallido);
        }
    }
} 