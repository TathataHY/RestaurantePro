namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Commands;

/// <summary>
/// Tests unitarios para ModificarReservacionHandler
/// Valida la lógica completa de modificación de reservaciones con validaciones de negocio
/// </summary>
public class ModificarReservacionHandlerTests
{
    private readonly Mock<IReservacionRepository> _reservacionRepositoryMock;
    private readonly Mock<IMesaRepository> _mesaRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IDisponibilidadService> _disponibilidadServiceMock;
    private readonly Mock<ICommunicationService> _notificacionServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ModificarReservacionHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ModificarReservacionHandler _handler;

    public ModificarReservacionHandlerTests()
    {
        _reservacionRepositoryMock = new Mock<IReservacionRepository>();
        _mesaRepositoryMock = new Mock<IMesaRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _disponibilidadServiceMock = new Mock<IDisponibilidadService>();
        _notificacionServiceMock = new Mock<ICommunicationService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ModificarReservacionHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new ModificarReservacionHandler(
            _reservacionRepositoryMock.Object,
            _mesaRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _disponibilidadServiceMock.Object,
            _notificacionServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_ModificacionBasica_DeberiaModificarReservacionExitosamente()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var nuevaMesaId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(5),
            NuevaHoraReservacion = new TimeSpan(19, 0, 0),
            NuevoNumeroPersonas = 6,
            NuevaMesaId = nuevaMesaId,
            MotivoModificacion = "Cliente solicitó cambio de fecha",
            UsuarioId = Guid.NewGuid()
        };

        var reservacionExistente = CreateMockReservacion(reservacionId);
        var nuevaMesa = CreateMockMesa(nuevaMesaId, 6);
        var reservacionDto = CreateMockReservacionDto(reservacionId);

        SetupRepositoriosMocks(reservacionExistente, nuevaMesa);
        SetupDisponibilidadMock(true);
        
        _mapperMock.Setup(x => x.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(reservacionId, result.Value.Id);
        
        _reservacionRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ModificacionConCambioMesa_DeberiaValidarDisponibilidad()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var nuevaMesaId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(3),
            NuevaHoraReservacion = new TimeSpan(20, 30, 0),
            NuevoNumeroPersonas = 4,
            NuevaMesaId = nuevaMesaId,
            MotivoModificacion = "Cambio de mesa por preferencia del cliente",
            UsuarioId = Guid.NewGuid()
        };

        var reservacionExistente = CreateMockReservacion(reservacionId);
        var nuevaMesa = CreateMockMesa(nuevaMesaId, 4);
        var reservacionDto = CreateMockReservacionDto(reservacionId);

        SetupRepositoriosMocks(reservacionExistente, nuevaMesa);
        SetupDisponibilidadMock(true);
        
        _mapperMock.Setup(x => x.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        _disponibilidadServiceMock.Verify(x => x.VerificarDisponibilidadMesaAsync(
            nuevaMesaId, 
            It.IsAny<DateTime>(), 
            It.IsAny<TimeSpan>(), 
            It.IsAny<TimeSpan>(),
            It.Is<Guid?>(id => id == reservacionId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ModificacionConCambioCliente_DeberiaValidarCliente()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var nuevoClienteId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(2),
            NuevaHoraReservacion = new TimeSpan(18, 0, 0),
            NuevoNumeroPersonas = 3,
            NuevoClienteId = nuevoClienteId,
            MotivoModificacion = "Transferencia de reservación a otro cliente",
            UsuarioId = Guid.NewGuid()
        };

        var reservacionExistente = CreateMockReservacion(reservacionId);
        var nuevoCliente = CreateMockCliente(nuevoClienteId);
        var reservacionDto = CreateMockReservacionDto(reservacionId);

        SetupRepositoriosMocks(reservacionExistente, null, nuevoCliente);
        SetupDisponibilidadMock(true);
        
        _mapperMock.Setup(x => x.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        _clienteRepositoryMock.Verify(x => x.ObtenerPorIdAsync(nuevoClienteId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ModificacionCompleta_DeberiaEnviarNotificaciones()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(7),
            NuevaHoraReservacion = new TimeSpan(21, 0, 0),
            NuevoNumeroPersonas = 8,
            MotivoModificacion = "Aumento de personas en la reservación",
            ObservacionesModificacion = "Celebración especial",
            UsuarioId = Guid.NewGuid()
        };

        var reservacionExistente = CreateMockReservacion(reservacionId);
        var reservacionDto = CreateMockReservacionDto(reservacionId);

        SetupRepositoriosMocks(reservacionExistente);
        SetupDisponibilidadMock(true);
        
        _mapperMock.Setup(x => x.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        _notificacionServiceMock.Setup(x => x.EnviarNotificacionModificacionReservacionAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        _notificacionServiceMock.Verify(x => x.EnviarNotificacionModificacionReservacionAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_ReservacionInexistente_DeberiaRetornarError()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(1),
            NuevaHoraReservacion = new TimeSpan(19, 0, 0),
            NuevoNumeroPersonas = 4,
            MotivoModificacion = "Test modificación",
            UsuarioId = Guid.NewGuid()
        };

        _reservacionRepositoryMock.Setup(x => x.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservacion?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no fue encontrada", result.Error);
    }

    [Fact]
    public async Task Handle_ReservacionCancelada_DeberiaRetornarError()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(1),
            NuevaHoraReservacion = new TimeSpan(19, 0, 0),
            NuevoNumeroPersonas = 4,
            MotivoModificacion = "Test modificación",
            UsuarioId = Guid.NewGuid()
        };

        var reservacionCancelada = CreateMockReservacion(reservacionId);
        reservacionCancelada.Cancelar("Test cancelación");

        _reservacionRepositoryMock.Setup(x => x.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacionCancelada);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no puede ser modificada", result.Error);
    }

    [Fact]
    public async Task Handle_MesaNoDisponible_DeberiaRetornarError()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var nuevaMesaId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(1),
            NuevaHoraReservacion = new TimeSpan(19, 0, 0),
            NuevoNumeroPersonas = 4,
            NuevaMesaId = nuevaMesaId,
            MotivoModificacion = "Cambio de mesa",
            UsuarioId = Guid.NewGuid()
        };

        var reservacionExistente = CreateMockReservacion(reservacionId);
        var nuevaMesa = CreateMockMesa(nuevaMesaId, 4);

        SetupRepositoriosMocks(reservacionExistente, nuevaMesa);
        SetupDisponibilidadMock(false); // Mesa NO disponible

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no está disponible", result.Error);
    }

    [Fact]
    public async Task Handle_ClienteInexistente_DeberiaRetornarError()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var nuevoClienteId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(1),
            NuevaHoraReservacion = new TimeSpan(19, 0, 0),
            NuevoNumeroPersonas = 4,
            NuevoClienteId = nuevoClienteId,
            MotivoModificacion = "Cambio de cliente",
            UsuarioId = Guid.NewGuid()
        };

        var reservacionExistente = CreateMockReservacion(reservacionId);

        _reservacionRepositoryMock.Setup(x => x.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacionExistente);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(nuevoClienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Cliente no encontrado", result.Error);
    }

    [Fact]
    public async Task Handle_MesaConCapacidadInsuficiente_DeberiaRetornarError()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var nuevaMesaId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(1),
            NuevaHoraReservacion = new TimeSpan(19, 0, 0),
            NuevoNumeroPersonas = 8,
            NuevaMesaId = nuevaMesaId,
            MotivoModificacion = "Cambio de mesa",
            UsuarioId = Guid.NewGuid()
        };

        var reservacionExistente = CreateMockReservacion(reservacionId);
        var mesaPequena = CreateMockMesa(nuevaMesaId, 4); // Solo para 4 personas

        SetupRepositoriosMocks(reservacionExistente, mesaPequena);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("capacidad insuficiente", result.Error);
    }

    #endregion

    #region Tests de Logging y Auditoria

    [Fact]
    public async Task Handle_ModificacionExitosa_DeberiaLoggearCorrectamente()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(1),
            NuevaHoraReservacion = new TimeSpan(19, 0, 0),
            NuevoNumeroPersonas = 4,
            MotivoModificacion = "Test logging",
            UsuarioId = Guid.NewGuid()
        };

