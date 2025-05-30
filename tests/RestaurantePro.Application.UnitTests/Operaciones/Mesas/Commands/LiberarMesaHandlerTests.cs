using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Commands;

public class LiberarMesaHandlerTests
{
    private readonly Mock<IMesaRepository> _mockMesaRepository;
    private readonly Mock<ILogger<LiberarMesaHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly LiberarMesaHandler _handler;

    public LiberarMesaHandlerTests()
    {
        _mockMesaRepository = new Mock<IMesaRepository>();
        _mockLogger = new Mock<ILogger<LiberarMesaHandler>>();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        
        _mockCurrentUser.Setup(x => x.UserId).Returns("test-user-id");
        
        _handler = new LiberarMesaHandler(
            _mockMesaRepository.Object,
            _mockLogger.Object,
            _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ConMesaOcupada_DeberiaLiberarMesaCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        mesa.MarcarComoOcupada(); // Mesa ocupada inicialmente
        var command = LiberarMesaCommand.Crear(mesaId, observaciones: "Mesa liberada después de cena");

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Disponible);
        
        _mockMesaRepository.Verify(x => x.ActualizarAsync(mesa), Times.Once);
        _mockMesaRepository.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConMesaReservada_DeberiaLiberarMesaCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(2, 6, "Interior");
        mesa.MarcarComoReservada(); // Mesa reservada inicialmente
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Disponible);
    }

    [Fact]
    public async Task Handle_ConMesaYaDisponible_DeberiaMantenerseSinCambios()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(3, 2, "Barra");
        // Mesa ya está disponible por defecto
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Disponible);
        
        _mockMesaRepository.Verify(x => x.ActualizarAsync(mesa), Times.Once);
        _mockMesaRepository.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConMesaNoEncontrada_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync((Mesa?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Mesa no encontrada");
        
        _mockMesaRepository.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Never);
        _mockMesaRepository.Verify(x => x.GuardarCambiosAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ThrowsAsync(new Exception("Error de conexión"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al liberar la mesa");
    }

    [Fact]
    public async Task Handle_ConErrorAlGuardar_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        mesa.MarcarComoOcupada();
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);
        _mockMesaRepository.Setup(x => x.GuardarCambiosAsync())
            .ThrowsAsync(new Exception("Error al guardar cambios"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al liberar la mesa");
    }

    [Fact]
    public async Task Handle_ConObservaciones_DeberiaLoggearObservaciones()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        mesa.MarcarComoOcupada();
        var observaciones = "Mesa liberada por cambio de turno";
        var command = LiberarMesaCommand.Crear(mesaId, observaciones: observaciones);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que se loggearon las observaciones
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(observaciones)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConMeseroId_DeberiaLiberarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        mesa.MarcarComoOcupada();
        var command = LiberarMesaCommand.Crear(mesaId, meseroId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Disponible);
    }

    [Theory]
    [InlineData("Interior")]
    [InlineData("Terraza")]
    [InlineData("VIP")]
    [InlineData("Barra")]
    public async Task Handle_ConDiferentesUbicaciones_DeberiaLiberarCorrectamente(string ubicacion)
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, ubicacion);
        mesa.MarcarComoOcupada();
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Disponible);
        mesa.Ubicacion.Should().Be(ubicacion);
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(8, 10, "Salón Principal");
        mesa.MarcarComoOcupada();
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar logging de inicio
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando liberación de mesa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("liberada correctamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConMesaNoEncontrada_DeberiLoggearWarning()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync((Mesa?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mesa") && v.ToString()!.Contains("no encontrada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConExcepcion_DeberiLoggearError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ThrowsAsync(new Exception("Error crítico del sistema"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error interno al liberar mesa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(2, 4)]
    [InlineData(6, 8)]
    [InlineData(10, 12)]
    public async Task Handle_ConDiferentesCapacidades_DeberiaLiberarCorrectamente(int numero, int capacidad)
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(numero, capacidad, "Terraza");
        mesa.MarcarComoOcupada();
        var command = LiberarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Disponible);
        mesa.Numero.Should().Be(numero);
        mesa.Capacidad.Should().Be(capacidad);
    }
} 