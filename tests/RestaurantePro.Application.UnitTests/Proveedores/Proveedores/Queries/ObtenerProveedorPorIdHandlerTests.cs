namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Queries;

/// <summary>
/// Pruebas unitarias para ObtenerProveedorPorIdHandler
/// Tests que cubren todos los escenarios de consulta de proveedor por ID
/// </summary>
public class ObtenerProveedorPorIdHandlerTests
{
    private readonly Mock<IProveedorRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerProveedorPorIdHandler>> _mockLogger;
    private readonly ObtenerProveedorPorIdHandler _handler;
    private readonly ObtenerProveedorPorIdQuery _queryValida;
    private readonly Domain.Proveedores.Entities.Proveedor _proveedorActivo;
    private readonly Domain.Proveedores.Entities.Proveedor _proveedorInactivo;
    private readonly ProveedorDto _proveedorDto;
    private readonly Guid _proveedorId;
    private readonly Guid _usuarioId;

    public ObtenerProveedorPorIdHandlerTests()
    {
        _mockRepository = new Mock<IProveedorRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerProveedorPorIdHandler>>();
        _handler = new ObtenerProveedorPorIdHandler(
            _mockRepository.Object, 
            _mockMapper.Object, 
            _mockLogger.Object);

        // Setup de datos de prueba
        _proveedorId = Guid.NewGuid();
        _usuarioId = Guid.NewGuid();
        _queryValida = CrearQueryValida();
        _proveedorActivo = CrearProveedorActivo();
        _proveedorInactivo = CrearProveedorInactivo();
        _proveedorDto = CrearProveedorDto();
    }

