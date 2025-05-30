using RestaurantePro.Application.Proveedores.Proveedores.Commands.CrearProveedor;
using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using RestaurantePro.Domain.Proveedores.Interfaces;

namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Commands;

/// <summary>
/// Pruebas unitarias para CrearProveedorHandler
/// Tests comprensivos que cubren todos los escenarios de creación, validación y manejo de errores
/// </summary>
public class CrearProveedorHandlerTests
{
    private readonly Mock<IProveedorRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CrearProveedorHandler>> _mockLogger;
    private readonly CrearProveedorHandler _handler;
    private readonly CrearProveedorCommand _commandValido;
    private readonly ProveedorDto _proveedorDtoEjemplo;
    private readonly Domain.Proveedores.Entities.Proveedor _proveedorEjemplo;

    public CrearProveedorHandlerTests()
    {
        _mockRepository = new Mock<IProveedorRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CrearProveedorHandler>>();
        _handler = new CrearProveedorHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);

        // Setup de datos de prueba
        _commandValido = CrearCommandValido();
        _proveedorDtoEjemplo = CrearProveedorDtoEjemplo();
        _proveedorEjemplo = CrearProveedorEjemplo();
    }

    [Fact]
    public async Task Handle_ConDatosValidos_DeberiaCrearProveedorCorrectamente()
    {
        // Arrange
        var command = CrearProveedorCommand.CrearBasico(
            "Distribuidora ABC",
            "Juan Pérez",
            "contacto@abc.com",
            "555-123-4567",
            "México",
            "XAXX010101000",
            Guid.NewGuid()
        );

        _mockRepository.Setup(r => r.ObtenerPorRFCAsync("XAXX010101000", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.BuscarAsync("contacto@abc.com", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(_proveedorDtoEjemplo);

        _mockRepository.Verify(r => r.ObtenerPorRFCAsync(command.RFC, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.BuscarAsync(command.Email, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConRFCDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        var proveedorExistente = CrearProveedorEjemplo();
        
        _mockRepository.Setup(r => r.ObtenerPorRFCAsync(command.RFC, CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor> { proveedorExistente });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Ya existe un proveedor con el RFC");
        result.Error.Should().Contain(command.RFC);

        _mockRepository.Verify(r => r.ObtenerPorRFCAsync(command.RFC, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConEmailDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        var proveedorExistente = CrearProveedorEjemplo();
        
        _mockRepository.Setup(r => r.ObtenerPorRFCAsync(command.RFC, CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.BuscarAsync(command.Email, CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor> { proveedorExistente });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Ya existe un proveedor con el email");
        result.Error.Should().Contain(command.Email);

        _mockRepository.Verify(r => r.BuscarAsync(command.Email, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConProveedorConCredito_DeberiaLoggearInformacionCredito()
    {
        // Arrange
        var command = CrearProveedorCommand.CrearConCredito(
            "Proveedor con Crédito",
            "María García",
            "maria@proveedor.com",
            "555-987-6543",
            "Calle Principal 123",
            "Guadalajara",
            "XAXX010102000",
            45, // días de crédito
            "Banco Azteca - 1234567890",
            Guid.NewGuid()
        );

        _mockRepository.Setup(r => r.ObtenerPorRFCAsync("XAXX010102000", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.BuscarAsync("maria@proveedor.com", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.DiasCredito.Should().Be(45);
        command.InformacionBancaria.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ConProveedorInternacional_DeberiaLoggearInformacionInternacional()
    {
        // Arrange
        var command = CrearProveedorCommand.CrearInternacional(
            "International Supplier Inc",
            "John Smith",
            "john@supplier.com",
            "555-555-0123",
            "123 Main Street",
            "New York",
            "Estados Unidos",
            "TAX123456789",
            Guid.NewGuid()
        );

        _mockRepository.Setup(r => r.ObtenerPorRFCAsync("TAX123456789", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.BuscarAsync("john@supplier.com", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.Pais.Should().NotBe("México");
        command.DiasCredito.Should().Be(30); // Default para internacionales
    }

    [Fact]
    public async Task Handle_ConErrorEnValidacionRFC_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorRFCAsync(command.RFC, CancellationToken.None))
                      .ThrowsAsync(new Exception("Error en base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error al validar el RFC en el sistema");

        _mockRepository.Verify(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnValidacionEmail_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorRFCAsync(command.RFC, CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.BuscarAsync(command.Email, CancellationToken.None))
                      .ThrowsAsync(new Exception("Error en búsqueda"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error al validar el email en el sistema");

        _mockRepository.Verify(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorioAgregar_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorRFCAsync(command.RFC, CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.BuscarAsync(command.Email, CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None))
                      .ThrowsAsync(new Exception("Error al guardar en base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al crear el proveedor");
    }

    [Fact]
    public async Task Handle_ConDatosDeCreateDto_DeberiaCrearCorrectamente()
    {
        // Arrange
        var dto = new ProveedorCreateDto
        {
            Nombre = "Proveedor desde DTO",
            NombreContacto = "Contacto DTO",
            Email = "dto@proveedor.com",
            Telefono = "555-999-8888",
            Direccion = "Dirección DTO",
            Ciudad = "Ciudad DTO",
            CodigoPostal = "12345",
            Pais = "México",
            RFC = "XAXX010103000",
            InformacionBancaria = "Banco DTO",
            DiasCredito = 15
        };

        var command = CrearProveedorCommand.DesdeDto(dto, Guid.NewGuid());

        _mockRepository.Setup(r => r.ObtenerPorRFCAsync("XAXX010103000", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.BuscarAsync("dto@proveedor.com", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.Nombre.Should().Be(dto.Nombre);
        command.NombreContacto.Should().Be(dto.NombreContacto);
        command.Email.Should().Be(dto.Email);
        command.DiasCredito.Should().Be(dto.DiasCredito);
    }

    [Fact]
    public async Task Handle_ConExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var command = _commandValido;
        
        // Configurar validaciones exitosas
        _mockRepository.Setup(r => r.ObtenerPorRFCAsync(command.RFC, CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.BuscarAsync(command.Email, CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        // Configurar excepción en AgregarAsync para que se maneje en el catch general
        _mockRepository.Setup(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None))
                      .ThrowsAsync(new InvalidOperationException("Error crítico del sistema"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al crear el proveedor");
    }

    [Fact]
    public async Task Handle_ConParametrosMinimos_DeberiaUsarValoresPorDefecto()
    {
        // Arrange
        var command = new CrearProveedorCommand
        {
            Nombre = "Proveedor Mínimo",
            NombreContacto = "Contacto Mínimo",
            Email = "minimo@proveedor.com",
            Telefono = "555-000-0000",
            Direccion = "Dirección",
            Ciudad = "Ciudad",
            RFC = "XAXX010104000",
            UsuarioId = Guid.NewGuid()
            // Campos opcionales no establecidos
        };

        _mockRepository.Setup(r => r.ObtenerPorRFCAsync("XAXX010104000", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.BuscarAsync("minimo@proveedor.com", CancellationToken.None))
                      .ReturnsAsync(new List<Domain.Proveedores.Entities.Proveedor>());

        _mockRepository.Setup(r => r.AgregarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.Pais.Should().Be("México"); // Default
        command.DiasCredito.Should().Be(0); // Default (contado)
        command.CodigoPostal.Should().BeEmpty(); // Opcional
        command.InformacionBancaria.Should().BeEmpty(); // Opcional
    }

    #region Helper Methods

    private CrearProveedorCommand CrearCommandValido()
    {
        return new CrearProveedorCommand
        {
            Nombre = "Distribuidora XYZ",
            NombreContacto = "Carlos Mendoza",
            Email = "carlos@xyz.com",
            Telefono = "555-123-4567",
            Direccion = "Av. Principal 456",
            Ciudad = "Monterrey",
            CodigoPostal = "64000",
            Pais = "México",
            RFC = "XAXX010101000",
            InformacionBancaria = "BBVA - 1234567890",
            DiasCredito = 30,
            UsuarioId = Guid.NewGuid()
        };
    }

    private ProveedorDto CrearProveedorDtoEjemplo()
    {
        return new ProveedorDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Distribuidora XYZ",
            NombreContacto = "Carlos Mendoza",
            Email = "carlos@xyz.com",
            Telefono = "555-123-4567",
            Direccion = "Av. Principal 456",
            Ciudad = "Monterrey",
            CodigoPostal = "64000",
            Pais = "México",
            RFC = "XAXX010101000",
            InformacionBancaria = "BBVA - 1234567890",
            DiasCredito = 30,
            Activo = true,
            FechaRegistro = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow,
            CreadoPor = "admin"
        };
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorEjemplo()
    {
        var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
            "Distribuidora XYZ",           // nombre
            "Carlos Mendoza",              // nombreContacto
            "carlos@xyz.com",              // email
            "555-123-4567",                // telefono
            "Av. Principal 456",           // direccion
            "Monterrey",                   // ciudad
            "64000",                       // codigoPostal
            "México",                      // pais
            "XAXX010101000",               // rfc
            "BBVA - 1234567890",           // informacionBancaria
            30                             // diasCredito
        );

        proveedor.GetType().GetProperty("Id")?.SetValue(proveedor, Guid.NewGuid());
        return proveedor;
    }

    #endregion
} 