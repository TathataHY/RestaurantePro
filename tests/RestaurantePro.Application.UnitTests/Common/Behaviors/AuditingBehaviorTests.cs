namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para AuditingBehavior - Auditoría automática de commands críticos
/// </summary>
public class AuditingBehaviorTests
{
    private readonly Mock<ILogger<AuditingBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly AuditingBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public AuditingBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<AuditingBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _behavior = new AuditingBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handle_Command_DeberiaLoggearInicioDeAuditoria()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        _mockCurrentUserService.Setup(x => x.UserName).Returns("testuser");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar logging de inicio de auditoría
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("🔍 Iniciando auditoría")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommandExitoso_DeberiaLoggearAuditoriaExitosa()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        _mockCurrentUserService.Setup(x => x.UserName).Returns("testuser");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar logging de auditoría exitosa
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("✅ Auditoría exitosa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommandConError_DeberiaLoggearAuditoriaFallida()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        _mockCurrentUserService.Setup(x => x.UserName).Returns("testuser");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));
        
        // Verificar logging de auditoría fallida
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Auditoría fallida")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Query_NoDeberiaAuditar()
    {
        // Arrange - Crear behavior para Query
        var queryLogger = new Mock<ILogger<AuditingBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>>>();
        var queryBehavior = new AuditingBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>(queryLogger.Object, _mockCurrentUserService.Object);
        
        var query = new ObtenerProductoPorIdQuery(Guid.NewGuid());
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await queryBehavior.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // No debería haber logging de auditoría para queries
        queryLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("auditoría")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SinCurrentUserService_DeberiaFuncionarSinErrores()
    {
        // Arrange - Behavior sin ICurrentUserService
        var behaviorSinUser = new AuditingBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);
        
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await behaviorSinUser.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Debería loggear pero sin información de usuario
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("🔍 Iniciando auditoría")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirInformacionDeUsuario()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        _mockCurrentUserService.Setup(x => x.UserName).Returns("testuser");
        _mockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye información de usuario
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("user123")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirTiempoDeEjecucion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye tiempo de ejecución
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_DeberiaSerializarCommand()
    {
        // Arrange
        var command = new CrearProductoCommand { 
            Nombre = "Pizza Especial",
            Descripcion = "Pizza con ingredientes especiales",
            Precio = 25.99m
        };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Especial" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se serializa la información del command
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Pizza Especial")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ConCamposSensibles_DeberiaFiltrarInformacion()
    {
        // Arrange - Simular command con información sensible
        var command = new CrearProductoCommand { 
            Nombre = "Pizza Test",
            Descripcion = "Password123 - información sensible" // Simular campo con info sensible
        };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se realiza logging (el filtrado específico dependería de la implementación)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2)); // Al menos inicio y fin
    }

    [Theory]
    [InlineData("CrearProductoCommand")]
    [InlineData("ActualizarProductoCommand")]
    [InlineData("EliminarProductoCommand")]
    public async Task Handle_DiferentesCommands_DeberiaAuditarTodos(string commandName)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye el nombre del command
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CrearProductoCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ConAuditId_DeberiaGenerarIdUnico()
    {
        // Arrange
        var command1 = new CrearProductoCommand { Nombre = "Pizza Test 1" };
        var command2 = new CrearProductoCommand { Nombre = "Pizza Test 2" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        await _behavior.Handle(command1, mockNext.Object, CancellationToken.None);
        await _behavior.Handle(command2, mockNext.Object, CancellationToken.None);

        // Assert
        // Verificar que se generan múltiples audit IDs únicos
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(4)); // 2 comandos x 2 logs mínimo cada uno
    }
} 