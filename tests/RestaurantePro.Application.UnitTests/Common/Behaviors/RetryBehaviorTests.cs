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
            MaxRetryAttempts = 3,
            BaseDelayMilliseconds = 100,
            MaxDelayMilliseconds = 30000,
            UseExponentialBackoff = true,
            UseJitter = true
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
            .Select(_ => new RetryBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, _mockRetrySettings.Object))
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
    public async Task Handle_RetryPolicyCustom_DeberiaUsarConfiguracion()
    {
        // Arrange
        var customSettings = new RetrySettings
        {
            MaxRetryAttempts = 2, // Solo 2 reintentos
            BaseDelayMilliseconds = 50,
            UseExponentialBackoff = false,
            UseJitter = false
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var customBehavior = new RetryBehavior<CrearProductoCommand, Result<ProductoDto>>(_mockLogger.Object, mockCustomSettings.Object);
        
        var command = new CrearProductoCommand { Nombre = "Pizza Test" };
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.Setup(x => x()).ThrowsAsync(new TimeoutException("Timeout persistente"));

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => 
            customBehavior.Handle(command, mockNext.Object, CancellationToken.None));

        // Debería haber intentado solo 3 veces total (1 inicial + 2 reintentos)
        mockNext.Verify(x => x(), Times.Exactly(3));
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

        // Verificar que se loggea cada reintento
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Reintento")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ExcepcionCompleja_DeberiaSerializarContexto()
    {
        // Arrange
        var command = new CrearProductoCommand { Nombre = "Pizza Compleja" };
        var excepcionCompleja = new InvalidOperationException("Operación compleja falló");
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        mockNext.SetupSequence(x => x())
            .ThrowsAsync(excepcionCompleja)
            .ReturnsAsync(Result.Success(new ProductoDto { Nombre = "Pizza Compleja" }));

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Verificar que se loggea información detallada del contexto
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Pizza Compleja")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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
        
        var mockNext = new Mock<RequestHandlerDelegate<Result<ProductoDto>>>();
        
        // Configurar fallos según el número de reintento específico
        var sequence = mockNext.SetupSequence(x => x());
        for (int i = 0; i < numeroReintento; i++)
        {
            sequence = sequence.ThrowsAsync(new TimeoutException($"Fallo {i + 1}"));
        }
        sequence.ReturnsAsync(expectedResult);

        // Act
        var result = await _behavior.Handle(command, mockNext.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // Verificar que se ejecutó el número correcto de veces
        mockNext.Verify(x => x(), Times.Exactly(numeroReintento + 1));
        
        // Verificar que se loggeó el número específico de reintento
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Reintento {numeroReintento}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
} 
