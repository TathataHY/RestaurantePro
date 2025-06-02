namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Commands;

/// <summary>
/// 🎟️ Tests para CrearReservacionHandler
/// Validaciones empresariales de reservaciones, disponibilidad y notificaciones
/// </summary>
public class CrearReservacionHandlerTests
{
    private readonly Mock<IReservacionRepository> _reservacionRepositoryMock;
    private readonly Mock<IMesaRepository> _mesaRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IDisponibilidadService> _disponibilidadServiceMock;
    private readonly Mock<ICommunicationService> _notificacionServiceMock;
    private readonly Mock<IValidacionReservacionService> _validacionServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CrearReservacionHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly CrearReservacionHandler _handler;

    public CrearReservacionHandlerTests()
    {
        _reservacionRepositoryMock = new Mock<IReservacionRepository>();
        _mesaRepositoryMock = new Mock<IMesaRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _disponibilidadServiceMock = new Mock<IDisponibilidadService>();
        _notificacionServiceMock = new Mock<ICommunicationService>();
        _validacionServiceMock = new Mock<IValidacionReservacionService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CrearReservacionHandler>>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();

        _handler = new CrearReservacionHandler(
            _reservacionRepositoryMock.Object,
            _mesaRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _disponibilidadServiceMock.Object,
            _notificacionServiceMock.Object,
            _validacionServiceMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserMock.Object,
            _dateTimeServiceMock.Object);
    }

