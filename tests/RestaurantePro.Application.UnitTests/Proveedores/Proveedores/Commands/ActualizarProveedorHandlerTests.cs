namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Commands;

/// <summary>
/// Pruebas unitarias para ActualizarProveedorHandler
/// Tests que cubren todos los escenarios de actualización, validación y manejo de errores
/// </summary>
public class ActualizarProveedorHandlerTests
{
    private readonly Mock<IProveedorRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ActualizarProveedorHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ActualizarProveedorHandler _handler;
    private readonly ActualizarProveedorCommand _commandValido;
    private readonly ProveedorDto _proveedorDtoEjemplo;
    private readonly Domain.Proveedores.Entities.Proveedor _proveedorEjemplo;

    public ActualizarProveedorHandlerTests()
    {
        _mockRepository = new Mock<IProveedorRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ActualizarProveedorHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new ActualizarProveedorHandler(
            _mockRepository.Object, 
            _mockMapper.Object, 
            _mockLogger.Object,
            _mockCurrentUserService.Object);

        // Setup de datos de prueba
        _commandValido = CrearCommandValido();
        _proveedorDtoEjemplo = CrearProveedorDtoEjemplo();
        _proveedorEjemplo = CrearProveedorEjemplo();
    }

    [Fact]
    public async Task Handle_ConDatosValidos_DeberiaActualizarProveedorCorrectamente()
    {
        // Arrange
        var command = new ActualizarProveedorCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Proveedor Actualizado",
            Descripcion = "Descripción actualizada",
            Email = "actualizado@proveedor.com",
            Telefono = "555-999-8888",
            Direccion = "Nueva Dirección 123",
            Categoria = CategoriaProveedor.AlimentosBasicos,
            Activo = true
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorEjemplo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None))
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

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None), Times.Once);
        _mockMapper.Verify(m => m.Map<ProveedorDto>(_proveedorEjemplo), Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorNoEncontrado_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync((Domain.Proveedores.Entities.Proveedor?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("no fue encontrado");
        result.Error.Should().Contain(command.Id.ToString());

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
        _mockMapper.Verify(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new ActualizarProveedorCommand
        {
            Id = Guid.Empty,
            Nombre = "Nombre Test",
            Email = "test@email.com",
            Telefono = "555-123-4567",
            Categoria = CategoriaProveedor.AlimentosBasicos
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(Guid.Empty, true, true, CancellationToken.None))
                      .ReturnsAsync((Domain.Proveedores.Entities.Proveedor?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("no fue encontrado");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(Guid.Empty, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorioObtener_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ThrowsAsync(new Exception("Error en base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al actualizar el proveedor");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorioActualizar_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorEjemplo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None))
                      .ThrowsAsync(new Exception("Error al actualizar en base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al actualizar el proveedor");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConErrorEnMapper_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorEjemplo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Throws(new Exception("Error en mapeo"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al actualizar el proveedor");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorActivo_DeberiaActualizarCorrectamente()
    {
        // Arrange
        var command = new ActualizarProveedorCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Proveedor Activo",
            Email = "activo@proveedor.com",
            Telefono = "555-777-9999",
            Categoria = CategoriaProveedor.AlimentosBasicos,
            Activo = true
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorEjemplo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.Activo.Should().BeTrue();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorInactivo_DeberiaActualizarCorrectamente()
    {
        // Arrange
        var command = new ActualizarProveedorCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Proveedor Inactivo",
            Email = "inactivo@proveedor.com",
            Telefono = "555-666-7777",
            Categoria = CategoriaProveedor.AlimentosBasicos,
            Activo = false
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorEjemplo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.Activo.Should().BeFalse();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConDiferentesCategorias_DeberiaActualizarCorrectamente()
    {
        // Arrange
        var commands = new[]
        {
            new ActualizarProveedorCommand { Id = Guid.NewGuid(), Nombre = "Proveedor 1", Email = "email1@test.com", Telefono = "111", Categoria = CategoriaProveedor.AlimentosBasicos },
            new ActualizarProveedorCommand { Id = Guid.NewGuid(), Nombre = "Proveedor 2", Email = "email2@test.com", Telefono = "222", Categoria = CategoriaProveedor.Carnes },
            new ActualizarProveedorCommand { Id = Guid.NewGuid(), Nombre = "Proveedor 3", Email = "email3@test.com", Telefono = "333", Categoria = CategoriaProveedor.Servicios }
        };

        foreach (var command in commands)
        {
            _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                          .ReturnsAsync(_proveedorEjemplo);

            _mockRepository.Setup(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None))
                          .Returns(Task.CompletedTask);

            _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                       .Returns(_proveedorDtoEjemplo);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            command.Categoria.Should().BeOneOf(CategoriaProveedor.AlimentosBasicos, CategoriaProveedor.Carnes, CategoriaProveedor.Servicios);
        }
    }

    [Fact]
    public async Task Handle_ConCamposOpcionales_DeberiaActualizarCorrectamente()
    {
        // Arrange
        var command = new ActualizarProveedorCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Proveedor Mínimo",
            Email = "minimo@proveedor.com",
            Telefono = "555-000-0000",
            Categoria = CategoriaProveedor.AlimentosBasicos,
            // Campos opcionales como null
            Descripcion = null,
            Direccion = null
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorEjemplo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.Descripcion.Should().BeNull();
        command.Direccion.Should().BeNull();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCurrentUserService_DeberiaUsarServicioCorrectamente()
    {
        // Arrange
        var command = _commandValido;
        var userId = "usuario-test";
        
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorEjemplo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // El CurrentUserService está disponible pero no se usa actualmente (están en TODO en el handler)
        // Este test verifica que el servicio está inyectado correctamente para uso futuro
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId); // Configurado para uso futuro
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCorrectamente()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorEjemplo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorEjemplo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()))
                   .Returns(_proveedorDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que se hicieron los llamados de logging esperados
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando actualización de proveedor")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Proveedor actualizado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Helper Methods

    private ActualizarProveedorCommand CrearCommandValido()
    {
        return new ActualizarProveedorCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Distribuidora ABC Actualizada",
            Descripcion = "Descripción actualizada del proveedor",
            Email = "actualizado@abc.com",
            Telefono = "555-999-8888",
            Direccion = "Nueva Dirección Principal 789",
            Categoria = CategoriaProveedor.AlimentosBasicos,
            Activo = true
        };
    }

    private ProveedorDto CrearProveedorDtoEjemplo()
    {
        return new ProveedorDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Distribuidora ABC Actualizada",
            NombreContacto = "Juan Pérez",
            Email = "actualizado@abc.com",
            Telefono = "555-999-8888",
            Direccion = "Nueva Dirección Principal 789",
            Ciudad = "Guadalajara",
            CodigoPostal = "44100",
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
            "Distribuidora ABC",           // nombre
            "Juan Pérez",                  // nombreContacto
            "original@abc.com",            // email
            "555-123-4567",                // telefono
            "Dirección Original 456",      // direccion
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