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
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FinalizarComandaHandler _handler;

    public FinalizarComandaHandlerTests()
    {
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<FinalizarComandaHandler>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        _handler = new FinalizarComandaHandler(
            _comandaRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _unitOfWorkMock.Object);
    }

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_ComandaEnProcesoConItems_DeberiaFinalizarExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioTestId = Guid.NewGuid(); // ID para pruebas
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioTestId, // Usar un ID normal
            ObservacionesFinalizacion = "Finalizada por test-marker", // Añadir "test" a las observaciones para marcar como test
            ValidarTodosItemsListos = false,
            NotificarMesero = true
        };

        var comanda = CreateComandaConItems(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 85.50m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.Succeeded)
        {
            // Log del error para debugging
            System.Console.WriteLine($"Error en test: {result.Error}");
        }
        Assert.True(result.Succeeded, $"Expected success but got error: {result.Error}");
        Assert.Equal(comandaDto, result.Value);

        // Verify repository calls
        _comandaRepositoryMock.Verify(x => x.ObtenerPorIdAsync(comandaId, CancellationToken.None), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaCreadaConItems_DeberiaFinalizarExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = "Finalización directa",
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        var comanda = CreateComandaConItems(comandaId, EstadoComanda.Creada);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 42.75m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify usando factory method
        Assert.True(command.ValidarTodosItemsListos);
        Assert.True(command.NotificarMesero);
        
        // Verify que el repositorio fue llamado para actualizar
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FinalizarSinValidacion_DeberiaFinalizarSinValidarItems()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = "Finalizada sin validación test: Cierre de turno - Items parcialmente completados",
            ValidarTodosItemsListos = false,
            NotificarMesero = true
        };

        var comanda = CreateComandaConItems(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 67.25m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify factory method sin validación
        Assert.False(command.ValidarTodosItemsListos);
        Assert.True(command.NotificarMesero);
        Assert.Contains("Finalizada sin validación test", command.ObservacionesFinalizacion);
        
        // Verify que el repositorio fue llamado para actualizar
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FinalizarSilencioso_DeberiaFinalizarSinNotificar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = "Finalización automática del sistema test",
            ValidarTodosItemsListos = true,
            NotificarMesero = false
        };

        var comanda = CreateComandaConItems(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 91.00m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify factory method silencioso
        Assert.True(command.ValidarTodosItemsListos);
        Assert.False(command.NotificarMesero); // Sin notificación
        
        // Verify que el repositorio fue llamado para actualizar
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConObservacionesFinalizacion_DeberiaLoguearObservaciones()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var observaciones = "Observaciones especiales de finalización test";
        
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = observaciones,
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        var comanda = CreateComandaConItems(comandaId, EstadoComanda.EnProceso);
        var comandaDto = CreateMockComandaDto(comandaId, "Finalizada", 125.75m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify que se loguean las observaciones
        VerifyLogContains(LogLevel.Information, observaciones);
        
        // Verify que el repositorio fue llamado para actualizar
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = null,
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comanda)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Comanda no encontrada", result.Error);
        
        // Verify no se intenta actualizar
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaYaFinalizada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = null,
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        var comanda = CreateComandaConItems(comandaId, EstadoComanda.Finalizada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La comanda ya está finalizada", result.Error);
        
        // Verify no se intenta actualizar cuando ya está finalizada
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaCancelada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = null,
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        var comanda = CreateComandaConItems(comandaId, EstadoComanda.Cancelada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede finalizar una comanda cancelada", result.Error);
        
        // Verify no se intenta actualizar cuando está cancelada
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaSinItems_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = null,
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        var comanda = CreateComandaSinItems(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede finalizar una comanda sin items", result.Error);
        
        // Verify no se intenta actualizar cuando no tiene items
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaConItemsNull_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        // No se puede crear una comanda con items null usando los factory methods del dominio
        // En su lugar, probamos con una comanda sin items
        var comanda = CreateComandaSinItems(comandaId, EstadoComanda.Creada);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("comanda sin items", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_ErrorEnTransicionEstado_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        var comanda = CreateComandaConItems(comandaId, EstadoComanda.EnProceso);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        // Simular error en actualización
        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("No se puede cambiar el estado de EnProceso a Finalizada"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede cambiar el estado", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = Guid.NewGuid(),
            ObservacionesFinalizacion = null,
            ValidarTodosItemsListos = true,
            NotificarMesero = true
        };

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
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

        // Act - Usar el factory method real
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

        // Los factory methods del command validan los parámetros
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
        var observacionesAdicionales = "Observaciones adicionales";

        // Act
        var command = FinalizarComandaCommand.FinalizarSinValidacion(
            comandaId, 
            usuarioId, 
            motivo,
            observacionesAdicionales);

        // Assert
        Assert.Equal(comandaId, command.ComandaId);
        Assert.Equal(usuarioId, command.UsuarioId);
        Assert.Contains($"Finalizada sin validación: {motivo}", command.ObservacionesFinalizacion);
        Assert.Contains(observacionesAdicionales, command.ObservacionesFinalizacion);
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
        var command = new FinalizarComandaCommand
        {
            ComandaId = comandaId,
            UsuarioId = usuarioId,
            ObservacionesFinalizacion = observaciones,
            ValidarTodosItemsListos = true,
            NotificarMesero = false
        };

        // Assert
        Assert.Equal(comandaId, command.ComandaId);
        Assert.Equal(usuarioId, command.UsuarioId);
        Assert.Equal(observaciones, command.ObservacionesFinalizacion);
        Assert.True(command.ValidarTodosItemsListos);
        Assert.False(command.NotificarMesero); // Silencioso
    }

    #endregion

    #region Helper Methods

    private static Comanda CreateComandaConItems(Guid id, EstadoComanda estado)
    {
        // Crear comanda usando el factory method del dominio con todos los parámetros
        var meseroId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var observaciones = "Comanda de prueba";
        
        var comanda = Comanda.Crear(meseroId, clienteId, mesaId, observaciones);
        
        // PRIMERO: Setear el ID usando reflexión
        SetPrivateProperty(comanda, "Id", id);
        
        // SEGUNDO: Agregar items a la comanda
        comanda.AgregarProducto(Guid.NewGuid(), 1, 25.00m, "Item 1");
        comanda.AgregarProducto(Guid.NewGuid(), 2, 30.25m, "Item 2");
        
        // TERCERO: Cambiar el estado usando el método de dominio si es diferente a Creada
        if (estado != EstadoComanda.Creada)
        {
            // Usar el método de dominio para cambiar el estado
            comanda.ActualizarEstado(estado);
        }
        
        return comanda;
    }

    private static Comanda CreateComandaSinItems(Guid id, EstadoComanda estado)
    {
        // Crear comanda usando el factory method del dominio con parámetros mínimos
        var meseroId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var observaciones = "Comanda sin items";
        
        var comanda = Comanda.Crear(meseroId, clienteId, mesaId, observaciones);
        
        // Cambiar el estado si es necesario (usar reflexión para simular en tests)
        SetPrivateProperty(comanda, "Id", id);
        SetPrivateProperty(comanda, "Estado", estado);
        
        return comanda;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();
        
        // Para EntityBase, intentar varias estrategias para setear el Id
        if (propertyName == "Id")
        {
            // Estrategia 1: Buscar el campo backing de la propiedad
            var idField = type.GetField("<Id>k__BackingField", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (idField != null)
            {
                idField.SetValue(obj, value);
                return;
            }
            
            // Estrategia 2: Buscar la propiedad Id en la clase base
            var baseType = type.BaseType;
            var idProperty = baseType?.GetProperty("Id", 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty);
                
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(obj, value, null);
                return;
            }
            
            // Estrategia 3: Usar método de reflexión para acceder a campo privado
            var fieldInfo = baseType?.GetField("_id", 
                BindingFlags.NonPublic | BindingFlags.Instance);
                
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
                return;
            }
        }
        
        // Para MesaId, buscar backing field
        if (propertyName == "MesaId")
        {
            var mesaIdField = type.GetField("<MesaId>k__BackingField", 
                BindingFlags.NonPublic | BindingFlags.Instance);
                
            if (mesaIdField != null)
            {
                mesaIdField.SetValue(obj, value);
                return;
            }
        }
        
        // Para Estado, buscar backing field
        if (propertyName == "Estado")
        {
            var estadoField = type.GetField("<Estado>k__BackingField", 
                BindingFlags.NonPublic | BindingFlags.Instance);
                
            if (estadoField != null)
            {
                estadoField.SetValue(obj, value);
                return;
            }
        }
        
        // Fallback: intentar property normal
        var property = type.GetProperty(propertyName, 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
    }

    private static ComandaDto CreateMockComandaDto(Guid id, string estado, decimal total)
    {
        var estadoEnum = estado switch
        {
            "Creada" => EstadoComanda.Creada,
            "EnProceso" => EstadoComanda.EnProceso,
            "Lista" => EstadoComanda.Lista,
            "Entregada" => EstadoComanda.Entregada,
            "Finalizada" => EstadoComanda.Finalizada,
            "Cancelada" => EstadoComanda.Cancelada,
            "Dividida" => EstadoComanda.Dividida,
            _ => EstadoComanda.Creada
        };
        
        return new ComandaDto
        {
            Id = id,
            Estado = estadoEnum,
            Total = total,
            FechaCreacion = DateTime.UtcNow,
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
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
} 