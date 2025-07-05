namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Commands;

/// <summary>
/// Pruebas unitarias para DesactivarProveedorHandler
/// Tests que cubren todos los escenarios de desactivación de proveedores
/// </summary>
public class DesactivarProveedorHandlerTests
{
    private readonly Mock<IProveedorRepository> _mockRepository;
    private readonly Mock<ILogger<DesactivarProveedorHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly DesactivarProveedorHandler _handler;
    private readonly DesactivarProveedorCommand _commandValido;
    private readonly Domain.Proveedores.Entities.Proveedor _proveedorActivo;
    private readonly Domain.Proveedores.Entities.Proveedor _proveedorInactivo;

    public DesactivarProveedorHandlerTests()
    {
        _mockRepository = new Mock<IProveedorRepository>();
        _mockLogger = new Mock<ILogger<DesactivarProveedorHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new DesactivarProveedorHandler(
            _mockRepository.Object, 
            _mockLogger.Object,
            _mockCurrentUserService.Object);

        // Setup de datos de prueba
        _commandValido = CrearCommandValido();
        _proveedorActivo = CrearProveedorActivo();
        _proveedorInactivo = CrearProveedorInactivo();
    }

    [Fact]
    public async Task Handle_ConProveedorActivo_DeberiaDesactivarCorrectamente()
    {
        // Arrange
        var command = new DesactivarProveedorCommand
        {
            Id = Guid.NewGuid(),
            RazonDesactivacion = "Cambio de proveedor principal"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None), Times.Once);
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
    }

    [Fact]
    public async Task Handle_ConProveedorYaDesactivado_DeberiaRetornarExito()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorInactivo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new DesactivarProveedorCommand
        {
            Id = Guid.Empty,
            RazonDesactivacion = "Test de ID vacío"
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
        result.Error.Should().Contain("Error interno del servidor al desactivar el proveedor");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorioActualizar_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None))
                      .ThrowsAsync(new Exception("Error al actualizar en base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al desactivar el proveedor");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConRazonDesactivacion_DeberiaUsarRazonCorrectamente()
    {
        // Arrange
        var command = new DesactivarProveedorCommand
        {
            Id = Guid.NewGuid(),
            RazonDesactivacion = "Proveedor no cumple con estándares de calidad"
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.RazonDesactivacion.Should().NotBeNullOrEmpty();
        command.RazonDesactivacion.Should().Contain("estándares de calidad");

        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_SinRazonDesactivacion_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var command = new DesactivarProveedorCommand
        {
            Id = Guid.NewGuid(),
            RazonDesactivacion = null
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.RazonDesactivacion.Should().BeNull();

        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCurrentUserService_DeberiaUsarServicioCorrectamente()
    {
        // Arrange
        var command = _commandValido;
        var userId = "usuario-admin";
        
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // El CurrentUserService está disponible para uso futuro (aunque no se usa actualmente)
        _mockCurrentUserService.Setup(s => s.UserId).Returns(userId);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCorrectamente()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando desactivación de proveedor")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("✅ Proveedor desactivado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorYaDesactivado_DeberiLoggearCorrectamente()
    {
        // Arrange
        var command = _commandValido;
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorInactivo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar logging específico para proveedor ya desactivado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Proveedor ya está desactivado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConProveedorNoEncontrado_DeberiLoggearWarning()
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

        // Verificar logging de warning
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Proveedor no encontrado para desactivar")),
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
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error inesperado al desactivar proveedor")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Proveedor no cumple estándares")]
    [InlineData("Cambio de política de proveedores")]
    [InlineData("Problemas de entrega recurrentes")]
    [InlineData("Finalización de contrato")]
    public async Task Handle_ConDiferentesRazones_DeberiaDesactivarCorrectamente(string razon)
    {
        // Arrange
        var command = new DesactivarProveedorCommand
        {
            Id = Guid.NewGuid(),
            RazonDesactivacion = razon
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.Id, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorActivo);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeTrue();
        command.RazonDesactivacion.Should().Be(razon);

        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorActivo, CancellationToken.None), Times.Once);
    }

    #region Helper Methods

    private DesactivarProveedorCommand CrearCommandValido()
    {
        return new DesactivarProveedorCommand
        {
            Id = Guid.NewGuid(),
            RazonDesactivacion = "Razón de prueba para desactivación"
        };
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorActivo()
    {
        var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
            "Distribuidora Activa SA",     // nombre
            "María González",              // nombreContacto
            "maria@activa.com",            // email
            "555-111-2222",                // telefono
            "Calle Principal 123",         // direccion
            "Santiago",            // ciudad
            "7640000",                   // codigoPostal
            "Chile",                      // pais
            "XAXX010101000",               // rfc
            "Santander - 9876543210",      // informacionBancaria
            45                             // diasCredito
        );

        // Establecer como activo explícitamente
        proveedor.Activar();
        proveedor.GetType().GetProperty("Id")?.SetValue(proveedor, Guid.NewGuid());
        return proveedor;
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorInactivo()
    {
        var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
            "Distribuidora Inactiva SA",   // nombre
            "Pedro Hernández",             // nombreContacto
            "pedro@inactiva.com",          // email
            "555-333-4444",                // telefono
            "Avenida Secundaria 456",      // direccion
            "Valparaíso",                 // ciudad
            "2340000",                   // codigoPostal
            "Chile",                      // pais
            "XAXX010101001",               // rfc
            "BBVA - 1234567890",           // informacionBancaria
            30                             // diasCredito
        );

        // Desactivar el proveedor
        proveedor.Desactivar("Proveedor desactivado para pruebas");
        proveedor.GetType().GetProperty("Id")?.SetValue(proveedor, Guid.NewGuid());
        return proveedor;
    }

    #endregion
} 