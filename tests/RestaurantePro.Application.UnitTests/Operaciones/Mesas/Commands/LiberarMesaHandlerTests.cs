namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Commands;

/// <summary>
/// Tests unitarios para LiberarMesaHandler
/// Valida la lógica de liberación de mesas
/// </summary>
public class LiberarMesaHandlerTests
{
    private readonly Mock<IMesaRepository> _mesaRepositoryMock;
    private readonly Mock<ILogger<LiberarMesaHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly LiberarMesaHandler _handler;

    public LiberarMesaHandlerTests()
    {
        _mesaRepositoryMock = new Mock<IMesaRepository>();
        _loggerMock = new Mock<ILogger<LiberarMesaHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new LiberarMesaHandler(
            _mesaRepositoryMock.Object,
            _loggerMock.Object,
            _currentUserServiceMock.Object);
    }

    #region Tests de Factory Methods del Command

    [Fact]
    public void LiberarMesaCommand_Crear_ConMesaId_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var observaciones = "Finalización de servicio";

        // Act
        var command = LiberarMesaCommand.Crear(mesaId, null, observaciones);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.Null(command.MeseroId);
        Assert.Equal(observaciones, command.Observaciones);
    }

    [Fact]
    public void LiberarMesaCommand_Crear_ConMeseroId_DeberiaConfigurarMesero()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var observaciones = "Mesa liberada por mesero";

        // Act
        var command = LiberarMesaCommand.Crear(mesaId, meseroId, observaciones);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.Equal(meseroId, command.MeseroId);
        Assert.Equal(observaciones, command.Observaciones);
    }

    [Fact]
    public void LiberarMesaCommand_Crear_SinObservaciones_DeberiaCrearCommandBasico()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();

        // Act
        var command = LiberarMesaCommand.Crear(mesaId, meseroId);

        // Assert
        Assert.Equal(mesaId, command.MesaId);
        Assert.Equal(meseroId, command.MeseroId);
        Assert.Null(command.Observaciones);
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
            MeseroId = null,
            Observaciones = "Finalización de servicio"
        };

        var mesa = CreateMockMesa(mesaId, EstadoMesa.Ocupada);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(mesa), Times.Once);
        _mesaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_LiberacionConMesero_DeberiaLiberarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var meseroId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = meseroId,
            Observaciones = "Mesa atendida completamente"
        };

        var mesa = CreateMockMesa(mesaId, EstadoMesa.Ocupada);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(mesa), Times.Once);
        _mesaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_LiberacionConObservaciones_DeberiaLiberarCorrectamente()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var observaciones = "Mesa requiere limpieza especial";
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = null,
            Observaciones = observaciones
        };

        var mesa = CreateMockMesa(mesaId, EstadoMesa.Ocupada);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(mesa), Times.Once);
        _mesaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Once);
    }

    #endregion

    #region Tests de Errores

    [Fact]
    public async Task Handle_MesaInexistente_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = null,
            Observaciones = "Test"
        };

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync((Mesa?)null);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Mesa no encontrada", result.Error);
        _mesaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Mesa>()), Times.Never);
        _mesaRepositoryMock.Verify(x => x.GuardarCambiosAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_MesaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new LiberarMesaCommand
        {
            MesaId = Guid.Empty,
            MeseroId = null,
            Observaciones = "Test"
        };

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Mesa no encontrada", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = null,
            Observaciones = "Test"
        };

        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ThrowsAsync(new Exception("Error de base de datos"));

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error liberando mesa", result.Error);
    }

    #endregion

    #region Tests de Logging

    [Fact]
    public async Task Handle_LiberacionExitosa_DeberiaLoggearProceso()
    {
        // Arrange
        var mesaId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new LiberarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = null,
            Observaciones = "Test logging"
        };

        var mesa = CreateMockMesa(mesaId, EstadoMesa.Ocupada);
        _mesaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(mesaId))
            .ReturnsAsync(mesa);

        _currentUserServiceMock.Setup(x => x.UserId)
            .Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);

        // Verificar que se loggeó el inicio del proceso
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando liberación de mesa")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Mesa CreateMockMesa(Guid mesaId, EstadoMesa estado)
    {
        // Crear mesa usando el constructor que requiere parámetros
        var mesa = Mesa.Crear(1, 4, "Salón principal", "Mesa estándar");
        
        // Usar reflexión para establecer el ID y estado si es necesario
        var idProperty = typeof(Mesa).GetProperty("Id");
        if (idProperty != null && idProperty.CanWrite)
        {
            idProperty.SetValue(mesa, mesaId);
        }

        // Establecer estado según sea necesario
        if (estado == EstadoMesa.Ocupada)
        {
            mesa.MarcarComoOcupada();
        }

        return mesa;
    }

    #endregion
} 