    /// <summary>
    /// ✅ Test: Crear reservación exitosa con todas las validaciones
    /// </summary>
    [Fact]
    public async Task Handle_CrearReservacionExitosa_DeberiaRetornarSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Now.AddDays(1);
        var reservacionId = Guid.NewGuid();

        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaReservacion,
            NumeroPersonas = 4,
            Observaciones = "Celebración de cumpleaños",
            TelefonoContacto = "+1234567890"
        };

        var cliente = Cliente.Crear(
            "Juan Pérez",
            "juan.perez@email.com",
            "+1234567890");

        var mesa = Mesa.Crear(1, 4, TipoMesa.Interior);

        var reservacion = Reservacion.Crear(
            mesaId,
            clienteId,
            fechaReservacion,
            TimeSpan.FromHours(2), // duracionEstimada
            4, // cantidadPersonas
            "+1234567890", // telefono
            "juan.perez@email.com"); // email (sin observaciones)
        reservacion.GetType().GetProperty("Id")?.SetValue(reservacion, reservacionId);

        var reservacionDto = new ReservacionDto
        {
            Id = reservacionId,
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaReservacion,
            NumeroPersonas = 4,
            Estado = EstadoReservacion.Pendiente,
            Observaciones = "Celebración de cumpleaños"
        };

        // Setup mocks
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _validacionServiceMock.Setup(x => x.ValidarHorarioPermitido(fechaReservacion))
            .Returns(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarCapacidadMesa(mesa, 4))
            .Returns(Result.Success());
        _disponibilidadServiceMock.Setup(x => x.VerificarDisponibilidadAsync(mesaId, fechaReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _reservacionRepositoryMock.Setup(x => x.CrearAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);
        _mapperMock.Setup(x => x.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(reservacionDto);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(reservacionId);
        result.Value.Estado.Should().Be(EstadoReservacion.Pendiente);

        // Verify repository calls
        _reservacionRepositoryMock.Verify(x => x.CrearAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ❌ Test: Cliente no encontrado
    /// </summary>
    [Fact]
    public async Task Handle_ClienteNoEncontrado_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = Guid.NewGuid(),
            FechaHora = DateTime.Now.AddDays(1),
            NumeroPersonas = 4
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeFalse();
        result.Error.Should().Contain("Cliente no encontrado");

        // Verify no se creó reservación
        _reservacionRepositoryMock.Verify(x => x.CrearAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ❌ Test: Mesa no encontrada
    /// </summary>
    [Fact]
    public async Task Handle_MesaNoEncontrada_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = DateTime.Now.AddDays(1),
            NumeroPersonas = 4
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+123456789");

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mesa?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeFalse();
        result.Error.Should().Contain("Mesa no encontrada");

        // Verify no se creó reservación
        _reservacionRepositoryMock.Verify(x => x.CrearAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ❌ Test: Fecha en el pasado
    /// </summary>
    [Fact]
    public async Task Handle_FechaEnElPasado_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaPasada = DateTime.Now.AddDays(-1);

        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaPasada,
            NumeroPersonas = 4
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+123456789");
        var mesa = Mesa.Crear(1, 4, TipoMesa.Interior);

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        // Act & Assert - El factory method de Reservacion ya valida que la fecha no esté en el pasado
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsSuccess().Should().BeFalse();
        result.Error.Should().Contain("fecha");
    }

    /// <summary>
    /// ❌ Test: Mesa no disponible en el horario solicitado
    /// </summary>
    [Fact]
    public async Task Handle_MesaNoDisponible_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Now.AddDays(1);

        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaReservacion,
            NumeroPersonas = 4
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+123456789");
        var mesa = Mesa.Crear(1, 4, TipoMesa.Interior);

        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _validacionServiceMock.Setup(x => x.ValidarHorarioPermitido(fechaReservacion))
            .Returns(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarCapacidadMesa(mesa, 4))
            .Returns(Result.Success());
        _disponibilidadServiceMock.Setup(x => x.VerificarDisponibilidadAsync(mesaId, fechaReservacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("La mesa no está disponible en el horario solicitado"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeFalse();
        result.Error.Should().Contain("disponible");
    }

    /// <summary>
    /// ❌ Test: Capacidad de mesa insuficiente
    /// </summary>
    [Fact]
    public async Task Handle_CapacidadInsuficiente_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Now.AddDays(1);

        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaReservacion,
            NumeroPersonas = 8 // Más personas que la capacidad
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+123456789");
        var mesa = Mesa.Crear(1, 4, TipoMesa.Interior); // Capacidad máxima 4

        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _validacionServiceMock.Setup(x => x.ValidarHorarioPermitido(fechaReservacion))
            .Returns(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarCapacidadMesa(mesa, 8))
            .Returns(Result.Failure("La mesa no tiene capacidad suficiente para 8 personas"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeFalse();
        result.Error.Should().Contain("capacidad");
    }

    /// <summary>
    /// ❌ Test: Horario no permitido para reservaciones
    /// </summary>
    [Fact]
    public async Task Handle_HorarioNoPermitido_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaInvalida = DateTime.Now.AddDays(1).Date.AddHours(3); // 3 AM

        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaInvalida,
            NumeroPersonas = 4
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+123456789");
        var mesa = Mesa.Crear(1, 4, TipoMesa.Interior);

        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _validacionServiceMock.Setup(x => x.ValidarHorarioPermitido(fechaInvalida))
            .Returns(Result.Failure("Las reservaciones solo se permiten entre 12:00 PM y 11:00 PM"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeFalse();
        result.Error.Should().Contain("horario");
    }

    /// <summary>
    /// ✅ Test: Notificación de confirmación enviada después de crear reservación
    /// </summary>
    [Fact]
    public async Task Handle_ReservacionCreada_DeberiaEnviarNotificacion()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Now.AddDays(1);
        var reservacionId = Guid.NewGuid();

        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaReservacion,
            NumeroPersonas = 4,
            TelefonoContacto = "+1234567890"
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan.perez@email.com", "+1234567890");
        var mesa = Mesa.Crear(1, 4, TipoMesa.Interior);
        var reservacion = Reservacion.Crear(
            mesaId,
            clienteId,
            fechaReservacion,
            TimeSpan.FromHours(2), // duracionEstimada
            4, // cantidadPersonas
            "+1234567890", // telefono
            "juan.perez@email.com", // email
            "Celebración de cumpleaños"); // observaciones
        reservacion.GetType().GetProperty("Id")?.SetValue(reservacion, reservacionId);

        // Setup successful creation
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _validacionServiceMock.Setup(x => x.ValidarHorarioPermitido(It.IsAny<DateTime>())).Returns(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarCapacidadMesa(It.IsAny<Mesa>(), It.IsAny<int>())).Returns(Result.Success());
        _disponibilidadServiceMock.Setup(x => x.VerificarDisponibilidadAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _reservacionRepositoryMock.Setup(x => x.CrearAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);
        _mapperMock.Setup(x => x.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(new ReservacionDto { Id = reservacionId });
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeTrue();

        // Verify notificación enviada
        _notificacionServiceMock.Verify(x => x.EnviarConfirmacionReservacionAsync(
            It.Is<Guid>(id => id == clienteId),
            It.Is<ReservacionDto>(r => r.Id == reservacionId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ✅ Test: Cliente con múltiples reservaciones en el mismo día (validación de límite)
    /// </summary>
    [Fact]
    public async Task Handle_ClienteConMultiplesReservacionesMismoDia_DeberiaValidarLimite()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Now.AddDays(1);

        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaReservacion,
            NumeroPersonas = 4
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+123456789");
        var mesa = Mesa.Crear(1, 4, TipoMesa.Interior);

        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _validacionServiceMock.Setup(x => x.ValidarHorarioPermitido(It.IsAny<DateTime>())).Returns(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarCapacidadMesa(It.IsAny<Mesa>(), It.IsAny<int>())).Returns(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarLimiteReservacionesPorCliente(clienteId, fechaReservacion.Date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("El cliente ya tiene 2 reservaciones para esta fecha"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeFalse();
        result.Error.Should().Contain("límite");

        // Verify se validó el límite
        _validacionServiceMock.Verify(x => x.ValidarLimiteReservacionesPorCliente(
            clienteId, fechaReservacion.Date, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ✅ Test: Reservación con observaciones especiales
    /// </summary>
    [Fact]
    public async Task Handle_ReservacionConObservacionesEspeciales_DeberiaCrearCorrectamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Now.AddDays(1);
        var observacionesEspeciales = "Cliente con silla de ruedas, necesita mesa accesible";

        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaReservacion,
            NumeroPersonas = 2,
            Observaciones = observacionesEspeciales,
            TelefonoContacto = "+1234567890"
        };

        var cliente = Cliente.Crear("María García", "maria@email.com", "+1234567890");
        var mesa = Mesa.Crear(1, 4, TipoMesa.Accesible); // Mesa accesible
        var reservacionId = Guid.NewGuid();

        // Setup all validations to pass
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _validacionServiceMock.Setup(x => x.ValidarHorarioPermitido(It.IsAny<DateTime>())).Returns(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarCapacidadMesa(It.IsAny<Mesa>(), It.IsAny<int>())).Returns(Result.Success());
        _disponibilidadServiceMock.Setup(x => x.VerificarDisponibilidadAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarLimiteReservacionesPorCliente(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var reservacion = Reservacion.Crear(
            mesaId,
            clienteId,
            fechaReservacion,
            TimeSpan.FromHours(2), // duracionEstimada
            2, // cantidadPersonas - corregido para este test específico
            "+1234567890", // telefono
            "maria@email.com", // email
            observacionesEspeciales); // observaciones
        reservacion.GetType().GetProperty("Id")?.SetValue(reservacion, reservacionId);

        _reservacionRepositoryMock.Setup(x => x.CrearAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacion);
        _mapperMock.Setup(x => x.Map<ReservacionDto>(It.IsAny<Reservacion>()))
            .Returns(new ReservacionDto
            {
                Id = reservacionId,
                Observaciones = observacionesEspeciales,
                NumeroPersonas = 2
            });
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeTrue();
        result.Value.Observaciones.Should().Be(observacionesEspeciales);

        // Verify se creó con las observaciones correctas
        _reservacionRepositoryMock.Verify(x => x.CrearAsync(
            It.Is<Reservacion>(r => r.Observaciones == observacionesEspeciales), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ❌ Test: Error al guardar en base de datos
    /// </summary>
    [Fact]
    public async Task Handle_ErrorBaseDatos_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = DateTime.Now.AddDays(1),
            NumeroPersonas = 4
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+123456789");
        var mesa = Mesa.Crear(1, 4, TipoMesa.Interior);

        // Setup all validations to pass but database to fail
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _validacionServiceMock.Setup(x => x.ValidarHorarioPermitido(It.IsAny<DateTime>())).Returns(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarCapacidadMesa(It.IsAny<Mesa>(), It.IsAny<int>())).Returns(Result.Success());
        _disponibilidadServiceMock.Setup(x => x.VerificarDisponibilidadAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarLimiteReservacionesPorCliente(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conectividad de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess().Should().BeFalse();
        result.Error.Should().Contain("Error");
    }

    /// <summary>
    /// ✅ Test: Validaciones de diferentes tipos de mesa
    /// </summary>
    [Theory]
    [InlineData(TipoMesa.Interior, 4, true)]
    [InlineData(TipoMesa.Terraza, 6, true)]
    [InlineData(TipoMesa.VIP, 2, true)]
    [InlineData(TipoMesa.Accesible, 4, true)]
    public async Task Handle_DiferentesTiposDeMesa_DeberiaValidarCorrectamente(
        TipoMesa tipoMesa, int capacidad, bool esperarExito)
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var fechaReservacion = DateTime.Now.AddDays(1);

        var command = new CrearReservacionCommand
        {
            ClienteId = clienteId,
            MesaId = mesaId,
            FechaHora = fechaReservacion,
            NumeroPersonas = capacidad - 1 // Una persona menos que la capacidad
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+123456789");
        var mesa = Mesa.Crear(1, capacidad, tipoMesa);

        // Setup mocks
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _validacionServiceMock.Setup(x => x.ValidarHorarioPermitido(It.IsAny<DateTime>())).Returns(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarCapacidadMesa(It.IsAny<Mesa>(), It.IsAny<int>())).Returns(Result.Success());
        _disponibilidadServiceMock.Setup(x => x.VerificarDisponibilidadAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _validacionServiceMock.Setup(x => x.ValidarLimiteReservacionesPorCliente(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        if (esperarExito)
        {
            var reservacion = Reservacion.Crear(
                mesaId,
                clienteId,
                fechaReservacion,
                TimeSpan.FromHours(2), // duracionEstimada
                4, // cantidadPersonas
                "+1234567890", // telefono
                "juan.perez@email.com", // email
                "Celebración de cumpleaños"); // observaciones
            _reservacionRepositoryMock.Setup(x => x.CrearAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservacion);
            _mapperMock.Setup(x => x.Map<ReservacionDto>(It.IsAny<Reservacion>()))
                .Returns(new ReservacionDto { Id = Guid.NewGuid() });
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        }

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (esperarExito)
        {
            result.IsSuccess().Should().BeTrue();
        }
        else
        {
            result.IsSuccess().Should().BeFalse();
        }
    }
} 