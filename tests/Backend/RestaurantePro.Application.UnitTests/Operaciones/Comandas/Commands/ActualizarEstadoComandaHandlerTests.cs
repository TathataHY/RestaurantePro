namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Commands;

public class ActualizarEstadoComandaHandlerTests
{
    private readonly Mock<IComandaRepository> _mockComandaRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ActualizarEstadoComandaHandler>> _mockLogger;
    private readonly ActualizarEstadoComandaHandler _handler;

    public ActualizarEstadoComandaHandlerTests()
    {
        _mockComandaRepository = new Mock<IComandaRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ActualizarEstadoComandaHandler>>();
        
        _handler = new ActualizarEstadoComandaHandler(
            _mockComandaRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            NuevoEstado = "EnProceso",
            UsuarioId = Guid.NewGuid()
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comanda?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("No se encontró una comanda");
    }

    [Fact]
    public async Task Handle_EstadoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");

        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "EstadoInexistente",
            UsuarioId = usuarioId
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("no es válido");
    }

    [Fact]
    public async Task Handle_MismoEstado_DeberiaRetornarExitoSinCambios()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");
        
        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "Creada", // Mismo estado actual
            UsuarioId = usuarioId
        };

        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            Estado = EstadoComanda.Creada,
            UsuarioId = usuarioId
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockMapper.Setup(m => m.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Estado.Should().Be(EstadoComanda.Creada);

        // No debería llamar a ActualizarAsync
        _mockComandaRepository.Verify(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CambiarAEnProceso_DeberiaActualizarEstadoExitosamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");
        comanda.AgregarItem(Guid.NewGuid(), "Item de prueba", 1, 10000m);

        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "EnProceso",
            UsuarioId = usuarioId
        };

        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            Estado = EstadoComanda.EnProceso,
            UsuarioId = usuarioId
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockComandaRepository.Setup(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Estado.Should().Be(EstadoComanda.EnProceso);

        _mockComandaRepository.Verify(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockComandaRepository.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CambiarALista_DeberiaActualizarEstadoExitosamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");
        comanda.AgregarItem(Guid.NewGuid(), "Item de prueba", 1, 10000m);
        comanda.MarcarEnPreparacion(); // Cambiar a EnProceso primero

        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "Lista",
            UsuarioId = usuarioId
        };

        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            Estado = EstadoComanda.Lista,
            UsuarioId = usuarioId
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockComandaRepository.Setup(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Estado.Should().Be(EstadoComanda.Lista);
    }

    [Fact]
    public async Task Handle_CambiarAEntregada_DeberiaActualizarEstadoExitosamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");
        comanda.AgregarItem(Guid.NewGuid(), "Item de prueba", 1, 10000m);
        comanda.MarcarEnPreparacion(); 
        comanda.MarcarLista(); // Cambiar a Lista primero

        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "Entregada",
            UsuarioId = usuarioId
        };

        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            Estado = EstadoComanda.Entregada,
            UsuarioId = usuarioId
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockComandaRepository.Setup(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Estado.Should().Be(EstadoComanda.Entregada);
    }

    [Fact]
    public async Task Handle_CambiarAFinalizada_DeberiaActualizarEstadoExitosamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");
        comanda.AgregarItem(Guid.NewGuid(), "Item de prueba", 1, 10000m);
        comanda.MarcarEnPreparacion(); 
        comanda.MarcarLista(); 
        comanda.MarcarEntregada(); // Cambiar a Entregada primero

        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "Finalizada",
            UsuarioId = usuarioId
        };

        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            Estado = EstadoComanda.Finalizada,
            UsuarioId = usuarioId
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockComandaRepository.Setup(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Estado.Should().Be(EstadoComanda.Finalizada);
    }

    [Fact]
    public async Task Handle_CambiarACancelada_DeberiaActualizarEstadoExitosamente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");

        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "Cancelada",
            UsuarioId = usuarioId,
            Observaciones = "Cliente canceló el pedido"
        };

        var comandaDto = new ComandaDto
        {
            Id = comanda.Id,
            Estado = EstadoComanda.Cancelada,
            UsuarioId = usuarioId,
            Observaciones = "Cliente canceló el pedido"
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockComandaRepository.Setup(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        _mockMapper.Setup(m => m.Map<ComandaDto>(It.IsAny<Comanda>()))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Estado.Should().Be(EstadoComanda.Cancelada);
    }

    [Fact]
    public async Task Handle_TransicionInvalida_DeberiaRetornarError()
    {
        // Arrange - Intentar ir de Creada directamente a Lista (saltándose EnProceso)
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");

        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "Lista", // Transición inválida desde Creada
            UsuarioId = usuarioId
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("No se puede marcar la comanda como lista");

        _mockComandaRepository.Verify(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BusinessRuleViolation_DeberiaRetornarError()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");

        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "EnProceso",
            UsuarioId = usuarioId
        };

        var businessException = new BusinessRuleViolationException("ComandaSinItems", "Comanda", "Operaciones");

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockComandaRepository.Setup(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(businessException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(businessException.Message);
    }

    [Fact]
    public async Task Handle_ErrorGeneral_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            NuevoEstado = "EnProceso",
            UsuarioId = Guid.NewGuid()
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conexión a base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al actualizar el estado de la comanda");
    }

    [Fact]
    public async Task Handle_ErrorEnGuardarCambios_DeberiaRetornarError()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Comanda de prueba");
        comanda.AgregarItem(Guid.NewGuid(), "Item de prueba", 1, 10000m);

        var command = new ActualizarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "EnProceso",
            UsuarioId = usuarioId
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(command.ComandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockComandaRepository.Setup(r => r.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockComandaRepository.Setup(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error al guardar en base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al actualizar el estado de la comanda");
    }
} 