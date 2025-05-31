namespace RestaurantePro.Application.UnitTests.Proveedores.ContactosProveedor.Commands;

/// <summary>
/// Pruebas unitarias para EliminarContactoHandler
/// Tests que cubren todos los escenarios de eliminación de contactos de proveedores
/// </summary>
public class EliminarContactoHandlerTests
{
    private readonly Mock<IProveedorRepository> _mockRepository;
    private readonly Mock<ILogger<EliminarContactoHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly EliminarContactoHandler _handler;
    private readonly EliminarContactoCommand _commandValido;
    private readonly Domain.Proveedores.Entities.Proveedor _proveedorConContactos;
    private readonly Guid _proveedorId;
    private readonly Guid _contactoId;
    private readonly Guid _contactoId2;

    public EliminarContactoHandlerTests()
    {
        _mockRepository = new Mock<IProveedorRepository>();
        _mockLogger = new Mock<ILogger<EliminarContactoHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _handler = new EliminarContactoHandler(
            _mockRepository.Object, 
            _mockLogger.Object,
            _mockCurrentUserService.Object);

        // Setup de datos de prueba
        _proveedorId = Guid.NewGuid();
        _contactoId = Guid.NewGuid();
        _contactoId2 = Guid.NewGuid();
        _proveedorConContactos = CrearProveedorConContactos();
        _commandValido = CrearCommandValido();
    }

