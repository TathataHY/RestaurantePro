namespace RestaurantePro.Application.UnitTests.Core.Productos.Queries;

public class ObtenerProductosPorCategoriaHandlerTests
{
    private readonly Mock<IProductoRepository> _mockProductoRepository;
    private readonly Mock<IProductoCategoriaRepository> _mockCategoriaRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerProductosPorCategoriaHandler>> _mockLogger;
    private readonly Mock<INotificationManager> _mockNotificationManager;
    private readonly Mock<ILogger<ProductoBuilder>> _mockBuilderLogger;
    private readonly ObtenerProductosPorCategoriaHandler _handler;

    public ObtenerProductosPorCategoriaHandlerTests()
    {
        _mockProductoRepository = new Mock<IProductoRepository>();
        _mockCategoriaRepository = new Mock<IProductoCategoriaRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerProductosPorCategoriaHandler>>();
        _mockNotificationManager = new Mock<INotificationManager>();
        _mockBuilderLogger = new Mock<ILogger<ProductoBuilder>>();
        _handler = new ObtenerProductosPorCategoriaHandler(
            _mockProductoRepository.Object,
            _mockCategoriaRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_CategoriaExistente_DeberiaRetornarProductos()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var query = new ObtenerProductosPorCategoriaQuery(categoriaId, soloActivos: true);

        var categoria = CrearCategoriaTest(categoriaId, "Pizzas");
        var productos = new List<Producto>
        {
            CrearProductoTest("Pizza Margherita", 1), // Popularidad: 1
            CrearProductoTest("Pizza Pepperoni", 3),  // Popularidad: 3
            CrearProductoTest("Pizza Hawaiana", 2)    // Popularidad: 2
        };

        var productosDto = new List<ProductoDto>
        {
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Pizza Margherita" },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Pizza Pepperoni" },
            new ProductoDto { Id = Guid.NewGuid(), Nombre = "Pizza Hawaiana" }
        };

        _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);

        _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);

        // Verificar que se llamaron los repositorios correctos
        _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
        _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConOrdenamientoPorPopularidad_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var query = new ObtenerProductosPorCategoriaQuery(categoriaId, soloActivos: true, ordenarPorPopularidad: true);

        var categoria = CrearCategoriaTest(categoriaId, "Pizzas");
        var productos = new List<Producto>
        {
            CrearProductoTest("Pizza A", 5), // Popularidad: 5
            CrearProductoTest("Pizza B", 10), // Popularidad: 10 (más popular)
            CrearProductoTest("Pizza C", 3)   // Popularidad: 3
        };

        var productosDto = new List<ProductoDto>
        {
            new ProductoDto { Nombre = "Pizza B" }, // Debería ser primero
            new ProductoDto { Nombre = "Pizza A" },
            new ProductoDto { Nombre = "Pizza C" }
        };

        _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);

        _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        // Verificar que el mapper recibió productos ordenados por popularidad (descendente)
        _mockMapper.Verify(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SinOrdenamientoPorPopularidad_DeberiaOrdenarPorNombre()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var query = new ObtenerProductosPorCategoriaQuery(categoriaId, soloActivos: true, ordenarPorPopularidad: false);

        var categoria = CrearCategoriaTest(categoriaId, "Pizzas");
        var productos = new List<Producto>
        {
            CrearProductoTest("Pizza Z", 10),
            CrearProductoTest("Pizza A", 5),
            CrearProductoTest("Pizza M", 3)
        };

        var productosDto = new List<ProductoDto>
        {
            new ProductoDto { Nombre = "Pizza A" }, // Debería ser primero alfabéticamente
            new ProductoDto { Nombre = "Pizza M" },
            new ProductoDto { Nombre = "Pizza Z" }
        };

        _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);

        _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        // Verificar que el mapper recibió productos ordenados por nombre
        _mockMapper.Verify(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoriaNoExistente_DeberiaRetornarError()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var query = new ObtenerProductosPorCategoriaQuery(categoriaId);

        _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductoCategoria?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain($"Categoría con ID {categoriaId} no encontrada");

        // Verificar que NO se llamó al repositorio de productos
        _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SinProductosEnCategoria_DeberiaRetornarListaVacia()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var query = new ObtenerProductosPorCategoriaQuery(categoriaId);

        var categoria = CrearCategoriaTest(categoriaId, "Categoría Vacía");
        var productos = new List<Producto>();
        var productosDto = new List<ProductoDto>();

        _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);

        _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorioCategoria_DeberiaRetornarError()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var query = new ObtenerProductosPorCategoriaQuery(categoriaId);
        var excepcionMensaje = "Error de base de datos en categorías";

        _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(excepcionMensaje));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al obtener productos");
        result.Error.Should().Contain(excepcionMensaje);
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorioProductos_DeberiaRetornarError()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var query = new ObtenerProductosPorCategoriaQuery(categoriaId);
        var excepcionMensaje = "Error de base de datos en productos";

        var categoria = CrearCategoriaTest(categoriaId, "Pizzas");

        _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);

        _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(excepcionMensaje));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al obtener productos");
        result.Error.Should().Contain(excepcionMensaje);
    }

    private Producto CrearProductoTest(string nombre, int popularidad)
    {
        // Setup mocks básicos
        var mockNotificationManager = new Mock<INotificationManager>();
        var mockBuilderLogger = new Mock<ILogger<ProductoBuilder>>();
        
        mockNotificationManager.Setup(x => x.HasErrors).Returns(false);

        var builder = new ProductoBuilder(mockNotificationManager.Object, mockBuilderLogger.Object);
        
        var resultado = builder
            .ConNombre(nombre)
            .ConDescripcion($"Descripción de {nombre}")
            .ConPrecio(100m)
            .EnCategoria("Categoría Test")
            .ConPopularidadInicial(popularidad)
            .Construir();

        return resultado.Value;
    }

    private ProductoCategoria CrearCategoriaTest(Guid id, string nombre)
    {
        // Crear usando reflection ya que no sabemos el constructor exacto
        var categoria = Activator.CreateInstance(typeof(ProductoCategoria), true) as ProductoCategoria;
        
        // Usar reflection para set valores si es necesario
        var idProperty = typeof(ProductoCategoria).GetProperty("Id");
        idProperty?.SetValue(categoria, id);
        
        var nombreProperty = typeof(ProductoCategoria).GetProperty("Nombre");
        nombreProperty?.SetValue(categoria, nombre);

        return categoria!;
    }
} 