namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Commands;

/// <summary>
/// Tests unitarios para AsignarMesaHandler
/// Valida la lógica completa de asignación de mesas con gestión operacional del restaurante
/// </summary>
public class AsignarMesaHandlerTests
{
    private readonly Mock<IMesaRepository> _mesaRepositoryMock;
    private readonly Mock<ILogger<AsignarMesaHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AsignarMesaHandler _handler;

    public AsignarMesaHandlerTests()
    {
        _mesaRepositoryMock = new Mock<IMesaRepository>();
        _loggerMock = new Mock<ILogger<AsignarMesaHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new AsignarMesaHandler(
            _mesaRepositoryMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void Crear_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var observaciones = "Mesa para pareja";

        // Act
        var command = AsignarMesaCommand.Crear(mesaId, meseroId, observaciones);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.Equal(meseroId, command.MeseroId);
        Assert.Equal(observaciones, command.Observaciones);
        Assert.Equal("Manual", command.TipoAsignacion);
        Assert.True(command.NotificarMesero);
        Assert.True(command.ValidarReservacion);
    }

    [Fact]
    public void Crear_ConParametrosMinimos_DeberiaUsarValoresPorDefecto()
    {
        // Arrange
        var mesaId = Guid.NewGuid();

        // Act
        var command = AsignarMesaCommand.Crear(mesaId);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.Null(command.MeseroId);
        Assert.Null(command.Observaciones);
        Assert.Equal("Manual", command.TipoAsignacion);
        Assert.True(command.NotificarMesero);
        Assert.True(command.ValidarReservacion);
        Assert.False(command.BuscarMejorMesa);
    }

    [Fact]
    public void Crear_ConObservaciones_DeberiaAsignarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var observaciones = "Cliente VIP";

        // Act
        var command = AsignarMesaCommand.Crear(mesaId, observaciones: observaciones);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.Equal(observaciones, command.Observaciones);
        Assert.Null(command.MeseroId);
    }

    [Fact]
    public void Command_ConConfiguracionPersonalizada_DeberiaPermitirModificacion()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var reservacionId = Guid.NewGuid();

        // Act
        var command = AsignarMesaCommand.Crear(mesaId);
        command.ReservacionId = reservacionId;
        command.TipoAsignacion = "ConReservacion";
        command.NumeroPersonas = 4;
        command.BuscarMejorMesa = true;

        // Assert
        Assert.Equal("ConReservacion", command.TipoAsignacion);
        Assert.Equal(reservacionId, command.ReservacionId);
        Assert.Equal(4, command.NumeroPersonas);
        Assert.True(command.BuscarMejorMesa);
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
            TipoAsignacion = "Manual",
            Observaciones = "Mesa para dos personas"
        };

        var mesa = CreateMockMesaDisponible(mesaId, 4, "Zona Central");

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Once);
        _mesaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_AsignacionConReservacion_DeberiaAsignarCorrectamente()
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
            NumeroPersonas = 4
        };

        var mesa = CreateMockMesaDisponible(mesaId, 4, "Zona VIP");

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AsignacionAutomatica_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            TipoAsignacion = "Automatica",
            NumeroPersonas = 4,
            MeseroId = meseroId,
            BuscarMejorMesa = true
        };

        var mesa = CreateMockMesaDisponible(mesaId, 4, "Terraza");

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AsignacionVIP_DeberiaAsignarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var command = new AsignarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = meseroId,
            TipoAsignacion = "VIP",
            NumeroPersonas = 6,
            NotificarMesero = true
        };

        var mesa = CreateMockMesaVIP(mesaId, 6, "Zona VIP");

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Once);
    }

    #endregion

    #region Tests de Escenarios de Error

    [Fact]
    public async Task Handle_MesaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = AsignarMesaCommand.Crear(mesaId);

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync((Mesa?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Mesa no encontrada", result.Error);
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MesaYaOcupada_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = AsignarMesaCommand.Crear(mesaId);

        var mesaOcupada = CreateMockMesaOcupada(mesaId, 4, "Zona Central");

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesaOcupada);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExcepcionRepositorio_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = AsignarMesaCommand.Crear(mesaId);

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno del servidor", result.Error);
    }

    [Fact]
    public async Task Handle_ConObservaciones_DeberiaLoggearCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var observaciones = "Cliente con silla de ruedas";
        var command = AsignarMesaCommand.Crear(mesaId, observaciones: observaciones);

        var mesa = CreateMockMesaDisponible(mesaId, 4, "Zona Accesible");

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Mesa CreateMockMesaDisponible(Guid id, int capacidad, string ubicacion)
    {
        var mesa = Mesa.Crear(Random.Shared.Next(1, 100), capacidad, ubicacion);
        // Usar reflexión para establecer el ID ya que es privado
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, id);
        return mesa;
    }

    private static Mesa CreateMockMesaOcupada(Guid id, int capacidad, string ubicacion)
    {
        var mesa = Mesa.Crear(Random.Shared.Next(1, 100), capacidad, ubicacion);
        // Usar reflexión para establecer el ID ya que es privado
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, id);
        // Marcar como ocupada
        mesa.MarcarComoOcupada();
        return mesa;
    }

    private static Mesa CreateMockMesaVIP(Guid id, int capacidad, string ubicacion)
    {
        var mesa = Mesa.Crear(Random.Shared.Next(1, 10), capacidad, ubicacion);
        // Usar reflexión para establecer el ID ya que es privado
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, id);
        return mesa;
    }

    #endregion
} 