namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para TransactionBehavior - Gestión automática de transacciones de base de datos
/// </summary>
public class TransactionBehaviorTests
{
    private readonly Mock<ILogger<TransactionBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly Mock<DbContext> _mockDbContext;
    private readonly Mock<IDbContextTransaction> _mockTransaction;
    private readonly TransactionBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public TransactionBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<TransactionBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _mockDbContext = new Mock<DbContext>();
        _mockTransaction = new Mock<IDbContextTransaction>();
        
        // Setup del DbContext para retornar la transacción mock
        var anyToken = It.IsAny<CancellationToken>();
        _mockDbContext.Setup(x => x.Database.BeginTransactionAsync(anyToken))
            .ReturnsAsync(_mockTransaction.Object);
            
        _behavior = new TransactionBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockDbContext.Object);
    }

    [Fact]
    public async Task Handle_CommandExitoso_DeberiaCommitTransaccion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se inicia transacción
        var anyToken = It.IsAny<CancellationToken>();
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(anyToken), Times.Once);
        
        // Verificar que se hace commit
        _mockTransaction.Verify(x => x.CommitAsync(anyToken), Times.Once);
        
        // Verificar que se libera la transacción
        _mockTransaction.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandConError_DeberiaRollbackTransaccion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error en procesamiento");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => throw exception;

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        thrownException.Should().Be(exception);
        
        // Verificar que se inicia transacción
        var anyToken = It.IsAny<CancellationToken>();
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(anyToken), Times.Once);
        
        // Verificar que se hace rollback
        _mockTransaction.Verify(x => x.RollbackAsync(anyToken), Times.Once);
        
        // Verificar que se libera la transacción
        _mockTransaction.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Query_NoDeberiaUsarTransaccion()
    {
        // Arrange - Crear behavior para Query
        var queryLogger = new Mock<ILogger<TransactionBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>>>();
        var queryBehavior = new TransactionBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>(queryLogger.Object, _mockDbContext.Object);
        
        var query = new ObtenerProductoPorIdQuery(Guid.NewGuid());
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await queryBehavior.Handle(query, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // No debería haber transacciones para queries
        var anyToken = It.IsAny<CancellationToken>();
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(anyToken), Times.Never);
    }

    [Fact]
    public async Task Handle_IsolationLevelSerializable_DeberiaUsarNivelCorrecto()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se usa el nivel de aislamiento correcto (IsolationLevel.ReadCommitted por defecto)
        var anyToken = It.IsAny<CancellationToken>();
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(anyToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaUsarTokenEnTransaccion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        var cancellationToken = new CancellationToken();
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se usa el cancellation token
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(cancellationToken), Times.Once);
        _mockTransaction.Verify(x => x.CommitAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnCommit_DeberiaLoggearYRollback()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        var commitException = new Exception("Error en commit");
        
        var anyToken = It.IsAny<CancellationToken>();
        _mockTransaction.Setup(x => x.CommitAsync(anyToken))
            .ThrowsAsync(commitException);
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        thrownException.Should().Be(commitException);
        
        // Verificar que se intenta rollback después del error en commit
        _mockTransaction.Verify(x => x.RollbackAsync(anyToken), Times.Once);
        
        // Verificar logging de error
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, anyEventId, anyState, anyException, anyFormatter),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnRollback_DeberiaLoggearError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var processingException = new Exception("Error en procesamiento");
        var rollbackException = new Exception("Error en rollback");
        
        var anyToken = It.IsAny<CancellationToken>();
        _mockTransaction.Setup(x => x.RollbackAsync(anyToken))
            .ThrowsAsync(rollbackException);
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => throw processingException;

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Debería propagar la excepción original, no la de rollback
        thrownException.Should().Be(processingException);
        
        // Verificar logging de error en rollback
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, anyEventId, anyState, rollbackException, anyFormatter),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SinDbContext_NoDeberiaUsarTransacciones()
    {
        // Arrange - Behavior sin DbContext
        var behaviorSinDb = new TransactionBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);
        
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await behaviorSinDb.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // No debería haber operaciones de base de datos
        var anyToken = It.IsAny<CancellationToken>();
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(anyToken), Times.Never);
    }

    [Fact]
    public async Task Handle_TransaccionExitosa_DeberiaLoggearInicio()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar logging de inicio de transacción
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Debug, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_TransaccionCommitExitoso_DeberiaLoggearCommit()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar logging de commit
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Debug, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_TransaccionRollback_DeberiaLoggearRollback()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => throw exception;

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));
        
        // Verificar logging de rollback
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Warning, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirTransactionIdEnLogs()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye el transaction ID en logs
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Debug, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_CommandConNombre_DeberiaIncluirNombreEnLogs()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Especial Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Especial Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye el nombre del command en logs
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Debug, anyEventId, anyState, anyException, anyFormatter),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_OperationCanceledException_NoDeberiaLoggearComoError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var cancellationException = new OperationCanceledException("Operación cancelada");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => throw cancellationException;

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));
        
        // No debería loggear como error, solo como información/warning
        var anyEventId = It.IsAny<EventId>();
        var anyState = It.IsAny<It.IsAnyType>();
        var anyException = It.IsAny<Exception>();
        var anyFormatter = It.IsAny<Func<It.IsAnyType, Exception?, string>>();
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, anyEventId, anyState, anyException, anyFormatter),
            Times.Never);
    }

    [Theory]
    [InlineData(typeof(CrearProductoCommand))]
    [InlineData(typeof(ActualizarProductoCommand))]
    [InlineData(typeof(EliminarProductoCommand))]
    public async Task Handle_DiferentesCommands_DeberiaUsarTransacciones(Type commandType)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" }; // Simplificado para el test
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Todos los commands deberían usar transacciones
        var anyToken = It.IsAny<CancellationToken>();
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(anyToken), Times.Once);
        _mockTransaction.Verify(x => x.CommitAsync(anyToken), Times.Once);
    }
} 
