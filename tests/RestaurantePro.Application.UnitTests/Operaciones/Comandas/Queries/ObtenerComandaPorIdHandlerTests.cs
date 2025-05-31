namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Queries;

public class ObtenerComandaPorIdHandlerTests
{
    private readonly Mock<IComandaRepository> _mockComandaRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerComandaPorIdHandler>> _mockLogger;
    private readonly ObtenerComandaPorIdHandler _handler;

    public ObtenerComandaPorIdHandlerTests()
    {
        _mockComandaRepository = new Mock<IComandaRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerComandaPorIdHandler>>();
        
        _handler = new ObtenerComandaPorIdHandler(
            _mockComandaRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ComandaNoExiste_DeberiaRetornarError()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var query = new ObtenerComandaPorIdQuery(comandaId, true);

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comanda?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("No se encontró una comanda");
        result.Error.Should().Contain(comandaId.ToString());

        _mockMapper.Verify(m => m.Map<ComandaDto>(It.IsAny<Comanda>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComandaExisteSinItems_DeberiaRetornarExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        
        var query = new ObtenerComandaPorIdQuery(comandaId, false);
        
        var comanda = Comanda.Crear(usuarioId, null, mesaId, "Comanda de prueba");
        
        var comandaDto = new ComandaDto
        {
            Id = comandaId,
            UsuarioId = usuarioId,
            MesaId = mesaId,
            Estado = EstadoComanda.Creada,
            Observaciones = "Comanda de prueba",
            Total = 0m,
            Items = new List<ItemComandaDto>()
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockMapper.Setup(m => m.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(comandaId);
        result.Value.Estado.Should().Be(EstadoComanda.Creada);
        result.Value.Items.Should().BeEmpty();

        _mockComandaRepository.Verify(r => r.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<ComandaDto>(comanda), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaExisteConItems_DeberiaRetornarExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        
        var query = new ObtenerComandaPorIdQuery(comandaId, true);
        
        var comanda = Comanda.Crear(usuarioId, null, mesaId, "Comanda con items");
        comanda.AgregarItem(productoId, "Hamburguesa Clásica", 2, 18000m, "Sin cebolla");
        
        var comandaDto = new ComandaDto
        {
            Id = comandaId,
            UsuarioId = usuarioId,
            MesaId = mesaId,
            Estado = EstadoComanda.Creada,
            Observaciones = "Comanda con items",
            Total = 36000m,
            Items = new List<ItemComandaDto>
            {
                new ItemComandaDto
                {
                    ProductoId = productoId,
                    NombreProducto = "Hamburguesa Clásica",
                    Cantidad = 2,
                    PrecioUnitario = 18000m,
                    Observaciones = "Sin cebolla"
                }
            }
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockMapper.Setup(m => m.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(comandaId);
        result.Value.Estado.Should().Be(EstadoComanda.Creada);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().NombreProducto.Should().Be("Hamburguesa Clásica");
        result.Value.Total.Should().Be(36000m);

        _mockComandaRepository.Verify(r => r.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()), Times.Once);
        _mockMapper.Verify(m => m.Map<ComandaDto>(comanda), Times.Once);
    }

    [Fact]
    public async Task Handle_ComandaEnEstadoEnProceso_DeberiaRetornarExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        
        var query = new ObtenerComandaPorIdQuery(comandaId, true);
        
        var comanda = Comanda.Crear(usuarioId, null, mesaId, "Comanda en proceso");
        comanda.AgregarItem(Guid.NewGuid(), "Pizza Margherita", 1, 25000m);
        comanda.MarcarEnPreparacion();
        
        var comandaDto = new ComandaDto
        {
            Id = comandaId,
            UsuarioId = usuarioId,
            MesaId = mesaId,
            Estado = EstadoComanda.EnProceso,
            Observaciones = "Comanda en proceso",
            Total = 25000m
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockMapper.Setup(m => m.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Estado.Should().Be(EstadoComanda.EnProceso);
        result.Value.Total.Should().Be(25000m);
    }

    [Fact]
    public async Task Handle_ComandaFinalizada_DeberiaRetornarExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        
        var query = new ObtenerComandaPorIdQuery(comandaId, true);
        
        var comanda = Comanda.Crear(usuarioId, clienteId, mesaId, "Comanda finalizada");
        comanda.AgregarItem(Guid.NewGuid(), "Ensalada César", 1, 12000m);
        comanda.MarcarEnPreparacion();
        comanda.MarcarLista();
        comanda.MarcarEntregada();
        comanda.MarcarPagada();
        
        var comandaDto = new ComandaDto
        {
            Id = comandaId,
            UsuarioId = usuarioId,
            ClienteId = clienteId,
            MesaId = mesaId,
            Estado = EstadoComanda.Finalizada,
            Observaciones = "Comanda finalizada",
            Total = 12000m
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockMapper.Setup(m => m.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Estado.Should().Be(EstadoComanda.Finalizada);
        result.Value.ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public async Task Handle_ComandaCancelada_DeberiaRetornarExitosamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        
        var query = new ObtenerComandaPorIdQuery(comandaId, false);
        
        var comanda = Comanda.Crear(usuarioId, null, mesaId, "Comanda cancelada");
        comanda.Cancelar("Cliente canceló el pedido");
        
        var comandaDto = new ComandaDto
        {
            Id = comandaId,
            UsuarioId = usuarioId,
            MesaId = mesaId,
            Estado = EstadoComanda.Cancelada,
            Observaciones = "Comanda cancelada",
            Total = 0m
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockMapper.Setup(m => m.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Estado.Should().Be(EstadoComanda.Cancelada);
    }

    [Fact]
    public async Task Handle_QueryConIncluirItemsFalse_DeberiaUsarParametroCorrectamente()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        var query = new ObtenerComandaPorIdQuery
        {
            ComandaId = comandaId,
            IncluirItems = false
        };
        
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Test query");
        
        var comandaDto = new ComandaDto
        {
            Id = comandaId,
            UsuarioId = usuarioId,
            Estado = EstadoComanda.Creada
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockMapper.Setup(m => m.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verificar que se llamó con IncluirItems = false
        _mockComandaRepository.Verify(r => r.ObtenerPorIdAsync(comandaId, false, It.IsAny<CancellationToken>()), Times.Once);
        _mockComandaRepository.Verify(r => r.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ErrorEnRepositorio_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var query = new ObtenerComandaPorIdQuery(comandaId, true);

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conexión a base de datos"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al buscar la comanda");

        _mockMapper.Verify(m => m.Map<ComandaDto>(It.IsAny<Comanda>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ErrorEnMapper_DeberiaRetornarErrorInterno()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var query = new ObtenerComandaPorIdQuery(comandaId, true);
        
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Test mapper error");

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockMapper.Setup(m => m.Map<ComandaDto>(comanda))
            .Throws(new Exception("Error de mapeo"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error interno del servidor al buscar la comanda");
    }

    [Fact]
    public async Task Handle_QueryConConstructorParametros_DeberiaFuncionar()
    {
        // Arrange
        var comandaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        
        // Usar constructor con parámetros
        var query = new ObtenerComandaPorIdQuery(comandaId, true);
        
        var comanda = Comanda.Crear(usuarioId, null, Guid.NewGuid(), "Test constructor");
        
        var comandaDto = new ComandaDto
        {
            Id = comandaId,
            UsuarioId = usuarioId,
            Estado = EstadoComanda.Creada
        };

        _mockComandaRepository.Setup(r => r.ObtenerPorIdAsync(comandaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comanda);

        _mockMapper.Setup(m => m.Map<ComandaDto>(comanda))
            .Returns(comandaDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        // Verificar que los parámetros del constructor se usaron correctamente
        query.ComandaId.Should().Be(comandaId);
        query.IncluirItems.Should().BeTrue();
    }
} 