        var reservacionExistente = CreateMockReservacion(reservacionId);
        var reservacionDto = CreateMockReservacionDto(reservacionId);

        SetupRepositoriosMocks(reservacionExistente);
        SetupDisponibilidadMock(true);
        
        _mapperMock.Setup(x => x.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando modificación")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("modificada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnModificacion_DeberiaLoggearError()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var command = new ModificarReservacionCommand
        {
            ReservacionId = reservacionId,
            NuevaFechaReservacion = DateTime.Today.AddDays(1),
            NuevaHoraReservacion = new TimeSpan(19, 0, 0),
            NuevoNumeroPersonas = 4,
            MotivoModificacion = "Test error",
            UsuarioId = Guid.NewGuid()
        };

        _reservacionRepositoryMock.Setup(x => x.ObtenerPorIdAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupRepositoriosMocks(Reservacion reservacion, Mesa? mesa = null, Cliente? cliente = null)
    {
        _reservacionRepositoryMock.Setup(x => x.ObtenerPorIdAsync(reservacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        if (mesa != null)
        {
            _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesa.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mesa);
        }

        if (cliente != null)
        {
            _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
        }

        _reservacionRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private void SetupDisponibilidadMock(bool disponible)
    {
        _disponibilidadServiceMock.Setup(x => x.VerificarDisponibilidadMesaAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), 
            It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(disponible);
    }

    private static Reservacion CreateMockReservacion(Guid id)
    {
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var reservacion = Reservacion.Crear(
            mesaId,
            clienteId,
            DateTime.Today.AddDays(1),
            new TimeSpan(19, 0, 0),
            4,
            "612345678",
            "cliente@email.com",
            "Reservación de prueba");
        
        // Usar reflexión para setear el ID
        typeof(EntityBase).GetProperty("Id")?.SetValue(reservacion, id);
        
        return reservacion;
    }

    private static Mesa CreateMockMesa(Guid id, int capacidad)
    {
        var mesa = Mesa.Crear(1, capacidad, TipoMesa.Interior, EstadoMesa.Disponible);
        
        // Usar reflexión para setear el ID
        typeof(EntityBase).GetProperty("Id")?.SetValue(mesa, id);
        
        return mesa;
    }

    private static Cliente CreateMockCliente(Guid id)
    {
        var cliente = Cliente.Crear(
            "Cliente Test",
            "cliente@test.com",
            "612345678",
            DateTime.Today.AddYears(-30),
            "Masculino");
        
        // Usar reflexión para setear el ID
        typeof(EntityBase).GetProperty("Id")?.SetValue(cliente, id);
        
        return cliente;
    }

    private static ReservacionDto CreateMockReservacionDto(Guid id)
    {
        return new ReservacionDto
        {
            Id = id,
            ClienteId = Guid.NewGuid(),
            MesaId = Guid.NewGuid(),
            FechaReservacion = DateTime.Today.AddDays(1),
            HoraReservacion = new TimeSpan(19, 0, 0),
            NumeroPersonas = 4,
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow
        };
    }

    #endregion
} 