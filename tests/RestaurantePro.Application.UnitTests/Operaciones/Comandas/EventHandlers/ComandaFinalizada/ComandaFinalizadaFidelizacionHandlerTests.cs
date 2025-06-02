using RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaFinalizada;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Events;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.EventHandlers.ComandaFinalizada;

/// <summary>
/// Tests para ComandaFinalizadaFidelizacionHandler - Gestión automática de puntos de fidelización
/// </summary>
public class ComandaFinalizadaFidelizacionHandlerTests
{
    private readonly Mock<IComercialServiceFacade> _mockComercialServiceFacade;
    private readonly Mock<IClienteRepository> _mockClienteRepository;
    private readonly Mock<ILogger<ComandaFinalizadaFidelizacionHandler>> _mockLogger;
    private readonly ComandaFinalizadaFidelizacionHandler _handler;

    public ComandaFinalizadaFidelizacionHandlerTests()
    {
        _mockComercialServiceFacade = new Mock<IComercialServiceFacade>();
        _mockClienteRepository = new Mock<IClienteRepository>();
        _mockLogger = new Mock<ILogger<ComandaFinalizadaFidelizacionHandler>>();
        
        _handler = new ComandaFinalizadaFidelizacionHandler(
            _mockComercialServiceFacade.Object,
            _mockClienteRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ComandaFinalizadaConCliente_DeberiaAcumularPuntos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        var cliente = CreateMockCliente(clienteId, "Juan Pérez", "juan@email.com");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockComercialServiceFacade.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, montoTotal, comandaId, "Comanda finalizada", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(15)); // 15 puntos acumulados

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        
        _mockComercialServiceFacade.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, montoTotal, comandaId, "Comanda finalizada", It.IsAny<CancellationToken>()), Times.Once);

        // Debería loggear acumulación exitosa
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("🎯 Puntos acumulados exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaSinCliente_NoDeberiaAcumularPuntos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        Guid? clienteId = null; // Comanda sin cliente
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockComercialServiceFacade.Verify(x => x.AcumularPuntosPorCompraAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // Debería loggear que no hay cliente
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("💡 Comanda sin cliente asociado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteNoEncontrado_DeberiaLoggearAdvertencia()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!); // Cliente no encontrado

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockClienteRepository.Verify(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockComercialServiceFacade.Verify(x => x.AcumularPuntosPorCompraAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // Debería loggear advertencia
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Cliente no encontrado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnAcumulacionPuntos_DeberiaLoggearError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        var cliente = CreateMockCliente(clienteId, "Juan Pérez", "juan@email.com");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockComercialServiceFacade.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, montoTotal, comandaId, "Comanda finalizada", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<int>("Error procesando puntos"));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error acumulando puntos")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MontoMuyBajo_NoDeberiaAcumularPuntos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 5.00m; // Monto muy bajo
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        var cliente = CreateMockCliente(clienteId, "Juan Pérez", "juan@email.com");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // No debería llamar al servicio de acumulación para montos bajos
        _mockComercialServiceFacade.Verify(x => x.AcumularPuntosPorCompraAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        // Debería loggear que el monto es insuficiente
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("💰 Monto insuficiente para acumular puntos")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(50.00, 5)]    // $50 = 5 puntos (1 punto por cada $10)
    [InlineData(100.00, 10)]  // $100 = 10 puntos
    [InlineData(155.50, 15)]  // $155.50 = 15 puntos (redondeo hacia abajo)
    [InlineData(200.00, 20)]  // $200 = 20 puntos
    public async Task Handle_DiferentesMontos_DeberiaCalcularPuntosCorrectamente(
        decimal montoTotal, int puntosEsperados)
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        var cliente = CreateMockCliente(clienteId, "Juan Pérez", "juan@email.com");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockComercialServiceFacade.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, montoTotal, comandaId, "Comanda finalizada", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(puntosEsperados));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockComercialServiceFacade.Verify(x => x.AcumularPuntosPorCompraAsync(
            clienteId, montoTotal, comandaId, "Comanda finalizada", It.IsAny<CancellationToken>()), Times.Once);

        // Debería loggear los puntos calculados
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(puntosEsperados.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_DeberiaLoggearYPropagar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        var repositoryException = new Exception("Error de conexión a base de datos");
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(repositoryException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(evento, CancellationToken.None));

        exception.Should().Be(repositoryException);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error procesando fidelización")),
                repositoryException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 150.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(evento, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_ClienteVIP_DeberiaAplicarBonificacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 100.00m;
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        var clienteVIP = CreateMockClienteVIP(clienteId, "María VIP", "maria@vip.com");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteVIP);

        _mockComercialServiceFacade.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, montoTotal, comandaId, "Comanda finalizada", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(15)); // 10 puntos base + 5 bonificación VIP

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("👑 Cliente VIP")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirContextoComandaEnLogs()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var fechaFinalizacion = new DateTime(2025, 1, 17, 14, 30, 0);
        var montoTotal = 275.50m;
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, fechaFinalizacion, montoTotal);

        var cliente = CreateMockCliente(clienteId, "Carlos Cliente", "carlos@email.com");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockComercialServiceFacade.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, montoTotal, comandaId, "Comanda finalizada", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(27));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(comandaId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("275.50")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Carlos Cliente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_AcumulacionExitosa_DeberiaEjecutarPoliticaClientesFrecuentes()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 500.00m; // Monto alto que podría cambiar segmento
        var evento = new ComandaFinalizadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        var cliente = CreateMockCliente(clienteId, "Ana Ascenso", "ana@email.com");
        
        _mockClienteRepository.Setup(x => x.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _mockComercialServiceFacade.Setup(x => x.AcumularPuntosPorCompraAsync(
                clienteId, montoTotal, comandaId, "Comanda finalizada", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(50));

        _mockComercialServiceFacade.Setup(x => x.EjecutarPoliticaClientesFrecuentesAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Contains(clienteId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new Dictionary<Guid, SegmentoCliente> 
            { 
                { clienteId, SegmentoCliente.Frecuente } 
            }));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockComercialServiceFacade.Verify(x => x.EjecutarPoliticaClientesFrecuentesAsync(
            It.Is<IEnumerable<Guid>>(ids => ids.Contains(clienteId)), It.IsAny<CancellationToken>()), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("🎊 Segmento de cliente actualizado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // Helper methods para crear clientes mock
    private static Cliente CreateMockCliente(Guid id, string nombre, string email)
    {
        var cliente = new Mock<Cliente>();
        cliente.SetupGet(x => x.Id).Returns(id);
        cliente.SetupGet(x => x.Nombre).Returns(ClienteNombre.Create(nombre));
        cliente.SetupGet(x => x.Email).Returns(Email.Create(email));
        cliente.SetupGet(x => x.Segmento).Returns(SegmentoCliente.Regular);
        return cliente.Object;
    }

    private static Cliente CreateMockClienteVIP(Guid id, string nombre, string email)
    {
        var cliente = new Mock<Cliente>();
        cliente.SetupGet(x => x.Id).Returns(id);
        cliente.SetupGet(x => x.Nombre).Returns(ClienteNombre.Create(nombre));
        cliente.SetupGet(x => x.Email).Returns(Email.Create(email));
        cliente.SetupGet(x => x.Segmento).Returns(SegmentoCliente.VIP);
        return cliente.Object;
    }
} 