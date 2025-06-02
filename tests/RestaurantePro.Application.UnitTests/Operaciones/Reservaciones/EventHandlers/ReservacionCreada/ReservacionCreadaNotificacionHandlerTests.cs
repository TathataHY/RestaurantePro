namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.EventHandlers.ReservacionCreada;

using ReservacionCreadaEvent = RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion.ReservacionCreada;

/// <summary>
/// Tests para ReservacionCreadaNotificacionHandler - Confirmaciones automáticas de reservaciones
/// </summary>
public class ReservacionCreadaNotificacionHandlerTests
{
    private readonly Mock<IReservacionRepository> _mockReservacionRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ISMSService> _mockSMSService;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly Mock<IMesaRepository> _mockMesaRepository;
    private readonly Mock<ILogger<ReservacionCreadaNotificacionHandler>> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly ReservacionCreadaNotificacionHandler _handler;

    public ReservacionCreadaNotificacionHandlerTests()
    {
        _mockReservacionRepository = new Mock<IReservacionRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockSMSService = new Mock<ISMSService>();
        _mockClienteRepository = new Mock<IClienteRepository>();
        _mockMesaRepository = new Mock<IMesaRepository>();
        _mockLogger = new Mock<ILogger<ReservacionCreadaNotificacionHandler>>();
        _mockMediator = new Mock<IMediator>();
        
        _handler = new ReservacionCreadaNotificacionHandler(
            _mockReservacionRepository.Object,
            _mockClienteRepository.Object,
            _mockMesaRepository.Object,
            _mockLogger.Object,
            _mockMediator.Object,
            _mockEmailService.Object,
            _mockSMSService.Object);
    }

    [Fact]
    public async Task Handle_ReservacionCreada_DeberiaEnviarConfirmacionCompleta()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddDays(1).AddHours(19); // Mañana a las 7 PM
        var numeroPersonas = 4;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Luis Reservador", "luis@email.com", "+1234567890");
        var mesa = CreateMockMesa(mesaId, 4, 15); // Mesa 15 para 4 personas
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.SendHtmlEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockSMSService.Setup(x => x.SendSMSWithTrackingAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMesaRepository.Verify(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar envío de Email de confirmación
        _mockEmailService.Verify(x => x.SendHtmlEmailAsync(
            "luis@email.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        // Verificar envío de SMS recordatorio
        _mockSMSService.Verify(x => x.SendSMSWithTrackingAsync(
            "+1234567890", It.IsAny<string>(), clienteId, "ConfirmacionReservacion"), Times.Once);

        // Verificar notificación interna al personal
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);

        // Debería loggear confirmación exitosa
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("📅 Confirmación de reservación enviada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteNoEncontrado_DeberiaLoggearAdvertencia()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddDays(1).AddHours(20);
        var numeroPersonas = 2;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!); // Cliente no encontrado

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(x => x.SendHtmlEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockSMSService.Verify(x => x.SendSMSWithTrackingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);

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
    public async Task Handle_MesaNoEncontrada_DeberiaLoggearAdvertencia()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddDays(1).AddHours(18);
        var numeroPersonas = 6;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "María Mesa", "maria@email.com", "+9876543210");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mesa)null!); // Mesa no encontrada

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockMesaRepository.Verify(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()), Times.Once);

