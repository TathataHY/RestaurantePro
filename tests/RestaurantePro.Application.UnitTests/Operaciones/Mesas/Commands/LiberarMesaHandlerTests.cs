using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Services;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Commands;

/// <summary>
/// Tests unitarios para LiberarMesaHandler
/// Valida la lógica completa de liberación de mesas con validaciones de estado y eventos automáticos
/// </summary>
public class LiberarMesaHandlerTests
{
    private readonly Mock<IOperacionesServiceFacade> _operacionesServiceFacadeMock;
    private readonly Mock<IMesaRepository> _mesaRepositoryMock;
    private readonly Mock<ILogger<LiberarMesaHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly LiberarMesaHandler _handler;

    public LiberarMesaHandlerTests()
    {
        _operacionesServiceFacadeMock = new Mock<IOperacionesServiceFacade>();
        _mesaRepositoryMock = new Mock<IMesaRepository>();
        _loggerMock = new Mock<ILogger<LiberarMesaHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();

        _handler = new LiberarMesaHandler(
            _operacionesServiceFacadeMock.Object,
            _mesaRepositoryMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _backgroundJobServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void LiberarMesaBasica_ConMesaId_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var motivo = "Finalización de servicio";

        // Act
        var command = LiberarMesaCommand.LiberarMesaBasica(mesaId, motivo);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.Equal(motivo, command.MotivoLiberacion);
        Assert.False(command.LiberacionForzada);
        Assert.True(command.ValidarEstadoMesa);
        Assert.True(command.NotificarLiberacion);
        Assert.False(command.MantenimientoRequerido);
    }

    [Fact]
    public void LiberarMesaForzada_ConAutorizacion_DeberiaConfigurarForzada()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var motivo = "Emergencia sanitaria";
        var usuarioAutoriza = "SUPERVISOR_001";

        // Act
        var command = LiberarMesaCommand.LiberarMesaForzada(mesaId, motivo, usuarioAutoriza);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.Equal(motivo, command.MotivoLiberacion);
        Assert.True(command.LiberacionForzada);
        Assert.Equal(usuarioAutoriza, command.UsuarioAutoriza);
        Assert.False(command.ValidarEstadoMesa);
        Assert.True(command.NotificarLiberacion);
    }

    [Fact]
    public void LiberarMesaConMantenimiento_ConRequerimientos_DeberiaConfigurarMantenimiento()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var tipoMantenimiento = "Limpieza profunda";
        var observaciones = "Derrame de líquidos";

