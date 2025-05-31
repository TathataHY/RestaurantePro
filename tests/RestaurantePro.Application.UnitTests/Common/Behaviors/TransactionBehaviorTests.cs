using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductoPorId;

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
        
        // Configurar el mock del DbContext
        _mockDbContext.Setup(x => x.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockTransaction.Object);
            
        _behavior = new TransactionBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockDbContext.Object);
    }

    [Fact]
    public async Task Handle_CommandExitoso_DeberiaCommitTransaccion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se inicia transacción
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar que se hace commit
        _mockTransaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar que se libera la transacción
        _mockTransaction.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CommandConError_DeberiaRollbackTransaccion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error en procesamiento");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        thrownException.Should().Be(exception);
        
        // Verificar que se inicia transacción
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar que se hace rollback
        _mockTransaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        
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
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await queryBehavior.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // No debería haber transacciones para queries
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IsolationLevelSerializable_DeberiaUsarNivelCorrecto()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se usa el nivel de aislamiento correcto (IsolationLevel.ReadCommitted por defecto)
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaUsarTokenEnTransaccion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        var cancellationToken = new CancellationToken();
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, cancellationToken);

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
        
        _mockTransaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(commitException);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        thrownException.Should().Be(commitException);
        
        // Verificar que se intenta rollback después del error en commit
        _mockTransaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar logging de error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error en commit")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnRollback_DeberiaLoggearError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var processingException = new Exception("Error en procesamiento");
        var rollbackException = new Exception("Error en rollback");
        
        _mockTransaction.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(rollbackException);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(processingException);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        // Debería propagar la excepción original, no la de rollback
        thrownException.Should().Be(processingException);
        
        // Verificar logging de error en rollback
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error en rollback")),
                rollbackException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SinDbContext_NoDeberiaUsarTransacciones()
    {
        // Arrange - Behavior sin DbContext
        var behaviorSinDb = new TransactionBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object);
        
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await behaviorSinDb.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // No debería intentar usar transacciones
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TransaccionExitosa_DeberiaLoggearInicio()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar logging de inicio de transacción
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("🔄 Iniciando transacción")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TransaccionCommitExitoso_DeberiaLoggearCommit()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar logging de commit exitoso
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("✅ Transacción confirmada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TransaccionRollback_DeberiaLoggearRollback()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));
        
        // Verificar logging de rollback
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Transacción revertida")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirTransactionIdEnLogs()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye TransactionId en logs
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TransactionId")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2)); // Al menos inicio y commit
    }

    [Fact]
    public async Task Handle_CommandConNombre_DeberiaIncluirNombreEnLogs()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Especial Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Especial Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye el nombre del command en logs
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CrearProductoCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(1));
    }

    [Fact]
    public async Task Handle_OperationCanceledException_NoDeberiaLoggearComoError()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var cancellationToken = new CancellationToken(canceled: true);
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(new OperationCanceledException(cancellationToken));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _behavior.Handle(command, mockNext.Object, cancellationToken));
        
        // Verificar que se hace rollback pero no se loggea como error
        _mockTransaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // No debería loggear como error para cancelaciones
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Theory]
    [InlineData(typeof(CrearProductoCommand))]
    [InlineData(typeof(ActualizarProductoCommand))]
    [InlineData(typeof(EliminarProductoCommand))]
    public async Task Handle_DiferentesCommands_DeberiaUsarTransacciones(Type commandType)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Todos los commands deberían usar transacciones
        _mockDbContext.Verify(x => x.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockTransaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
} 