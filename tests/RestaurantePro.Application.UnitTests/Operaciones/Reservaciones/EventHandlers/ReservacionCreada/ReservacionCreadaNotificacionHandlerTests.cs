
namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.EventHandlers.ReservacionCreada;

/// <summary>
/// Tests para ReservacionCreadaNotificacionHandler - Confirmaciones automáticas de reservaciones
/// </summary>
public class ReservacionCreadaNotificacionHandlerTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ISMSService> _mockSMSService;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly Mock<IMesaRepository> _mockMesaRepository;
    private readonly Mock<ILogger<ReservacionCreadaNotificacionHandler>> _mockLogger;
    private readonly ReservacionCreadaNotificacionHandler _handler;

    public ReservacionCreadaNotificacionHandlerTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockSMSService = new Mock<ISMSService>();
        _mockClienteRepository = new Mock<IClienteRepository>();
        _mockMesaRepository = new Mock<IMesaRepository>();
        _mockLogger = new Mock<ILogger<ReservacionCreadaNotificacionHandler>>();
        
        _handler = new ReservacionCreadaNotificacionHandler(
            _mockNotificationService.Object,
            _mockEmailService.Object,
            _mockSMSService.Object,
            _mockClienteRepository.Object,
            _mockMesaRepository.Object,
            _mockLogger.Object);
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
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Luis Reservador", "luis@email.com", "+1234567890");
        var mesa = CreateMockMesa(mesaId, 4, 15); // Mesa 15 para 4 personas
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.EnviarConfirmacionReservacionAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockSMSService.Setup(x => x.EnviarRecordatorioReservacionAsync(
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMesaRepository.Verify(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar envío de Email de confirmación
        _mockEmailService.Verify(x => x.EnviarConfirmacionReservacionAsync(
            "luis@email.com",
            "Luis Reservador",
            fechaReservacion,
            numeroPersonas,
            15, // número de mesa
            It.IsAny<CancellationToken>()), Times.Once);

        // Verificar envío de SMS recordatorio
        _mockSMSService.Verify(x => x.EnviarRecordatorioReservacionAsync(
            "+1234567890",
            fechaReservacion,
            numeroPersonas,
            15, // número de mesa
            It.IsAny<CancellationToken>()), Times.Once);

        // Verificar notificación interna al personal
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("Nueva reservación confirmada")),
            It.IsAny<string>(),
            NivelPrioridad.Media,
            It.IsAny<CancellationToken>()), Times.Once);

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
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!); // Cliente no encontrado

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(x => x.EnviarConfirmacionReservacionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockSMSService.Verify(x => x.EnviarRecordatorioReservacionAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);

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
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

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
        _mockEmailService.Verify(x => x.EnviarConfirmacionReservacionAsync(
            "maria@email.com",
            "María Mesa",
            fechaReservacion,
            numeroPersonas,
            0, // Sin número de mesa
            It.IsAny<CancellationToken>()), Times.Once);

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
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Carlos Sin Email", null, "+1234567890"); // Sin email
        var mesa = CreateMockMesa(mesaId, 4, 8);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockSMSService.Setup(x => x.EnviarRecordatorioReservacionAsync(
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // No debe enviar email
        _mockEmailService.Verify(x => x.EnviarConfirmacionReservacionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        
        // Debe enviar SMS
        _mockSMSService.Verify(x => x.EnviarRecordatorioReservacionAsync(
            "+1234567890", fechaReservacion, numeroPersonas, 8, It.IsAny<CancellationToken>()), Times.Once);

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
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Ana Sin Teléfono", "ana@email.com", null); // Sin teléfono
        var mesa = CreateMockMesa(mesaId, 6, 12);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.EnviarConfirmacionReservacionAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Debe enviar email
        _mockEmailService.Verify(x => x.EnviarConfirmacionReservacionAsync(
            "ana@email.com", "Ana Sin Teléfono", fechaReservacion, numeroPersonas, 12, It.IsAny<CancellationToken>()), Times.Once);
        
        // No debe enviar SMS
        _mockSMSService.Verify(x => x.EnviarRecordatorioReservacionAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);

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
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Cliente Test", "test@email.com", "+1234567890");
        var mesa = CreateMockMesa(mesaId, numeroPersonas, 1);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.EnviarConfirmacionReservacionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockSMSService.Setup(x => x.EnviarRecordatorioReservacionAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
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
    public async Task Handle_ReservacionParaHoy_DeberiaEnviarAlertaUrgente()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Today.AddHours(20); // Hoy mismo - urgente
        var numeroPersonas = 4;
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Cliente Urgente", "urgente@email.com", "+9999999999");
        var mesa = CreateMockMesa(mesaId, 4, 5);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.EnviarConfirmacionReservacionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockSMSService.Setup(x => x.EnviarRecordatorioReservacionAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Debería enviar notificación de prioridad alta para reservaciones del mismo día
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("URGENTE") || msg.Contains("mismo día")),
            It.IsAny<string>(),
            NivelPrioridad.Alta,
            It.IsAny<CancellationToken>()), Times.Once);

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
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Cliente Error", "error@email.com", "+1234567890");
        var mesa = CreateMockMesa(mesaId, 4, 10);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.EnviarConfirmacionReservacionAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Error enviando email"));

        _mockSMSService.Setup(x => x.EnviarRecordatorioReservacionAsync(
                It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(),
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error enviando confirmación por email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // SMS debería enviarse exitosamente
        _mockSMSService.Verify(x => x.EnviarRecordatorioReservacionAsync(
            "+1234567890", fechaReservacion, numeroPersonas, 10, It.IsAny<CancellationToken>()), Times.Once);
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
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

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
        var evento = new ReservacionCreadaEvent(reservacionId, clienteId, mesaId, fechaReservacion, numeroPersonas);

        var cliente = CreateMockCliente(clienteId, "Roberto Contexto", "roberto@email.com", "+5554433221");
        var mesa = CreateMockMesa(mesaId, 6, 22);
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockEmailService.Setup(x => x.EnviarConfirmacionReservacionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockSMSService.Setup(x => x.EnviarRecordatorioReservacionAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

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
        var cliente = new Mock<Cliente>();
        cliente.SetupGet(x => x.Id).Returns(id);
        cliente.SetupGet(x => x.NombreCompleto).Returns(nombre);
        cliente.SetupGet(x => x.Email).Returns(email);
        cliente.SetupGet(x => x.Telefono).Returns(telefono);
        cliente.Setup(x => x.TieneEmail()).Returns(!string.IsNullOrEmpty(email));
        cliente.Setup(x => x.TieneTelefono()).Returns(!string.IsNullOrEmpty(telefono));
        return cliente.Object;
    }

    private static Mesa CreateMockMesa(Guid id, int capacidad, int numero)
    {
        var mesa = new Mock<Mesa>();
        mesa.SetupGet(x => x.Id).Returns(id);
        mesa.SetupGet(x => x.Capacidad).Returns(capacidad);
        mesa.SetupGet(x => x.Numero).Returns(numero);
        return mesa.Object;
    }
} 