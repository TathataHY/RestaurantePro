namespace RestaurantePro.Application.UnitTests.Proveedores.ContactosProveedor.Commands;

/// <summary>
/// Pruebas unitarias para AgregarContactoHandler
/// Tests que cubren todos los escenarios de creación de contactos para proveedores
/// </summary>
public class AgregarContactoHandlerTests
{
    private readonly Mock<IProveedorRepository> _mockRepository;
    private readonly Mock<IContactoProveedorRepository> _mockContactoRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AgregarContactoHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly AgregarContactoHandler _handler;
    private readonly AgregarContactoCommand _commandValido;
    private readonly Domain.Proveedores.Entities.Proveedor _proveedorActivo;
    private readonly ContactoProveedorDto _contactoDto;
    private readonly Guid _proveedorId;
    private readonly Guid _contactoId;

    public AgregarContactoHandlerTests()
    {
        _mockRepository = new Mock<IProveedorRepository>();
        _mockContactoRepository = new Mock<IContactoProveedorRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AgregarContactoHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new AgregarContactoHandler(
            _mockRepository.Object, 
            _mockContactoRepository.Object,
            _mockMapper.Object, 
            _mockLogger.Object,
            _mockCurrentUserService.Object);

        // Setup de datos de prueba
        _proveedorId = Guid.NewGuid();
        _contactoId = Guid.NewGuid();
        _commandValido = CrearCommandValido();
        _proveedorActivo = CrearProveedorActivo();
        _contactoDto = CrearContactoDto();
    }

