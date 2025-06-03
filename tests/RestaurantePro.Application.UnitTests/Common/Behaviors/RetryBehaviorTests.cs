namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para RetryBehavior - Comportamiento de reintentos con backoff exponencial
/// </summary>
public class RetryBehaviorTests
{
    private readonly Mock<ILogger<RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>>> _mockLogger;
    private readonly Mock<IOptions<RetrySettings>> _mockRetrySettings;
    private readonly RetryBehavior<CrearFacturaCommand, Result<FacturaDto>> _behavior;

    public RetryBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>>>();
        _mockRetrySettings = new Mock<IOptions<RetrySettings>>();
        
        var retrySettings = new RetrySettings
        {
            MaxAttempts = 3,
            BaseDelayMs = 100,
            MaxDelayMs = 30000,
            Enabled = true
        };
        
        _mockRetrySettings.Setup(x => x.Value).Returns(retrySettings);
        _behavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, _mockRetrySettings.Object);
    }

    [Fact]
    public async Task Handle_RequestExitoso_NoDeberiaReintentar()
    {
        // Arrange
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        var expectedResult = Result.Success(new FacturaDto { Id = Guid.NewGuid() });
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => {
            callCount++;
            return Task.FromResult(expectedResult);
        };

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
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        var expectedResult = Result.Success(new FacturaDto { Id = Guid.NewGuid() });
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
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
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
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
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
        {
            callCount++;
            throw new TimeoutException("Timeout persistente");
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        exception.Message.Should().Contain("Timeout persistente");
        
        // Debería haber intentado el máximo de reintentos (3 por defecto)
        callCount.Should().Be(3);
    }

    [Theory]
    [InlineData(typeof(TimeoutException))]
    [InlineData(typeof(TaskCanceledException))]
    [InlineData(typeof(HttpRequestException))]
    public async Task Handle_ExcepcionesTransitorias_DeberiaReintentar(Type exceptionType)
    {
        // Arrange
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        var expectedResult = Result.Success(new FacturaDto { Id = Guid.NewGuid() });
        
        var transitoryException = exceptionType == typeof(HttpRequestException) 
            ? new HttpRequestException("Error transitorio")
            : (Exception)Activator.CreateInstance(exceptionType, "Error transitorio")!;
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
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
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        var expectedResult = Result.Success(new FacturaDto { Id = Guid.NewGuid() });
        
        var tiemposEjecucion = new List<DateTime>();
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
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
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        var cancellationTokenSource = new CancellationTokenSource();
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
        {
            callCount++;
            if (callCount == 1)
            {
                cancellationTokenSource.Cancel(); // Cancelar en primera ejecución
                throw new TimeoutException("Timeout");
            }
            return Task.FromResult(Result.Success(new FacturaDto()));
        };

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => 
            _behavior.Handle(command, nextDelegate, cancellationTokenSource.Token));

        // Debería haber intentado al menos una vez
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ConJitter_DeberiaVariarTiemposDeEspera()
    {
        // Arrange
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        var expectedResult = Result.Success(new FacturaDto { Id = Guid.NewGuid() });
        
        var tiemposEjecucion = new List<DateTime>();
        var delays = new List<TimeSpan>();
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
        {
            callCount++;
            tiemposEjecucion.Add(DateTime.UtcNow);
            
            // Calcular delay si no es la primera ejecución
            if (tiemposEjecucion.Count > 1)
            {
                delays.Add(tiemposEjecucion.Last() - tiemposEjecucion[^2]);
            }
            
            if (callCount <= 3) // Fallar las primeras 3 veces
                throw new TimeoutException("Timeout temporal");
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // El jitter debería producir tiempos diferentes entre intentos
        var tiemposDistintos = delays.Select(d => d.TotalMilliseconds).Distinct().Count();
        tiemposDistintos.Should().BeGreaterThan(1, "El jitter debería producir tiempos diferentes");
    }

    [Fact]
    public async Task Handle_RetryPolicyCustom_DeberiaUsarConfiguracion()
    {
        // Arrange
        var customSettings = new RetrySettings
        {
            MaxAttempts = 2,
            BaseDelayMs = 50,
            MaxDelayMs = 5000
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var customBehavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, mockCustomSettings.Object);
        
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
        {
            callCount++;
            throw new TimeoutException("Siempre falla");
        };

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => 
            customBehavior.Handle(command, nextDelegate, CancellationToken.None));

        // Debería respetar MaxAttempts personalizado (2)
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_MultiplesFallas_DeberiaLoggearCadaReintento()
    {
        // Arrange
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
        {
            callCount++;
            throw new TimeoutException($"Fallo {callCount}");
        };

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // Verificar que se loggeó el reintento
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Reintentando")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ExcepcionCompleja_DeberiaSerializarContexto()
    {
        // Arrange
        var command = new CrearFacturaCommand();
        var comandaId = Guid.NewGuid();
        command.ComandasIds.Add(comandaId);
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
        {
            callCount++;
            var complexException = new InvalidOperationException("Operación compleja falló");
            complexException.Data["RequestId"] = comandaId;
            complexException.Data["Timestamp"] = DateTime.UtcNow;
            throw complexException;
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _behavior.Handle(command, nextDelegate, CancellationToken.None));

        // No debería reintentar InvalidOperationException (no es transitoria)
        callCount.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Handle_ReintentoEnNumeroEspecifico_DeberiaLoggearNumeroCorrect(int numeroReintento)
    {
        // Arrange
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        var expectedResult = Result.Success(new FacturaDto { Id = Guid.NewGuid() });
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
        {
            callCount++;
            if (callCount < numeroReintento + 1) // Fallar hasta llegar al número de reintento deseado
                throw new TimeoutException($"Fallo {callCount}");
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _behavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(numeroReintento + 1); // número de reintentos + 1 intento inicial exitoso
    }
} 
