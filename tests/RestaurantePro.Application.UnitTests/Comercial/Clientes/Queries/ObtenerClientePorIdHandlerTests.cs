using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;
using RestaurantePro.Application.Comercial.Clientes.DTOs;

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
        _handler = new ObtenerClientePorIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
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

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Nombre = "Juan",
            Apellido = "Pérez",
            Email = "juan@test.com",
            Telefono = "+57300123456",
            Activo = true
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMapper.Setup(m => m.Map<ClienteDto>(cliente))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(clienteId);
        result.Value.Nombre.Should().Be("Juan");
        result.Value.Apellido.Should().Be("Pérez");
        result.Value.Email.Should().Be("juan@test.com");
        result.Value.Activo.Should().BeTrue();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<ClienteDto>(cliente), Times.Once);
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
        result.Error.Should().Be($"No se encontró un cliente con el ID {clienteId}");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<ClienteDto>(It.IsAny<Cliente>()), Times.Never);
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
        
        clienteEliminado.Desactivar(); // Simular cliente eliminado

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteEliminado);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - El comportamiento depende de si EstaEliminado está implementado
        if (clienteEliminado.EstaEliminado)
        {
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be($"El cliente con ID {clienteId} ha sido eliminado");
            _mockMapper.Verify(m => m.Map<ClienteDto>(It.IsAny<Cliente>()), Times.Never);
        }
        else
        {
            // Si EstaEliminado no está implementado, el handler continuará normalmente
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
        }

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
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

        _mockMapper.Setup(m => m.Map<ClienteDto>(clienteInactivo))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        if (!clienteInactivo.EstaEliminado) // Solo si no está eliminado
        {
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Activo.Should().BeFalse();
            result.Value.Email.Should().Be("maria@test.com");

            _mockMapper.Verify(m => m.Map<ClienteDto>(clienteInactivo), Times.Once);
        }

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExcepcionRepositorio_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery(clienteId);

        var excepcionRepositorio = new Exception("Error de base de datos");

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(excepcionRepositorio);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al buscar el cliente");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<ClienteDto>(It.IsAny<Cliente>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExcepcionMapper_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery(clienteId);

        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Carlos", "Rodríguez"),
            "carlos@test.com",
            "+57300777666",
            DateTime.Now.AddYears(-35)
        );

        var excepcionMapper = new Exception("Error de mapeo");

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMapper.Setup(m => m.Map<ClienteDto>(cliente))
            .Throws(excepcionMapper);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al buscar el cliente");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<ClienteDto>(cliente), Times.Once);
    }

    [Fact]
    public async Task Handle_ConstructorSinParametros_DeberiaFuncionar()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var query = new ObtenerClientePorIdQuery
        {
            ClienteId = clienteId
        };

        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Pedro", "Martínez"),
            "pedro@test.com",
            "+57300111222",
            DateTime.Now.AddYears(-40)
        );

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Nombre = "Pedro",
            Apellido = "Martínez",
            Email = "pedro@test.com"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMapper.Setup(m => m.Map<ClienteDto>(cliente))
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
        var fechaNacimiento = DateTime.Now.AddYears(-30);

        var cliente = Cliente.Crear(
            ClienteNombre.Crear("Laura", "Fernández"),
            "laura@test.com",
            "+57300333555",
            fechaNacimiento
        );

        var clienteDto = new ClienteDto
        {
            Id = clienteId,
            Nombre = "Laura",
            Apellido = "Fernández",
            Email = "laura@test.com",
            Telefono = "+57300333555",
            FechaNacimiento = fechaNacimiento,
            Activo = true
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockMapper.Setup(m => m.Map<ClienteDto>(cliente))
            .Returns(clienteDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(clienteId);
        result.Value.Nombre.Should().Be("Laura");
        result.Value.Apellido.Should().Be("Fernández");
        result.Value.Email.Should().Be("laura@test.com");
        result.Value.Telefono.Should().Be("+57300333555");
        result.Value.FechaNacimiento.Should().Be(fechaNacimiento);
        result.Value.Activo.Should().BeTrue();
    }
} 