    [Fact]
    public async Task Handle_ConDatosValidos_DeberiaCrearContactoCorrectamente()
    {
        // Arrange
        var command = new AgregarContactoCommand
        {
            ProveedorId = _proveedorId,
            Nombre = "Juan Carlos",
            Apellidos = "Pérez Gómez", 
            Email = "juan.carlos@proveedor.com",
            Telefono = "555-123-4567",
            Cargo = "Gerente de Ventas",
            EsPrincipal = false
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockContactoRepository.Setup(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()))
                   .Returns(_contactoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().Be(_contactoDto);

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Once);
        _mockMapper.Verify(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorNoEncontrado_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync((Domain.Proveedores.Entities.Proveedor?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El proveedor especificado no existe");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Never);
        _mockMapper.Verify(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConEmailDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var command = new AgregarContactoCommand
        {
            ProveedorId = _proveedorId,
            Nombre = "María",
            Apellidos = "González",
            Email = "contacto.existente@proveedor.com", // Email que ya existe
            Telefono = "555-987-6543",
            Cargo = "Administradora"
        };

        var proveedorConContactos = CrearProveedorConContactos();
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(proveedorConContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Ya existe un contacto con este email para el proveedor");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConLimiteMaximoContactos_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        var proveedorConMaximoContactos = CrearProveedorConMaximoContactos();
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(proveedorConMaximoContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("ha alcanzado el límite máximo de contactos (10)");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConNombreVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new AgregarContactoCommand
        {
            ProveedorId = _proveedorId,
            Nombre = "", // Nombre vacío
            Apellidos = "González",
            Email = "nuevo@proveedor.com",
            Telefono = "555-987-6543",
            Cargo = "Administradora"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El nombre del contacto es obligatorio");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConEmailVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new AgregarContactoCommand
        {
            ProveedorId = _proveedorId,
            Nombre = "María",
            Apellidos = "González",
            Email = "", // Email vacío
            Telefono = "555-987-6543",
            Cargo = "Administradora"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El email del contacto es obligatorio");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConContactoPrincipal_DeberiaLoggearInformacion()
    {
        // Arrange
        var command = new AgregarContactoCommand
        {
            ProveedorId = _proveedorId,
            Nombre = "Ana",
            Apellidos = "Martínez",
            Email = "ana.martinez@proveedor.com",
            Telefono = "555-111-2222",
            Cargo = "Directora General",
            EsPrincipal = true // Contacto principal
        };

        var proveedorConContactos = CrearProveedorConContactos();
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(proveedorConContactos);

        _mockRepository.Setup(r => r.ActualizarAsync(proveedorConContactos, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.GuardarCambiosAsync(CancellationToken.None))
                      .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()))
                   .Returns(_contactoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar logging específico para contacto principal
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Se establecerá nuevo contacto principal")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SinApellidos_DeberiaUsarSoloNombre()
    {
        // Arrange
        var command = new AgregarContactoCommand
        {
            ProveedorId = _proveedorId,
            Nombre = "Roberto",
            Apellidos = "", // Sin apellidos
            Email = "roberto@proveedor.com",
            Telefono = "555-333-4444",
            Cargo = "Asistente"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.GuardarCambiosAsync(CancellationToken.None))
                      .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()))
                   .Returns(_contactoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // El nombre completo debería ser solo "Roberto" sin apellidos
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorioObtener_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        var exception = new Exception("Error de base de datos");
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al crear el contacto");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorioActualizar_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        var exception = new Exception("Error al agregar contacto en base de datos");
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockContactoRepository.Setup(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None))
                      .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al crear el contacto");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConErrorEnGuardarCambios_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        var exception = new Exception("Error al guardar cambios");
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockContactoRepository.Setup(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None))
                      .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al crear el contacto");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCorrectamente()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockContactoRepository.Setup(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()))
                   .Returns(_contactoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar logging inicial
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando creación de contacto")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Contacto creado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorNoEncontrado_DeberiLoggearWarning()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync((Domain.Proveedores.Entities.Proveedor?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();

        // Verificar logging de warning
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Proveedor no encontrado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConEmailDuplicado_DeberiLoggearWarning()
    {
        // Arrange
        var command = new AgregarContactoCommand
        {
            ProveedorId = _proveedorId,
            Nombre = "María",
            Apellidos = "González",
            Email = "contacto.existente@proveedor.com",
            Telefono = "555-987-6543",
            Cargo = "Administradora"
        };

        var proveedorConContactos = CrearProveedorConContactos();
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(proveedorConContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();

        // Verificar logging de warning para email duplicado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ya existe un contacto con email")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcion_DeberiLoggearError()
    {
        // Arrange
        var command = _commandValido;
        var exception = new Exception("Error de prueba");
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error inesperado al crear contacto")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Juan", "Pérez", "juan.perez@proveedor.com", "Gerente")]
    [InlineData("María", "González", "maria.gonzalez@proveedor.com", "Directora")]
    [InlineData("Carlos", "Rodríguez", "carlos.rodriguez@proveedor.com", "Supervisor")]
    [InlineData("Ana", "Martínez", "ana.martinez@proveedor.com", "Coordinadora")]
    public async Task Handle_ConDiferentesContactos_DeberiaCrearCorrectamente(
        string nombre, string apellidos, string email, string cargo)
    {
        // Arrange
        var command = new AgregarContactoCommand
        {
            ProveedorId = _proveedorId,
            Nombre = nombre,
            Apellidos = apellidos,
            Email = email,
            Telefono = "555-123-4567",
            Cargo = cargo
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockContactoRepository.Setup(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()))
                   .Returns(_contactoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().Be(_contactoDto);

        _mockContactoRepository.Verify(r => r.AgregarAsync(It.IsAny<ContactoProveedor>(), CancellationToken.None), Times.Once);
    }

    #region Helper Methods

    private AgregarContactoCommand CrearCommandValido()
    {
        return new AgregarContactoCommand
        {
            ProveedorId = _proveedorId,
            Nombre = "Pedro",
            Apellidos = "Rodríguez",
            Email = "pedro.rodriguez@proveedor.com",
            Telefono = "555-456-7890",
            Cargo = "Supervisor de Ventas",
            EsPrincipal = false
        };
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorActivo()
    {
        var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
            "Proveedor Test SA",          // nombre
            "Juan Pérez",                 // nombreContacto
            "juan@test.com",              // email
            "555-123-4567",               // telefono
            "Calle Test 123",             // direccion
            "Ciudad Test",                // ciudad
            "12345",                      // codigoPostal
            "Chile",                     // pais
            "XAXX010101000",              // rfc
            "Banco Test - 1234567890",    // informacionBancaria
            30                            // diasCredito
        );

        proveedor.Activar();
        proveedor.GetType().GetProperty("Id")?.SetValue(proveedor, _proveedorId);
        return proveedor;
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorConContactos()
    {
        var proveedor = CrearProveedorActivo();
        
        // Agregar un contacto existente
        proveedor.AgregarContacto(
            "Contacto Existente",
            "Gerente",
            "555-111-2222",
            "contacto.existente@proveedor.com",
            false);

        return proveedor;
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorConMaximoContactos()
    {
        var proveedor = CrearProveedorActivo();
        
        // Agregar 10 contactos para alcanzar el límite
        for (int i = 1; i <= 10; i++)
        {
            proveedor.AgregarContacto(
                $"Contacto {i}",
                $"Cargo {i}",
                $"555-{i:000}-{i:0000}",
                $"contacto{i}@proveedor.com",
                i == 1);
        }

        return proveedor;
    }

    private ContactoProveedorDto CrearContactoDto()
    {
        return new ContactoProveedorDto
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            Nombre = "Pedro Rodríguez",
            Cargo = "Supervisor de Ventas",
            Email = "pedro.rodriguez@proveedor.com",
            Telefono = "555-456-7890",
            EsPrincipal = false
        };
    }

    #endregion
} 