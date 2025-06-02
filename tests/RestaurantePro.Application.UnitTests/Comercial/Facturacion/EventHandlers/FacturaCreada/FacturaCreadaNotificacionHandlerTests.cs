using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;

namespace RestaurantePro.Application.UnitTests.Comercial.Facturacion.EventHandlers.FacturaCreada;

/// <summary>
/// Tests para FacturaCreadaNotificacionHandler - Notificaciones automáticas multi-canal
/// </summary>
public class FacturaCreadaNotificacionHandlerTests
{
    private readonly Mock<IFacturaRepository> _mockFacturaRepository;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ISMSService> _mockSMSService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<FacturaCreadaNotificacionHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly FacturaCreadaNotificacionHandler _handler;

    public FacturaCreadaNotificacionHandlerTests()
    {
        _mockFacturaRepository = new Mock<IFacturaRepository>();
        _mockClienteRepository = new Mock<IClienteRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockSMSService = new Mock<ISMSService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<FacturaCreadaNotificacionHandler>>();
        _mockMediator = new Mock<IMediator>();
        
        _handler = new FacturaCreadaNotificacionHandler(
            _mockFacturaRepository.Object,
            _mockClienteRepository.Object,
            _mockLogger.Object,
            _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_FacturaCreadaConCliente_DeberiaEnviarNotificacionesMultiCanal()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0001";
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        // Usar el evento de Domain con la firma correcta
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

        var cliente = CreateMockCliente(clienteId, "María García", "maria@email.com", "+1234567890");
        var factura = CreateMockFactura(facturaId, clienteId, numeroFactura, 275.50m);
        
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);
            
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockFacturaRepository.Verify(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);

        // Verificar que se loggea el envío exitoso
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Notificaciones enviadas exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FacturaSinCliente_DeberiaEnviarSoloNotificacionInterna()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var numeroFactura = "F-2025-0002";
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockSMSService.Verify(x => x.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        // Solo notificación interna
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.Is<string>(msg => msg.Contains("Factura generada") && msg.Contains("cliente ocasional")),
            It.IsAny<string>()), Times.Once);

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
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!); // Cliente no encontrado

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockSMSService.Verify(x => x.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

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
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

        var cliente = CreateMockCliente(clienteId, "Carlos Sin Email", null, "+1234567890"); // Sin email
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockSMSService.Setup(x => x.SendSMSAsync(
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // No debe enviar email
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        
        // Debe enviar SMS
        _mockSMSService.Verify(x => x.SendSMSAsync(
            "+1234567890", It.Is<string>(s => s.Contains(numeroFactura) && s.Contains("180.00"))), Times.Once);

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
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

        var cliente = CreateMockCliente(clienteId, "Ana Sin Teléfono", "ana@email.com", null); // Sin teléfono
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Debe enviar email
        _mockEmailService.Verify(x => x.SendEmailAsync(
            "ana@email.com", 
            It.Is<string>(s => s.Contains("Factura") && s.Contains(numeroFactura)),
            It.Is<string>(s => s.Contains("Ana Sin Teléfono") && s.Contains("220.00"))), Times.Once);
        
        // No debe enviar SMS
        _mockSMSService.Verify(x => x.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

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
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

        var cliente = CreateMockCliente(clienteId, "Cliente Error", "error@email.com", "+1234567890");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        _mockSMSService.Setup(x => x.SendSMSAsync(
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
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
        _mockSMSService.Verify(x => x.SendSMSAsync(
            "+1234567890", It.Is<string>(s => s.Contains(numeroFactura) && s.Contains("300.00"))), Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnSMS_DeberiaLoggearYContinuar()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0007";
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

        var cliente = CreateMockCliente(clienteId, "Cliente SMS Error", "cliente@email.com", "+1234567890");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockSMSService.Setup(x => x.SendSMSAsync(
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

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
        _mockEmailService.Verify(x => x.SendEmailAsync(
            "cliente@email.com", 
            It.Is<string>(s => s.Contains("Factura") && s.Contains(numeroFactura)),
            It.Is<string>(s => s.Contains("Cliente SMS Error") && s.Contains("250.00"))), Times.Once);
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
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

        var cliente = CreateMockCliente(clienteId, "Cliente Test", "test@email.com", "+1234567890");
        var factura = CreateMockFactura(facturaId, clienteId, numeroFactura, montoTotal);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
            
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockSMSService.Setup(x => x.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.IsAny<Guid>(),
            It.Is<string>(msg => msg.Contains(montoTotal.ToString("C"))),
            It.IsAny<string>(),
            prioridadEsperada.ToString()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var facturaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var numeroFactura = "F-2025-0008";
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

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
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = new DateTime(2025, 1, 17, 16, 30, 0);
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

        var cliente = CreateMockCliente(clienteId, "Pedro Contexto", "pedro@email.com", "+5551234567");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockSMSService.Setup(x => x.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

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
        var tipoFactura = TipoFactura.Normal;
        var fechaEmision = DateTime.UtcNow;
        
        var evento = new Domain.Comercial.Facturacion.Events.FacturaCreada(
            facturaId, 
            numeroFactura, 
            tipoFactura, 
            fechaEmision);

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
        var clienteNombre = ClienteNombre.Crear(nombre.Split(' ')[0], nombre.Split(' ').Length > 1 ? nombre.Split(' ')[1] : "");
        var clienteEmail = !string.IsNullOrEmpty(email) ? Email.Create(email) : null;
        var clienteTelefono = !string.IsNullOrEmpty(telefono) ? PhoneNumber.Create(telefono) : null;
        
        return Cliente.Crear(
            id,
            clienteNombre,
            clienteEmail!,
            clienteTelefono!,
            DateTime.Now.AddYears(-30),
            true);
    }

    // Helper method para crear facturas mock
    private static Factura CreateMockFactura(Guid facturaId, Guid clienteId, string numeroFactura, decimal total)
    {
        var factura = new Mock<Factura>();
        factura.SetupGet(x => x.Id).Returns(facturaId);
        factura.SetupGet(x => x.ClienteId).Returns(clienteId);
        factura.SetupGet(x => x.NumeroFactura).Returns(numeroFactura);
        factura.SetupGet(x => x.Total).Returns(total);
        factura.SetupGet(x => x.FechaCreacion).Returns(DateTime.UtcNow);
        factura.SetupGet(x => x.FechaVencimiento).Returns(DateTime.UtcNow.AddDays(30));
        return factura.Object;
    }
} 