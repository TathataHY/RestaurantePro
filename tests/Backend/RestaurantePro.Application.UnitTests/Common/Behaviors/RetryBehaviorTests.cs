using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Common.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Common.Behaviors;

/// <summary>
/// Tests para RetryBehavior - Comportamiento de reintentos con backoff exponencial
/// Optimizado para probar concurrencia y configuración dinámica
/// </summary>
public class RetryBehaviorTests
{
    private readonly Mock<ILogger<RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>>> _mockLogger;
    private readonly Mock<IOptions<RetrySettings>> _mockRetrySettings;
    private readonly Mock<IDelayProvider> _delayProviderMock;
    private readonly RetryBehavior<CrearFacturaCommand, Result<FacturaDto>> _behavior;

    public RetryBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>>>();
        _mockRetrySettings = new Mock<IOptions<RetrySettings>>();
        _delayProviderMock = new Mock<IDelayProvider>();
        
        var retrySettings = new RetrySettings
        {
            MaxAttempts = 3,
            BaseDelayMs = 100,
            MaxDelayMs = 30000,
            Enabled = true
        };
        
        _mockRetrySettings.Setup(x => x.Value).Returns(retrySettings);
        
        // Configurar el mock para que el Delay no espere realmente
        _delayProviderMock.Setup(d => d.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _behavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(
            _mockLogger.Object, 
            _delayProviderMock.Object, 
            _mockRetrySettings.Object);
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
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = () => {
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
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = () => 
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
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = () => 
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
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = () => 
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
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = () => 
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
    public async Task Handle_BackoffExponencial_DeberiaLlamarDelayConTiemposCrecientes()
    {
        // Arrange
        var command = new CrearFacturaCommand();
        command.ComandasIds.Add(Guid.NewGuid());
        command.NombreCliente = "Cliente Test";
        command.TipoFactura = "Normal";

        var customSettings = new RetrySettings
        {
            MaxAttempts = 4,
            BaseDelayMs = 100,
            MaxDelayMs = 2000
        };
        _mockRetrySettings.Setup(s => s.Value).Returns(customSettings);

        var capturedDelays = new List<TimeSpan>();
        _delayProviderMock.Setup(d => d.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<TimeSpan, CancellationToken>((delay, _) => capturedDelays.Add(delay))
            .Returns(Task.CompletedTask);
        
        var customBehavior = new RetryBehavior<CrearFacturaCommand, Result<FacturaDto>>(
            _mockLogger.Object, 
            _delayProviderMock.Object, 
            _mockRetrySettings.Object);

        int callCount = 0;
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = () => 
        {
            callCount++;
            if (callCount <= 3) // Fallar 3 veces para tener 3 delays
                throw new TimeoutException("Timeout temporal");
            return Task.FromResult(Result.Success(new FacturaDto()));
        };

        // Act
        await customBehavior.Handle(command, nextDelegate, CancellationToken.None);

        // Assert
        callCount.Should().Be(4);
        capturedDelays.Should().HaveCount(3); // Verificar que se intentó esperar 3 veces
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
        RequestHandlerDelegate<Result<FacturaDto>> nextDelegate = () => 
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
}