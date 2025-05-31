namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Commands;

/// <summary>
/// Tests unitarios para AsignarMesaHandler
/// Valida la lógica completa de asignación de mesas con gestión operacional del restaurante
/// </summary>
public class AsignarMesaHandlerTests
{
    private readonly Mock<IMesaRepository> _mesaRepositoryMock;
    private readonly Mock<IOperacionesServiceFacade> _operacionesServiceFacadeMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AsignarMesaHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly AsignarMesaHandler _handler;

    public AsignarMesaHandlerTests()
    {
        _mesaRepositoryMock = new Mock<IMesaRepository>();
        _operacionesServiceFacadeMock = new Mock<IOperacionesServiceFacade>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AsignarMesaHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _handler = new AsignarMesaHandler(
            _mesaRepositoryMock.Object,
            _operacionesServiceFacadeMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _notificationServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void AsignacionDirecta_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var observaciones = "Mesa para pareja";

        // Act
        var command = AsignarMesaCommand.AsignacionDirecta(mesaId, meseroId, 2, observaciones);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.Equal(meseroId, command.MeseroId);
        Assert.Equal(2, command.NumeroPersonas);
        Assert.Equal(observaciones, command.Observaciones);
        Assert.Equal("Directa", command.TipoAsignacion);
        Assert.Equal(3, command.Prioridad);
        Assert.True(command.NotificarMesero);
    }

    [Fact]
    public void AsignacionAutomatica_ConPreferencias_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var numeroPersonas = 4;
        var meseroId = Guid.NewGuid();
        var zonaPreferida = "Terraza";

        // Act
        var command = AsignarMesaCommand.AsignacionAutomatica(numeroPersonas, meseroId, zonaPreferida);

        // Assert
        Assert.Equal("Automatica", command.TipoAsignacion);
        Assert.Equal(numeroPersonas, command.NumeroPersonas);
        Assert.Equal(meseroId, command.MeseroId);
        Assert.Equal(zonaPreferida, command.ZonaPreferida);
        Assert.Equal(2, command.Prioridad);
        Assert.True(command.BuscarMejorMesa);
    }

    [Fact]
    public void AsignacionConReservacion_ConCodigoReservacion_DeberiaAsociarReservacion()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var reservacionId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var codigoReservacion = "RES-2025-001";

        // Act
        var command = AsignarMesaCommand.AsignacionConReservacion(mesaId, reservacionId, meseroId, codigoReservacion);

