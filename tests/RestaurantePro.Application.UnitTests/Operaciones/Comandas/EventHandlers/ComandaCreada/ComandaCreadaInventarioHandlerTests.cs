using RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaCreada;
using RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Core.Notificaciones.Services;
using RestaurantePro.Domain.Core.SharedKernel.Exceptions;

namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.EventHandlers.ComandaCreada;

/// <summary>
/// Tests para ComandaCreadaInventarioHandler - Verificación automática de inventario
/// </summary>
public class ComandaCreadaInventarioHandlerTests
{
    private readonly Mock<IIngredienteRepository> _mockIngredienteRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<ComandaCreadaInventarioHandler>> _mockLogger;
    private readonly ComandaCreadaInventarioHandler _handler;

    public ComandaCreadaInventarioHandlerTests()
    {
        _mockIngredienteRepository = new Mock<IIngredienteRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<ComandaCreadaInventarioHandler>>();
        
        _handler = new ComandaCreadaInventarioHandler(
            _mockIngredienteRepository.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ComandaConIngredientesDisponibles_DeberiaVerificarSinAlertas()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaCreadaEvent(comandaId, clienteId, DateTime.UtcNow, 150.00m);

        var ingredientes = new List<Ingrediente>
        {
            CreateIngredienteWithStock("Tomate", 100, 10), // Stock suficiente
            CreateIngredienteWithStock("Queso", 50, 5),    // Stock suficiente
            CreateIngredienteWithStock("Masa", 30, 8)      // Stock suficiente
        };

        _mockIngredienteRepository.Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredientes);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockIngredienteRepository.Verify(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // No debería enviar notificaciones de alerta
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NivelPrioridad>(),
            It.IsAny<CancellationToken>()), Times.Never);