        // Act
        var command = LiberarMesaCommand.LiberarMesaConMantenimiento(
            mesaId, tipoMantenimiento, observaciones);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.True(command.MantenimientoRequerido);
        Assert.Equal(tipoMantenimiento, command.TipoMantenimiento);
        Assert.Equal(observaciones, command.ObservacionesMantenimiento);
        Assert.True(command.NotificarMantenimiento);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_LiberacionBasicaExitosa_DeberiaLiberarMesaCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MotivoLiberacion = "Finalización de servicio",
            ValidarEstadoMesa = true,
            NotificarLiberacion = true,
            LiberacionForzada = false
        };

        var resultadoLiberacion = CreateMockResultadoLiberacionExitosa(mesaId);

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoLiberacion));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(mesaId, result.Value.MesaId);
        Assert.Equal(EstadoMesa.Disponible, result.Value.EstadoAnterior);
        Assert.Equal(EstadoMesa.Disponible, result.Value.EstadoActual);
        Assert.True(result.Value.LiberacionExitosa);
        Assert.False(result.Value.MantenimientoRequerido);
        Assert.True(result.Value.NotificacionEnviada);
    }

    [Fact]
    public async Task Handle_LiberacionForzadaConAutorizacion_DeberiaLiberarSinValidaciones()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MotivoLiberacion = "Emergencia médica",
            LiberacionForzada = true,
            UsuarioAutoriza = "MANAGER_001",
            ValidarEstadoMesa = false,
            NotificarLiberacion = true
        };

        var resultadoForzada = CreateMockResultadoLiberacionForzada(mesaId);

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoForzada));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.LiberacionForzada);
        Assert.Equal("MANAGER_001", result.Value.UsuarioQueAutorizo);
        Assert.Equal("Emergencia médica", result.Value.MotivoLiberacion);
        Assert.True(result.Value.LiberacionExitosa);
        Assert.True(result.Value.NotificacionEnviada);
    }

    [Fact]
    public async Task Handle_LiberacionConMantenimiento_DeberiaMarcarMantenimiento()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MotivoLiberacion = "Finalización con limpieza",
            MantenimientoRequerido = true,
            TipoMantenimiento = "Limpieza profunda",
            ObservacionesMantenimiento = "Derrame de salsa",
            NotificarMantenimiento = true
        };

        var resultadoConMantenimiento = CreateMockResultadoConMantenimiento(mesaId);

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoConMantenimiento));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.MantenimientoRequerido);
        Assert.Equal("Limpieza profunda", result.Value.TipoMantenimiento);
        Assert.Equal("Derrame de salsa", result.Value.ObservacionesMantenimiento);
        Assert.Equal(EstadoMesa.Mantenimiento, result.Value.EstadoActual);
        Assert.True(result.Value.NotificacionMantenimientoEnviada);
    }

    [Fact]
    public async Task Handle_LiberacionConEventosAutomaticos_DeberiaDispararEventos()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MotivoLiberacion = "Finalización completa",
            NotificarLiberacion = true,
            DispararEventos = true
        };

        var resultadoConEventos = CreateMockResultadoConEventos(mesaId);

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoConEventos));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.EventosDisparados);
        Assert.Equal(3, result.Value.TiposEventosDisparados.Count);
        Assert.Contains("MesaLiberada", result.Value.TiposEventosDisparados);
        Assert.Contains("EstadoMesaCambiado", result.Value.TiposEventosDisparados);
        Assert.Contains("NotificacionPersonal", result.Value.TiposEventosDisparados);

        // Verificar que se programó el job de eventos
        _backgroundJobServiceMock.Verify(
            x => x.EnqueueBackgroundJob("ProcessMesaEvents", It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_LiberacionConLimpiezaRapida_DeberiaCompletarRapidamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MotivoLiberacion = "Servicio rápido",
            LimpiezaRapida = true,
            TiempoEstimadoLimpieza = TimeSpan.FromMinutes(5)
        };

        var resultadoRapido = CreateMockResultadoLimpiezaRapida(mesaId);

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoRapido));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.LimpiezaRapidaAplicada);
        Assert.Equal(TimeSpan.FromMinutes(5), result.Value.TiempoEstimadoLimpieza);
        Assert.True(result.Value.TiempoLiberacion < TimeSpan.FromMinutes(1));
        Assert.Equal(EstadoMesa.Disponible, result.Value.EstadoActual);
    }

    [Fact]
    public async Task Handle_LiberacionConFacturacionPendiente_DeberiaNotificarFacturacion()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MotivoLiberacion = "Cliente se retira",
            VerificarFacturacion = true
        };

        var resultadoConFacturacion = CreateMockResultadoConFacturacionPendiente(mesaId);

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultadoConFacturacion));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Value.FacturacionPendiente);
        Assert.NotNull(result.Value.ComandaId);
        Assert.Equal(1250.75m, result.Value.MontoFacturacionPendiente);
        Assert.True(result.Value.NotificacionFacturacionEnviada);
        Assert.Contains("Facturación pendiente", result.Value.AlertasGeneradas);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_MesaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var mesaInexistente = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaInexistente,
            MotivoLiberacion = "Test",
            ValidarEstadoMesa = true
        };

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<LiberarMesaResult>("Mesa no encontrada en el sistema"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Mesa no encontrada en el sistema", result.Error);
    }

    [Fact]
    public async Task Handle_MesaYaDisponible_DeberiaRetornarError()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MotivoLiberacion = "Test",
            ValidarEstadoMesa = true,
            LiberacionForzada = false
        };

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<LiberarMesaResult>("La mesa ya se encuentra disponible"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La mesa ya se encuentra disponible", result.Error);
    }

    [Fact]
    public async Task Handle_MesaEnMantenimiento_DeberiaRetornarError()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MotivoLiberacion = "Test",
            ValidarEstadoMesa = true,
            LiberacionForzada = false
        };

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<LiberarMesaResult>("No se puede liberar mesa en mantenimiento"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede liberar mesa en mantenimiento", result.Error);
    }

    [Fact]
    public async Task Handle_LiberacionForzadaSinAutorizacion_DeberiaRetornarError()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MotivoLiberacion = "Emergencia",
            LiberacionForzada = true,
            UsuarioAutoriza = "" // Sin autorización
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La liberación forzada requiere autorización de supervisor", result.Error);
    }

    [Fact]
    public async Task Handle_MotivoLiberacionVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MotivoLiberacion = "", // Motivo vacío
            ValidarEstadoMesa = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Debe especificar un motivo para la liberación", result.Error);
    }

    [Fact]
    public async Task Handle_MantenimientoSinTipo_DeberiaRetornarError()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MotivoLiberacion = "Con mantenimiento",
            MantenimientoRequerido = true,
            TipoMantenimiento = "" // Sin tipo de mantenimiento
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Debe especificar el tipo de mantenimiento requerido", result.Error);
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ErrorServicioOperaciones_DeberiaRetornarErrorServicio()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MotivoLiberacion = "Test",
            ValidarEstadoMesa = true
        };

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<LiberarMesaResult>("Error en proceso de liberación"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error en proceso de liberación", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MotivoLiberacion = "Test",
            ValidarEstadoMesa = true
        };

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conectividad"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno del sistema", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorNotificacionesYContinua_DeberiaLogearYProceder()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MotivoLiberacion = "Test notificaciones",
            NotificarLiberacion = true
        };

        var resultado = CreateMockResultadoLiberacionExitosa(command.MesaId);

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultado));

        _backgroundJobServiceMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<string>(), It.IsAny<object>()))
            .Throws(new Exception("Error en servicio de notificaciones"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // Debe continuar exitosamente

        // Verificar que se loggeó el warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al programar notificaciones")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_LiberacionExitosa_DeberiaLoggearProceso()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MotivoLiberacion = "Test logging",
            ValidarEstadoMesa = true
        };

        var resultado = CreateMockResultadoLiberacionExitosa(command.MesaId);

        _operacionesServiceFacadeMock.Setup(x => x.LiberarMesaAsync(
            It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(resultado));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar logging de inicio
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando liberación de mesa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mesa liberada exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static LiberarMesaResult CreateMockResultadoLiberacionExitosa(Guid mesaId)
    {
        return new LiberarMesaResult
        {
            MesaId = mesaId,
            NumeroMesa = 15,
            EstadoAnterior = EstadoMesa.Ocupada,
            EstadoActual = EstadoMesa.Disponible,
            LiberacionExitosa = true,
            LiberacionForzada = false,
            MotivoLiberacion = "Finalización de servicio",
            FechaLiberacion = DateTime.UtcNow,
            TiempoLiberacion = TimeSpan.FromSeconds(2.5),
            MantenimientoRequerido = false,
            NotificacionEnviada = true,
            EventosDisparados = false
        };
    }

    private static LiberarMesaResult CreateMockResultadoLiberacionForzada(Guid mesaId)
    {
        return new LiberarMesaResult
        {
            MesaId = mesaId,
            NumeroMesa = 8,
            EstadoAnterior = EstadoMesa.Ocupada,
            EstadoActual = EstadoMesa.Disponible,
            LiberacionExitosa = true,
            LiberacionForzada = true,
            UsuarioQueAutorizo = "MANAGER_001",
            MotivoLiberacion = "Emergencia médica",
            FechaLiberacion = DateTime.UtcNow,
            TiempoLiberacion = TimeSpan.FromSeconds(1.2),
            NotificacionEnviada = true,
            AlertasGeneradas = new List<string> { "Liberación forzada registrada" }
        };
    }

    private static LiberarMesaResult CreateMockResultadoConMantenimiento(Guid mesaId)
    {
        return new LiberarMesaResult
        {
            MesaId = mesaId,
            NumeroMesa = 12,
            EstadoAnterior = EstadoMesa.Ocupada,
            EstadoActual = EstadoMesa.Mantenimiento,
            LiberacionExitosa = true,
            MantenimientoRequerido = true,
            TipoMantenimiento = "Limpieza profunda",
            ObservacionesMantenimiento = "Derrame de salsa",
            FechaLiberacion = DateTime.UtcNow,
            TiempoLiberacion = TimeSpan.FromSeconds(3.1),
            NotificacionEnviada = true,
            NotificacionMantenimientoEnviada = true,
            TiempoEstimadoMantenimiento = TimeSpan.FromMinutes(15)
        };
    }

    private static LiberarMesaResult CreateMockResultadoConEventos(Guid mesaId)
    {
        return new LiberarMesaResult
        {
            MesaId = mesaId,
            NumeroMesa = 20,
            EstadoAnterior = EstadoMesa.Ocupada,
            EstadoActual = EstadoMesa.Disponible,
            LiberacionExitosa = true,
            MotivoLiberacion = "Finalización completa",
            FechaLiberacion = DateTime.UtcNow,
            TiempoLiberacion = TimeSpan.FromSeconds(2.8),
            NotificacionEnviada = true,
            EventosDisparados = true,
            TiposEventosDisparados = new List<string>
            {
                "MesaLiberada",
                "EstadoMesaCambiado",
                "NotificacionPersonal"
            }
        };
    }

    private static LiberarMesaResult CreateMockResultadoLimpiezaRapida(Guid mesaId)
    {
        return new LiberarMesaResult
        {
            MesaId = mesaId,
            NumeroMesa = 5,
            EstadoAnterior = EstadoMesa.Ocupada,
            EstadoActual = EstadoMesa.Disponible,
            LiberacionExitosa = true,
            LimpiezaRapidaAplicada = true,
            TiempoEstimadoLimpieza = TimeSpan.FromMinutes(5),
            FechaLiberacion = DateTime.UtcNow,
            TiempoLiberacion = TimeSpan.FromSeconds(0.8),
            NotificacionEnviada = true,
            MotivoLiberacion = "Servicio rápido"
        };
    }

    private static LiberarMesaResult CreateMockResultadoConFacturacionPendiente(Guid mesaId)
    {
        return new LiberarMesaResult
        {
            MesaId = mesaId,
            NumeroMesa = 18,
            EstadoAnterior = EstadoMesa.Ocupada,
            EstadoActual = EstadoMesa.Disponible,
            LiberacionExitosa = true,
            FacturacionPendiente = true,
            ComandaId = Guid.NewGuid(),
            MontoFacturacionPendiente = 1250.75m,
            FechaLiberacion = DateTime.UtcNow,
            TiempoLiberacion = TimeSpan.FromSeconds(2.2),
            NotificacionEnviada = true,
            NotificacionFacturacionEnviada = true,
            AlertasGeneradas = new List<string> 
            { 
                "Facturación pendiente por $1,250.75",
                "Notificar a caja inmediatamente"
            }
        };
    }

    #endregion
} 