using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarMesa;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Commands;

public class AsignarMesaHandlerTests
{
    private readonly Mock<IMesaRepository> _mockMesaRepository;
    private readonly Mock<ILogger<AsignarMesaHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUser;
    private readonly AsignarMesaHandler _handler;

    public AsignarMesaHandlerTests()
    {
        _mockMesaRepository = new Mock<IMesaRepository>();
        _mockLogger = new Mock<ILogger<AsignarMesaHandler>>();
        _mockCurrentUser = new Mock<ICurrentUserService>();
        
        _mockCurrentUser.Setup(x => x.UserId).Returns("test-user-id");
        
        _handler = new AsignarMesaHandler(
            _mockMesaRepository.Object,
            _mockLogger.Object,
            _mockCurrentUser.Object);
    }

    [Fact]
    public async Task Handle_ConMesaDisponible_DeberiaAsignarMesaCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var command = AsignarMesaCommand.Crear(mesaId, observaciones: "Mesa para familia");

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Ocupada);
        
        _mockMesaRepository.Verify(x => x.ActualizarAsync(mesa), Times.Once);
        _mockMesaRepository.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ConMesaNoEncontrada_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = AsignarMesaCommand.Crear(mesaId);

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
    public async Task Handle_ConMesaYaOcupada_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        mesa.MarcarComoOcupada(); // Ya está ocupada
        var command = AsignarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("no puede marcarse como ocupada");
        
        _mockMesaRepository.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Never);
        _mockMesaRepository.Verify(x => x.GuardarCambiosAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ConMesaReservada_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        mesa.MarcarComoReservada(); // Mesa reservada
        var command = AsignarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("no puede marcarse como ocupada");
    }

    [Fact]
    public async Task Handle_ConErrorEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = AsignarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al asignar la mesa");
    }

    [Fact]
    public async Task Handle_ConErrorAlGuardar_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var command = AsignarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);
        _mockMesaRepository.Setup(x => x.GuardarCambiosAsync())
            .ThrowsAsync(new Exception("Error al guardar"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al asignar la mesa");
    }

    [Fact]
    public async Task Handle_ConObservaciones_DeberiaLoggearObservaciones()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var observaciones = "Mesa para celebración especial";
        var command = AsignarMesaCommand.Crear(mesaId, observaciones: observaciones);

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
    public async Task Handle_ConMeseroId_DeberiaAsignarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, "Terraza");
        var command = AsignarMesaCommand.Crear(mesaId, meseroId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Ocupada);
    }

    [Theory]
    [InlineData("Interior")]
    [InlineData("Terraza")]
    [InlineData("Privado")]
    public async Task Handle_ConDiferentesUbicaciones_DeberiaAsignarCorrectamente(string ubicacion)
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(1, 4, ubicacion);
        var command = AsignarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        mesa.Estado.Should().Be(EstadoMesa.Ocupada);
        mesa.Ubicacion.Should().Be(ubicacion);
    }

    [Fact]
    public async Task Handle_DeberiLoggearInformacionCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var mesa = Mesa.Crear(5, 6, "VIP");
        var command = AsignarMesaCommand.Crear(mesaId);

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando asignación de mesa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
            
        // Verificar logging de éxito
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("asignada correctamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConMesaNoEncontrada_DeberiLoggearWarning()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = AsignarMesaCommand.Crear(mesaId);

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
        var command = AsignarMesaCommand.Crear(mesaId);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ThrowsAsync(new Exception("Error crítico"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error interno al asignar mesa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
} 