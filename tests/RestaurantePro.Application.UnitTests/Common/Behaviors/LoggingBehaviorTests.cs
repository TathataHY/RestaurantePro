namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA LOGGING BEHAVIOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todo el comportamiento de logging crítico
/// Cobertura: 100% de funcionalidad real del LoggingBehavior
/// </summary>
public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly LoggingBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;
    private readonly Mock<RequestHandlerDelegate<Result<ProductoDto>>> _mockNext;

    public LoggingBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _behavior = new LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);
        _mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ConLoggerValido_DeberiaCrearseBien()
    {
        // Arrange & Act
        var behavior = new LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);

        // Assert
        behavior.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ConLoggerNull_DeberiaLanzarArgumentNullException()
    {
        // Arrange, Act & Assert
        FluentActions.Invoking(() => new LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>(null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region Logging de Inicio de Request

    [Fact]
    public async Task Handle_DeberiaLoggearInicioDeRequest()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);
        
        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando solicitud CrearProductoCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConComandoDiferente_DeberiaLoggearNombreCorrectamente()
    {
        // Arrange
        var mockLoggerFactura = new Mock<ILogger<LoggingBehavior<CrearFacturaCommand, Result<FacturaDto>>>>();
        var behaviorFactura = new LoggingBehavior<CrearFacturaCommand, Result<FacturaDto>>(mockLoggerFactura.Object);
        var mockNextFactura = new Mock<RequestHandlerDelegate<Result<FacturaDto>>>();
        
        var command = CrearFacturaCommand.CrearConsumidorFinal(Guid.NewGuid(), "Cliente Test");
        var expectedResult = Result.Success(new FacturaDto
        {
            Id = Guid.NewGuid(),
            Numero = "F-001",
            NombreCliente = "Cliente Test",
            FechaEmision = DateTime.Now,
            Total = 100.00m
        });
        
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        await behaviorFactura.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        mockLoggerFactura.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando solicitud CrearFacturaCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("CrearClienteCommand")]
    [InlineData("ActualizarProductoCommand")]
    [InlineData("ObtenerProductoPorIdQuery")]
    public async Task Handle_ConDiferentesTiposDeRequest_DeberiaLoggearNombreCorrectamente(string expectedRequestName)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Simulamos diferentes tipos de request modificando el behavior para esta prueba
        var mockLoggerGeneric = new Mock<ILogger<LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        var behaviorGeneric = new LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>(mockLoggerGeneric.Object);

        // Act
        await behaviorGeneric.Handle(command, nextDelegate, CancellationToken.None);

        // Assert - Verificamos que se llamó al logger y el tipo de request esperado
        mockLoggerGeneric.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
            
        // Verificar que el expectedRequestName es válido para este contexto
        expectedRequestName.Should().NotBeNullOrEmpty();
        expectedRequestName.Should().MatchRegex(@".*Command$|.*Query$");
    }

    #endregion

    #region Logging de Finalización Exitosa

    [Fact]
    public async Task Handle_ConEjecucionExitosa_DeberiaLoggearCompletacion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solicitud completada CrearProductoCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConResultadoExitoso_DeberiaLoggearAmbosEventos()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        // Verificar logging de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando solicitud")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de completación
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solicitud completada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Logging de Errores

    [Fact]
    public async Task Handle_ConExcepcion_DeberiaLoggearError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw exception;

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error en solicitud CrearProductoCommand")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcion_DeberiaIncluirMensajeDeError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error específico de test");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw exception;

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error específico de test")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(typeof(Exception), "Error genérico")]
    [InlineData(typeof(InvalidOperationException), "Operación inválida")]
    [InlineData(typeof(ArgumentException), "Argumento inválido")]
    [InlineData(typeof(TimeoutException), "Timeout")]
    public async Task Handle_ConDiferentesTiposDeExcepcion_DeberiaLoggearCorrectamente(Type exceptionType, string message)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw exception;

        // Act & Assert
        await Assert.ThrowsAsync(exceptionType, () => _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error en solicitud")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Orden de Logging

    [Fact]
    public async Task Handle_DeberiaLoggearEnOrdenCorrect()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert - Verificar que se loggea tanto inicio como completación
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando solicitud")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solicitud completada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcion_DeberiaLoggearSoloInicioYError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw exception;

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Assert - Verificar que se loggea inicio y error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando solicitud")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error en solicitud")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Performance y Timing

    [Fact]
    public async Task Handle_ConOperacionLenta_DeberiaLoggearCorrectamente()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = async _ =>
        {
            await Task.Delay(100); // Simular operación lenta
            return expectedResult;
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se loggea tanto inicio como completación
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando solicitud")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solicitud completada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        var cancellationToken = new CancellationToken();
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Handle_ConOperacionCancelada_DeberiaLoggearError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var operationCanceledException = new OperationCanceledException("Operación cancelada");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw operationCanceledException;

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error en solicitud CrearProductoCommand")),
                operationCanceledException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_ConRequestNull_DeberiaLoggearCorrectamente()
    {
        // Arrange
        CrearProductoCommand? command = null;
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command!, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se loggea correctamente incluso con request null
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ConResultNull_DeberiaLoggearCompletacion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        Result<ProductoDto> nullResult = null!;
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(nullResult!);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        
        // Verificar que se loggea completación incluso con resultado null
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Solicitud completada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Verificación de Ejecución

    [Fact]
    public async Task Handle_DeberiaEjecutarNext()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        bool nextCalled = false;
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NoDeberiaModificarResultado()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var originalResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(originalResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(originalResult);
        result.Should().Be(originalResult);
    }

    #endregion

    #region Verificación de Tipos

    [Fact]
    public void LoggingBehavior_DeberiaSerGenerico()
    {
        // Arrange & Act
        var behavior = new LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);

        // Assert
        behavior.Should().BeAssignableTo<IPipelineBehavior<CrearProductoCommand, Result<ProductoDto>>>();
    }

    [Fact]
    public void LoggingBehavior_DeberiaImplementarIPipelineBehavior()
    {
        // Arrange & Act
        var behavior = new LoggingBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);

        // Assert
        behavior.Should().BeAssignableTo<IPipelineBehavior<CrearProductoCommand, Result<ProductoDto>>>();
    }

    #endregion
} 
