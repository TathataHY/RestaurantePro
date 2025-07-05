namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Commands;

public class ActualizarClienteHandlerTests
{
    private readonly Mock<IClienteRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ActualizarClienteHandler>> _mockLogger;
    private readonly ActualizarClienteHandler _handler;

    public ActualizarClienteHandlerTests()
    {
        _mockRepository = new Mock<IClienteRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ActualizarClienteHandler>>();
        _handler = new ActualizarClienteHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ActualizarEmail_DeberiaActualizarExitosamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new ActualizarClienteCommand(clienteId)
        {
            Email = "nuevo.email@test.com"
        };

        var clienteExistente = Cliente.Crear(
            ClienteNombre.Crear("Juan", "Pérez"),
            "email.viejo@test.com",
            "+57300123456",
            DateTime.Now.AddYears(-30)
        );

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Email = "nuevo.email@test.com",
            Nombre = "Juan",
            Apellido = "Pérez"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        _mockRepository.Setup(r => r.ObtenerPorEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        _mockRepository.Setup(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ClienteDto>(It.IsAny<Cliente>()))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Email.Should().Be("nuevo.email@test.com");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.ObtenerPorEmailAsync(command.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ActualizarTelefono_DeberiaActualizarExitosamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new ActualizarClienteCommand(clienteId)
        {
            Telefono = "+57300999888"
        };

        var clienteExistente = Cliente.Crear(
            ClienteNombre.Crear("María", "García"),
            "maria@test.com",
            "+57300123456",
            DateTime.Now.AddYears(-25)
        );

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Telefono = "+57300999888"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        _mockRepository.Setup(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ClienteDto>(It.IsAny<Cliente>()))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Telefono.Should().Be("+57300999888");

        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new ActualizarClienteCommand(clienteId)
        {
            Email = "nuevo@test.com"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be($"No se encontró el cliente con ID {clienteId}");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailYaExisteEnOtroCliente_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var otroClienteId = Guid.NewGuid();
        var emailDuplicado = "email.duplicado@test.com";

        var command = new ActualizarClienteCommand(clienteId)
        {
            Email = emailDuplicado
        };

        var clienteExistente = Cliente.Crear(
            ClienteNombre.Crear("Juan", "Pérez"),
            "juan@test.com",
            "+57300123456",
            DateTime.Now.AddYears(-30)
        );

        var otroCliente = Cliente.Crear(
            ClienteNombre.Crear("María", "García"),
            emailDuplicado,
            "+57300987654",
            DateTime.Now.AddYears(-25)
        );

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        _mockRepository.Setup(r => r.ObtenerPorEmailAsync(emailDuplicado, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otroCliente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be($"Ya existe otro cliente registrado con el email {emailDuplicado}");

        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActivarCliente_DeberiaReactivarExitosamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new ActualizarClienteCommand(clienteId)
        {
            EstaActivo = true
        };

        var clienteInactivo = Cliente.Crear(
            ClienteNombre.Crear("Ana", "López"),
            "ana@test.com",
            "+57300555444",
            DateTime.Now.AddYears(-28)
        );
        clienteInactivo.Desactivar(); // Cliente inactivo

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Activo = true
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteInactivo);

        _mockRepository.Setup(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ClienteDto>(It.IsAny<Cliente>()))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Activo.Should().BeTrue();

        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DesactivarCliente_DeberiaDesactivarExitosamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new ActualizarClienteCommand(clienteId)
        {
            EstaActivo = false
        };

        var clienteActivo = Cliente.Crear(
            ClienteNombre.Crear("Carlos", "Rodríguez"),
            "carlos@test.com",
            "+57300777666",
            DateTime.Now.AddYears(-35)
        );

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Activo = false
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteActivo);

        _mockRepository.Setup(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ClienteDto>(It.IsAny<Cliente>()))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Activo.Should().BeFalse();

        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ActualizarEmailYTelefono_DeberiaActualizarAmbos()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new ActualizarClienteCommand(clienteId)
        {
            Email = "nuevo@test.com",
            Telefono = "+57300111222"
        };

        var clienteExistente = Cliente.Crear(
            ClienteNombre.Crear("Pedro", "Martínez"),
            "pedro@test.com",
            "+57300333444",
            DateTime.Now.AddYears(-40)
        );

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Email = "nuevo@test.com",
            Telefono = "+57300111222"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        _mockRepository.Setup(r => r.ObtenerPorEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        _mockRepository.Setup(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ClienteDto>(It.IsAny<Cliente>()))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Email.Should().Be("nuevo@test.com");
        result.Value.Telefono.Should().Be("+57300111222");
    }

    [Fact]
    public async Task Handle_ExcepcionBusinessRule_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new ActualizarClienteCommand(clienteId)
        {
            Email = "test@test.com"
        };

        var clienteExistente = Cliente.Crear(
            ClienteNombre.Crear("Test", "User"),
            "test.viejo@test.com",
            "+57300123456",
            DateTime.Now.AddYears(-30)
        );

        var businessException = new BusinessRuleViolationException("TestRule", "Cliente", "Comercial");

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        _mockRepository.Setup(r => r.ObtenerPorEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        _mockRepository.Setup(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(businessException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Se violó la regla de negocio 'TestRule' en Cliente");
    }

    [Fact]
    public async Task Handle_ExcepcionGeneral_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var command = new ActualizarClienteCommand(clienteId)
        {
            Telefono = "+57300999888"
        };

        var excepcionGeneral = new Exception("Error de base de datos");

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionGeneral);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al actualizar el cliente");
    }
} 