namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Commands;

/// <summary>
/// Tests unitarios para FinalizarComandaHandler
/// Valida la lógica de finalización de comandas y transiciones de estado
/// </summary>
public class FinalizarComandaHandlerTests
{
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<FinalizarComandaHandler>> _loggerMock;
    private readonly FinalizarComandaHandler _handler;

    public FinalizarComandaHandlerTests()
    {
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<FinalizarComandaHandler>>();
        
        _handler = new FinalizarComandaHandler(
            _comandaRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_ComandaEnProcesoConItems_DeberiaFinalizarExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = "Comanda completada satisfactoriamente",
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        var comanda = CreateMockComandaConItems(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 85.50m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(comanda.Object))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda.Object))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(comandaId, result.Value.Id);
        Assert.Equal("Finalizada", result.Value.Estado);
        Assert.Equal(85.50m, result.Value.Total);

        // Verify transición de estado
        comanda.Verify(x => x.ActualizarEstado(EstadoComanda.Finalizada), Times.Once);
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(comanda.Object), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreadaConItems_DeberiaFinalizarExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = FinalizarComandaCommand.Crear(comandaId, usuarioId, "Finalización directa");

        var comanda = CreateMockComandaConItems(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 42.75m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda.Object))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify usando factory method
        Assert.True(command.ValidarTodosItemsListos);
        Assert.True(command.NotificarMesero);
        
        comanda.Verify(x => x.ActualizarEstado(EstadoComanda.Finalizada), Times.Once);
    }

    [Fact]
    public async Task Handle_FinalizarSinValidacion_DeberiaFinalizarSinValidarItems()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = FinalizarComandaCommand.FinalizarSinValidacion(
            comandaId, 
            usuarioId, 
            "Cierre de turno",
            "Items parcialmente completados");

        var comanda = CreateMockComandaConItems(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 67.25m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda.Object))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify factory method sin validación
        Assert.False(command.ValidarTodosItemsListos);
        Assert.True(command.NotificarMesero);
        Assert.Contains("Finalizada sin validación: Cierre de turno", command.ObservacionesFinalizacion);
        
        comanda.Verify(x => x.ActualizarEstado(EstadoComanda.Finalizada), Times.Once);
    }

    [Fact]
    public async Task Handle_FinalizarSilencioso_DeberiaFinalizarSinNotificar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = FinalizarComandaCommand.FinalizarSilencioso(
            comandaId, 
            usuarioId, 
            "Finalización automática del sistema");

        var comanda = CreateMockComandaConItems(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 91.00m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda.Object))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify factory method silencioso
        Assert.True(command.ValidarTodosItemsListos);
        Assert.False(command.NotificarMesero); // Sin notificación
        
        comanda.Verify(x => x.ActualizarEstado(EstadoComanda.Finalizada), Times.Once);
    }

    [Fact]
    public async Task Handle_ConObservacionesFinalizacion_DeberiaLoguearObservaciones()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var observaciones = "Cliente satisfecho con el servicio";
        
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = observaciones,
            NotificarMesero = false
        };

        var comanda = CreateMockComandaConItems(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 55.80m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda.Object))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify logging de observaciones
        VerifyLogContains(LogLevel.Information, observaciones);
        comanda.Verify(x => x.ActualizarEstado(EstadoComanda.Finalizada), Times.Once);
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = FinalizarComandaCommand.Crear(comandaId, Guid.NewGuid());

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync((Comanda)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Comanda no encontrada", result.Error);
        
        // Verify no se intenta actualizar
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaYaFinalizada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = FinalizarComandaCommand.Crear(comandaId, Guid.NewGuid());

        var comanda = CreateMockComandaConItems(comandaId, EstadoComanda.Finalizada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La comanda ya está finalizada", result.Error);
        
        // Verify no se intenta cambiar estado
        comanda.Verify(x => x.ActualizarEstado(It.IsAny<EstadoComanda>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaCancelada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = FinalizarComandaCommand.Crear(comandaId, Guid.NewGuid());

        var comanda = CreateMockComandaConItems(comandaId, EstadoComanda.Cancelada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede finalizar una comanda cancelada", result.Error);
        
        comanda.Verify(x => x.ActualizarEstado(It.IsAny<EstadoComanda>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaSinItems_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = FinalizarComandaCommand.Crear(comandaId, Guid.NewGuid());

        var comanda = CreateMockComandaSinItems(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede finalizar una comanda sin items", result.Error);
        
        comanda.Verify(x => x.ActualizarEstado(It.IsAny<EstadoComanda>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaConItemsNull_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = FinalizarComandaCommand.Crear(comandaId, Guid.NewGuid());

        var comanda = new Mock<Comanda>();
        comanda.Setup(x => x.Id).Returns(comandaId);
        comanda.Setup(x => x.Estado).Returns(EstadoComanda.Creada);
        comanda.Setup(x => x.Items).Returns((ICollection<ItemComanda>)null);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede finalizar una comanda sin items", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorEnTransicionEstado_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = FinalizarComandaCommand.Crear(comandaId, Guid.NewGuid());

        var comanda = CreateMockComandaConItems(comandaId, EstadoComanda.EnProceso);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ReturnsAsync(comanda.Object);

        comanda.Setup(x => x.ActualizarEstado(EstadoComanda.Finalizada))
            .Throws(new InvalidOperationException("Transición de estado no válida desde EnProceso"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Transición de estado no válida desde EnProceso", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = FinalizarComandaCommand.Crear(comandaId, Guid.NewGuid());

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Error interno al finalizar comanda", result.Error);
    }

    #endregion

    #region Tests de Factory Methods

    [Fact]
    public void Crear_ConParametrosValidos_DeberiaCrearCommandCorrectamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var observaciones = "Test observaciones";

        // Act
        var command = FinalizarComandaCommand.Crear(comandaId, usuarioId, observaciones);

        // Assert
        Assert.Equal(comandaId, command.ComandaId);
        Assert.Equal(usuarioId, command.UsuarioId);
        Assert.Equal(observaciones, command.ObservacionesFinalizacion);
        Assert.True(command.ValidarTodosItemsListos);
        Assert.True(command.NotificarMesero);
        Assert.NotNull(command.FechaFinalizacion);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "ComandaId no puede estar vacío")]
    [InlineData("00000000-0000-0000-0000-000000000000", "UsuarioId no puede estar vacío")]
    public void Crear_ConParametrosInvalidos_DeberiaLanzarExcepcion(string guidString, string expectedMessage)
    {
        // Arrange
        var invalidGuid = Guid.Parse(guidString);
        var validGuid = Guid.NewGuid();

        // Act & Assert
        if (expectedMessage.Contains("ComandaId"))
        {
            var ex = Assert.Throws<ArgumentException>(() => 
                FinalizarComandaCommand.Crear(invalidGuid, validGuid));
            Assert.Contains(expectedMessage, ex.Message);
        }
        else
        {
            var ex = Assert.Throws<ArgumentException>(() => 
                FinalizarComandaCommand.Crear(validGuid, invalidGuid));
            Assert.Contains(expectedMessage, ex.Message);
        }
    }

    [Fact]
    public void FinalizarSinValidacion_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var motivo = "Emergencia";
        var observaciones = "Observaciones adicionales";

        // Act
        var command = FinalizarComandaCommand.FinalizarSinValidacion(comandaId, usuarioId, motivo, observaciones);

        // Assert
        Assert.Equal(comandaId, command.ComandaId);
        Assert.Equal(usuarioId, command.UsuarioId);
        Assert.Contains($"Finalizada sin validación: {motivo}", command.ObservacionesFinalizacion);
        Assert.Contains(observaciones, command.ObservacionesFinalizacion);
        Assert.False(command.ValidarTodosItemsListos);
        Assert.True(command.NotificarMesero);
    }

    [Fact]
    public void FinalizarSilencioso_DeberiaConfigurarCorrectamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var observaciones = "Finalización automática";

        // Act
        var command = FinalizarComandaCommand.FinalizarSilencioso(comandaId, usuarioId, observaciones);

        // Assert
        Assert.Equal(comandaId, command.ComandaId);
        Assert.Equal(usuarioId, command.UsuarioId);
        Assert.Equal(observaciones, command.ObservacionesFinalizacion);
        Assert.True(command.ValidarTodosItemsListos);
        Assert.False(command.NotificarMesero); // Silencioso
    }

    #endregion

    #region Helper Methods

    private static Mock<Comanda> CreateMockComandaConItems(Guid id, EstadoComanda estado)
    {
        var mock = new Mock<Comanda>();
        mock.Setup(x => x.Id).Returns(id);
        mock.Setup(x => x.Estado).Returns(estado);
        
        // Mock items collection with at least one item
        var items = new List<ItemComanda> 
        { 
            new Mock<ItemComanda>().Object,
            new Mock<ItemComanda>().Object
        };
        mock.Setup(x => x.Items).Returns(items);
        
        // Mock total
        var total = new Mock<TotalComanda>();
        total.Setup(x => x.Total).Returns(85.50m);
        mock.Setup(x => x.Total).Returns(total.Object);
        
        return mock;
    }

    private static Mock<Comanda> CreateMockComandaSinItems(Guid id, EstadoComanda estado)
    {
        var mock = new Mock<Comanda>();
        mock.Setup(x => x.Id).Returns(id);
        mock.Setup(x => x.Estado).Returns(estado);
        mock.Setup(x => x.Items).Returns(new List<ItemComanda>());
        return mock;
    }

    private static ComandaDto CreateMockComandaDto(Guid id, string estado, decimal total)
    {
        return new ComandaDto
        {
            Id = id,
            Estado = estado,
            Total = total,
            FechaCreacion = DateTime.UtcNow,
            NumeroComanda = "COM-001",
            Items = new List<ItemComandaDto>
            {
                new ItemComandaDto { Id = Guid.NewGuid(), NombreProducto = "Item 1", Cantidad = 1, PrecioUnitario = 25.00m },
                new ItemComandaDto { Id = Guid.NewGuid(), NombreProducto = "Item 2", Cantidad = 2, PrecioUnitario = 30.25m }
            }
        };
    }

    private void VerifyLogContains(LogLevel level, string message)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
} 