        // Debería enviar notificaciones pero sin número de mesa específico
        _mockEmailService.Verify(x => x.SendHtmlEmailAsync(
            "maria@email.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        // Debería loggear advertencia sobre mesa no encontrada
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Mesa no encontrada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteSinEmail_NoDeberiaEnviarEmail()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddDays(2).AddHours(21);
        var numeroPersonas = 3;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Carlos Sin Email", null, "+1234567890"); // Sin email
        var mesa = CreateMockMesa(mesaId, 4, 8);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockSMSService.Setup(x => x.SendSMSWithTrackingAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // No debe enviar email
        _mockEmailService.Verify(x => x.SendHtmlEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        
        // Debe enviar SMS
        _mockSMSService.Verify(x => x.SendSMSWithTrackingAsync(
            "+1234567890", It.IsAny<string>(), clienteId, "ConfirmacionReservacion"), Times.Once);

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
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddDays(3).AddHours(19);
        var numeroPersonas = 5;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Ana Sin Teléfono", "ana@email.com", null); // Sin teléfono
        var mesa = CreateMockMesa(mesaId, 6, 12);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.SendHtmlEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockSMSService.Setup(x => x.SendSMSWithTrackingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Debe enviar email
        _mockEmailService.Verify(x => x.SendHtmlEmailAsync(
            "ana@email.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        
        // No debe enviar SMS
        _mockSMSService.Verify(x => x.SendSMSWithTrackingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()), Times.Never);

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

    [Theory]
    [InlineData(2, NivelPrioridad.Baja)]       // Mesa pequeña - prioridad baja
    [InlineData(4, NivelPrioridad.Media)]      // Mesa estándar - prioridad media  
    [InlineData(8, NivelPrioridad.Media)]      // Mesa grande - prioridad media
    [InlineData(12, NivelPrioridad.Alta)]      // Mesa VIP - prioridad alta
    public async Task Handle_DiferentesNumeroPersonas_DeberiaUsarPrioridadApropiada(
        int numeroPersonas, NivelPrioridad prioridadEsperada)
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddDays(1).AddHours(20);
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Cliente Test", "test@email.com", "+1234567890");
        var mesa = CreateMockMesa(mesaId, numeroPersonas, 1);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.SendHtmlEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockSMSService.Setup(x => x.SendSMSWithTrackingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            prioridadEsperada.ToString()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReservacionParaHoy_DeberiaEnviarAlertaUrgente()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddHours(20); // Hoy mismo - urgente
        var numeroPersonas = 4;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Cliente Urgente", "urgente@email.com", "+9999999999");
        var mesa = CreateMockMesa(mesaId, 4, 5);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.SendHtmlEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockSMSService.Setup(x => x.SendSMSWithTrackingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Debería enviar notificación de prioridad alta para reservaciones del mismo día
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.IsAny<Guid>(),
            It.Is<string>(msg => msg.Contains("URGENTE") || msg.Contains("mismo día")),
            It.IsAny<string>(),
            "Alta"), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("🚨 Reservación para el mismo día")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnEmail_DeberiaLoggearYContinuar()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddDays(1).AddHours(19);
        var numeroPersonas = 4;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Cliente Error", "error@email.com", "+1234567890");
        var mesa = CreateMockMesa(mesaId, 4, 10);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.SendHtmlEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        _mockSMSService.Setup(x => x.SendSMSWithTrackingAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        // No debería lanzar excepción, solo loggear el error
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error enviando confirmación por email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // SMS debería enviarse exitosamente
        _mockSMSService.Verify(x => x.SendSMSWithTrackingAsync(
            "+1234567890", It.IsAny<string>(), clienteId, "ConfirmacionReservacion"), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddDays(1).AddHours(19);
        var numeroPersonas = 4;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(evento, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_DeberiaIncluirContextoReservacionEnLogs()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = new DateTime(2025, 1, 18, 20, 0, 0); // 18 Enero 2025, 8 PM
        var numeroPersonas = 6;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion.Date, fechaReservacion.TimeOfDay, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Roberto Contexto", "roberto@email.com", "+5554433221");
        var mesa = CreateMockMesa(mesaId, 6, 22);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.SendHtmlEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockSMSService.Setup(x => x.SendSMSWithTrackingAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(reservacionId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Roberto Contexto")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mesa 22")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    // Helper methods para crear mocks
    private static Cliente CreateMockCliente(Guid id, string nombre, string? email, string? telefono)
    {
        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Juan", "Pérez"), 
            email ?? "test@email.com", 
            telefono ?? "+1234567890",
            new DateTime(1990, 1, 1)); // Usar el método de fábrica real
        
        // Configurar el ID usando reflexión si es necesario
        typeof(EntityBase).GetProperty("Id")?.SetValue(cliente, id);
        
        return cliente;
    }

    private static Mesa CreateMockMesa(Guid id, int capacidad, int numero)
    {
        var mesa = Mesa.Crear(numero, capacidad, "Interior"); // Usar la firma correcta con 3 parámetros
        
        // Configurar el ID usando reflexión si es necesario  
        typeof(EntityBase).GetProperty("Id")?.SetValue(mesa, id);
        
        return mesa;
    }
} 