using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Notifications;
using RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaCreada;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda;
using ComandaCreadaDomainEvent = RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaCreada;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.EventHandlers.ComandaCreada;

/// <summary>
/// Tests para ComandaCreadaSignalRHandler - Notificaciones en tiempo real
/// </summary>
public class ComandaCreadaSignalRHandlerTests
{
    private readonly Mock<ISignalRService> _mockSignalRService;
    private readonly Mock<IComandaRepository> _mockComandaRepository;
    private readonly Mock<ILogger<ComandaCreadaSignalRHandler>> _mockLogger;
    private readonly ComandaCreadaSignalRHandler _handler;

    public ComandaCreadaSignalRHandlerTests()
    {
        _mockSignalRService = new Mock<ISignalRService>();
        _mockComandaRepository = new Mock<IComandaRepository>();
        _mockLogger = new Mock<ILogger<ComandaCreadaSignalRHandler>>();

        _handler = new ComandaCreadaSignalRHandler(
            _mockSignalRService.Object,
            _mockComandaRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaEnviarNotificacionesSignalR()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(Guid.NewGuid(), mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");
        var numeroComanda = comanda.NumeroComanda;

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(evento.ComandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockSignalRService.Verify(x => x.NotificarNuevaComandaAsync(
            It.Is<NuevaComandaNotificationDto>(data => data.ComandaId == comanda.Id && data.MesaId == mesaId && data.Estado == "Creada")), Times.Once);

        _mockSignalRService.Verify(x => x.NotificarGrupoAsync(
            "Meseros",
            "ComandaCreada",
            It.Is<object>(data => data.ToString()!.Contains(numeroComanda))), Times.Once);

        _mockSignalRService.Verify(x => x.NotificarGrupoAsync(
            "Administradores",
            "NuevaComandaRegistrada",
            It.Is<object>(data => data.ToString()!.Contains(numeroComanda))), Times.Once);

        _mockSignalRService.Verify(x => x.NotificarEventoSistemaAsync(
            "NuevaComandaRegistrada",
            It.Is<object>(data => data.ToString()!.Contains(numeroComanda))), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreadaConMesero_DeberiaNotificarMeseroEspecifico()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(Guid.NewGuid(), mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");
        var numeroComanda = comanda.NumeroComanda;

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(evento.ComandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockSignalRService.Verify(x => x.NotificarUsuarioAsync(
            meseroId.ToString(),
            "ComandaCreada",
            It.Is<object>(data => data.ToString()!.Contains(numeroComanda))), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreadaConItems_DeberiaIncluirItemsEnNotificacion()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(Guid.NewGuid(), mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");
        comanda.AgregarItem(Guid.NewGuid(), "Hamburguesa", 2, 15.99m, "Sin cebolla");
        comanda.AgregarItem(Guid.NewGuid(), "Pizza", 1, 12.50m, "Bien cocido");
        var numeroComanda = comanda.NumeroComanda;

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(evento.ComandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockSignalRService.Verify(x => x.NotificarActualizacionComandaAsync(
            comanda.Id,
            comanda.Estado.ToString(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaNoEncontrada_DeberiaLoggearWarningYNoEnviarNotificaciones()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(Guid.NewGuid(), mesaId, meseroId);

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(evento.ComandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comanda?)null);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockSignalRService.Verify(x => x.NotificarGrupoAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ErrorEnSignalR_DeberiaLoggearErrorYNoInterrumpirFlujo()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(Guid.NewGuid(), mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");
        var numeroComanda = comanda.NumeroComanda;

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(evento.ComandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockSignalRService
            .Setup(x => x.NotificarGrupoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new Exception("Error de conexión SignalR"));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert - El handler no debe lanzar excepción
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaEnviarNotificacionCorrectaACocina()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockSignalRService.Verify(x => x.NotificarNuevaComandaAsync(
            It.Is<NuevaComandaNotificationDto>(data => data.ComandaId == comanda.Id)), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaEnviarNotificacionCorrectaAMesero()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockSignalRService.Verify(x => x.NotificarUsuarioAsync(
            meseroId.ToString(),
            "ComandaCreada",
            It.Is<object>(data => data.ToString()!.Contains(comanda.NumeroComanda))), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaEnviarNotificacionCorrectaASistema()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockSignalRService.Verify(x => x.NotificarEventoSistemaAsync(
            "NuevaComandaRegistrada",
            It.Is<object>(data => data.ToString()!.Contains(comanda.NumeroComanda))), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaActualizarEstadoComanda()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockSignalRService.Verify(x => x.NotificarActualizacionComandaAsync(
            comanda.Id,
            comanda.Estado.ToString(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaEnviarNotificacionAAdministradores()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockSignalRService.Verify(x => x.NotificarGrupoAsync(
            "Administradores",
            "NuevaComandaRegistrada",
            It.Is<object>(data => data.ToString()!.Contains(comanda.NumeroComanda))), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaIncluirDatosCompletosEnNotificacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert - Verificar que se envían los datos correctos
        _mockSignalRService.Verify(x => x.NotificarNuevaComandaAsync(
            It.Is<NuevaComandaNotificationDto>(data => 
                data.ComandaId == comanda.Id &&
                data.MesaId == mesaId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaManejarExcepcionesDeRepositorio()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert - El handler no debe lanzar excepción
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaLoggearInformacionCorrecta()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert - Verificar que se loggea la información correcta
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando notificación SignalR")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Notificaciones SignalR enviadas correctamente")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaEnviarNotificacionConPrioridadCorrecta()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert - Verificar que se envía la notificación con prioridad correcta
        _mockSignalRService.Verify(x => x.NotificarNuevaComandaAsync(
            It.Is<NuevaComandaNotificationDto>(data => data.Estado == comanda.Estado.ToString())), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreada_DeberiaEnviarNotificacionConTimestamp()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var evento = new ComandaCreadaDomainEvent(comandaId, mesaId, meseroId);

        var comanda = Comanda.Crear(meseroId, Guid.NewGuid(), mesaId, "Mesa cerca de la ventana");

        _mockComandaRepository
            .Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert - Verificar que se incluye el timestamp en la notificación
        _mockSignalRService.Verify(x => x.NotificarNuevaComandaAsync(
            It.Is<NuevaComandaNotificationDto>(data => data.FechaCreacion == comanda.FechaCreacion)), Times.Once);
    }
} 