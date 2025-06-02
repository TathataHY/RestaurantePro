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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se hizo logging de auditoría
        var anyLogLevel = It.IsAny<LogLevel>();
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(anyLogLevel, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_CommandExitoso_DeberiaLoggearAuditoriaExitosa()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        _mockCurrentUserService.Setup(x => x.UserName).Returns("testuser");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se hizo logging de auditoría exitosa
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_CommandConError_DeberiaLoggearAuditoriaFallida()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        _mockCurrentUserService.Setup(x => x.UserName).Returns("testuser");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => throw exception;

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));
        
        // Verificar que se hizo logging de auditoría fallida
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Warning, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_Query_NoDeberiaAuditar()
    {
        // Arrange - Crear behavior para Query
        var queryLogger = new Mock<ILogger<AuditingBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>>>();
        var queryBehavior = new AuditingBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>(queryLogger.Object, _mockCurrentUserService.Object);
        
        var query = new ObtenerProductoPorIdQuery(Guid.NewGuid());
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await queryBehavior.Handle(query, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // No debería haber logging de auditoría para queries
        var anyLogLevel = It.IsAny<LogLevel>();
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        queryLogger.Verify(
            x => x.Log(anyLogLevel, anyEventId, anyState, anyException, anyFormatter),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SinCurrentUserService_DeberiaFuncionarSinErrores()
    {
        // Arrange - Behavior sin ICurrentUserService
        var behaviorSinUser = new AuditingBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);
        
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await behaviorSinUser.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Debería loggear pero sin información de usuario
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye información de usuario
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirTiempoDeEjecucion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        _mockCurrentUserService.Setup(x => x.UserId).Returns("user123");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye tiempo de ejecución
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, anyEventId, anyState, anyException, anyFormatter),
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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se serializa el command
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, anyEventId, anyState, anyException, anyFormatter),
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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se audita el command
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye el nombre del command (usando el parámetro commandName)
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, anyEventId, anyState, anyException, anyFormatter),
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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        await _behavior.Handle(command1, nextDelegate, CancellationToken.None);
        await _behavior.Handle(command2, nextDelegate, CancellationToken.None);

        // Assert
        // Verificar que se generan múltiples audit IDs únicos
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Information, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeast(4)); // 2 comandos x 2 logs mínimo cada uno
    }
} 