        // Debería loggear verificación exitosa
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("✅ Verificación de inventario exitosa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaConIngredientesBajoStock_DeberiaEnviarAlertas()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaCreadaEvent(comandaId, clienteId, DateTime.UtcNow, 150.00m);

        var ingredientes = new List<Ingrediente>
        {
            CreateIngredienteWithStock("Tomate", 100, 3),  // Stock bajo (< 5)
            CreateIngredienteWithStock("Queso", 50, 2),    // Stock bajo (< 5)
            CreateIngredienteWithStock("Masa", 30, 10)     // Stock OK
        };

        _mockIngredienteRepository.Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredientes);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Debería enviar 2 notificaciones (Tomate y Queso)
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("Tomate")),
            It.IsAny<string>(),
            NivelPrioridad.Media,
            It.IsAny<CancellationToken>()), Times.Once);

        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("Queso")),
            It.IsAny<string>(),
            NivelPrioridad.Media,
            It.IsAny<CancellationToken>()), Times.Once);

        // Debería loggear alertas
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Ingredientes con stock bajo detectados")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaConIngredientesAgotados_DeberiaEnviarAlertasCriticas()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaCreadaEvent(comandaId, clienteId, DateTime.UtcNow, 150.00m);

        var ingredientes = new List<Ingrediente>
        {
            CreateIngredienteWithStock("Tomate", 100, 0),  // Agotado
            CreateIngredienteWithStock("Queso", 50, 0),    // Agotado
            CreateIngredienteWithStock("Masa", 30, 2)      // Bajo stock
        };

        _mockIngredienteRepository.Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredientes);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        // Alertas críticas para agotados
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("Tomate") && msg.Contains("AGOTADO")),
            It.IsAny<string>(),
            NivelPrioridad.Alta,
            It.IsAny<CancellationToken>()), Times.Once);

        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("Queso") && msg.Contains("AGOTADO")),
            It.IsAny<string>(),
            NivelPrioridad.Alta,
            It.IsAny<CancellationToken>()), Times.Once);

        // Alerta media para bajo stock
        _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("Masa")),
            It.IsAny<string>(),
            NivelPrioridad.Media,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnRepositorio_DeberiaLoggearYPropagar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaCreadaEvent(comandaId, clienteId, DateTime.UtcNow, 150.00m);

        var repositoryException = new Exception("Error de conexión a base de datos");
        _mockIngredienteRepository.Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(repositoryException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(evento, CancellationToken.None));

        exception.Should().Be(repositoryException);

        // Debería loggear el error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error verificando inventario")),
                repositoryException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ErrorEnNotificacion_NoDeberiaDetenerProceso()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaCreadaEvent(comandaId, clienteId, DateTime.UtcNow, 150.00m);

        var ingredientes = new List<Ingrediente>
        {
            CreateIngredienteWithStock("Tomate", 100, 2)  // Stock bajo
        };

        _mockIngredienteRepository.Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredientes);

        _mockNotificationService.Setup(x => x.EnviarNotificacionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NivelPrioridad>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error enviando notificación"));

        // Act
        // No debería lanzar excepción, solo loggear el error
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error enviando notificación")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaCreadaEvent(comandaId, clienteId, DateTime.UtcNow, 150.00m);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancelar inmediatamente

        _mockIngredienteRepository.Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(evento, cancellationTokenSource.Token));
    }

    [Theory]
    [InlineData(0, NivelPrioridad.Alta)]     // Agotado
    [InlineData(1, NivelPrioridad.Alta)]     // Agotado  
    [InlineData(2, NivelPrioridad.Media)]    // Bajo stock
    [InlineData(4, NivelPrioridad.Media)]    // Bajo stock
    [InlineData(5, null)]                    // Stock OK - sin alerta
    [InlineData(10, null)]                   // Stock OK - sin alerta
    public async Task Handle_DiferentesNivelesStock_DeberiaGenerarAlertasApropiadas(
        int stockActual, NivelPrioridad? nivelEsperado)
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaCreadaEvent(comandaId, clienteId, DateTime.UtcNow, 150.00m);

        var ingredientes = new List<Ingrediente>
        {
            CreateIngredienteWithStock("Test Ingrediente", 100, stockActual)
        };

        _mockIngredienteRepository.Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredientes);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        if (nivelEsperado.HasValue)
        {
            _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                nivelEsperado.Value,
                It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            _mockNotificationService.Verify(x => x.EnviarNotificacionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<NivelPrioridad>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Fact]
    public async Task Handle_InventarioVacio_DeberiaLoggearInfo()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new ComandaCreadaEvent(comandaId, clienteId, DateTime.UtcNow, 150.00m);

        var ingredientesVacios = new List<Ingrediente>();

        _mockIngredienteRepository.Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredientesVacios);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No hay ingredientes registrados")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirContextoComandaEnLogs()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var montoTotal = 275.50m;
        var evento = new ComandaCreadaEvent(comandaId, clienteId, DateTime.UtcNow, montoTotal);

        var ingredientes = new List<Ingrediente>
        {
            CreateIngredienteWithStock("Tomate", 100, 10)
        };

        _mockIngredienteRepository.Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ingredientes);

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
    }

    // Helper method para crear ingredientes con stock
    private static Ingrediente CreateIngredienteWithStock(string nombre, decimal stockMinimo, decimal stockActual)
    {
        // Simular creación de ingrediente con stock específico
        // Esta implementación dependería de la implementación real de Ingrediente
        var ingrediente = new Mock<Ingrediente>();
        ingrediente.SetupGet(x => x.Nombre).Returns(nombre);
        ingrediente.SetupGet(x => x.StockMinimo).Returns(stockMinimo);
        ingrediente.SetupGet(x => x.StockActual).Returns(stockActual);
        ingrediente.Setup(x => x.TieneStockBajo()).Returns(stockActual < 5);
        ingrediente.Setup(x => x.EstaAgotado()).Returns(stockActual <= 0);
        return ingrediente.Object;
    }
} 