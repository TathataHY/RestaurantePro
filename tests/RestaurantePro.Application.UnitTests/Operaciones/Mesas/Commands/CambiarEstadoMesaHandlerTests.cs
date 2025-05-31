namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Commands;

public class CambiarEstadoMesaHandlerTests
{
    private readonly Mock<IMesaRepository> _mockMesaRepository;
    private readonly Mock<ILogger<CambiarEstadoMesaHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly CambiarEstadoMesaHandler _handler;

    public CambiarEstadoMesaHandlerTests()
    {
        _mockMesaRepository = new Mock<IMesaRepository>();
        _mockLogger = new Mock<ILogger<CambiarEstadoMesaHandler>>();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        
        _mockCurrentUser.Setup(x => x.UserId).Returns("test-user-id");
        
        _handler = new CambiarEstadoMesaHandler(
            _mockMesaRepository.Object,
            _mockLogger.Object,
            _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_CambiarADisponible_DeberiaCambiarEstadoCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        mesa.MarcarComoOcupada(); // Estado inicial: Ocupada
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Disponible);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Disponible);
        
        _mockMesaRepository.Verify(x => x.ActualizarAsync(mesa), Times.Once);
        _mockMesaRepository.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CambiarAOcupada_DeberiaCambiarEstadoCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(2, 6, "Interior");
        // Estado inicial: Disponible
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Ocupada);
    }

    [Fact]
    public async Task Handle_CambiarAReservada_DeberiaCambiarEstadoCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(3, 8, "VIP");
        // Estado inicial: Disponible
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Reservada);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Reservada);
    }

    [Fact]
    public async Task Handle_CambiarAFueraDeServicio_ConMotivo_DeberiaCambiarEstadoCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(4, 4, "Terraza");
        var motivo = "Mesa dañada - reparación necesaria";
        var command = CambiarEstadoMesaCommand.MarcarFueraDeServicio(mesaId, motivo);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.FueraDeServicio);
    }

    [Fact]
    public async Task Handle_ConMesaNoEncontrada_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync((Mesa?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Mesa no encontrada");
        
        _mockMesaRepository.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Never);
        _mockMesaRepository.Verify(x => x.GuardarCambiosAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ConTransicionInvalida_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        mesa.MarcarComoOcupada(); // Mesa ocupada
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Reservada); // Transición inválida

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("no puede marcarse como reservada");
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al cambiar el estado de la mesa");
    }

    [Fact]
    public async Task Handle_ConErrorAlGuardar_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);
        _mockMesaRepository.Setup(x => x.GuardarCambiosAsync())
            .ThrowsAsync(new Exception("Error al guardar"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al cambiar el estado de la mesa");
    }

    [Fact]
    public async Task Handle_ConMotivo_DeberiaLoggearMotivo()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var motivo = "Limpieza profunda programada";
        var command = CambiarEstadoMesaCommand.MarcarFueraDeServicio(mesaId, motivo);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que se loggeó el motivo
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(motivo)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConObservaciones_DeberiaLoggearObservaciones()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var observaciones = "Cambio solicitado por gerente de turno";
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada, observaciones: observaciones);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que se loggearon las observaciones
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(observaciones)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(EstadoMesa.Disponible)]
    [InlineData(EstadoMesa.Ocupada)]
    [InlineData(EstadoMesa.Reservada)]
    public async Task Handle_ConEstadosValidos_DeberiaCambiarCorrectamente(EstadoMesa nuevoEstado)
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, nuevoEstado);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        if (nuevoEstado == EstadoMesa.Ocupada || nuevoEstado == EstadoMesa.Reservada)
        {
            // Estas transiciones son válidas desde Disponible
            result.Succeeded.Should().BeTrue();
            mesa.Estado.Should().Be(nuevoEstado);
        }
        else if (nuevoEstado == EstadoMesa.Disponible)
        {
            // Esta transición es válida (mesa ya está disponible)
            result.Succeeded.Should().BeTrue();
            mesa.Estado.Should().Be(EstadoMesa.Disponible);
        }
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(5, 6, "Salón Principal");
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar logging de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando cambio de estado de mesa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("cambió de estado correctamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConMesaNoEncontrada_DeberiLoggearWarning()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync((Mesa?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mesa") && v.ToString()!.Contains("no encontrada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcion_DeberiLoggearError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ThrowsAsync(new Exception("Error crítico"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error interno al cambiar estado de mesa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConUsuarioId_DeberiaCambiarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada, usuarioId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Ocupada);
    }

    [Theory]
    [InlineData("Interior", 2, 4)]
    [InlineData("Terraza", 5, 6)]
    [InlineData("VIP", 10, 8)]
    [InlineData("Barra", 1, 2)]
    public async Task Handle_ConDiferentesMesas_DeberiaCambiarCorrectamente(string ubicacion, int numero, int capacidad)
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(numero, capacidad, ubicacion);
        var command = CambiarEstadoMesaCommand.CambiarA(mesaId, EstadoMesa.Ocupada);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Ocupada);
        mesa.Ubicacion.Should().Be(ubicacion);
        mesa.Numero.Should().Be(numero);
        mesa.Capacidad.Should().Be(capacidad);
    }

    [Fact]
    public async Task Handle_ConFactoryMethodMarcarFueraDeServicio_DeberiaCambiarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var motivo = "Mantenimiento preventivo";
        var usuarioId = Guid.NewGuid();
        var observaciones = "Programado para esta semana";
        var command = CambiarEstadoMesaCommand.MarcarFueraDeServicio(mesaId, motivo, usuarioId, observaciones);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.FueraDeServicio);
        command.Motivo.Should().Be(motivo);
        command.UsuarioId.Should().Be(usuarioId);
        command.Observaciones.Should().Be(observaciones);
    }
} 