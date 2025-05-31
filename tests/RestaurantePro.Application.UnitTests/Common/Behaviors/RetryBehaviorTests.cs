using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.DTOs;

namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para RetryBehavior - Reintentos automáticos con backoff exponencial
/// </summary>
public class RetryBehaviorTests
{
    private readonly Mock<ILogger<RetryBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly Mock<IMetricsService> _mockMetricsService;
    private readonly RetryBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public RetryBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<RetryBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _mockMetricsService = new Mock<IMetricsService>();
        _behavior = new RetryBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockMetricsService.Object);
    }

    [Fact]
    public async Task Handle_RequestExitoso_NoDeberiaReintentar()
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
        mockNext.Verify(x => x(), Times.Once); // Solo una ejecución, sin reintentos
    }

    [Fact]
    public async Task Handle_ExcepcionTransitoria_DeberiaReintentar()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        
        // Primera llamada: falla con excepción transitoria
        // Segunda llamada: exitosa
        mockNext.SetupSequence(x => x())
            .ThrowsAsync(new TimeoutException("Timeout transitorio"))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        mockNext.Verify(x => x(), Times.Exactly(2)); // Primera falla + reintento exitoso
    }

    [Fact]
    public async Task Handle_ExcepcionNoTransitoria_NoDeberiaReintentar()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(new ArgumentException("Parámetro inválido"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        // No debe reintentar para excepciones no transitorias
        mockNext.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_MaximosReintentos_DeberiaLanzarUltimaExcepcion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(new TimeoutException("Timeout persistente"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        exception.Message.Should().Contain("Timeout persistente");
        
        // Debería haber intentado el máximo de reintentos (3 por defecto + 1 intento inicial = 4)
        mockNext.Verify(x => x(), Times.Exactly(4));
    }

    [Theory]
    [InlineData(typeof(TimeoutException))]
    [InlineData(typeof(TaskCanceledException))]
    [InlineData(typeof(HttpRequestException))]
    [InlineData(typeof(SocketException))]
    public async Task Handle_ExcepcionesTransitorias_DeberiaReintentar(Type exceptionType)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var transitoryException = (Exception)Activator.CreateInstance(exceptionType, "Error transitorio")!;
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.SetupSequence(x => x())
            .ThrowsAsync(transitoryException)
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        mockNext.Verify(x => x(), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_BackoffExponencial_DeberiaEsperarTiemposCrecientes()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var tiemposEjecucion = new List<DateTime>();
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x())
            .Callback(() => tiemposEjecucion.Add(DateTime.UtcNow))
            .ThrowsAsync(new TimeoutException("Timeout"))
            .Callback(() => tiemposEjecucion.Add(DateTime.UtcNow))
            .ThrowsAsync(new TimeoutException("Timeout"))
            .Callback(() => tiemposEjecucion.Add(DateTime.UtcNow))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        tiemposEjecucion.Should().HaveCount(3);
        
        // Verificar que hay delays entre ejecuciones (backoff exponencial)
        var delay1 = tiemposEjecucion[1] - tiemposEjecucion[0];
        var delay2 = tiemposEjecucion[2] - tiemposEjecucion[1];
        
        delay1.Should().BeGreaterThan(TimeSpan.Zero);
        delay2.Should().BeGreaterThan(delay1); // Backoff exponencial
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var cancellationTokenSource = new CancellationTokenSource();
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x())
            .Callback(() => cancellationTokenSource.Cancel()) // Cancelar en primera ejecución
            .ThrowsAsync(new TimeoutException("Timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _behavior.Handle(command, mockNext.Object, cancellationTokenSource.Token));

        // No debería continuar reintentando después de cancelación
        mockNext.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConJitter_DeberiaVariarTiemposDeEspera()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        // Crear múltiples behaviors para probar variabilidad del jitter
        var behaviors = Enumerable.Range(0, 5)
            .Select(_ => new RetryBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockMetricsService.Object))
            .ToList();
        
        var tiemposDelay = new List<TimeSpan>();
        
        foreach (var behaviorTest in behaviors)
        {
            var inicio = DateTime.UtcNow;
            var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
            mockNext.SetupSequence(x => x())
                .ThrowsAsync(new TimeoutException("Test"))
                .ReturnsAsync(Result.Success(new ProductoDto()));

            try
            {
                await behaviorTest.Handle(command, mockNext.Object, CancellationToken.None);
                var tiempoTotal = DateTime.UtcNow - inicio;
                tiemposDelay.Add(tiempoTotal);
            }
            catch
            {
                // Ignorar excepciones para este test específico
            }
        }

        // Assert
        // Con jitter, los tiempos deberían variar entre ejecuciones
        var tiemposDistintos = tiemposDelay.Distinct().Count();
        tiemposDistintos.Should().BeGreaterThan(1, "El jitter debería producir tiempos diferentes");
    }

    [Fact]
    public async Task Handle_ReintentoExitoso_DeberiaLoggearMetricas()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.SetupSequence(x => x())
            .ThrowsAsync(new TimeoutException("Timeout"))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar logging de reintento
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Reintentando")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        // Verificar métricas de reintentos
        _mockMetricsService.Verify(x => x.IncrementCounter("retry_attempts", It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultiplesFallas_DeberiaLoggearCadaReintento()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(new TimeoutException("Timeout persistente"));

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => 
            _behavior.Handle(command, mockNext.Object, CancellationToken.None));

        // Verificar que se loggea cada reintento (3 reintentos)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Reintentando")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(3));
            
        // Verificar logging de agotamiento de reintentos
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Agotados los reintentos")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExcepcionCompleja_DeberiaSerializarContexto()
    {
        // Arrange
        var command = new CrearProductoCommand { 
            Nombre = "Pizza Compleja",
            Descripcion = "Descripción detallada",
            Precio = 25.99m
        };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Compleja" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.SetupSequence(x => x())
            .ThrowsAsync(new TimeoutException("Database timeout"))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se incluye contexto del command en logs
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Pizza Compleja")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Handle_ReintentoEnNumeroEspecifico_DeberiaLoggearNumeroCorrect(int numeroReintento)
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        
        // Configurar fallas hasta el reintento específico
        var setupSequence = mockNext.SetupSequence(x => x());
        for (int i = 0; i < numeroReintento; i++)
        {
            setupSequence = setupSequence.ThrowsAsync(new TimeoutException($"Timeout {i + 1}"));
        }
        setupSequence.ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se loggea el número correcto de reintento
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Intento {numeroReintento}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
} 