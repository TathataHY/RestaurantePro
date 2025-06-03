namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para TransactionBehavior - Gestión automática de transacciones de base de datos
/// </summary>
public class TransactionBehaviorTests
{
    private readonly Mock<ILogger<TransactionBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly TransactionBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public TransactionBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<TransactionBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _behavior = new TransactionBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_CommandExitoso_DeberiaCompletarseCorrectamente()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se loggea el inicio de transacción
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando transacción para CrearProductoCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        // Verificar que se loggea el commit exitoso
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Transacción confirmada exitosamente para CrearProductoCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommandConError_DeberiaLoggearRollback()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error en procesamiento");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => throw exception;

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        thrownException.Should().Be(exception);
        
        // Verificar que se loggea el error y rollback
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error en transacción para CrearProductoCommand")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Query_NoDeberiaUsarTransaccion()
    {
        // Arrange - Crear behavior para Query
        var queryLogger = new Mock<ILogger<TransactionBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>>>();
        var queryBehavior = new TransactionBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>(queryLogger.Object);
        
        var query = new ObtenerProductoPorIdQuery(Guid.NewGuid());
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        var result = await queryBehavior.Handle(query, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // No debería haber logging de transacciones para queries
        queryLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando transacción")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

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
    public async Task Handle_CommandConNombre_DeberiaIncluirNombreEnLogs()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que el nombre del command aparece en los logs
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CrearProductoCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
} 
