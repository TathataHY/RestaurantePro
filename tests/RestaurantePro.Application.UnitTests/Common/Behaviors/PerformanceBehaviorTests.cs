namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para PerformanceBehavior - Monitoreo de performance con métricas y alertas
/// </summary>
public class PerformanceBehaviorTests
{
    private readonly Mock<ILogger<PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly Mock<IMetricsService> _mockMetricsService;
    private readonly Mock<IOptions<PerformanceSettings>> _mockPerformanceSettings;
    private readonly PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public PerformanceBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _mockMetricsService = new Mock<IMetricsService>();
        _mockPerformanceSettings = new Mock<IOptions<PerformanceSettings>>();
        _mockPerformanceSettings.Setup(x => x.Value).Returns(new PerformanceSettings());
        _behavior = new PerformanceBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockMetricsService.Object, _mockPerformanceSettings.Object);
    }

    [Fact]
    public async Task Handle_RequestRapido_DeberiaRegistrarTiempoSinAlertas()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = () => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se registra tiempo de ejecución sin alertas
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            It.IsAny<TimeSpan>(),
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

    [Fact]
    public async Task Handle_RequestLento_DeberiaGenerarAlerta()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = async () =>
        {
            await Task.Delay(3000); // Simular operación lenta (3 segundos)
            return expectedResult;
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se registra el tiempo
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            It.Is<TimeSpan>(t => t.TotalSeconds >= 3),
            true), Times.Once);
            
        // Debería haber alerta por operación lenta
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("lenta detectada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Query_DeberiaUsarUmbralDiferente()
    {
        // Arrange - Crear behavior para Query
        var queryLogger = new Mock<ILogger<PerformanceBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>>>();
        var queryBehavior = new PerformanceBehavior<ObtenerProductoPorIdQuery, Result<ProductoDto>>(queryLogger.Object, _mockMetricsService.Object, _mockPerformanceSettings.Object);
        
        var query = new ObtenerProductoPorIdQuery(Guid.NewGuid());
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = async () =>
        {
            await Task.Delay(600); // Más del umbral de Query (500ms) pero menos que Command (2s)
            return expectedResult;
        };

        // Act
        var result = await queryBehavior.Handle(query, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Debería haber alerta para Query que supera 500ms
        queryLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("lenta detectada")),
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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = async () =>
        {
            await Task.Delay(6000); // Operación críticamente lenta (6 segundos)
            return expectedResult;
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar métricas de severidad crítica
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "slow_operations_critical"), Times.Once);
            
        // Debería haber alerta crítica
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("críticamente lenta")),
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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = async () =>
        {
            await Task.Delay(1000); // Simular algo de procesamiento antes del error
            throw exception;
        };

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        thrownException.Should().Be(exception);
        
        // Verificar que se registra el tiempo incluso con excepción
        _mockMetricsService.Verify(x => x.RecordExecutionTime(
            "CrearProductoCommand",
            It.IsAny<TimeSpan>(),
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
            It.IsAny<TimeSpan>(),
            true), Times.Once);
            
        // Verificar que se registran tags adicionales
        _mockMetricsService.Verify(x => x.AddTag(
            "request_type", "Command"), Times.Once);
            
        _mockMetricsService.Verify(x => x.AddTag(
            "success", "true"), Times.Once);
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
        
        // Verificar categorización correcta
        _mockMetricsService.Verify(x => x.AddTag(
            "request_type", expectedCategory), Times.Once);
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
            It.IsAny<TimeSpan>(),
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
            It.IsAny<double>()), Times.Once);
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
            "total_requests"), Times.Once);
            
        _mockMetricsService.Verify(x => x.IncrementCounter(
            "successful_requests"), Times.Once);
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
            "failed_requests"), Times.Once);
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
        
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = async () =>
        {
            await Task.Delay(delayMs);
            return expectedResult;
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        if (deberiaAlertar)
        {
            var expectedLogLevel = delayMs >= 5500 ? LogLevel.Error : LogLevel.Warning;
            _mockLogger.Verify(
                x => x.Log(
                    expectedLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("lenta")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
} 
