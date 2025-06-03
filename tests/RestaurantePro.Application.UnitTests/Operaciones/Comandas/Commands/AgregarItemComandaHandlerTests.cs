namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Commands;

/// <summary>
/// Tests unitarios para AgregarItemComandaHandler
/// Valida la lógica de agregar productos a comandas existentes con personalizaciones
/// </summary>
public class AgregarItemComandaHandlerTests
{
    private readonly Mock<IComandaRepository> _comandaRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AgregarItemComandaHandler>> _loggerMock;
    private readonly AgregarItemComandaHandler _handler;

    public AgregarItemComandaHandlerTests()
    {
        _comandaRepositoryMock = new Mock<IComandaRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AgregarItemComandaHandler>>();
        
        _handler = new AgregarItemComandaHandler(
            _comandaRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #region Tests de Escenarios Exitosos

    [Fact]
    public async Task Handle_ComandaCreadaSinPersonalizaciones_DeberiaAgregarItemExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Pizza Margarita",
            Cantidad = 2,
            PrecioUnitario = 25.50m,
            Observaciones = "Sin cebolla",
            UsuarioId = usuarioId
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(productoId, 2, 25.50m);
        var comandaDto = CreateMockComandaDto(comandaId, 51.00m);

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda.Object);

        comanda.Setup(x => x.AgregarItem(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .Returns(itemComanda.Object);

        _mapperMock.Setup(x => x.Map<ComandaDto>(comanda.Object))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(comandaId, result.Value.Id);
        Assert.Equal(51.00m, result.Value.Total);

        // Verify domain method was called
        comanda.Verify(x => x.AgregarItem(
            productoId,
            "Pizza Margarita",
            2,
            25.50m,
            It.IsAny<string>()), Times.Once);

        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(comanda.Object, CancellationToken.None), Times.Once);
        _comandaRepositoryMock.Verify(x => x.GuardarCambiosAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaEnProcesoSinPersonalizaciones_DeberiaAgregarItemExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Hamburguesa Clásica",
            Cantidad = 1,
            PrecioUnitario = 35.00m,
            UsuarioId = Guid.NewGuid()
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.EnProceso);
        var itemComanda = CreateMockItemComanda(productoId, 1, 35.00m);
        var comandaDto = CreateMockComandaDto(comandaId, 35.00m);

        SetupRepositoryAndMapper(comanda, itemComanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(35.00m, result.Value.Total);
        
        comanda.Verify(x => x.AgregarItem(
            productoId,
            "Hamburguesa Clásica",
            1,
            35.00m,
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPersonalizacionExtra_DeberiaAgregarItemYPersonalizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Pizza Especial",
            Cantidad = 1,
            PrecioUnitario = 28.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Extra",
                    IngredienteId = ingredienteId,
                    Cantidad = 1,
                    PrecioAdicional = 3.50m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(productoId, 1, 28.00m);
        var comandaDto = CreateMockComandaDto(comandaId, 31.50m);

        SetupRepositoryAndMapper(comanda, itemComanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(31.50m, result.Value.Total);
        
        // Verify item was added
        comanda.Verify(x => x.AgregarItem(
            productoId,
            "Pizza Especial",
            1,
            28.00m,
            It.IsAny<string>()), Times.Once);

        // Verify personalization was applied
        itemComanda.Verify(x => x.AgregarPersonalizacionExtra(
            ingredienteId,
            It.IsAny<string>(),
            1,
            3.50m), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPersonalizacionQuitar_DeberiaAgregarItemYPersonalizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Ensalada César",
            Cantidad = 1,
            PrecioUnitario = 18.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Quitar",
                    IngredienteId = ingredienteId,
                    Cantidad = 0,
                    PrecioAdicional = 0
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.EnProceso);
        var itemComanda = CreateMockItemComanda(productoId, 1, 18.00m);
        var comandaDto = CreateMockComandaDto(comandaId, 18.00m);

        SetupRepositoryAndMapper(comanda, itemComanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify personalization was applied
        itemComanda.Verify(x => x.AgregarPersonalizacionQuitar(
            ingredienteId,
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPersonalizacionSustituir_DeberiaAgregarItemYPersonalizacion()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var ingredienteOriginalId = Guid.NewGuid();
        var ingredienteSustitutoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Sandwich Especial",
            Cantidad = 1,
            PrecioUnitario = 22.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Sustituir",
                    IngredienteId = ingredienteOriginalId,
                    IngredienteSustitucionId = ingredienteSustitutoId,
                    Cantidad = 1,
                    PrecioAdicional = 2.00m
                }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(productoId, 1, 22.00m);
        var comandaDto = CreateMockComandaDto(comandaId, 24.00m);

        SetupRepositoryAndMapper(comanda, itemComanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify personalization was applied
        itemComanda.Verify(x => x.AgregarPersonalizacionSustituir(
            ingredienteOriginalId,
            It.IsAny<string>(),
            ingredienteSustitutoId,
            It.IsAny<string>(),
            1,
            2.00m), Times.Once);
    }

    [Fact]
    public async Task Handle_ConMultiplesPersonalizaciones_DeberiaAgregarTodasLasPersonalizaciones()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var command = new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Pizza Personalizada",
            Cantidad = 1,
            PrecioUnitario = 30.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto { Tipo = "Extra", IngredienteId = Guid.NewGuid(), Cantidad = 1, PrecioAdicional = 2.50m },
                new PersonalizacionCreateDto { Tipo = "Quitar", IngredienteId = Guid.NewGuid(), Cantidad = 0, PrecioAdicional = 0 },
                new PersonalizacionCreateDto { Tipo = "Sustituir", IngredienteId = Guid.NewGuid(), IngredienteSustitucionId = Guid.NewGuid(), Cantidad = 1, PrecioAdicional = 1.50m }
            }
        };

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(productoId, 1, 30.00m);
        var comandaDto = CreateMockComandaDto(comandaId, 34.00m);

        SetupRepositoryAndMapper(comanda, itemComanda, comandaDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        
        // Verify all personalizations were applied
        itemComanda.Verify(x => x.AgregarPersonalizacionExtra(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>()), Times.Once);

        itemComanda.Verify(x => x.AgregarPersonalizacionQuitar(
            It.IsAny<Guid>(),
            It.IsAny<string>()), Times.Once);

        itemComanda.Verify(x => x.AgregarPersonalizacionSustituir(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>()), Times.Once);
    }

    #endregion

    #region Tests de Validaciones y Errores

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comanda)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La comanda especificada no existe", result.Error);
        
        // Verify no se intenta actualizar
        _comandaRepositoryMock.Verify(x => x.ActualizarAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaFinalizada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Finalizada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede agregar items a una comanda en estado 'Finalizada'", result.Error);
        
        // Verify no se intenta agregar item
        comanda.Verify(x => x.AgregarItem(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<decimal>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaCancelada_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Cancelada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede agregar items a una comanda en estado 'Cancelada'", result.Error);
    }

    [Fact]
    public async Task Handle_ComandaLista_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Lista);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se puede agregar items a una comanda en estado 'Lista'", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorAlAgregarProducto_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda.Object);

        comanda.Setup(x => x.AgregarItem(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .Throws(new ArgumentException("Ya existe un item con el producto especificado"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Ya existe un item con el producto especificado", result.Error);
    }

    [Fact]
    public async Task Handle_BusinessRuleViolationException_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda.Object);

        comanda.Setup(x => x.AgregarItem(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .Throws(new BusinessRuleViolationException("AgregarItem", "Comanda", "Violación de regla de negocio", "Comandas"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Violación de regla de negocio", result.Error);
    }

    #endregion

    #region Tests de Validaciones de Personalizaciones

    [Fact]
    public async Task Handle_PersonalizacionIngredienteIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Extra";
            personalizacion.IngredienteId = Guid.Empty;
            personalizacion.Cantidad = 1;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(Guid.NewGuid(), 1, 25.00m);
        
        SetupRepositoryAndMapper(comanda, itemComanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El ID del ingrediente es requerido para la personalización", result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionTipoInvalido_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "INVALIDO";
            personalizacion.IngredienteId = Guid.NewGuid();
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(Guid.NewGuid(), 1, 25.00m);
        
        SetupRepositoryAndMapper(comanda, itemComanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Tipo de personalización no válido: INVALIDO", result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionExtraCantidadCero_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Extra";
            personalizacion.IngredienteId = Guid.NewGuid();
            personalizacion.Cantidad = 0;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(Guid.NewGuid(), 1, 25.00m);
        
        SetupRepositoryAndMapper(comanda, itemComanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("La cantidad debe ser mayor que cero para personalizaciones de tipo Extra", result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionSustituirSinIngredienteReemplazo_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Sustituir";
            personalizacion.IngredienteId = Guid.NewGuid();
            personalizacion.IngredienteSustitucionId = Guid.Empty;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(Guid.NewGuid(), 1, 25.00m);
        
        SetupRepositoryAndMapper(comanda, itemComanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Se requiere especificar el ingrediente de sustitución para personalizaciones de tipo Sustituir", result.Error);
    }

    [Fact]
    public async Task Handle_PersonalizacionPrecioNegativo_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Extra";
            personalizacion.IngredienteId = Guid.NewGuid();
            personalizacion.Cantidad = 1;
            personalizacion.PrecioAdicional = -5.00m;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(Guid.NewGuid(), 1, 25.00m);
        
        SetupRepositoryAndMapper(comanda, itemComanda, new ComandaDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("El precio adicional no puede ser negativo", result.Error);
    }

    [Fact]
    public async Task Handle_ErrorAlAplicarPersonalizacion_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateCommandWithPersonalization(comandaId, personalizacion => 
        {
            personalizacion.Tipo = "Extra";
            personalizacion.IngredienteId = Guid.NewGuid();
            personalizacion.Cantidad = 1;
            personalizacion.PrecioAdicional = 3.50m;
        });

        var comanda = CreateMockComanda(comandaId, EstadoComanda.Creada);
        var itemComanda = CreateMockItemComanda(Guid.NewGuid(), 1, 25.00m);
        
        SetupRepositoryAndMapper(comanda, itemComanda, new ComandaDto());

        itemComanda.Setup(x => x.AgregarPersonalizacionExtra(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>()))
            .Throws(new InvalidOperationException("No se pueden agregar personalizaciones a un ítem completado"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("No se pueden agregar personalizaciones a un ítem completado", result.Error);
    }

    [Fact]
    public async Task Handle_ExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var command = CreateBasicCommand(comandaId, Guid.NewGuid());

        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(comandaId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Ocurrió un error interno al procesar la solicitud", result.Error);
    }

    #endregion

    #region Helper Methods

    private static AgregarItemComandaCommand CreateBasicCommand(Guid comandaId, Guid productoId)
    {
        return new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = productoId,
            NombreProducto = "Test Product",
            Cantidad = 1,
            PrecioUnitario = 25.00m,
            UsuarioId = Guid.NewGuid()
        };
    }

    private static AgregarItemComandaCommand CreateCommandWithPersonalization(
        Guid comandaId,
        Action<PersonalizacionCreateDto> configurePersonalization)
    {
        var personalizacion = new PersonalizacionCreateDto();
        configurePersonalization(personalizacion);

        return new AgregarItemComandaCommand
        {
            ComandaId = comandaId,
            ProductoId = Guid.NewGuid(),
            NombreProducto = "Test Product with Personalization",
            Cantidad = 1,
            PrecioUnitario = 25.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto> { personalizacion }
        };
    }

    private static Mock<Comanda> CreateMockComanda(Guid id, EstadoComanda estado)
    {
        var mock = new Mock<Comanda>();
        mock.Setup(x => x.Estado).Returns(estado);
        mock.Setup(x => x.PuedeAgregarItems()).Returns(estado != EstadoComanda.Finalizada && estado != EstadoComanda.Cancelada);
        return mock;
    }

    private static Mock<ItemComanda> CreateMockItemComanda(Guid productoId, int cantidad, decimal precio)
    {
        var mock = new Mock<ItemComanda>();
        mock.Setup(x => x.ProductoId).Returns(productoId);
        mock.Setup(x => x.Cantidad).Returns(cantidad);
        mock.Setup(x => x.PrecioUnitario).Returns(precio);
        mock.Setup(x => x.Subtotal).Returns(cantidad * precio);
        return mock;
    }

    private static ComandaDto CreateMockComandaDto(Guid id, decimal total)
    {
        return new ComandaDto
        {
            Id = id,
            Estado = EstadoComanda.EnProceso,
            Total = total,
            FechaCreacion = DateTime.UtcNow,
            Items = new List<ItemComandaDto>
            {
                new ItemComandaDto 
                { 
                    Id = Guid.NewGuid(), 
                    NombreProducto = "Test Product", 
                    Cantidad = 1, 
                    PrecioUnitario = total
                }
            }
        };
    }

    private void SetupRepositoryAndMapper(Mock<Comanda> comanda, Mock<ItemComanda> itemComanda, ComandaDto comandaDto)
    {
        _comandaRepositoryMock.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda.Object);

        _comandaRepositoryMock.Setup(x => x.ActualizarAsync(comanda.Object, CancellationToken.None))
            .Returns(Task.CompletedTask);

        _comandaRepositoryMock.Setup(x => x.GuardarCambiosAsync(CancellationToken.None))
            .ReturnsAsync(1);

        comanda.Setup(x => x.AgregarItem(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .Returns(itemComanda.Object);

        if (comandaDto != null)
        {
            _mapperMock.Setup(x => x.Map<ComandaDto>(comanda.Object))
                .Returns(comandaDto);
        }
    }

    #endregion
}