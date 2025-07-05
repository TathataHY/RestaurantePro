namespace RestaurantePro.Application.UnitTests.Proveedores.ContactosProveedor.Commands;

public class ActualizarContactoHandlerTests
{
    private readonly Mock<IProveedorRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ActualizarContactoHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly ActualizarContactoHandler _handler;
    
    private readonly Guid _proveedorId = Guid.NewGuid();
    private readonly Guid _contactoId = Guid.NewGuid();
    private readonly ActualizarContactoCommand _commandValido;
    private readonly Domain.Proveedores.Entities.Proveedor _proveedorConContactos;
    private readonly ContactoProveedorDto _contactoDtoEsperado;

    public ActualizarContactoHandlerTests()
    {
        _mockRepository = new Mock<IProveedorRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ActualizarContactoHandler>>();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        
        _handler = new ActualizarContactoHandler(
            _mockRepository.Object, 
            _mockMapper.Object, 
            _mockLogger.Object,
            _mockCurrentUser.Object);
        
        _commandValido = new ActualizarContactoCommand
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            Nombre = "Juan Carlos",
            Apellidos = "González López",
            Email = "juan.gonzalez@proveedor.com",
            Telefono = "555-123-4567",
            Cargo = "Gerente Comercial",
            EsPrincipal = true
        };

        _proveedorConContactos = CrearProveedorConContactos();

        _contactoDtoEsperado = new ContactoProveedorDto
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            Nombre = "Juan Carlos González López",
            Email = "juan.gonzalez@proveedor.com",
            Telefono = "555-123-4567",
            Cargo = "Gerente Comercial",
            EsPrincipal = true
        };
    }

    [Fact]
    public async Task Handle_ConDatosCompletos_DeberiaActualizarCorrectamente()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.GuardarCambiosAsync(CancellationToken.None))
                      .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()))
                   .Returns(_contactoDtoEsperado);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(_contactoDtoEsperado);
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
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
        _mockMapper.Verify(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConContactoNoEncontrado_DeberiaRetornarError()
    {
        // Arrange
        var contactoInexistente = Guid.NewGuid();
        var command = new ActualizarContactoCommand
        {
            Id = contactoInexistente,
            ProveedorId = _proveedorId,
            Nombre = "Test",
            Email = "test@test.com"
        };
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El contacto especificado no existe");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConEmailDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var emailExistente = "existing@email.com";
        var proveedorConContactos = CrearProveedorConContactosConEmails();
        
        var command = new ActualizarContactoCommand
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            Nombre = "Test",
            Email = emailExistente // Email que ya existe en otro contacto
        };
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(proveedorConContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Ya existe otro contacto con este email para el proveedor");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConNombreVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new ActualizarContactoCommand
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            Nombre = "", // Nombre vacío
            Email = "test@test.com"
        };
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El nombre del contacto es obligatorio");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConEmailVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new ActualizarContactoCommand
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            Nombre = "Test",
            Email = "" // Email vacío
        };
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El email del contacto es obligatorio");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ThrowsAsync(new Exception("Error en repositorio"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al actualizar el contacto");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCorrectamente()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.GuardarCambiosAsync(CancellationToken.None))
                      .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<ContactoProveedorDto>(It.IsAny<ContactoProveedor>()))
                   .Returns(_contactoDtoEsperado);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando actualización de contacto")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Métodos auxiliares
    private Domain.Proveedores.Entities.Proveedor CrearProveedorConContactos()
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
        
        // Agregar el contacto que vamos a actualizar
        proveedor.AgregarContacto(
            "Juan Carlos González López",
            "Gerente Comercial",
            "555-123-4567",
            "juan.gonzalez@proveedor.com",
            true);

        // Obtener el contacto agregado y setear su ID
        var contacto = proveedor.Contactos.First();
        contacto.GetType().GetProperty("Id")?.SetValue(contacto, _contactoId);

        return proveedor;
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorConContactosConEmails()
    {
        var proveedor = CrearProveedorConContactos();

        // Agregar otro contacto con email existente
        proveedor.AgregarContacto(
            "María López",
            "Directora",
            "555-9999",
            "existing@email.com",
            false);

        return proveedor;
    }
} 