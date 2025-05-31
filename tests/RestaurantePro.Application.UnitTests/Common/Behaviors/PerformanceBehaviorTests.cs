using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductoPorId;

namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para PerformanceBehavior - Monitoreo de performance con métricas y alertas
/// </summary>
public class PerformanceBehaviorTests
{
    private readonly Mock<ILogger<PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly Mock<IMetricsService> _mockMetricsService;
    private readonly PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public PerformanceBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _mockMetricsService = new Mock<IMetricsService>();
        _behavior = new PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockMetricsService.Object);
    }

    [Fact]
    public async Task Handle_RequestRapido_DeberiaRegistrarTiempoSinAlertas()
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
        
        // Verificar que se registra tiempo de ejecución sin alertas
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            It.IsAny<TimeSpan>(),
            It.IsAny<Dictionary<string, object>>()), Times.Once);
            
        // No debería haber logging de alertas para requests rápidos
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_RequestLento_DeberiaGenerarAlerta()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x())
            .Returns(async () =>
            {
                await Task.Delay(3000); // Simular operación lenta (3 segundos)
                return expectedResult;
            });

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se registra el tiempo
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            It.Is<TimeSpan>(t => t.TotalSeconds >= 3),
            It.IsAny<Dictionary<string, object>>()), Times.Once);
            
        // Debería haber alerta por operación lenta
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Operación lenta detectada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Query_DeberiaUsarUmbralDiferente()
    {
        // Arrange - Crear behavior para Query
        var queryLogger = new Mock<ILogger<PerformanceBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>>>();
        var queryBehavior = new PerformanceBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>(queryLogger.Object, _mockMetricsService.Object);
        
        var query = new ObtenerProductoPorIdQuery(Guid.NewGuid());
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x())
            .Returns(async () =>
            {
                await Task.Delay(600); // Más del umbral de Query (500ms) pero menos que Command (2s)
                return expectedResult;
            });

        // Act
        var result = await queryBehavior.Handle(query, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Debería haber alerta para Query que supera 500ms
        queryLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Operación lenta detectada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OperacionCriticamenteLenta_DeberiaGenerarAlertaCritica()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x())
            .Returns(async () =>
            {
                await Task.Delay(6000); // Operación críticamente lenta (6 segundos)
                return expectedResult;
            });

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar métricas de severidad crítica
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "slow_operations_critical",
            It.IsAny<Dictionary<string, object>>()), Times.Once);
            
        // Debería haber alerta crítica
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("🚨 Operación críticamente lenta")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcion_DeberiaRegistrarTiempoYPropagar()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x())
            .Returns(async () =>
            {
                await Task.Delay(1000); // Simular algo de procesamiento antes del error
                throw exception;
            });

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        thrownException.Should().Be(exception);
        
        // Verificar que se registra el tiempo incluso con excepción
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            It.IsAny<TimeSpan>(),
            It.Is<Dictionary<string, object>>(d => d.ContainsKey("success") && (bool)d["success"] == false)), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirMetadatosCompletos()
    {
        // Arrange
        var command = new CrearProductoCommand { 
            Nombre = "Pizza Especial",
            Descripcion = "Descripción detallada",
            Precio = 25.99m
        };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Especial" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluyen metadatos completos
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            It.IsAny<TimeSpan>(),
            It.Is<Dictionary<string, object>>(d => 
                d.ContainsKey("request_type") &&
                d.ContainsKey("success") &&
                d.ContainsKey("operation_category"))), Times.Once);
    }

    [Theory]
    [InlineData("CrearProductoCommand", "Command")]
    [InlineData("ObtenerProductoPorIdQuery", "Query")]
    public async Task Handle_DiferentesTiposRequest_DeberiaCategorizarCorrectamente(string requestType, string expectedCategory)
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
        
        // Verificar categorización correcta
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand", // Siempre será CrearProductoCommand en este test
            It.IsAny<TimeSpan>(),
            It.Is<Dictionary<string, object>>(d => 
                d.ContainsKey("operation_category") &&
                d["operation_category"].ToString() == "Command")), Times.Once);
    }

    [Fact]
    public async Task Handle_MultiplesEjecuciones_DeberiaRegistrarCadaUna()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        await _behavior.Handle(command, mockNext.Object, CancellationToken.None);
        await _behavior.Handle(command, mockNext.Object, CancellationToken.None);
        await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        // Verificar que se registran todas las ejecuciones
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            It.IsAny<TimeSpan>(),
            It.IsAny<Dictionary<string, object>>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_DeberiaRegistrarHistograma()
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
        
        // Verificar registro en histograma
        _mockMetricsService.Verify(x => x.RecordHistogram(
            "request_duration_ms",
            It.IsAny<double>(),
            It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConContadoresOperaciones_DeberiaIncrementarContadores()
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
        
        // Verificar contadores
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "operations_total",
            It.IsAny<Dictionary<string, object>>()), Times.Once);
            
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "operations_success",
            It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConError_DeberiaIncrementarContadorErrores()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));
        
        // Verificar contadores de error
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "operations_total",
            It.IsAny<Dictionary<string, object>>()), Times.Once);
            
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "operations_error",
            It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiaGenerarOperationId()
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
        
        // Verificar que se genera operation_id único
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("🏁 Operación completada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OperacionesSimultaneas_DeberiaGenerarIdsUnicos()
    {
        // Arrange
        var command1 = new CrearProductoCommand { Nombre = "Pizza Test 1" };
        var command2 = new CrearProductoCommand { Nombre = "Pizza Test 2" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var task1 = _behavior.Handle(command1, mockNext.Object, CancellationToken.None);
        var task2 = _behavior.Handle(command2, mockNext.Object, CancellationToken.None);
        
        await Task.WhenAll(task1, task2);

        // Assert
        // Verificar que se registran ambas operaciones con IDs únicos
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            It.IsAny<TimeSpan>(),
            It.IsAny<Dictionary<string, object>>()), Times.Exactly(2));
    }

    [Theory]
    [InlineData(100, false)]   // Rápido - sin alerta
    [InlineData(1500, false)]  // Medio - sin alerta para Command
    [InlineData(2500, true)]   // Lento - con alerta
    [InlineData(5500, true)]   // Muy lento - con alerta crítica
    public async Task Handle_DiferentiTempos_DeberiaGenerarAlertasApropiadas(int delayMs, bool deberiaAlertar)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x())
            .Returns(async () =>
            {
                await Task.Delay(delayMs);
                return expectedResult;
            });

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        if (deberiaAlertar)
        {
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("lenta")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
        else
        {
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
} 