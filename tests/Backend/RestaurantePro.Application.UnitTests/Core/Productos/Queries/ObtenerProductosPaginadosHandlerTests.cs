namespace RestaurantePro.Application.UnitTests.Core.Productos.Queries;

public class ObtenerProductosPaginadosHandlerTests
{
    private readonly Mock<IProductoRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ObtenerProductosPaginadosHandler>> _mockLogger;
    private readonly Mock<INotificationManager> _mockNotificationManager;
    private readonly Mock<ILogger<ProductoBuilder>> _mockBuilderLogger;
    private readonly ObtenerProductosPaginadosHandler _handler;

    public ObtenerProductosPaginadosHandlerTests()
    {
        _mockRepository = new Mock<IProductoRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ObtenerProductosPaginadosHandler>>();
        _mockNotificationManager = new Mock<INotificationManager>();
        _mockBuilderLogger = new Mock<ILogger<ProductoBuilder>>();
        _handler = new ObtenerProductosPaginadosHandler(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ConsultaBásica_DeberiaRetornarProductosPaginados()
    {
        // Arrange
        var query = new ObtenerProductosPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SoloActivos = true,
            OrderBy = "Nombre",
            OrderDirection = "asc"
        };

        var productos = GenerarProductosTest(15);
        var productosDto = GenerarProductosDtoTest(10);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(15);
        result.Value.PageNumber.Should().Be(1);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ConFiltroDeTexto_DeberiaFiltrarProductos()
    {
        // Arrange
        var query = new ObtenerProductosPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            Filtro = "pizza",
            SoloActivos = true
        };

        var productos = new List<Producto>
        {
            CrearProductoTest("Pizza Margherita", "Deliciosa pizza con tomate"),
            CrearProductoTest("Hamburguesa", "Rica hamburguesa"),
            CrearProductoTest("Pizza Pepperoni", "Pizza con pepperoni")
        };

        var productosDto = GenerarProductosDtoTest(2);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.Is<List<Producto>>(list => list.Count == 2)))
            .Returns(productosDto.Take(2).ToList());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ConCategoriaEspecifica_DeberiaFiltrarPorCategoria()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var query = new ObtenerProductosPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            CategoriaId = categoriaId,
            SoloActivos = true
        };

        var productos = GenerarProductosTest(5);
        var productosDto = GenerarProductosDtoTest(5);

        _mockRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        
        // Verificar que se llamó al método correcto
        _mockRepository.Verify(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.ObtenerTodosAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OrdenamientoPorPrecio_DeberiaOrdenarCorrectamente()
    {
        // Arrange
        var query = new ObtenerProductosPaginadosQuery
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "Precio",
            OrderDirection = "desc"
        };

        var productos = new List<Producto>
        {
            CrearProductoTest("Producto A", "Descripción A", 100m),
            CrearProductoTest("Producto B", "Descripción B", 50m),
            CrearProductoTest("Producto C", "Descripción C", 200m)
        };

        var productosDto = GenerarProductosDtoTest(3);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        
        // Verificar que el mapper recibió los productos ordenados por precio descendente
        _mockMapper.Verify(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SegundaPagina_DeberiaRetornarElementosCorrectos()
    {
        // Arrange
        var query = new ObtenerProductosPaginadosQuery
        {
            PageNumber = 2,
            PageSize = 5,
            SoloActivos = true
        };

        var productos = GenerarProductosTest(12);
        var productosDto = GenerarProductosDtoTest(5);

        _mockRepository.Setup(r => r.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(12);
        result.Value.PageNumber.Should().Be(2);
        result.Value.TotalPages.Should().Be(3);
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExcepcionEnRepositorio_DeberiaRetornarError()
    {
        // Arrange
        var query = new ObtenerProductosPaginadosQuery();
        var excepcionMensaje = "Error de base de datos";

        _mockRepository.Setup(r => r.ObtenerTodosAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
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
    public async Task Handle_SinProductos_DeberiaRetornarListaVacia()
    {
        // Arrange
        var query = new ObtenerProductosPaginadosQuery();
        var productos = new List<Producto>();
        var productosDto = new List<ProductoDto>();

        _mockRepository.Setup(r => r.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        _mockMapper.Setup(m => m.Map<List<ProductoDto>>(It.IsAny<List<Producto>>()))
            .Returns(productosDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    private List<Producto> GenerarProductosTest(int cantidad)
    {
        var productos = new List<Producto>();
        for (int i = 1; i <= cantidad; i++)
        {
            productos.Add(CrearProductoTest($"Producto {i}", $"Descripción {i}", i * 10m));
        }
        return productos;
    }

    private List<ProductoDto> GenerarProductosDtoTest(int cantidad)
    {
        var productosDto = new List<ProductoDto>();
        for (int i = 1; i <= cantidad; i++)
        {
            productosDto.Add(new ProductoDto
            {
                Id = Guid.NewGuid(),
                Nombre = $"Producto {i}",
                Descripcion = $"Descripción {i}",
                Precio = i * 10m
            });
        }
        return productosDto;
    }

    private Producto CrearProductoTest(string nombre, string descripcion, decimal precio = 100m)
    {
        // Setup mocks básicos
        var mockNotificationManager = new Mock<INotificationManager>();
        var mockBuilderLogger = new Mock<ILogger<ProductoBuilder>>();
        
        mockNotificationManager.Setup(x => x.HasErrors).Returns(false);

        var builder = new ProductoBuilder(mockNotificationManager.Object, mockBuilderLogger.Object);
        
        var resultado = builder
            .ConNombre(nombre)
            .ConDescripcion(descripcion)
            .ConPrecio(precio)
            .EnCategoria("Categoría Test")
            .Construir();

        return resultado.Value;
    }
} 