    [Fact]
    public async Task Handle_ConProveedorExistente_DeberiaRetornarProveedorCorrectamente()
    {
        // Arrange
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = _proveedorId,
            UsuarioId = _usuarioId,
            IncluirContactos = true,
            IncluirCategorias = true
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(_proveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Returns(_proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().Be(_proveedorDto);

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(_proveedorId, true, true, CancellationToken.None), Times.Once);
        _mockMapper.Verify(m => m.Map<ProveedorDto>(_proveedorActivo), Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorNoEncontrado_DeberiaRetornarError()
    {
        // Arrange
        var query = _queryValida;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync((Domain.Proveedores.Entities.Proveedor?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("No se encontró el proveedor");
        result.Error.Should().Contain(query.ProveedorId.ToString());

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockMapper.Verify(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = Guid.Empty,
            UsuarioId = _usuarioId,
            IncluirContactos = true,
            IncluirCategorias = false
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El ID del proveedor no puede estar vacío");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>(), CancellationToken.None), Times.Never);
        _mockMapper.Verify(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConProveedorInactivo_DeberiaRetornarProveedorYLoggearWarning()
    {
        // Arrange
        var query = _queryValida;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorInactivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorInactivo))
                   .Returns(_proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().Be(_proveedorDto);

        // Verificar logging específico para proveedor inactivo
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Consultando proveedor inactivo")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockMapper.Verify(m => m.Map<ProveedorDto>(_proveedorInactivo), Times.Once);
    }

    [Fact]
    public async Task Handle_SinIncluirContactos_DeberiaUsarParametrosCorrectos()
    {
        // Arrange
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = _proveedorId,
            UsuarioId = _usuarioId,
            IncluirContactos = false,
            IncluirCategorias = false
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(_proveedorId, false, false, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Returns(_proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(_proveedorId, false, false, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_SoloIncluirContactos_DeberiaUsarParametrosCorrectos()
    {
        // Arrange
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = _proveedorId,
            UsuarioId = _usuarioId,
            IncluirContactos = true,
            IncluirCategorias = false
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(_proveedorId, true, false, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Returns(_proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(_proveedorId, true, false, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = _queryValida;
        var exception = new Exception("Error de base de datos");
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None))
                      .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al buscar el proveedor");
        result.Error.Should().Contain(exception.Message);

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockMapper.Verify(m => m.Map<ProveedorDto>(It.IsAny<Domain.Proveedores.Entities.Proveedor>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnMapper_DeberiaRetornarError()
    {
        // Arrange
        var query = _queryValida;
        var exception = new Exception("Error de mapeo");
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Throws(exception);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al buscar el proveedor");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockMapper.Verify(m => m.Map<ProveedorDto>(_proveedorActivo), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCorrectamente()
    {
        // Arrange
        var query = _queryValida;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Returns(_proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar logging inicial
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Buscando proveedor por ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Proveedor encontrado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorConContactos_DeberiLoggearContactos()
    {
        // Arrange
        var query = _queryValida;
        var proveedorDtoConContactos = new ProveedorDto
        {
            Id = _proveedorId,
            Nombre = "Proveedor Test",
            Contactos = new List<ContactoProveedorDto>
            {
                new ContactoProveedorDto { Id = Guid.NewGuid(), Nombre = "Contacto 1" },
                new ContactoProveedorDto { Id = Guid.NewGuid(), Nombre = "Contacto 2" },
                new ContactoProveedorDto { Id = Guid.NewGuid(), Nombre = "Contacto 3" }
            }
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Returns(proveedorDtoConContactos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar logging de contactos incluidos
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Incluidos") && v.ToString()!.Contains("contactos")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorNoEncontrado_DeberiLoggearWarning()
    {
        // Arrange
        var query = _queryValida;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync((Domain.Proveedores.Entities.Proveedor?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
    public async Task Handle_ConExcepcion_DeberiLoggearError()
    {
        // Arrange
        var query = _queryValida;
        var exception = new Exception("Error de prueba");
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(query.ProveedorId, true, true, CancellationToken.None))
                      .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();

        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al obtener proveedor por ID")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Handle_ConDiferentesParametrosDeInclusión_DeberiaUsarParametrosCorrectamente(
        bool incluirContactos, bool incluirCategorias)
    {
        // Arrange
        var query = new ObtenerProveedorPorIdQuery
        {
            ProveedorId = _proveedorId,
            UsuarioId = _usuarioId,
            IncluirContactos = incluirContactos,
            IncluirCategorias = incluirCategorias
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(_proveedorId, incluirContactos, incluirCategorias, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Returns(_proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(_proveedorId, incluirContactos, incluirCategorias, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFactoryMethodConsultaBasica_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var query = ObtenerProveedorPorIdQuery.ConsultaBasica(_proveedorId, _usuarioId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(_proveedorId, false, false, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Returns(_proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        query.IncluirContactos.Should().BeFalse();
        query.IncluirCategorias.Should().BeFalse();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(_proveedorId, false, false, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFactoryMethodConsultaCompleta_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var query = ObtenerProveedorPorIdQuery.ConsultaCompleta(_proveedorId, _usuarioId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(_proveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Returns(_proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        query.IncluirContactos.Should().BeTrue();
        query.IncluirCategorias.Should().BeTrue();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(_proveedorId, true, true, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConFactoryMethodConsultaConContactos_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var query = ObtenerProveedorPorIdQuery.ConsultaConContactos(_proveedorId, _usuarioId);

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(_proveedorId, true, false, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockMapper.Setup(m => m.Map<ProveedorDto>(_proveedorActivo))
                   .Returns(_proveedorDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        query.IncluirContactos.Should().BeTrue();
        query.IncluirCategorias.Should().BeFalse();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(_proveedorId, true, false, CancellationToken.None), Times.Once);
    }

    #region Helper Methods

    private ObtenerProveedorPorIdQuery CrearQueryValida()
    {
        return new ObtenerProveedorPorIdQuery
        {
            ProveedorId = _proveedorId,
            UsuarioId = _usuarioId,
            IncluirContactos = true,
            IncluirCategorias = true
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

    private Domain.Proveedores.Entities.Proveedor CrearProveedorInactivo()
    {
        var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
            "Proveedor Inactivo SA",      // nombre
            "María González",             // nombreContacto
            "maria@inactivo.com",         // email
            "555-987-6543",               // telefono
            "Avenida Inactivo 456",       // direccion
            "Ciudad Inactivo",            // ciudad
            "54321",                      // codigoPostal
            "Chile",                     // pais
            "XAXX010101001",              // rfc
            "Banco Inactivo - 0987654321", // informacionBancaria
            15                            // diasCredito
        );

        proveedor.Desactivar("Proveedor desactivado para pruebas");
        proveedor.GetType().GetProperty("Id")?.SetValue(proveedor, Guid.NewGuid());
        return proveedor;
    }

    private ProveedorDto CrearProveedorDto()
    {
        return new ProveedorDto
        {
            Id = _proveedorId,
            Nombre = "Proveedor Test SA",
            NombreContacto = "Juan Pérez",
            Email = "juan@test.com",
            Telefono = "555-123-4567",
            Direccion = "Calle Test 123",
            Ciudad = "Ciudad Test",
            CodigoPostal = "12345",
            Pais = "Chile",
            RFC = "XAXX010101000",
            InformacionBancaria = "Banco Test - 1234567890",
            DiasCredito = 30,
            Activo = true,
            Contactos = new List<ContactoProveedorDto>()  // Lista vacía
        };
    }

    #endregion
} 