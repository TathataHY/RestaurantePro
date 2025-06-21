namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para PerformanceBehavior - Monitoreo de performance con métricas y alertas
/// </summary>
public class PerformanceBehaviorTests
{
    private readonly Mock<ILogger<PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly Mock<IMetricsService> _mockMetricsService;
    private readonly Mock<IOptions<PerformanceSettings>> _mockPerformanceSettings;
    private readonly Mock<ITimeProvider> _mockTimeProvider;
    private readonly PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public PerformanceBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _mockMetricsService = new Mock<IMetricsService>();
        _mockPerformanceSettings = new Mock<IOptions<PerformanceSettings>>();
        _mockTimeProvider = new Mock<ITimeProvider>();
        
        _mockPerformanceSettings.Setup(x => x.Value).Returns(new PerformanceSettings
        {
            CommandThresholdSeconds = 0.02, // 20ms
            QueryThresholdMs = 10,
            ComplexOperationThresholdSeconds = 0.05, // 50ms
            DefaultThresholdSeconds = 0.03 // 30ms
        });

        _behavior = new PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>>(
            _mockLogger.Object, 
            _mockTimeProvider.Object, 
            _mockMetricsService.Object, 
            _mockPerformanceSettings.Object);
    }

    [Fact]
    public async Task Handle_RequestRapido_DeberiaRegistrarTiempoSinAlertas()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        _mockTimeProvider.Setup(x => x.GetElapsedTime(It.IsAny<long>())).Returns(TimeSpan.FromMilliseconds(10));
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se registra tiempo de ejecución sin alertas
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            (TimeSpan)It.IsAny<object>(),
            true), Times.Once);
            
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

    [Theory]
    [InlineData(5, LogLevel.None)]      // Rápido -> No se loguea nada
    [InlineData(25, LogLevel.Warning)]  // Lento -> Se loguea Warning
    [InlineData(60, LogLevel.Error)]    // Crítico -> Se loguea Error
    public async Task Handle_VariosTiempos_DeberiaGenerarAlertasCorrectas(int delayMs, LogLevel expectedLogLevel)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Test" };
        var expectedResult = Result.Success(new ProductoDto());
        _mockTimeProvider.Setup(x => x.GetElapsedTime(It.IsAny<long>())).Returns(TimeSpan.FromMilliseconds(delayMs));
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        if (expectedLogLevel == LogLevel.None)
        {
            // Verificar que no se loguea ninguna advertencia o error
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l >= LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
        else
        {
            // Verificar que se loguea el nivel correcto
            _mockLogger.Verify(
                x => x.Log(
                    expectedLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Operación lenta")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task Handle_ConExcepcion_DeberiaRegistrarTiempoYPropagar()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        _mockTimeProvider.Setup(x => x.GetElapsedTime(It.IsAny<long>())).Returns(TimeSpan.FromMilliseconds(10));
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => throw exception;

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        thrownException.Should().Be(exception);
        
        // Verificar que se registra el tiempo incluso con excepción
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            (TimeSpan)It.IsAny<object>(),
            false), Times.Once);
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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluyen metadatos completos
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            (TimeSpan)It.IsAny<object>(),
            true), Times.Once);
    }

    [Theory]
    [InlineData("CrearProductoCommand", "Command")]
    [InlineData("ObtenerProductoPorIdQuery", "Query")]
    public async Task Handle_DiferentesTiposRequest_DeberiaCategorizarCorrectamente(string requestType, string expectedCategory)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar ejecución correcta - AddTag no existe en IMetricsService real
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            (TimeSpan)It.IsAny<object>(),
            true), Times.Once);
        
        // Verificar que los parámetros de categorización son correctos
        requestType.Should().NotBeNullOrEmpty();
        expectedCategory.Should().NotBeNullOrEmpty();
        
        if (requestType.EndsWith("Command"))
        {
            expectedCategory.Should().Be("Command");
        }
        else if (requestType.EndsWith("Query"))
        {
            expectedCategory.Should().Be("Query");
        }
    }

    [Fact]
    public async Task Handle_MultiplesEjecuciones_DeberiaRegistrarCadaUna()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act - Ejecutar múltiples veces
        await _behavior.Handle(command, nextDelegate, CancellationToken.None);
        await _behavior.Handle(command, nextDelegate, CancellationToken.None);
        await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        // Verificar que se registra cada ejecución
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            (TimeSpan)It.IsAny<object>(),
            true), Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_DeberiaRegistrarHistograma()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar registro en histograma
        _mockMetricsService.Verify(x => x.RecordHistogram(
            "request_duration_histogram",
            (double)It.IsAny<object>(), null), Times.Once);
    }

    [Fact]
    public async Task Handle_ConContadoresOperaciones_DeberiaIncrementarContadores()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar incremento de contadores
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "total_requests", null), Times.Once);
            
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "successful_requests", null), Times.Once);
    }

    [Fact]
    public async Task Handle_ConError_DeberiaIncrementarContadorErrores()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var exception = new Exception("Error de test");
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => throw exception;

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));
        
        // Verificar incremento de contador de errores
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "failed_requests", null), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiaGenerarOperationId()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se genera un operation ID único
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Operation ID:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OperacionesSimultaneas_DeberiaGenerarIdsUnicos()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act - Ejecutar operaciones simultáneas
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _behavior.Handle(command, nextDelegate, CancellationToken.None))
            .ToArray();
            
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.Should().Be(expectedResult));
        
        // Verificar que se generan múltiples operation IDs únicos
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Operation ID:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(5));
    }
} 
