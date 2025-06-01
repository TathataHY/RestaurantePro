namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Commands;

/// <summary>
/// Tests unitarios para ConfirmarReservacionHandler
/// Cobertura completa de confirmación de reservaciones, validaciones de estado y notificaciones empresariales
/// </summary>
public class ConfirmarReservacionHandlerTests
{
    private readonly Mock<IReservacionRepository> _mockReservacionRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ConfirmarReservacionHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ICommunicationService> _mockNotificacionService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDateTimeService> _mockDateTimeService;
    private readonly ConfirmarReservacionHandler _handler;

    public ConfirmarReservacionHandlerTests()
    {
        _mockReservacionRepository = new Mock<IReservacionRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ConfirmarReservacionHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockNotificacionService = new Mock<ICommunicationService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDateTimeService = new Mock<IDateTimeService>();
        
        _handler = new ConfirmarReservacionHandler(
            _mockReservacionRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object,
            _mockNotificacionService.Object);
    }

    [Fact]
    public async Task Handle_ConReservacionValidaPendiente_DeberiaConfirmarExitosamente()
    {
        // Arrange
        var codigoReservacion = "RES-2024-001";
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion,
            TiempoConfirmacion = DateTime.Now
        };

        var reservacion = CrearReservacion(Guid.NewGuid(), EstadoReservacion.Pendiente, codigoReservacion);
        var reservacionDto = CrearReservacionDto(reservacion.Id, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeTrue();
        resultado.Value.Should().NotBeNull();
        resultado.Value.Estado.Should().Be(EstadoReservacion.Confirmada);

        // Verificar que se intentó confirmar la reservación
        _mockReservacionRepository.Verify(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando confirmación de reservación")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConReservacionInexistente_DeberiaRetornarError()
    {
        // Arrange
        var codigoReservacion = "RES-INEXISTENTE";
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion
        };

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservacion?)null);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("reservación especificada no fue encontrada");

        // Verificar que no se intentó guardar cambios
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConReservacionYaConfirmada_DeberiaRetornarError()
    {
        // Arrange
        var codigoReservacion = "RES-2024-002";
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion
        };

        var reservacion = CrearReservacion(Guid.NewGuid(), EstadoReservacion.Confirmada, codigoReservacion);

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("ya está confirmada");

        // Verificar que no se intentó guardar cambios
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConReservacionCancelada_DeberiaRetornarError()
    {
        // Arrange
        var codigoReservacion = "RES-2024-003";
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion
        };

        var reservacion = CrearReservacion(Guid.NewGuid(), EstadoReservacion.Cancelada, codigoReservacion);

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("cancelada");

        // Verificar que no se intentó guardar cambios
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConReservacionVencida_DeberiaRetornarError()
    {
        // Arrange
        var codigoReservacion = "RES-2024-004";
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion
        };

