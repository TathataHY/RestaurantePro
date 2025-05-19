namespace RestaurantePro.Domain.UnitTests.Core.Productos.Policies
{
    public class VisibilidadCategoriasPolicyTests
    {
        private readonly Mock<IProductoCategoriaRepository> _mockCategoriaRepository;
        private readonly Mock<IProductoRepository> _mockProductoRepository;
        private readonly Mock<IDateTimeService> _mockDateTimeService;
        private readonly VisibilidadCategoriasPolicy _policy;

        public VisibilidadCategoriasPolicyTests()
        {
            _mockCategoriaRepository = new Mock<IProductoCategoriaRepository>();
            _mockProductoRepository = new Mock<IProductoRepository>();
            _mockDateTimeService = new Mock<IDateTimeService>();
            _policy = new VisibilidadCategoriasPolicy(
                _mockCategoriaRepository.Object,
                _mockProductoRepository.Object,
                _mockDateTimeService.Object);
        }
        
        [Fact]
        public async Task ObtenerCategoriasVisiblesAsync_SinOcultarVacias_DebeRetornarTodasLasCategoriasActivas()
        {
            // Arrange
            var categoriasActivas = new List<ProductoCategoria>
            {
                ProductoCategoria.Crear("Bebidas", "Categoría de bebidas", 2),
                ProductoCategoria.Crear("Platos Principales", "Platos principales", 1),
                ProductoCategoria.Crear("Postres", "Categoría de postres", 3)
            };
            
            _mockCategoriaRepository.Setup(r => r.ObtenerActivasAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoriasActivas);

            // Act
            var result = await _policy.ObtenerCategoriasVisiblesAsync(ocultarCategoriasVacias: false);

            // Assert
            result.Should().HaveCount(3);
            result[0].Nombre.Should().Be("Platos Principales"); // Orden 1
            result[1].Nombre.Should().Be("Bebidas"); // Orden 2
            result[2].Nombre.Should().Be("Postres"); // Orden 3
            
            _mockCategoriaRepository.Verify(r => r.ObtenerActivasAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task ObtenerCategoriasVisiblesAsync_OcultarVacias_DebeRetornarSoloCategoriasConProductos()
        {
            // Arrange
            var categoriaId1 = Guid.NewGuid();
            var categoriaId2 = Guid.NewGuid();
            var categoriaId3 = Guid.NewGuid();
            
            var categoriasActivas = new List<ProductoCategoria>
            {
                CrearCategoriaConId("Bebidas", "Categoría de bebidas", 2, categoriaId1),
                CrearCategoriaConId("Platos Principales", "Platos principales", 1, categoriaId2),
                CrearCategoriaConId("Postres", "Categoría de postres", 3, categoriaId3)
            };
            
            // Configurar productos solo para las categorías 1 y 3
            var productosCategoria1 = new List<Producto>
            {
                Producto.Crear("Coca Cola", "Refresco", new PrecioProducto(25.0m), categoriaId1)
            };
            
            var productosCategoria3 = new List<Producto>
            {
                Producto.Crear("Pastel", "Pastel de chocolate", new PrecioProducto(50.0m), categoriaId3)
            };
            
            _mockCategoriaRepository.Setup(r => r.ObtenerActivasAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoriasActivas);
                
            _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId1, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosCategoria1);
                
            _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId2, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Producto>());
                
            _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId3, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosCategoria3);

            // Act
            var result = await _policy.ObtenerCategoriasVisiblesAsync(ocultarCategoriasVacias: true);

            // Assert
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(categoriaId1); // Bebidas (orden 2)
            result[1].Id.Should().Be(categoriaId3); // Postres (orden 3)
            
            _mockCategoriaRepository.Verify(r => r.ObtenerActivasAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()), Times.Exactly(3));
        }
        
        [Fact]
        public async Task EsCategoriaVisibleAsync_CategoriaActivaConProductos_DebeRetornarTrue()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoria = CrearCategoriaConId("Bebidas", "Categoría de bebidas", 1, categoriaId);
            
            var productosCategoria = new List<Producto>
            {
                Producto.Crear("Coca Cola", "Refresco", new PrecioProducto(25.0m), categoriaId)
            };
            
            _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
                
            _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosCategoria);

            // Act
            var result = await _policy.EsCategoriaVisibleAsync(categoriaId);

            // Assert
            result.Should().BeTrue();
            
            _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task EsCategoriaVisibleAsync_CategoriaActivaSinProductos_DebeRetornarFalse()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoria = CrearCategoriaConId("Bebidas", "Categoría de bebidas", 1, categoriaId);
            
            _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
                
            _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Producto>());

            // Act
            var result = await _policy.EsCategoriaVisibleAsync(categoriaId);

            // Assert
            result.Should().BeFalse();
            
            _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task EsCategoriaVisibleAsync_CategoriaInactiva_DebeRetornarFalse()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoria = CrearCategoriaConId("Bebidas", "Categoría de bebidas", 1, categoriaId);
            categoria.Desactivar();
            
            _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);

            // Act
            var result = await _policy.EsCategoriaVisibleAsync(categoriaId);

            // Assert
            result.Should().BeFalse();
            
            _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task EsCategoriaVisibleAsync_CategoriaNoExiste_DebeRetornarFalse()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            
            _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductoCategoria)null);

            // Act
            var result = await _policy.EsCategoriaVisibleAsync(categoriaId);

            // Assert
            result.Should().BeFalse();
            
            _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task EsCategoriaVisibleAsync_SinOcultarVacias_DebeRetornarTrueParaCategoriaActiva()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoria = CrearCategoriaConId("Bebidas", "Categoría de bebidas", 1, categoriaId);
            
            _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);

            // Act
            var result = await _policy.EsCategoriaVisibleAsync(categoriaId, ocultarCategoriasVacias: false);

            // Assert
            result.Should().BeTrue();
            
            _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        #region Helpers
        
        private ProductoCategoria CrearCategoriaConId(string nombre, string descripcion, int orden, Guid id)
        {
            var categoria = ProductoCategoria.Crear(nombre, descripcion, orden);
            
            // Establecer el ID usando reflexión ya que el ID es generado automáticamente
            typeof(ProductoCategoria).GetProperty("Id").SetValue(categoria, id);
            
            return categoria;
        }
        
        #endregion
    }
} 