    [Fact]
    public async Task Handle_ConDatosValidos_DeberiaEliminarContactoCorrectamente()
    {
        // Arrange
        var command = new EliminarContactoCommand
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            MotivoEliminacion = "Contacto ya no trabaja en la empresa",
            TipoEliminacion = TipoEliminacion.Logica
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.GuardarCambiosAsync(CancellationToken.None))
                      .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.GuardarCambiosAsync(CancellationToken.None), Times.Once);
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
    }

    [Fact]
    public async Task Handle_ConContactoNoEncontrado_DeberiaRetornarError()
    {
        // Arrange
        var contactoInexistente = Guid.NewGuid();
        var command = new EliminarContactoCommand
        {
            Id = contactoInexistente,
            ProveedorId = _proveedorId,
            MotivoEliminacion = "Test motivo"
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
    public async Task Handle_ConContactoDeProveedorIncorrecto_DeberiaRetornarError()
    {
        // Arrange
        var proveedorIncorrectoId = Guid.NewGuid();
        var proveedorConContactos = CrearProveedorConContactos();
        
        // Simular que el contacto pertenece a otro proveedor diferente
        var contacto = proveedorConContactos.Contactos.First();
        contacto.GetType().GetProperty("ProveedorId")?.SetValue(contacto, Guid.NewGuid());
        
        var command = new EliminarContactoCommand
        {
            Id = contacto.Id,
            ProveedorId = _proveedorId, // Proveedor diferente al real del contacto
            MotivoEliminacion = "Test motivo"
        };
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(proveedorConContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El contacto no pertenece al proveedor especificado");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConUnicoContacto_DeberiaRetornarError()
    {
        // Arrange
        var proveedorConUnicoContacto = CrearProveedorConUnicoContacto();
        var contactoUnico = proveedorConUnicoContacto.Contactos.First();
        
        var command = new EliminarContactoCommand
        {
            Id = contactoUnico.Id,
            ProveedorId = _proveedorId,
            MotivoEliminacion = "Test motivo"
        };
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(proveedorConUnicoContacto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("No se puede eliminar el único contacto del proveedor");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
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
        result.Error.Should().Contain("Error interno del servidor al eliminar el contacto");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(It.IsAny<Domain.Proveedores.Entities.Proveedor>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorioActualizar_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        var exception = new Exception("Error al actualizar en base de datos");
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None))
                      .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al eliminar el contacto");

        _mockRepository.Verify(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConErrorEnGuardarCambios_DeberiaRetornarError()
    {
        // Arrange
        var command = _commandValido;
        var exception = new Exception("Error al guardar cambios");
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.GuardarCambiosAsync(CancellationToken.None))
                      .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno del servidor al eliminar el contacto");

        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.GuardarCambiosAsync(CancellationToken.None), Times.Once);
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando eliminación de contacto")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Contacto eliminado exitosamente")),
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
    public async Task Handle_ConContactoNoEncontrado_DeberiLoggearWarning()
    {
        // Arrange
        var contactoInexistente = Guid.NewGuid();
        var command = new EliminarContactoCommand
        {
            Id = contactoInexistente,
            ProveedorId = _proveedorId,
            MotivoEliminacion = "Test motivo"
        };
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();

        // Verificar logging de warning para contacto no encontrado
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Contacto no encontrado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConContactoProveedorIncorrecto_DeberiLoggearWarning()
    {
        // Arrange
        var proveedorConContactos = CrearProveedorConContactos();
        var contacto = proveedorConContactos.Contactos.First();
        contacto.GetType().GetProperty("ProveedorId")?.SetValue(contacto, Guid.NewGuid());
        
        var command = new EliminarContactoCommand
        {
            Id = contacto.Id,
            ProveedorId = _proveedorId,
            MotivoEliminacion = "Test motivo"
        };
        
        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(proveedorConContactos);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();

        // Verificar logging de warning para proveedor incorrecto
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Intento de eliminar contacto") && v.ToString()!.Contains("con proveedor incorrecto")),
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error inesperado al eliminar contacto")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(TipoEliminacion.Logica, "Contacto ya no trabaja aquí")]
    [InlineData(TipoEliminacion.Fisica, "Eliminar permanentemente por reorganización")]
    public async Task Handle_ConDiferentesTiposEliminacion_DeberiaFuncionarCorrectamente(
        TipoEliminacion tipoEliminacion, string motivo)
    {
        // Arrange
        var command = new EliminarContactoCommand
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            MotivoEliminacion = motivo,
            TipoEliminacion = tipoEliminacion
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.GuardarCambiosAsync(CancellationToken.None))
                      .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockRepository.Verify(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None), Times.Once);
        _mockRepository.Verify(r => r.GuardarCambiosAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ConConstructorParametros_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var command = new EliminarContactoCommand(_contactoId, _proveedorId, "Motivo de test");

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.GuardarCambiosAsync(CancellationToken.None))
                      .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        command.Id.Should().Be(_contactoId);
        command.ProveedorId.Should().Be(_proveedorId);
        command.MotivoEliminacion.Should().Be("Motivo de test");
    }

    [Fact]
    public async Task Handle_ConParametrosAdicionales_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var command = new EliminarContactoCommand
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            MotivoEliminacion = "Test con parámetros adicionales",
            ReasignarContactoPrincipal = true,
            NuevoContactoPrincipalId = _contactoId2,
            ForzarEliminacion = true,
            DatosAdicionales = new Dictionary<string, object>
            {
                ["usuario"] = "test-user",
                ["fecha"] = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.ObtenerPorIdAsync(command.ProveedorId, true, true, CancellationToken.None))
                      .ReturnsAsync(_proveedorConContactos);

        _mockRepository.Setup(r => r.ActualizarAsync(_proveedorConContactos, CancellationToken.None))
                      .Returns(Task.CompletedTask);

        _mockRepository.Setup(r => r.GuardarCambiosAsync(CancellationToken.None))
                      .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().BeTrue();
        
        command.ReasignarContactoPrincipal.Should().BeTrue();
        command.NuevoContactoPrincipalId.Should().Be(_contactoId2);
        command.ForzarEliminacion.Should().BeTrue();
        command.DatosAdicionales.Should().NotBeNull();
        command.DatosAdicionales.Should().ContainKey("usuario");
    }

    #region Helper Methods

    private EliminarContactoCommand CrearCommandValido()
    {
        return new EliminarContactoCommand
        {
            Id = _contactoId,
            ProveedorId = _proveedorId,
            MotivoEliminacion = "Contacto cambió de trabajo",
            TipoEliminacion = TipoEliminacion.Logica
        };
    }

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
            "México",                     // pais
            "XAXX010101000",              // rfc
            "Banco Test - 1234567890",    // informacionBancaria
            30                            // diasCredito
        );

        proveedor.Activar();
        proveedor.GetType().GetProperty("Id")?.SetValue(proveedor, _proveedorId);
        
        // Agregar múltiples contactos
        var contacto1 = proveedor.AgregarContacto(
            "Contacto Principal",
            "Gerente General",
            "555-111-1111",
            "gerente@proveedor.com",
            true);
        
        var contacto2 = proveedor.AgregarContacto(
            "Contacto Secundario",
            "Supervisor de Ventas",
            "555-222-2222",
            "ventas@proveedor.com",
            false);

        // Establecer IDs específicos para los contactos
        contacto1.GetType().GetProperty("Id")?.SetValue(contacto1, _contactoId);
        contacto2.GetType().GetProperty("Id")?.SetValue(contacto2, _contactoId2);
        
        return proveedor;
    }

    private Domain.Proveedores.Entities.Proveedor CrearProveedorConUnicoContacto()
    {
        var proveedor = Domain.Proveedores.Entities.Proveedor.Crear(
            "Proveedor Único SA",         // nombre
            "María González",             // nombreContacto
            "maria@unico.com",            // email
            "555-987-6543",               // telefono
            "Avenida Única 456",          // direccion
            "Ciudad Única",               // ciudad
            "54321",                      // codigoPostal
            "México",                     // pais
            "XAXX010101001",              // rfc
            "Banco Único - 0987654321",   // informacionBancaria
            15                            // diasCredito
        );

        proveedor.Activar();
        proveedor.GetType().GetProperty("Id")?.SetValue(proveedor, _proveedorId);
        
        // Agregar solo un contacto
        var contactoUnico = proveedor.AgregarContacto(
            "Único Contacto",
            "Propietario",
            "555-999-9999",
            "unico@proveedor.com",
            true);

        // Establecer ID específico para el contacto
        contactoUnico.GetType().GetProperty("Id")?.SetValue(contactoUnico, _contactoId);
        
        return proveedor;
    }

    #endregion
} 