        // Assert
        Assert.Equal("ConReservacion", command.TipoAsignacion);
        Assert.Equal(reservacionId, command.ReservacionId);
        Assert.Equal(codigoReservacion, command.CodigoReservacion);
        Assert.Equal(5, command.Prioridad);
        Assert.True(command.ValidarReservacion);
        Assert.True(command.NotificarCliente);
    }

    [Fact]
    public void AsignacionVIP_ConServiciosEspeciales_DeberiaConfigurarVIP()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var numeroPersonas = 6;
        var serviciosEspeciales = new List<string> { "Sommelier", "Chef Privado" };

        // Act
        var command = AsignarMesaCommand.AsignacionVIP(mesaId, meseroId, numeroPersonas, serviciosEspeciales);

        // Assert
        Assert.Equal("VIP", command.TipoAsignacion);
        Assert.Equal(serviciosEspeciales, command.ServiciosEspeciales);
        Assert.Equal(8, command.Prioridad);
        Assert.True(command.RequiereAprobacionGerencia);
        Assert.True(command.NotificarEquipoCompleto);
    }

    #endregion

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_AsignacionDirectaSimple_DeberiaAsignarMesaExitosamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = meseroId,
            NumeroPersonas = 2,
            TipoAsignacion = "Directa",
            Observaciones = "Mesa para dos personas"
        };

        var mesa = CreateMockMesaDisponible(mesaId, 4, "Zona Central");
        var mesaDto = CreateMockMesaDto(mesaId, 4, EstadoMesa.Ocupada);

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _mapperMock.Setup(x => x.Map<MesaDto>(It.IsAny<Mesa>()))
            .Returns(mesaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(mesaId, result.Value.Id);
        Assert.Equal(EstadoMesa.Ocupada, result.Value.Estado);
        Assert.Equal(meseroId, result.Value.MeseroAsignadoId);
        
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AsignacionAutomatica_DeberiaEncontrarMejorMesa()
    {
        // Arrange
        var meseroId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            TipoAsignacion = "Automatica",
            NumeroPersonas = 4,
            MeseroId = meseroId,
            ZonaPreferida = "Terraza",
            BuscarMejorMesa = true
        };

        var mesaOptima = CreateMockMesaDisponible(Guid.NewGuid(), 4, "Terraza");
        var mesaDto = CreateMockMesaDto(mesaOptima.Id, 4, EstadoMesa.Ocupada);

        _operacionesServiceFacadeMock.Setup(x => x.EncontrarMejorMesaAsync(
            It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(mesaOptima));
        _mapperMock.Setup(x => x.Map<MesaDto>(It.IsAny<Mesa>()))
            .Returns(mesaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(4, result.Value.Capacidad);
        Assert.Contains("Terraza", result.Value.Zona);
        
        _operacionesServiceFacadeMock.Verify(x => x.EncontrarMejorMesaAsync(4, "Terraza", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AsignacionConReservacion_DeberiaValidarReservacion()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var reservacionId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            ReservacionId = reservacionId,
            MeseroId = meseroId,
            TipoAsignacion = "ConReservacion",
            ValidarReservacion = true,
            CodigoReservacion = "RES-2025-001"
        };

        var mesa = CreateMockMesaDisponible(mesaId, 6, "Zona VIP");
        var mesaDto = CreateMockMesaDto(mesaId, 6, EstadoMesa.Reservada);

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _operacionesServiceFacadeMock.Setup(x => x.ValidarReservacionAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));
        _mapperMock.Setup(x => x.Map<MesaDto>(It.IsAny<Mesa>()))
            .Returns(mesaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(EstadoMesa.Reservada, result.Value.Estado);
        
        _operacionesServiceFacadeMock.Verify(x => x.ValidarReservacionAsync(reservacionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AsignacionVIP_DeberiaNotificarEquipoCompleto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = meseroId,
            NumeroPersonas = 8,
            TipoAsignacion = "VIP",
            ServiciosEspeciales = new List<string> { "Sommelier", "Chef Privado" },
            NotificarEquipoCompleto = true,
            RequiereAprobacionGerencia = true
        };

        var mesa = CreateMockMesaVIP(mesaId, 8, "Zona Privada");
        var mesaDto = CreateMockMesaDto(mesaId, 8, EstadoMesa.Ocupada);

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _mapperMock.Setup(x => x.Map<MesaDto>(It.IsAny<Mesa>()))
            .Returns(mesaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(TipoMesa.VIP, result.Value.Tipo);
        
        // Verificar que se notificó al equipo completo
        _notificationServiceMock.Verify(x => x.EnviarNotificacionAsync(
            It.Is<string>(msg => msg.Contains("VIP")),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    #endregion

    #region Tests de Validaciones de Negocio

    [Fact]
    public async Task Handle_MesaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            TipoAsignacion = "Directa"
        };

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mesa?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("no encontrada", result.Error);
    }

    [Fact]
    public async Task Handle_MesaYaOcupada_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            TipoAsignacion = "Directa"
        };

        var mesa = CreateMockMesaOcupada(mesaId, 4, "Zona Central");
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("ocupada", result.Error);
    }

    [Fact]
    public async Task Handle_CapacidadInsuficiente_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            NumeroPersonas = 6, // Más personas que la capacidad de la mesa
            TipoAsignacion = "Directa"
        };

        var mesa = CreateMockMesaDisponible(mesaId, 4, "Zona Central"); // Capacidad para 4
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("capacidad insuficiente", result.Error);
    }

    [Fact]
    public async Task Handle_ReservacionInvalida_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var reservacionId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            ReservacionId = reservacionId,
            TipoAsignacion = "ConReservacion",
            ValidarReservacion = true
        };

        var mesa = CreateMockMesaDisponible(mesaId, 4, "Zona Central");
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _operacionesServiceFacadeMock.Setup(x => x.ValidarReservacionAsync(reservacionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<bool>("Reservación no válida o expirada"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Reservación no válida", result.Error);
    }

    [Fact]
    public async Task Handle_AutomaticoSinMesasDisponibles_DeberiaRetornarError()
    {
        // Arrange
        var command = new AsignarMesaCommand
        {
            TipoAsignacion = "Automatica",
            NumeroPersonas = 4,
            BuscarMejorMesa = true
        };

        _operacionesServiceFacadeMock.Setup(x => x.EncontrarMejorMesaAsync(
            It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Mesa>("No hay mesas disponibles para el número de personas solicitado"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No hay mesas disponibles", result.Error);
    }

    #endregion

    #region Tests de Manejo de Errores

    [Fact]
    public async Task Handle_ExcepcionRepositorio_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            TipoAsignacion = "Directa"
        };

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorNotificaciones_DeberiaLoggearWarningPeroNoFallar()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = meseroId,
            TipoAsignacion = "Directa",
            NotificarMesero = true
        };

        var mesa = CreateMockMesaDisponible(mesaId, 4, "Zona Central");
        var mesaDto = CreateMockMesaDto(mesaId, 4, EstadoMesa.Ocupada);

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mesa);
        _mapperMock.Setup(x => x.Map<MesaDto>(It.IsAny<Mesa>()))
            .Returns(mesaDto);
        _notificationServiceMock.Setup(x => x.EnviarNotificacionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de notificación"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded); // Debe seguir siendo exitoso
        
        // Verificar que se loggeó un warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error al enviar notificación")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Métodos Helper

    private static Mesa CreateMockMesaDisponible(Guid id, int capacidad, string zona)
    {
        return new Mesa
        {
            Id = id,
            Numero = $"M{Random.Shared.Next(1, 100):D2}",
            Capacidad = capacidad,
            Estado = EstadoMesa.Disponible,
            Zona = zona,
            Tipo = TipoMesa.Regular,
            Activa = true
        };
    }

    private static Mesa CreateMockMesaOcupada(Guid id, int capacidad, string zona)
    {
        return new Mesa
        {
            Id = id,
            Numero = $"M{Random.Shared.Next(1, 100):D2}",
            Capacidad = capacidad,
            Estado = EstadoMesa.Ocupada,
            Zona = zona,
            Tipo = TipoMesa.Regular,
            Activa = true
        };
    }

    private static Mesa CreateMockMesaVIP(Guid id, int capacidad, string zona)
    {
        return new Mesa
        {
            Id = id,
            Numero = $"VIP{Random.Shared.Next(1, 10):D2}",
            Capacidad = capacidad,
            Estado = EstadoMesa.Disponible,
            Zona = zona,
            Tipo = TipoMesa.VIP,
            Activa = true
        };
    }

    private static MesaDto CreateMockMesaDto(Guid id, int capacidad, EstadoMesa estado)
    {
        return new MesaDto
        {
            Id = id,
            Numero = $"M{Random.Shared.Next(1, 100):D2}",
            Capacidad = capacidad,
            Estado = estado,
            Zona = "Zona Test",
            Tipo = TipoMesa.Regular,
            MeseroAsignadoId = Guid.NewGuid(),
            FechaAsignacion = DateTime.UtcNow
        };
    }

    #endregion
} 