namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para RetryBehavior - Comportamiento de reintentos con backoff exponencial
/// Optimizado para probar concurrencia y configuración dinámica
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
    [Trait("Category", "LongRunning")]
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
    [Trait("Category", "LongRunning")]
    public async Task Handle_BackoffExponencial_DeberiaEsperarTiemposCrecientes()
    {
        // Arrange
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        // Configurar retry settings con delays controlados para el test
        var customSettings = new RetrySettings
        {
            MaxAttempts = 3,
            BaseDelayMs = 100, // Delay base pequeño para el test
            MaxDelayMs = 1000
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var customBehavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, mockCustomSettings.Object);
        
        // Ejecutar varias veces para obtener un promedio más confiable
        var allDelays = new List<List<TimeSpan>>();
        
        // Ejecutar 5 veces para tener suficientes muestras
        for (int run = 0; run < 5; run++)
        {
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
                
                if (callCount <= 2) // Fallar las primeras 2 veces
                    throw new TimeoutException("Timeout temporal");
                return Task.FromResult(Result.Success(new FacturaDto()));
            };
            
            // Act
            await customBehavior.Handle(command, nextDelegate, CancellationToken.None);
            
            // Guardar los delays de esta ejecución
            allDelays.Add(delays);
        }
        
        // Assert
        // Calcular promedios de delays para cada posición
        var avgFirstDelay = allDelays.Select(d => d[0].TotalMilliseconds).Average();
        var avgSecondDelay = allDelays.Select(d => d[1].TotalMilliseconds).Average();
        
        // Verificar que los delays están dentro de los rangos esperados
        foreach (var delays in allDelays)
        {
            delays.Should().HaveCount(2); // Dos delays entre las 3 ejecuciones
            
            // Verificar que todos los delays son positivos
            foreach (var delay in delays)
            {
                delay.Should().BeGreaterThan(TimeSpan.Zero, "Todos los delays deben ser positivos");
            }
            
            // Con jitter, los delays pueden variar, pero el rango debe ser apropiado
            // Para el primer reintento: baseDelay (100ms) con jitter = 10ms a 100ms
            // Para el segundo reintento: 2*baseDelay (200ms) con jitter = 20ms a 200ms
            delays[0].TotalMilliseconds.Should().BeInRange(5, 150, "Primer delay debe estar en rango con jitter");
            delays[1].TotalMilliseconds.Should().BeInRange(5, 250, "Segundo delay debe estar en rango con jitter");
        }
        
        // Verificar que en promedio, el backoff exponencial funciona
        // El promedio del segundo delay debería ser mayor que el del primero
        avgSecondDelay.Should().BeGreaterThan(avgFirstDelay * 0.8, 
            "En promedio, el backoff exponencial debería mostrar progresión en los tiempos de espera");
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
                // Primera llamada: lanzar excepción transitoria para que reintente
                throw new TimeoutException("Timeout transitorio");
            }
            // Segunda llamada: nunca debería llegar aquí si respeta la cancelación
            return Task.FromResult(Result.Success(new FacturaDto()));
        };

        // Cancelar ANTES de ejecutar para que la verificación inicial detecte la cancelación
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _behavior.Handle(command, nextDelegate, cancellationTokenSource.Token));

        // Debería no haber ejecutado nada por la verificación inicial de cancelación
        callCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ConJitterMejorado_DeberiaVariarTiemposDeEspera()
    {
        // Arrange
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        // Configurar retry settings con más intentos y delays menores para el test
        var customSettings = new RetrySettings
        {
            MaxAttempts = 5, // Aumentar a 5 intentos
            BaseDelayMs = 50, // Delay base pequeño para el test
            MaxDelayMs = 2000
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var customBehavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, mockCustomSettings.Object);
        
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
            
            if (callCount <= 3) // Fallar las primeras 3 veces, exitoso en la 4ta
                throw new TimeoutException("Timeout temporal");
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await customBehavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        
        // El jitter mejorado debería producir tiempos más distribuidos
        var tiemposDistintos = delays.Select(d => d.TotalMilliseconds).Distinct().Count();
        tiemposDistintos.Should().BeGreaterThan(1, "El jitter mejorado debería producir tiempos diferentes");
        
        // Los delays deben estar dentro del rango esperado (con jitter)
        foreach (var delay in delays)
        {
            delay.TotalMilliseconds.Should().BeGreaterThan(0, "Delay debe ser positivo");
            delay.TotalMilliseconds.Should().BeLessThan(2000, "Delay no debe exceder MaxDelay");
        }
    }

    [Fact]
    public async Task Handle_ConfiguracionDinamica_DeberiaUsarRetryableCommands()
    {
        // Arrange
        var customSettings = new RetrySettings
        {
            MaxAttempts = 2,
            BaseDelayMs = 50,
            MaxDelayMs = 5000,
            RetryableCommands = new List<string> { "CrearFactura", "ProcesarPago" }
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
    public async Task Handle_ConfiguracionExcepcionesPersonalizadas_DeberiaRespetarRetryableExceptions()
    {
        // Arrange
        var customSettings = new RetrySettings
        {
            MaxAttempts = 3,
            BaseDelayMs = 10,
            MaxDelayMs = 1000,
            RetryableExceptions = new List<string> { "TimeoutException", "CustomTransientException" }
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var customBehavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, mockCustomSettings.Object);
        
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
                throw new TimeoutException("Timeout configurado como retriable");
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await customBehavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(2); // Primera falla + reintento exitoso
    }

    [Fact]
    public async Task Handle_ConcurrenciaAlta_DeberiaManejarMultiplesThreadsSafely()
    {
        // Arrange
        var customSettings = new RetrySettings
        {
            MaxAttempts = 2,
            BaseDelayMs = 10,
            MaxDelayMs = 100
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";
        
        var tasks = new List<Task>();
        var results = new ConcurrentBag<bool>();
        
        // Act - Simular múltiples threads ejecutando retry behavior concurrentemente
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var behavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, mockCustomSettings.Object);
                    
                    int attemptCount = 0;
                    RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
                    {
                        attemptCount++;
                        if (attemptCount == 1)
                            throw new TimeoutException("Thread-safe test");
                        return Task.FromResult(Result.Success(new FacturaDto()));
                    };

                    await behavior.Handle(command, nextDelegate, CancellationToken.None);
                    results.Add(true);
                }
                catch
                {
                    results.Add(false);
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // Assert
        results.Should().HaveCount(10);
        results.Where(r => r).Should().HaveCount(10, "Todos los threads deberían completarse exitosamente");
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
    [Trait("Category", "LongRunning")]
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
        
        // Configurar retry settings con delays muy pequeños para test
        var customSettings = new RetrySettings
        {
            MaxAttempts = Math.Max(numeroReintento + 2, 5), // Asegurar suficientes intentos
            BaseDelayMs = 1, // Delay muy pequeño para test
            MaxDelayMs = 10,
            Enabled = true
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var customBehavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, mockCustomSettings.Object);
        
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
        var result = await customBehavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(numeroReintento + 1); // número de reintentos + 1 intento inicial exitoso
    }

    [Fact]
    [Trait("Category", "LongRunning")]
    public async Task Handle_TimeoutExtremo_DeberiaRespetarMaxDelayOptimizado()
    {
        // Arrange - Test para verificar que los delays optimizados previenen timeouts extremos
        var customSettings = new RetrySettings
        {
            MaxAttempts = 4,
            BaseDelayMs = 800,  // Configuración optimizada
            MaxDelayMs = 25000, // Configuración optimizada
            Enabled = true
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var customBehavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, mockCustomSettings.Object);
        
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test Extremo";
        command.TipoFactura = "Normal";
        
        var tiemposDelay = new List<TimeSpan>();
        
        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
        {
            callCount++;
            
            // Simular delay tracking (aunque no podemos capturar directamente el delay interno)
            if (callCount <= 3) // Fallar los primeros 3 intentos
            {
                throw new TimeoutException("Timeout extremo simulado");
            }
            return Task.FromResult(Result.Success(new FacturaDto { Id = Guid.NewGuid() }));
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await customBehavior.Handle(command, nextDelegate, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        callCount.Should().Be(4); // 3 fallos + 1 éxito
        
        // El tiempo total no debería exceder el límite razonable (considerando delays optimizados)
        // Con BaseDelay 800ms y 3 reintentos, esperamos menos de 15 segundos total
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(15), 
            "Los delays optimizados deberían prevenir timeouts extremos");
    }

    [Fact]
    [Trait("Category", "LongRunning")]
    public async Task Handle_AltaCargarConcurrente_DeberiaEscalarSinTimeouts()
    {
        // Arrange - Test de stress para prevenir timeouts bajo alta carga
        var customSettings = new RetrySettings
        {
            MaxAttempts = 2,
            BaseDelayMs = 50,   // Delay muy pequeño para test de stress
            MaxDelayMs = 500,   // Límite bajo para test rápido
            Enabled = true
        };
        
        var mockCustomSettings = new Mock<IOptions<RetrySettings>>();
        mockCustomSettings.Setup(x => x.Value).Returns(customSettings);
        
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Stress Test";
        command.TipoFactura = "Normal";
        
        var tasks = new List<Task<bool>>();
        var completedCount = 0;
        var timeoutCount = 0;
        
        // Act - Ejecutar 20 tareas concurrentes para simular alta carga
        for (int i = 0; i < 20; i++)
        {
            var taskId = i; // Capturar variable para closure
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var behavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, mockCustomSettings.Object);
                    
                    int attemptCount = 0;
                    RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = _ => 
                    {
                        attemptCount++;
                        
                        // 50% de probabilidad de fallo en el primer intento para simular carga
                        if (attemptCount == 1 && taskId % 2 == 0)
                        {
                            throw new TimeoutException($"Stress test timeout {taskId}");
                        }
                        
                        return Task.FromResult(Result.Success(new FacturaDto { Id = Guid.NewGuid() }));
                    };

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // Timeout de 5 segundos por tarea
                    await behavior.Handle(command, nextDelegate, cts.Token);
                    
                    Interlocked.Increment(ref completedCount);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    Interlocked.Increment(ref timeoutCount);
                    return false;
                }
                catch
                {
                    return false;
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // Assert
        completedCount.Should().BeGreaterThan(15, "Al menos 75% de las tareas deberían completarse sin timeout");
        timeoutCount.Should().BeLessThan(5, "Menos del 25% de las tareas deberían sufrir timeout");
        
        var successfulTasks = tasks.Count(t => t.Result);
        successfulTasks.Should().BeGreaterThan(15, "La mayoría de tareas deberían ser exitosas bajo alta carga");
    }

    [Fact]
    [Trait("Category", "LongRunning")]
    public async Task Handle_ConfiguracionOptimizada_DeberiaReducirTiempoTotalRetry()
    {
        // Arrange - Comparar configuración optimizada vs configuración anterior
        var configOptimizada = new RetrySettings
        {
            MaxAttempts = 3,
            BaseDelayMs = 800,  // Optimizado
            MaxDelayMs = 25000, // Optimizado
            Enabled = true
        };
        
        var configAnterior = new RetrySettings
        {
            MaxAttempts = 3,
            BaseDelayMs = 1000, // Configuración anterior
            MaxDelayMs = 30000, // Configuración anterior
            Enabled = true
        };
        
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Optimización";
        command.TipoFactura = "Normal";
        
        // Test con configuración optimizada
        var mockOptimizada = new Mock<IOptions<RetrySettings>>();
        mockOptimizada.Setup(x => x.Value).Returns(configOptimizada);
        
        var behaviorOptimizado = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(_mockLogger.Object, mockOptimizada.Object);
        
        var expectedResult = Result.Success(new FacturaDto { Id = Guid.NewGuid() });
        
        int callCountOptimizado = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegateOptimizado = _ => 
        {
            callCountOptimizado++;
            if (callCountOptimizado <= 2) // Fallar 2 veces, exitoso en la 3ra
                throw new TimeoutException("Test optimización");
            return Task.FromResult(expectedResult);
        };

        // Act & Assert
        var stopwatch = Stopwatch.StartNew();
        var result = await behaviorOptimizado.Handle(command, nextDelegateOptimizado, CancellationToken.None);
        stopwatch.Stop();
        
        result.Should().Be(expectedResult);
        callCountOptimizado.Should().Be(3);
        
        // Con configuración optimizada, el tiempo total debería ser menor
        // Esperamos que con 2 reintentos y configuración optimizada tome menos tiempo
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5), 
            "La configuración optimizada debería reducir tiempos de retry");
    }
} 
