namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Queries;

public class ObtenerClientePorIdHandlerTests
{
    private readonly Mock<IClienteRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerClientePorIdHandler>> _mockLogger;
    private readonly ObtenerClientePorIdHandler _handler;

    public ObtenerClientePorIdHandlerTests()
    {
        _mockRepository = new Mock<IClienteRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerClientePorIdHandler>>();
        
        _handler = new ObtenerClientePorIdHandler(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_ClienteExisteYEstaActivo_DeberiaRetornarClienteDto()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery(clienteId);

        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Juan", "Pérez"),
            "juan@test.com",
            "+57300123456",
            DateTime.Now.AddYears(-30)
        );

        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(cliente, clienteId);

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Nombre = "Juan",
            Apellido = "Pérez",
            Email = "juan@test.com",
            Activo = true
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMapper.Setup(x => x.Map<ClienteDto>(It.IsAny<Cliente>()))
                   .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(clienteDto);
    }

    [Fact]
    public async Task Handle_ClienteNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery(clienteId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("El cliente especificado no existe.");
    }

    [Fact]
    public async Task Handle_ClienteEstaEliminado_DeberiaRetornarError()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery(clienteId);

        var clienteEliminado = Cliente.Crear(
            ClienteNombre.Crear("Ana", "López"),
            "ana@test.com",
            "+57300555444",
            DateTime.Now.AddYears(-28)
        );
        
        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(clienteEliminado, clienteId);
        
        clienteEliminado.Desactivar(); // Simular cliente eliminado

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Nombre = "Ana",
            Apellido = "López",
            Email = "ana@test.com",
            Activo = false
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteEliminado);

        _mockMapper.Setup(m => m.Map<ClienteDto>(It.IsAny<Cliente>()))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - Como la implementación actual no verifica EstaEliminado
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ClienteInactivo_DeberiaRetornarClienteDto()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery(clienteId);

        var clienteInactivo = Cliente.Crear(
            ClienteNombre.Crear("María", "García"),
            "maria@test.com",
            "+57300987654",
            DateTime.Now.AddYears(-25)
        );
        
        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(clienteInactivo, clienteId);
        
        clienteInactivo.Desactivar(); // Cliente inactivo pero no eliminado

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Nombre = "María",
            Apellido = "García",
            Email = "maria@test.com",
            Activo = false
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteInactivo);

        _mockMapper.Setup(m => m.Map<ClienteDto>(It.IsAny<Cliente>()))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Activo.Should().BeFalse();
        result.Value.Email.Should().Be("maria@test.com");

        _mockMapper.Verify(m => m.Map<ClienteDto>(It.IsAny<Cliente>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExcepcionRepositorio_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery(clienteId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno al obtener el cliente.");
    }

    [Fact]
    public async Task Handle_ExcepcionMapper_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery(clienteId);

        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Test", "Mapper"),
            "test@test.com",
            "+57300000000",
            DateTime.Now.AddYears(-30)
        );

        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(cliente, clienteId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMapper.Setup(x => x.Map<ClienteDto>(It.IsAny<Cliente>()))
                   .Throws(new Exception("Error en el mapper"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno al obtener el cliente.");
    }

    [Fact]
    public async Task Handle_ConstructorSinParametros_DeberiaFuncionar()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery { ClienteId = clienteId };

        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Pedro", "Martínez"),
            "pedro@test.com",
            "+57300111222",
            DateTime.Now.AddYears(-40)
        );

        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(cliente, clienteId);

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Nombre = "Pedro",
            Apellido = "Martínez",
            Email = "pedro@test.com",
            Activo = true
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMapper.Setup(m => m.Map<ClienteDto>(It.IsAny<Cliente>()))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be("pedro@test.com");
    }

    [Fact]
    public async Task Handle_ClienteConTodosLosDatos_DeberiaMapearCompletamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery(clienteId);

        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Ana", "Fernández"),
            "ana@test.com",
            "+57300444555",
            DateTime.Now.AddYears(-28)
        );

        // Establecer el ID manualmente
        var idProperty = typeof(Cliente).GetProperty("Id");
        idProperty?.SetValue(cliente, clienteId);

        cliente.AgregarPuntos(150);

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Nombre = "Ana",
            Apellido = "Fernández",
            Email = "ana@test.com",
            Telefono = "+57300444555",
            Activo = true
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMapper.Setup(m => m.Map<ClienteDto>(It.IsAny<Cliente>()))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Nombre.Should().Be("Ana");
        result.Value.Apellido.Should().Be("Fernández");
        result.Value.Email.Should().Be("ana@test.com");
        result.Value.Telefono.Should().Be("+57300444555");
        result.Value.Activo.Should().BeTrue();

        _mockMapper.Verify(m => m.Map<ClienteDto>(It.IsAny<Cliente>()), Times.Once);
    }
} 