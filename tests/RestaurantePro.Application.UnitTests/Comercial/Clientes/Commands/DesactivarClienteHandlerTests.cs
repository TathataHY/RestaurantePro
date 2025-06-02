namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Commands;

/// <summary>
/// 🚫 Tests para DesactivarClienteHandler
/// Validaciones empresariales de desactivación, cancelación de reservaciones y auditoría
/// </summary>
public class DesactivarClienteHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IReservacionRepository> _reservacionRepositoryMock;
    private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<ICommunicationService> _notificacionServiceMock;
    private readonly Mock<IAuditingService> _auditingServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<DesactivarClienteHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<IClienteBusinessService> _clienteBusinessServiceMock;
    private readonly DesactivarClienteHandler _handler;

    public DesactivarClienteHandlerTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _reservacionRepositoryMock = new Mock<IReservacionRepository>();
        _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _notificacionServiceMock = new Mock<ICommunicationService>();
        _auditingServiceMock = new Mock<IAuditingService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<DesactivarClienteHandler>>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _clienteBusinessServiceMock = new Mock<IClienteBusinessService>();

        _handler = new DesactivarClienteHandler(
            _clienteRepositoryMock.Object,
            _reservacionRepositoryMock.Object,
            _tarjetaRepositoryMock.Object,
            _comandaRepositoryMock.Object,
            _notificacionServiceMock.Object,
            _auditingServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _currentUserMock.Object,
            _dateTimeServiceMock.Object,
            _clienteBusinessServiceMock.Object);
    }

    /// <summary>
    /// ✅ Test: Desactivar cliente exitosamente sin reservaciones pendientes
    /// </summary>
    [Fact]
    public async Task Handle_DesactivarClienteSinReservaciones_DeberiaRetornarSuccess()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioActualId = Guid.NewGuid();
        var motivo = "Cliente solicitó baja por cambio de ciudad";

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = true
        };

        var cliente = Cliente.Crear(
            "Juan Pérez",
            "juan.perez@email.com",
            "+1234567890");
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        var fechaActual = DateTime.Now;

        // Setup mocks
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioActualId);
        _dateTimeServiceMock.Setup(x => x.Now).Returns(fechaActual);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _reservacionRepositoryMock.Setup(x => x.ObtenerReservacionesPendientesAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservacion>());
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comanda>());
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify cliente fue desactivado
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<Cliente>(c => c.Id == clienteId && !c.EstaActivo), 
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify auditoría registrada
        _auditingServiceMock.Verify(x => x.RegistrarDesactivacionClienteAsync(
            clienteId, usuarioActualId, motivo, fechaActual, It.IsAny<CancellationToken>()), Times.Once);

        // Verify notificación enviada
        _notificacionServiceMock.Verify(x => x.EnviarNotificacionDesactivacionClienteAsync(
            clienteId, motivo, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ❌ Test: Cliente no encontrado
    /// </summary>
    [Fact]
    public async Task Handle_ClienteNoEncontrado_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = "Cliente solicitó baja"
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cliente no encontrado");

        // Verify no se realizaron operaciones
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
        _auditingServiceMock.Verify(x => x.RegistrarDesactivacionClienteAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ❌ Test: Cliente ya está desactivado
    /// </summary>
    [Fact]
    public async Task Handle_ClienteYaDesactivado_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = "Cliente solicitó baja"
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+1234567890");
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);
        cliente.Desactivar("Ya estaba desactivado");

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("ya está desactivado");

        // Verify no se realizó actualización
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ✅ Test: Cliente con reservaciones pendientes - cancelación automática
    /// </summary>
    [Fact]
    public async Task Handle_ClienteConReservacionesPendientes_DeberiaCancelarAutomaticamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioActualId = Guid.NewGuid();
        var motivo = "Cliente incumplió términos de servicio";

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = true
        };

        var cliente = Cliente.Crear("María García", "maria@email.com", "+1234567890");
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        var reservacion1 = Reservacion.Crear(
            clienteId, 
            Guid.NewGuid(), 
            DateTime.Now.AddDays(3), 
            4, 
            "Cena familiar", 
            "+1234567890");
        var reservacion2 = Reservacion.Crear(
            clienteId, 
            Guid.NewGuid(), 
            DateTime.Now.AddDays(7), 
            2, 
            "Cita de negocios", 
            "+1234567890");

        var reservacionesPendientes = new List<Reservacion> { reservacion1, reservacion2 };

        // Setup mocks
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioActualId);
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _reservacionRepositoryMock.Setup(x => x.ObtenerReservacionesPendientesAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacionesPendientes);
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comanda>());
        _reservacionRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify reservaciones fueron canceladas
        _reservacionRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<Reservacion>(r => r.Estado == EstadoReservacion.Cancelada), 
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Verify notificación de cancelaciones enviada
        _notificacionServiceMock.Verify(x => x.EnviarNotificacionCancelacionReservacionesAsync(
            clienteId, It.Is<List<Guid>>(ids => ids.Count == 2), It.IsAny<CancellationToken>()), Times.Once);

        // Verify cliente desactivado
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<Cliente>(c => !c.EstaActivo), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ❌ Test: Cliente con reservaciones pendientes sin autorización para cancelar
    /// </summary>
    [Fact]
    public async Task Handle_ClienteConReservacionesSinAutorizacion_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = "Prueba de desactivación",
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+1234567890");
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        var reservacionesPendientes = new List<Reservacion>
        {
            Reservacion.Crear(clienteId, Guid.NewGuid(), DateTime.Now.AddDays(3), 4, "", "+1234567890")
        };

        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _reservacionRepositoryMock.Setup(x => x.ObtenerReservacionesPendientesAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservacionesPendientes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("reservaciones pendientes");

        // Verify no se desactivó el cliente
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ✅ Test: Cliente con comandas activas - cierre automático
    /// </summary>
    [Fact]
    public async Task Handle_ClienteConComandasActivas_DeberiaCerrarAutomaticamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioActualId = Guid.NewGuid();
        var motivo = "Cliente con comportamiento inapropiado";

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = false // No notificar por comportamiento
        };

        var cliente = Cliente.Crear("Cliente Problema", "problema@email.com", "+1234567890");
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        var comanda1 = Comanda.Crear(Guid.NewGuid(), clienteId, Guid.NewGuid(), "Observaciones");
        var comanda2 = Comanda.Crear(Guid.NewGuid(), clienteId, Guid.NewGuid(), "Observaciones 2");

        var comandasActivas = new List<Comanda> { comanda1, comanda2 };

        // Setup mocks
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioActualId);
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _reservacionRepositoryMock.Setup(x => x.ObtenerReservacionesPendientesAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservacion>());
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comandasActivas);
        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify comandas fueron cerradas
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<Comanda>(c => c.Estado == EstadoComanda.Finalizada), 
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        // Verify no se envió notificación al cliente
        _notificacionServiceMock.Verify(x => x.EnviarNotificacionDesactivacionClienteAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ✅ Test: Suspender tarjeta de fidelización al desactivar cliente
    /// </summary>
    [Fact]
    public async Task Handle_ClienteConTarjetaFidelizacion_DeberiaSuspenderTarjeta()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var tarjetaId = Guid.NewGuid();
        var motivo = "Cliente cambió de ciudad";

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = true
        };

        var cliente = Cliente.Crear("Ana López", "ana@email.com", "+1234567890");
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        var tarjeta = TarjetaFidelizacion.Crear(clienteId, "FIDELIDAD001");
        tarjeta.GetType().GetProperty("Id")?.SetValue(tarjeta, tarjetaId);

        // Setup mocks
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _reservacionRepositoryMock.Setup(x => x.ObtenerReservacionesPendientesAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservacion>());
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comanda>());
        _tarjetaRepositoryMock.Setup(x => x.ObtenerPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tarjeta);
        _tarjetaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify tarjeta fue suspendida
        _tarjetaRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<TarjetaFidelizacion>(t => t.Estado == EstadoTarjeta.Suspendida), 
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify notificación de suspensión enviada
        _notificacionServiceMock.Verify(x => x.EnviarNotificacionSuspensionTarjetaAsync(
            clienteId, tarjetaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ✅ Test: Validación de autorización para desactivar cliente
    /// </summary>
    [Fact]
    public async Task Handle_UsuarioSinAutorizacion_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioMesero = Guid.NewGuid();

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = "Intento de desactivación sin autorización"
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+1234567890");

        _currentUserMock.Setup(x => x.UserId).Returns(usuarioMesero);
        _currentUserMock.Setup(x => x.Rol).Returns(RolUsuario.Mesero.ToString()); // Mesero no puede desactivar clientes
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("autorización");

        // Verify no se realizaron cambios
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ✅ Test: Administrador puede desactivar cualquier cliente
    /// </summary>
    [Fact]
    public async Task Handle_UsuarioAdministrador_DeberiaPermitirDesactivacion()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var usuarioAdmin = Guid.NewGuid();
        var motivo = "Desactivación administrativa";

        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = true
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+1234567890");
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        // Setup mocks para administrador
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioAdmin);
        _currentUserMock.Setup(x => x.Rol).Returns(RolUsuario.Administrador.ToString());
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _reservacionRepositoryMock.Setup(x => x.ObtenerReservacionesPendientesAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservacion>());
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comanda>());
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify cliente fue desactivado por admin
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(
            It.Is<Cliente>(c => !c.EstaActivo), It.IsAny<CancellationToken>()), Times.Once);

        // Verify auditoría registrada con usuario admin
        _auditingServiceMock.Verify(x => x.RegistrarDesactivacionClienteAsync(
            clienteId, usuarioAdmin, motivo, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ❌ Test: Error al guardar en base de datos
    /// </summary>
    [Fact]
    public async Task Handle_ErrorBaseDatos_DeberiaRetornarFailure()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = "Desactivación de prueba"
        };

        var cliente = Cliente.Crear("Juan Pérez", "juan@email.com", "+1234567890");

        // Setup validaciones exitosas pero error en base de datos
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.Rol).Returns(RolUsuario.Administrador.ToString());
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _reservacionRepositoryMock.Setup(x => x.ObtenerReservacionesPendientesAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservacion>());
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comanda>());
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conectividad de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error");
    }

    /// <summary>
    /// ✅ Test: Desactivación con motivos específicos del negocio
    /// </summary>
    [Theory]
    [InlineData("Cliente mudó de ciudad", true, "🏠 Cambio de residencia")]
    [InlineData("Comportamiento inapropiado", false, "⚠️ Violación de políticas")]
    [InlineData("Solicitud del cliente", true, "✅ Solicitud voluntaria")]
    [InlineData("Incumplimiento de pagos", false, "💳 Problemas financieros")]
    public async Task Handle_MotivoEspecifico_DeberiaCategorizarCorrectamente(
        string motivo, bool notificarCliente, string categoriaEsperada)
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            NotificarCliente = notificarCliente
        };

        var cliente = Cliente.Crear("Cliente Test", "test@email.com", "+1234567890");
        cliente.GetType().GetProperty("Id")?.SetValue(cliente, clienteId);

        // Setup mocks para caso exitoso
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.Rol).Returns(RolUsuario.Administrador.ToString());
        _dateTimeServiceMock.Setup(x => x.Now).Returns(DateTime.Now);
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _reservacionRepositoryMock.Setup(x => x.ObtenerReservacionesPendientesAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservacion>());
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comanda>());
        _clienteBusinessServiceMock.Setup(x => x.CategorizarMotivoDesactivacion(motivo))
            .Returns(categoriaEsperada);
        _clienteRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify categorización del motivo
        _clienteBusinessServiceMock.Verify(x => x.CategorizarMotivoDesactivacion(motivo), Times.Once);

        // Verify notificación según configuración
        if (notificarCliente)
        {
            _notificacionServiceMock.Verify(x => x.EnviarNotificacionDesactivacionClienteAsync(
                clienteId, motivo, It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            _notificacionServiceMock.Verify(x => x.EnviarNotificacionDesactivacionClienteAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    /// <summary>
    /// ✅ Test: Rollback automático en caso de fallo parcial
    /// </summary>
    [Fact]
    public async Task Handle_FalloEnCancelacionReservaciones_DeberiaHacerRollback()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = "Prueba de rollback",
        };

        var cliente = Cliente.Crear("Cliente Rollback", "rollback@email.com", "+1234567890");
        var reservacion = Reservacion.Crear(clienteId, Guid.NewGuid(), DateTime.Now.AddDays(1), 4, "", "+123456789");

        // Setup mocks para fallo en cancelación de reservaciones
        _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUserMock.Setup(x => x.Rol).Returns(RolUsuario.Administrador.ToString());
        _clienteRepositoryMock.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _reservacionRepositoryMock.Setup(x => x.ObtenerReservacionesPendientesAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reservacion> { reservacion });
        _comandaRepositoryMock.Setup(x => x.ObtenerComandasActivasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comanda>());
        _reservacionRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Reservacion>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error al cancelar reservación"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error");

        // Verify no se desactivó el cliente
        _clienteRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);

        // Verify no se guardaron cambios
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
} 