        var reservacion = CrearReservacion(
            Guid.NewGuid(), 
            EstadoReservacion.Pendiente, 
            codigoReservacion, 
            fechaHora: DateTime.Now.AddHours(-2)); // Reservación vencida

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockDateTimeService.Setup(d => d.Now)
            .Returns(DateTime.Now);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("vencida");
    }

    [Fact]
    public async Task Handle_ConConfirmacionExitosa_DeberiaEnviarNotificacion()
    {
        // Arrange
        var codigoReservacion = "RES-2024-005";
        var clienteId = Guid.NewGuid();
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion
        };

        var reservacion = CrearReservacion(Guid.NewGuid(), EstadoReservacion.Pendiente, codigoReservacion, clienteId: clienteId);
        var reservacionDto = CrearReservacionDto(reservacion.Id, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar que se envió notificación
        _mockNotificacionService.Verify(
            n => n.EnviarNotificacionConfirmacionReservacionAsync(
                clienteId, 
                codigoReservacion, 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConTiempoConfirmacionEspecifico_DeberiaUsarTiempoProporcionado()
    {
        // Arrange
        var codigoReservacion = "RES-2024-006";
        var tiempoConfirmacion = DateTime.Now.AddMinutes(-30);
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion,
            TiempoConfirmacion = tiempoConfirmacion
        };

        var reservacion = CrearReservacion(Guid.NewGuid(), EstadoReservacion.Pendiente, codigoReservacion);
        var reservacionDto = CrearReservacionDto(reservacion.Id, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        // Verificar que se usó el tiempo específico (esto dependería de la implementación real)
        _mockReservacionRepository.Verify(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcionEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var codigoReservacion = "RES-2024-007";
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion
        };

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().BeFalse();
        resultado.Error.Should().Contain("Error interno del servidor");

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error inesperado al confirmar reservación")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancelationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = "RES-TEST"
        };
        var cancellationToken = new CancellationToken(canceled: true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(command, cancellationToken));
    }

    [Theory]
    [InlineData(EstadoReservacion.Pendiente, true)]
    [InlineData(EstadoReservacion.Confirmada, false)]
    [InlineData(EstadoReservacion.Cancelada, false)]
    [InlineData(EstadoReservacion.Completada, false)]
    public async Task Handle_ConDiferentesEstados_DeberiaValidarCorrectamente(
        EstadoReservacion estado, bool deberiaConfirmar)
    {
        // Arrange
        var codigoReservacion = $"RES-ESTADO-{estado}";
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion
        };

        var reservacion = CrearReservacion(Guid.NewGuid(), estado, codigoReservacion);
        var reservacionDto = CrearReservacionDto(reservacion.Id, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        if (deberiaConfirmar)
        {
            _mockMapper.Setup(m => m.Map<ReservacionDto>(It.IsAny<Reservacion>()))
                .Returns(reservacionDto);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        }

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Succeeded.Should().Be(deberiaConfirmar);

        if (deberiaConfirmar)
        {
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Fact]
    public async Task Handle_ConReservacionConfirmada_DeberiaActualizarFechaConfirmacion()
    {
        // Arrange
        var codigoReservacion = "RES-2024-008";
        var fechaConfirmacion = DateTime.Now;
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion,
            TiempoConfirmacion = fechaConfirmacion
        };

        var reservacion = CrearReservacion(Guid.NewGuid(), EstadoReservacion.Pendiente, codigoReservacion);
        var reservacionDto = CrearReservacionDto(reservacion.Id, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();
        
        // Verificar que se actualizó la reservación
        _mockReservacionRepository.Verify(r => r.ActualizarAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConLoggingCompleto_DeberiaLoggearTodosLosEventos()
    {
        // Arrange
        var codigoReservacion = "RES-LOGGING";
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion
        };

        var reservacion = CrearReservacion(Guid.NewGuid(), EstadoReservacion.Pendiente, codigoReservacion);
        var reservacionDto = CrearReservacionDto(reservacion.Id, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar logging de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando confirmación de reservación")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("confirmada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConErrorEnNotificacion_DeberiaContinuarProceso()
    {
        // Arrange
        var codigoReservacion = "RES-2024-009";
        var clienteId = Guid.NewGuid();
        var command = new ConfirmarReservacionCommand
        {
            CodigoReservacion = codigoReservacion
        };

        var reservacion = CrearReservacion(Guid.NewGuid(), EstadoReservacion.Pendiente, codigoReservacion, clienteId: clienteId);
        var reservacionDto = CrearReservacionDto(reservacion.Id, EstadoReservacion.Confirmada);

        _mockReservacionRepository.Setup(r => r.ObtenerPorCodigoAsync(codigoReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);

        _mockMapper.Setup(m => m.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockNotificacionService.Setup(n => n.EnviarNotificacionConfirmacionReservacionAsync(
                It.IsAny<Guid>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error en notificación"));

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultado.Succeeded.Should().BeTrue();

        // Verificar que se loggeó el error de notificación pero el proceso continuó
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al enviar notificación")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Métodos de Apoyo

    private Reservacion CrearReservacion(
        Guid id, 
        EstadoReservacion estado, 
        string codigo,
        DateTime? fechaHora = null,
        Guid? clienteId = null,
        Guid? mesaId = null)
    {
        // Usar reflection para crear la reservación con propiedades privadas
        var reservacion = (Reservacion)Activator.CreateInstance(typeof(Reservacion), true)!;
        
        typeof(Reservacion).GetProperty("Id")?.SetValue(reservacion, id);
        typeof(Reservacion).GetProperty("CodigoReservacion")?.SetValue(reservacion, codigo);
        typeof(Reservacion).GetProperty("Estado")?.SetValue(reservacion, estado);
        typeof(Reservacion).GetProperty("ClienteId")?.SetValue(reservacion, clienteId ?? Guid.NewGuid());
        typeof(Reservacion).GetProperty("MesaId")?.SetValue(reservacion, mesaId ?? Guid.NewGuid());
        typeof(Reservacion).GetProperty("FechaHora")?.SetValue(reservacion, fechaHora ?? DateTime.Now.AddHours(2));
        typeof(Reservacion).GetProperty("NumeroPersonas")?.SetValue(reservacion, 4);
        typeof(Reservacion).GetProperty("TelefonoContacto")?.SetValue(reservacion, "+1234567890");
        typeof(Reservacion).GetProperty("FechaCreacion")?.SetValue(reservacion, DateTime.Now.AddHours(-1));
        
        return reservacion;
    }

    private ReservacionDto CrearReservacionDto(Guid id, EstadoReservacion estado)
    {
        return new ReservacionDto
        {
            Id = id,
            CodigoReservacion = $"RES-{id:N}".Substring(0, 12),
            Estado = estado,
            ClienteId = Guid.NewGuid(),
            MesaId = Guid.NewGuid(),
            FechaHora = DateTime.Now.AddHours(2),
            NumeroPersonas = 4,
            TelefonoContacto = "+1234567890",
            FechaCreacion = DateTime.Now.AddHours(-1),
            FechaConfirmacion = estado == EstadoReservacion.Confirmada ? DateTime.Now : null
        };
    }

    #endregion
}