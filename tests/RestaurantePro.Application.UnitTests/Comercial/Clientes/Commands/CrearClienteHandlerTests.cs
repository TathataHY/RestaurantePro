namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Commands;

public class CrearClienteHandlerTests
{
    private readonly Mock<IClienteRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CrearClienteHandler>> _mockLogger;
    private readonly CrearClienteHandler _handler;

    public CrearClienteHandlerTests()
    {
        _mockRepository = new Mock<IClienteRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CrearClienteHandler>>();
        _handler = new CrearClienteHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ComandoValido_DeberiaCrearClienteExitosamente()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "juan.perez@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25),
            EstaActivo = true
        };

        var clienteDto = new ClienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Juan Pérez",
            Email = "juan.perez@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = command.FechaNacimiento,
            Activo = true
        };

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
        result.Value.Should().NotBeNull();
        result.Value.Nombre.Should().Be("Juan Pérez");
        result.Value.Email.Should().Be("juan.perez@email.com");

        _mockRepository.Verify(r => r.ObtenerPorEmailAsync(command.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<ClienteDto>(It.IsAny<Cliente>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailYaExiste_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "juan.perez@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        var clienteExistente = Cliente.Crear(
            ClienteNombre.Crear("Cliente", "Existente"),
            "juan.perez@email.com",
            "+57300123456",
            DateTime.Now.AddYears(-30)
        );

        _mockRepository.Setup(r => r.ObtenerPorEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain($"Ya existe un cliente registrado con el email {command.Email}");

        _mockRepository.Verify(r => r.ObtenerPorEmailAsync(command.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ClienteInactivo_DeberiaCrearClienteDesactivado()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "María García",
            Email = "maria.garcia@email.com",
            Telefono = "+57300987654",
            FechaNacimiento = DateTime.Now.AddYears(-30),
            EstaActivo = false
        };

        var clienteDto = new ClienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = "María García",
            Email = "maria.garcia@email.com",
            Activo = false
        };

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
        result.Value.Activo.Should().BeFalse();

        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExcepcionBusinessRule_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Test Cliente",
            Email = "test@email.com",
            Telefono = "+57300111222",
            FechaNacimiento = DateTime.Now.AddYears(-20)
        };

        var businessException = new BusinessRuleViolationException("TestRule", "Cliente", "Comercial");

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
        var command = new CrearClienteCommand
        {
            Nombre = "Test Cliente",
            Email = "test@email.com",
            Telefono = "+57300111222",
            FechaNacimiento = DateTime.Now.AddYears(-20)
        };

        var excepcionGeneral = new Exception("Error de base de datos");

        _mockRepository.Setup(r => r.ObtenerPorEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionGeneral);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al crear el cliente");
    }

    [Fact]
    public async Task Handle_NombreConEspacios_DeberiaManejaNombreYApellidoCorrectamente()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Ana María Rodríguez López",
            Email = "ana.maria@email.com",
            Telefono = "+57300555666",
            FechaNacimiento = DateTime.Now.AddYears(-28)
        };

        var clienteDto = new ClienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = command.Nombre,
            Email = command.Email
        };

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
        result.Value.Nombre.Should().Be("Ana María Rodríguez López");

        _mockRepository.Verify(r => r.GuardarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
    }
} 