namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para RetryBehavior - Comportamiento de reintentos con backoff exponencial
/// </summary>
public class RetryBehaviorTests
{
    private readonly Mock<ILogger<RetryBehavior<CrearProductoCommand, Result<ProductoDto>>>> _mockLogger;
    private readonly Mock<IOptions<RetrySettings>> _mockRetrySettings;
    private readonly RetryBehavior<CrearProductoCommand, Result<ProductoDto>> _behavior;

    public RetryBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<RetryBehavior<CrearProductoCommand, Result<ProductoDto>>>>();
        _mockRetrySettings = new Mock<IOptions<RetrySettings>>();
        
        var retrySettings = new RetrySettings
        {
            MaxAttempts = 3,
            BaseDelayMs = 100,
            MaxDelayMs = 30000,
            Enabled = true
        };
        
        _mockRetrySettings.Setup(x => x.Value).Returns(retrySettings);
        _behavior = new RetryBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockRetrySettings.Object);
    }

    [Fact]
    public async Task Handle_RequestExitoso_NoDeberiaReintentar()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(1); // Solo una ejecución, sin reintentos
    }

    [Fact]
    public async Task Handle_ExcepcionTransitoria_DeberiaReintentar()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            if (callCount == 1)
                throw new TimeoutException("Timeout transitorio");
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(2); // Primera falla + reintento exitoso
    }

    [Fact]
    public async Task Handle_ExcepcionNoTransitoria_NoDeberiaReintentar()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            throw new ArgumentException("Parámetro inválido");
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // No debe reintentar para excepciones no transitorias
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_MaximosReintentos_DeberiaLanzarUltimaExcepcion()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            throw new TimeoutException("Timeout persistente");
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Timeout persistente");
        
        // Debería haber intentado el máximo de reintentos (3 por defecto + 1 intento inicial = 4)
        callCount.Should().Be(4);
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
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            if (callCount == 1)
                throw transitoryException;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_BackoffExponencial_DeberiaEsperarTiemposCrecientes()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        var expectedResult = Result.Success(new ProductoDto { Nombre = "Pizza Test" });
        
        var tiemposEjecucion = new List<DateTime>();
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            tiemposEjecucion.Add(DateTime.UtcNow);
            if (callCount <= 2)
                throw new TimeoutException("Timeout");
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

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
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            cancellationTokenSource.Cancel(); // Cancelar en primera ejecución
            throw new TimeoutException("Timeout");
        };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _behavior.Handle(command, nextDelegate, cancellationTokenSource.Token));

        // No debería continuar reintentando después de cancelación
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ConJitter_DeberiaVariarTiemposDeEspera()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        // Crear múltiples behaviors para probar variabilidad del jitter
        var behaviors = Enumerable.Range(0, 5)
            .Select(_ => new RetryBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockRetrySettings.Object))
            .ToList();
        
        var tiemposDelay = new List<TimeSpan>();
        
        foreach (var behaviorTest in behaviors)
        {
            var inicio = DateTime.UtcNow;
            int callCount = 0;
            RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
            {
                callCount++;
                if (callCount == 1)
                    throw new TimeoutException("Test");
                return Task.FromResult(Result.Success(new ProductoDto()));
            };

            try
            {
                await behaviorTest.Handle(command, nextDelegate, CancellationToken.None);
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
    public async Task Handle_RetryPolicyCustom_DeberiaUsarConfiguracion()
    {
        // Arrange
        var customSettings = new RetrySettings
        {
            MaxAttempts = 2, // Solo 2 reintentos
            BaseDelayMs = 50,
            Enabled = true
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var customBehavior = new RetryBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, mockCustomSettings.Object);
        
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            throw new TimeoutException("Timeout persistente");
        };

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => 
            customBehavior.Handle(command, nextDelegate, CancellationToken.None));

        // Debería haber intentado solo 3 veces total (1 inicial + 2 reintentos)
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_MultiplesFallas_DeberiaLoggearCadaReintento()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            throw new TimeoutException("Timeout persistente");
        };

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Verificar que se loggea cada reintento
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Reintentando")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ExcepcionCompleja_DeberiaSerializarContexto()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Compleja" };
        var excepcionCompleja = new InvalidOperationException("Operación compleja falló");
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            if (callCount == 1)
                throw excepcionCompleja;
            return Task.FromResult(Result.Success(new ProductoDto { Nombre = "Pizza Compleja" }));
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Verificar que se loggea información detallada del contexto
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Pizza Compleja")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.AtLeastOnce);
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
        
        int callCount = 0;
        RequestHandlerDelegate<Result<ProductoDto>> nextDelegate = _ => 
        {
            callCount++;
            if (callCount <= numeroReintento)
                throw new TimeoutException($"Fallo {callCount}");
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se ejecutó el número correcto de veces
        callCount.Should().Be(numeroReintento + 1);
        
        // Verificar que se loggeó el número específico de reintento
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Reintento {numeroReintento}")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
} 
