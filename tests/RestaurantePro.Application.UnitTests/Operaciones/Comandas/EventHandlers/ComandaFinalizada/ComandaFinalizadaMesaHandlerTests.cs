using RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaFinalizada;
using RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;
using RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.EventHandlers.ComandaFinalizada;

/// <summary>
/// Tests para ComandaFinalizadaMesaHandler - Liberación automática de mesas
/// </summary>
public class ComandaFinalizadaMesaHandlerTests
{
    private readonly Mock<IComandaRepository> _mockComandaRepository;
    private readonly Mock<IMesaRepository> _mockMesaRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<ComandaFinalizadaMesaHandler>> _mockLogger;
    private readonly ComandaFinalizadaMesaHandler _handler;

    public ComandaFinalizadaMesaHandlerTests()
    {
        _mockComandaRepository = new Mock<IComandaRepository>();
        _mockMesaRepository = new Mock<IMesaRepository>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<ComandaFinalizadaMesaHandler>>();
        
        _handler = new ComandaFinalizadaMesaHandler(
            _mockComandaRepository.Object,
            _mockMesaRepository.Object,
            _mockLogger.Object,
            _mockMediator.Object);
    }

    [Fact]
    public async Task Handle_ComandaFinalizadaConMesa_DeberiaLiberarMesa()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var mesa = CreateMockMesa(Guid.NewGuid(), 4, EstadoMesa.Ocupada);
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockMediator.Setup(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockMesaRepository.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        
        _mockMediator.Verify(x => x.Send(
            It.IsAny<LiberarMesaCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Debería loggear liberación exitosa
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("🪑 Mesa liberada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaSinMesa_NoDeberiaLiberarMesa()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comanda)null);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockMesaRepository.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()), Times.Never);

        // Debería loggear que no hay mesa
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("💡 Comanda sin mesa asignada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MesaNoEncontrada_DeberiaLoggearAdvertencia()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var comanda = new Mock<Comanda>();
        comanda.Setup(x => x.MesaId).Returns(Guid.NewGuid());

        _mockComandaRepository.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda.Object);

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mesa)null);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("⚠️ Mesa no encontrada")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockMediator.Verify(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MesaYaDisponible_NoDeberiaLiberarNuevamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var mesa = CreateMockMesa(Guid.NewGuid(), 4, EstadoMesa.Disponible);
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ℹ️ Mesa ya está disponible")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockMediator.Verify(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(EstadoMesa.Ocupada, true)]          // Debe liberar
    [InlineData(EstadoMesa.Reservada, true)]        // Debe liberar 
    [InlineData(EstadoMesa.EnLimpieza, true)]       // Debe liberar
    [InlineData(EstadoMesa.Disponible, false)]      // No debe liberar
    [InlineData(EstadoMesa.FueraDeServicio, false)] // No debe liberar
    public async Task Handle_DiferentesEstadosMesa_DeberiaLiberarSegunEstado(
        EstadoMesa estadoMesa, bool deberiaLiberar)
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var mesa = CreateMockMesa(Guid.NewGuid(), 4, estadoMesa);
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockMediator.Setup(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        if (deberiaLiberar)
        {
            _mockMediator.Verify(x => x.Send(
                It.IsAny<LiberarMesaCommand>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            _mockMediator.Verify(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Fact]
    public async Task Handle_ErrorEnLiberacion_DeberiaLoggearError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var mesa = CreateMockMesa(Guid.NewGuid(), 4, EstadoMesa.Ocupada);
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockMediator.Setup(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Unit>("Error liberando mesa"));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error ejecutando comando de liberación")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_DeberiaLoggearYPropagar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var repositoryException = new Exception("Error de conexión a base de datos");
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(repositoryException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(evento, CancellationToken.None));

        exception.Should().Be(repositoryException);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error procesando liberación de mesa")),
                repositoryException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConCancellationToken_DeberiaRespetarCancelacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _handler.Handle(evento, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_MesaConCapacidadGrande_DeberiaLoggearInformacionDetallada()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 350.00m);

        var mesaGrande = CreateMockMesa(mesaId, 8, EstadoMesa.Ocupada); // Mesa grande
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesaGrande);

        _mockMediator.Setup(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("8 personas")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_DeberiaIncluirContextoComandaEnLogs()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var fechaFinalizacion = new DateTime(2025, 1, 17, 15, 45, 0);
        var montoTotal = 425.75m;
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, montoTotal);

        var mesa = CreateMockMesa(mesaId, 6, EstadoMesa.Ocupada);
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockMediator.Setup(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(mesaId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_LiberacionExitosa_DeberiaNotificarDisponibilidad()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var mesa = CreateMockMesa(mesaId, 4, EstadoMesa.Ocupada);
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockMediator.Setup(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("💫 Mesa disponible para nuevas reservaciones")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExcepcionEnMediator_DeberiaLoggearError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var mesa = CreateMockMesa(mesaId, 4, EstadoMesa.Ocupada);
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        var mediatorException = new Exception("Error en comando de liberación");
        _mockMediator.Setup(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(mediatorException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(evento, CancellationToken.None));

        exception.Should().Be(mediatorException);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("❌ Error ejecutando comando de liberación")),
                mediatorException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_VerificarParametrosCommand_DeberiaUsarParametrosCorrectos()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var fechaFinalizacion = DateTime.UtcNow;
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var mesa = CreateMockMesa(mesaId, 4, EstadoMesa.Ocupada);
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        LiberarMesaCommand? capturedCommand = null;
        _mockMediator.Setup(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result>, CancellationToken>((cmd, _) => 
            {
                capturedCommand = cmd as LiberarMesaCommand;
            })
            .ReturnsAsync(Result.Success(Unit.Value));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.MesaId.Should().Be(mesaId);
    }

    [Theory]
    [InlineData(2, "👥 Mesa pequeña")]
    [InlineData(4, "🪑 Mesa estándar")]  
    [InlineData(6, "👥 Mesa familiar")]
    [InlineData(8, "🎉 Mesa grande")]
    [InlineData(12, "👑 Mesa VIP")]
    public async Task Handle_DiferentesCapacidades_DeberiaLoggearDescripcionApropiada(
        int capacidad, string descripcionEsperada)
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var evento = new RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada(comandaId, 150.00m);

        var mesa = CreateMockMesa(mesaId, capacidad, EstadoMesa.Ocupada);
        
        _mockMesaRepository.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        _mockMediator.Setup(x => x.Send(It.IsAny<LiberarMesaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Unit.Value));

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(descripcionEsperada)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    // Helper method para crear mesas mock
    private static Mesa CreateMockMesa(Guid id, int capacidad, EstadoMesa estado)
    {
        // Crear mesa real usando factory method
        var mesa = Mesa.Crear(Random.Shared.Next(1, 50), capacidad, "Interior");
        
        // Usar reflexión para establecer el ID y estado
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, id);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesa, estado);
        
        return mesa;
    }
} 