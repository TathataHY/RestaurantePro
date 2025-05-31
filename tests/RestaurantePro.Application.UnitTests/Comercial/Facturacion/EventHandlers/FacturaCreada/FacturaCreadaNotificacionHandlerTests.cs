using RestaurantePro.Application.Comercial.Facturacion.EventHandlers.FacturaCreada;
using RestaurantePro.Domain.Comercial.Facturacion.Events;
using RestaurantePro.Domain.Core.Notificaciones.Services;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Core.Notificaciones.Enums;

namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.EventHandlers.FacturaCreada;

/// <summary>
/// Tests para FacturaCreadaNotificacionHandler - Notificaciones automáticas multi-canal
/// </summary>
public class FacturaCreadaNotificacionHandlerTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ISMSService> _mockSMSService;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly Mock<ILogger<FacturaCreadaNotificacionHandler>> _mockLogger;
    private readonly FacturaCreadaNotificacionHandler _handler;

    public FacturaCreadaNotificacionHandlerTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockSMSService = new Mock<ISMSService>();
        _mockClienteRepository = new Mock<IClienteRepository>();
        _mockLogger = new Mock<ILogger<FacturaCreadaNotificacionHandler>>();
        
        _handler = new FacturaCreadaNotificacionHandler(
            _mockNotificationService.Object,
            _mockEmailService.Object,
            _mockSMSService.Object,
            _mockClienteRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_FacturaCreadaConCliente_DeberiaEnviarNotificacionesMultiCanal()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0001";
        var montoTotal = 275.50m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        var cliente = CreateMockCliente(clienteId, "María García", "maria@email.com", "+1234567890");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.EnviarFacturaAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockSMSService.Setup(x => x.EnviarConfirmacionFacturaAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar envío de Email
        _mockEmailService.Verify(x => x.EnviarFacturaAsync(
            "maria@email.com",
            "María García",
            numeroFactura,
            montoTotal,
            It.IsAny<CancellationToken>()), Times.Once);

        // Verificar envío de SMS
        _mockSMSService.Verify(x => x.EnviarConfirmacionFacturaAsync(
            "+1234567890",
            numeroFactura,
            montoTotal,
            It.IsAny<CancellationToken>()), Times.Once);

        // Verificar notificación interna
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("Factura generada")),
            It.IsAny<string>(),
            NivelPrioridad.Media,
            It.IsAny<CancellationToken>()), Times.Once);

        // Debería loggear envío exitoso
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("📧 Notificaciones multi-canal enviadas exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FacturaSinCliente_DeberiaEnviarSoloNotificacionInterna()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        Guid? clienteId = null; // Factura sin cliente
        var numeroFactura = "F-2025-0002";
        var montoTotal = 150.00m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockEmailService.Verify(x => x.EnviarFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSMSService.Verify(x => x.EnviarConfirmacionFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);

        // Solo notificación interna
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("Factura generada") && msg.Contains("cliente ocasional")),
            It.IsAny<string>(),
            NivelPrioridad.Baja,
            It.IsAny<CancellationToken>()), Times.Once);

        // Debería loggear que no hay cliente
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("💡 Factura sin cliente asociado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteNoEncontrado_DeberiaLoggearAdvertencia()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0003";
        var montoTotal = 200.00m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!); // Cliente no encontrado

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(x => x.EnviarFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSMSService.Verify(x => x.EnviarConfirmacionFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);

        // Debería loggear advertencia
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Cliente no encontrado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteSinEmail_NoDeberiaEnviarEmail()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0004";
        var montoTotal = 180.00m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        var cliente = CreateMockCliente(clienteId, "Carlos Sin Email", null, "+1234567890"); // Sin email
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockSMSService.Setup(x => x.EnviarConfirmacionFacturaAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // No debe enviar email
        _mockEmailService.Verify(x => x.EnviarFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
        
        // Debe enviar SMS
        _mockSMSService.Verify(x => x.EnviarConfirmacionFacturaAsync(
            "+1234567890", numeroFactura, montoTotal, It.IsAny<CancellationToken>()), Times.Once);

        // Debería loggear que no hay email
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("📧 Cliente sin email registrado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteSinTelefono_NoDeberiaEnviarSMS()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0005";
        var montoTotal = 220.00m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        var cliente = CreateMockCliente(clienteId, "Ana Sin Teléfono", "ana@email.com", null); // Sin teléfono
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.EnviarFacturaAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Debe enviar email
        _mockEmailService.Verify(x => x.EnviarFacturaAsync(
            "ana@email.com", "Ana Sin Teléfono", numeroFactura, montoTotal, It.IsAny<CancellationToken>()), Times.Once);
        
        // No debe enviar SMS
        _mockSMSService.Verify(x => x.EnviarConfirmacionFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);

        // Debería loggear que no hay teléfono
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("📱 Cliente sin teléfono registrado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnEmail_DeberiaLoggearYContinuar()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0006";
        var montoTotal = 300.00m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        var cliente = CreateMockCliente(clienteId, "Cliente Error", "error@email.com", "+1234567890");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.EnviarFacturaAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Error enviando email"));

        _mockSMSService.Setup(x => x.EnviarConfirmacionFacturaAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        // No debería lanzar excepción, solo loggear el error
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error enviando email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // SMS debería enviarse exitosamente
        _mockSMSService.Verify(x => x.EnviarConfirmacionFacturaAsync(
            "+1234567890", numeroFactura, montoTotal, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnSMS_DeberiaLoggearYContinuar()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0007";
        var montoTotal = 250.00m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        var cliente = CreateMockCliente(clienteId, "Cliente SMS Error", "cliente@email.com", "+error");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.EnviarFacturaAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockSMSService.Setup(x => x.EnviarConfirmacionFacturaAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Número de teléfono inválido"));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error enviando SMS")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Email debería enviarse exitosamente
        _mockEmailService.Verify(x => x.EnviarFacturaAsync(
            "cliente@email.com", "Cliente SMS Error", numeroFactura, montoTotal, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(50.00, NivelPrioridad.Baja)]      // Monto bajo
    [InlineData(150.00, NivelPrioridad.Media)]    // Monto medio
    [InlineData(500.00, NivelPrioridad.Media)]    // Monto alto
    [InlineData(1000.00, NivelPrioridad.Alta)]    // Monto muy alto (VIP)
    public async Task Handle_DiferentesMontos_DeberiaUsarPrioridadApropiada(
        decimal montoTotal, NivelPrioridad prioridadEsperada)
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-TEST";
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        var cliente = CreateMockCliente(clienteId, "Cliente Test", "test@email.com", "+1234567890");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.EnviarFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockSMSService.Setup(x => x.EnviarConfirmacionFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            prioridadEsperada,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0008";
        var montoTotal = 200.00m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(evento, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_DeberiaIncluirContextoFacturaEnLogs()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-1234";
        var fechaCreacion = new DateTime(2025, 1, 17, 16, 30, 0);
        var montoTotal = 387.25m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, fechaCreacion);

        var cliente = CreateMockCliente(clienteId, "Pedro Contexto", "pedro@email.com", "+5551234567");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.EnviarFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockSMSService.Setup(x => x.EnviarConfirmacionFacturaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(numeroFactura)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("387.25")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Pedro Contexto")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_DeberiaLoggearYPropagar()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0009";
        var montoTotal = 400.00m;
        var evento = new FacturaCreadaEvent(facturaId, numeroFactura, clienteId, montoTotal, DateTime.UtcNow);

        var repositoryException = new Exception("Error de conexión a base de datos");
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(repositoryException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(evento, CancellationToken.None));

        exception.Should().Be(repositoryException);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error procesando notificaciones de factura")),
                repositoryException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Helper method para crear clientes mock
    private static Cliente CreateMockCliente(Guid id, string nombre, string? email, string? telefono)
    {
        var cliente = new Mock<Cliente>();
        cliente.SetupGet(x => x.Id).Returns(id);
        cliente.SetupGet(x => x.NombreCompleto).Returns(nombre);
        cliente.SetupGet(x => x.Email).Returns(email);
        cliente.SetupGet(x => x.Telefono).Returns(telefono);
        cliente.Setup(x => x.TieneEmail()).Returns(!string.IsNullOrEmpty(email));
        cliente.Setup(x => x.TieneTelefono()).Returns(!string.IsNullOrEmpty(telefono));
        return cliente.Object;
    }
} 