namespace RestaurantePro.Application.UnitTests.Inventario.Ingredientes.Commands;

public class CrearIngredienteHandlerTests
{
    private readonly Mock<IIngredienteRepository> _mockIngredienteRepository;
    private readonly Mock<IInventarioServiceFacade> _mockInventarioService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CrearIngredienteHandler>> _mockLogger;
    private readonly CrearIngredienteHandler _handler;

    public CrearIngredienteHandlerTests()
    {
        _mockIngredienteRepository = new Mock<IIngredienteRepository>();
        _mockInventarioService = new Mock<IInventarioServiceFacade>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CrearIngredienteHandler>>();
        
        _handler = new CrearIngredienteHandler(
            _mockIngredienteRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockInventarioService.Object);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_DeberiaCrearIngredienteCorrectamente()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Tomate",
            Codigo = "TOM-001",
            Descripcion = "Tomates frescos",
            UnidadMedida = "Kilogramo",
            StockInicial = 10m,
            StockMinimo = 5m,
            Rotacion = "Media",
            Temporada = "TodoElAño",
            CostoInicial = 1500m,
            UsuarioId = Guid.NewGuid()
        };

        var ingredienteCreado = CrearIngredienteMock("Tomate", 10m, 5m);
        var ingredienteDto = CrearIngredienteDtoMock("Tomate");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>());

        _mockInventarioService.Setup(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ingredienteCreado));

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Nombre.Should().Be("Tomate");

        _mockInventarioService.Verify(s => s.RegistrarIngredienteAvanzadoAsync(
            "Tomate", "Tomates frescos", UnidadMedida.Kilogramo, 5m, 10m, 
            "TOM-001", It.IsAny<Guid?>(), RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño, 1500m,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConNombreDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Tomate",
            Codigo = "TOM-001",
            UnidadMedida = "Kilogramo",
            StockInicial = 10m,
            StockMinimo = 5m
        };

        var ingredienteExistente = CrearIngredienteMock("Tomate", 5m, 2m);

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync("Tomate", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente> { ingredienteExistente });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Ya existe un ingrediente registrado con el nombre Tomate");

        _mockInventarioService.Verify(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConRotacionInvalida_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Lechuga",
            Codigo = "LEC-001",
            UnidadMedida = "Kilogramo",
            Rotacion = "Invalida", // Rotación inválida
            StockInicial = 5m,
            StockMinimo = 2m
        };

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Rotación no válida: Invalida");

        _mockInventarioService.Verify(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConTemporadaInvalida_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Cebolla",
            Codigo = "CEB-001",
            UnidadMedida = "Kilogramo",
            Temporada = "Invalida", // Temporada inválida
            StockInicial = 8m,
            StockMinimo = 3m
        };

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Temporada no válida: Invalida");

        _mockInventarioService.Verify(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConErrorEnServicioInventario_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Zanahoria",
            Codigo = "ZAN-001",
            UnidadMedida = "Kilogramo",
            StockInicial = 12m,
            StockMinimo = 4m,
            UsuarioId = Guid.NewGuid()
        };

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>());

        _mockInventarioService.Setup(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Ingrediente>("Error al registrar ingrediente"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be("Error al registrar ingrediente");

        _mockMapper.Verify(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConExcepcionInesperada_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Pimiento",
            Codigo = "PIM-001",
            UnidadMedida = "Kilogramo",
            StockInicial = 6m,
            StockMinimo = 2m
        };

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de conexión a base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error al registrar el ingrediente");

        _mockInventarioService.Verify(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConValoresPorDefecto_DeberiaUsarConfiguracionPredeterminada()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Perejil",
            Codigo = "PER-001",
            UnidadMedida = "Gramo",
            StockInicial = 100m,
            StockMinimo = 20m
            // No especifica Rotacion ni Temporada (usará valores por defecto)
        };

        var ingredienteCreado = CrearIngredienteMock("Perejil", 100m, 20m);
        var ingredienteDto = CrearIngredienteDtoMock("Perejil");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>());

        _mockInventarioService.Setup(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ingredienteCreado));

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que se usan valores por defecto
        _mockInventarioService.Verify(s => s.RegistrarIngredienteAvanzadoAsync(
            "Perejil", "", UnidadMedida.Gramo, 20m, 100m, 
            "PER-001", null, RotacionIngrediente.Media, // Valor por defecto
            TemporadaIngrediente.TodoElAño, // Valor por defecto
            0m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConStockYCostoEspecificos_DeberiaCrearCorrectamente()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Aceite de Oliva",
            Codigo = "ACE-001",
            Descripcion = "Aceite de oliva extra virgen",
            UnidadMedida = "Litro",
            StockInicial = 25m,
            StockMinimo = 10m,
            Rotacion = "Baja",
            Temporada = "TodoElAño",
            CostoInicial = 8500m,
            UsuarioId = Guid.NewGuid()
        };

        var ingredienteCreado = CrearIngredienteMock("Aceite de Oliva", 25m, 10m);
        var ingredienteDto = CrearIngredienteDtoMock("Aceite de Oliva");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>());

        _mockInventarioService.Setup(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ingredienteCreado));

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Nombre.Should().Be("Aceite de Oliva");

        _mockInventarioService.Verify(s => s.RegistrarIngredienteAvanzadoAsync(
            "Aceite de Oliva", "Aceite de oliva extra virgen", UnidadMedida.Litro, 10m, 25m, 
            "ACE-001", It.IsAny<Guid?>(), RotacionIngrediente.Baja, TemporadaIngrediente.TodoElAño, 8500m,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConRotacionAlta_DeberiaCrearCorrectamente()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Pan",
            Codigo = "PAN-001",
            UnidadMedida = "Unidad",
            StockInicial = 50m,
            StockMinimo = 15m,
            Rotacion = "Alta", // Rotación alta para productos perecederos
            Temporada = "TodoElAño"
        };

        var ingredienteCreado = CrearIngredienteMock("Pan", 50m, 15m);
        var ingredienteDto = CrearIngredienteDtoMock("Pan");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>());

        _mockInventarioService.Setup(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ingredienteCreado));

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockInventarioService.Verify(s => s.RegistrarIngredienteAvanzadoAsync(
            "Pan", "", UnidadMedida.Unidad, 15m, 50m, 
            "PAN-001", null, RotacionIngrediente.Alta, TemporadaIngrediente.TodoElAño, 0m,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConIngredienteEstacional_DeberiaCrearCorrectamente()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Frutillas",
            Codigo = "FRU-001",
            UnidadMedida = "Kilogramo",
            StockInicial = 8m,
            StockMinimo = 3m,
            Rotacion = "Alta",
            Temporada = "Verano" // Ingrediente estacional
        };

        var ingredienteCreado = CrearIngredienteMock("Frutillas", 8m, 3m);
        var ingredienteDto = CrearIngredienteDtoMock("Frutillas");

        _mockIngredienteRepository.Setup(r => r.ObtenerPorNombreAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ingrediente>());

        _mockInventarioService.Setup(s => s.RegistrarIngredienteAvanzadoAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UnidadMedida>(), It.IsAny<decimal>(), 
            It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<RotacionIngrediente>(), 
            It.IsAny<TemporadaIngrediente>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ingredienteCreado));

        _mockMapper.Setup(m => m.Map<IngredienteDto>(It.IsAny<Ingrediente>()))
            .Returns(ingredienteDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockInventarioService.Verify(s => s.RegistrarIngredienteAvanzadoAsync(
            "Frutillas", "", UnidadMedida.Kilogramo, 3m, 8m, 
            "FRU-001", null, RotacionIngrediente.Alta, TemporadaIngrediente.Verano,
            0m, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Métodos de ayuda para crear mocks
    private static Ingrediente CrearIngredienteMock(string nombre, decimal stock, decimal stockMinimo)
    {
        return Ingrediente.Crear(
            Guid.NewGuid(),
            nombre,
            $"COD-{nombre.ToUpper()}",
            $"Descripción de {nombre}",
            UnidadMedida.Kilogramo,
            stockMinimo,
            stock,
            RotacionIngrediente.Media,
            TemporadaIngrediente.TodoElAño);
    }

    private static IngredienteDto CrearIngredienteDtoMock(string nombre)
    {
        return new IngredienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = $"Descripción de {nombre}",
            StockActual = 10m,
            StockMinimo = 5m,
            CostoUnitario = 1500m,
            Activo = true
        };
    }
} 