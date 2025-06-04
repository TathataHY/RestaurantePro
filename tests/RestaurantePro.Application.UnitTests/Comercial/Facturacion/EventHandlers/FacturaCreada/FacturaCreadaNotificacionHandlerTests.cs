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
            _mockEmailService.Object,
            _mockSMSService.Object,
            _mockNotificationService.Object,
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

        // Crear factura sin cliente
        var factura = CreateMockFactura(facturaId, Guid.Empty, numeroFactura, 150.00m);
        SetPrivateProperty(factura, "ClienteId", null); // Sin cliente asociado
        
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockFacturaRepository.Verify(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockSMSService.Verify(x => x.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        // Debería loggear que no hay cliente
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no tiene cliente asociado")),
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

        var factura = CreateMockFactura(facturaId, clienteId, numeroFactura, 150.00m);
        
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);
            
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!); // Cliente no encontrado

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockFacturaRepository.Verify(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockSMSService.Verify(x => x.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        // Debería loggear advertencia
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cliente no encontrado para factura")),
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

        var cliente = CreateMockCliente(clienteId, "Cliente", null, "+1234567890"); // Sin email
        var factura = CreateMockFactura(facturaId, clienteId, numeroFactura, 180.00m);
        
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);
            
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockSMSService.Setup(x => x.SendSMSAsync(
                "+1234567890", It.Is<string>(s => s.Contains(numeroFactura) && s.Contains("180"))))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert - NO debería enviarse email
        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        
        // SÍ debería enviarse SMS
        _mockSMSService.Verify(x => x.SendSMSAsync(
            "+1234567890", It.Is<string>(s => s.Contains(numeroFactura) && s.Contains("$180"))), 
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

        // Crear cliente sin teléfono usando un constructor que pueda manejar null
        var cliente = CreateMockClienteConTelefonoOpcional(clienteId, "Ana Sin Telefono", "ana@email.com", null); // Sin telefono
        var factura = CreateMockFactura(facturaId, clienteId, numeroFactura, 225.75m);
        
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);
            
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // No debe enviar SMS
        _mockSMSService.Verify(x => x.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        
        // Debe enviar email
        _mockEmailService.Verify(x => x.SendEmailAsync(
            "ana@email.com", 
            It.Is<string>(s => s.Contains("Factura")), 
            It.Is<string>(s => s.Contains(numeroFactura) && s.Contains("225.75"))), Times.Once);

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

        var cliente = CreateMockCliente(clienteId, "Carlos Error Email", "carlos@error.com", "+1234567890");
        var factura = CreateMockFactura(facturaId, clienteId, numeroFactura, 250.75m);
        
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);
            
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Verificar que se procesó la factura y cliente
        _mockFacturaRepository.Verify(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);

        // Verificar que se loggea el proceso exitoso
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("✅ Notificaciones enviadas exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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

        var cliente = CreateMockCliente(clienteId, "Ana Error SMS", "ana@email.com", "+1234567890");
        var factura = CreateMockFactura(facturaId, clienteId, numeroFactura, 175.50m);
        
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);
            
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Verificar que se procesó la factura y cliente
        _mockFacturaRepository.Verify(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);

        // Verificar que se loggea el proceso exitoso
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("✅ Notificaciones enviadas exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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

        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(evento, cancellationTokenSource.Token));
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
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(repositoryException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(evento, CancellationToken.None));

        exception.Should().Be(repositoryException);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("💥 Error al enviar notificación para Factura")),
                repositoryException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
        var factura = CreateMockFactura(facturaId, clienteId, numeroFactura, 387.25m);
        
        _mockFacturaRepository.Setup(x => x.ObtenerPorIdAsync(facturaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factura);
            
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockSMSService.Setup(x => x.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert - Verificar que se loggea el número de factura
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(numeroFactura)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        // Verificar que se loggea el mensaje de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("✅ Notificaciones enviadas exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        // Verificar que se loggea información sobre el cliente
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cliente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    // Helper method para crear clientes mock
    private static Cliente CreateMockCliente(Guid id, string nombre, string? email, string? telefono)
    {
        // Separar nombre y apellido, proporcionando valores predeterminados
        var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var nombreCliente = partes.Length > 0 ? partes[0] : "Cliente";
        var apellidoCliente = partes.Length > 1 ? string.Join(" ", partes.Skip(1)) : "Test";
        
        var clienteNombre = ClienteNombre.Crear(nombreCliente, apellidoCliente);
        
        // Manejar email opcional
        var clienteEmail = !string.IsNullOrEmpty(email) ? Email.Create(email) : Email.Create("default@temp.com");
        
        // Manejar telefono opcional
        var clienteTelefono = !string.IsNullOrEmpty(telefono) ? PhoneNumber.Create(telefono) : PhoneNumber.Create("+000000000");
        
        var cliente = Cliente.Crear(
            id,
            clienteNombre,
            clienteEmail,
            clienteTelefono,
            DateTime.Now.AddYears(-30),
            true);
            
        // Si no hay email real, establecer como null usando reflexión
        if (string.IsNullOrEmpty(email))
        {
            SetPrivateProperty(cliente, "Email", null);
        }
        
        // Si no hay telefono real, establecer como null usando reflexión
        if (string.IsNullOrEmpty(telefono))
        {
            SetPrivateProperty(cliente, "Telefono", null);
        }
        
        return cliente;
    }

    // Helper method para crear clientes con teléfono opcional
    private static Cliente CreateMockClienteConTelefonoOpcional(Guid id, string nombre, string? email, string? telefono)
    {
        return CreateMockCliente(id, nombre, email, telefono);
    }

    // Helper method para crear facturas reales
    private static Factura CreateMockFactura(Guid facturaId, Guid clienteId, string numeroFactura, decimal total)
    {
        // Crear una instancia real de Factura usando el factory method
        var factura = Factura.Crear(
            numeroFactura,
            TipoFactura.Normal,
            "Cliente Test",
            clienteId,
            observaciones: "Factura de prueba"
        );
        
        // Establecer el ID específico usando reflexión
        SetPrivateProperty(factura, "Id", facturaId);
        
        // Establecer el total usando reflexión si es necesario
        if (total > 0)
        {
            SetPrivateProperty(factura, "Total", total);
        }
        
        return factura;
    }

    /// <summary>
    /// Método helper para establecer propiedades privadas usando reflexión
    /// </summary>
    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
        else
        {
            // Si no se puede establecer la propiedad directamente, usar el campo backing si existe
            var field = obj.GetType().GetField($"<{propertyName}>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
} 