namespace RestaurantePro.Domain.UnitTests.Core.Productos.Services
{
    public class ProductoCategoriaServiceTests
    {
        private readonly Mock<IProductoRepository> _mockProductoRepository;
        private readonly Mock<IProductoCategoriaRepository> _mockCategoriaRepository;
        private readonly IProductoCategoriaService _service;

        public ProductoCategoriaServiceTests()
        {
            _mockProductoRepository = new Mock<IProductoRepository>();
            _mockCategoriaRepository = new Mock<IProductoCategoriaRepository>();
            _service = new ProductoCategoriaService(_mockProductoRepository.Object, _mockCategoriaRepository.Object);
        }

        [Fact]
        public async Task ObtenerProductosPorCategoriaAsync_CategoriaExiste_DebeRetornarProductos()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoria = ProductoCategoria.Crear("Bebidas", "Categoría de bebidas", 1);
            var productosMock = new List<Producto>
            {
                Producto.Crear("Coca Cola", "Refresco de cola", new PrecioProducto(25.0m), categoriaId, "Bebidas"),
                Producto.Crear("Agua Mineral", "Agua mineral", new PrecioProducto(15.0m), categoriaId, "Bebidas")
            };
            
            _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
            
            _mockProductoRepository.Setup(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosMock);

            // Act
            var result = await _service.ObtenerProductosPorCategoriaAsync(categoriaId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(productosMock);
            
            _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(categoriaId, true, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ObtenerProductosPorCategoriaAsync_CategoriaNoExiste_DebeLanzarExcepcion()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            
            _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductoCategoria)null);

            // Act & Assert
            Func<Task> act = async () => await _service.ObtenerProductosPorCategoriaAsync(categoriaId);
            
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*La categoría con ID {categoriaId} no existe*");
            
            _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorCategoriaAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task ActualizarCategoriaProductosAsync_CategoriaExiste_DebeActualizarProductos()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoria = ProductoCategoria.Crear("Bebidas", "Categoría de bebidas", 1);
            var productosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            
            var producto1 = Producto.Crear("Producto 1", "Descripción 1", new PrecioProducto(10.0m), Guid.NewGuid(), "Otra Categoría");
            var producto2 = Producto.Crear("Producto 2", "Descripción 2", new PrecioProducto(20.0m), Guid.NewGuid(), "Otra Categoría");
            
            _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
            
            _mockProductoRepository.Setup(r => r.ObtenerPorIdAsync(productosIds[0], It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto1);
            
            _mockProductoRepository.Setup(r => r.ObtenerPorIdAsync(productosIds[1], It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto2);

            // Act
            var result = await _service.ActualizarCategoriaProductosAsync(productosIds, categoriaId);

            // Assert
            result.Should().Be(2); // 2 productos actualizados
            
            producto1.CategoriaId.Should().Be(categoriaId);
            producto1.CategoriaNombre.Should().Be("Bebidas");
            producto2.CategoriaId.Should().Be(categoriaId);
            producto2.CategoriaNombre.Should().Be("Bebidas");
            
            _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _mockProductoRepository.Verify(r => r.ActualizarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
        
        [Fact]
        public async Task ActualizarCategoriaProductosAsync_ProductoNoExiste_DebeOmitirProducto()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var categoria = ProductoCategoria.Crear("Bebidas", "Categoría de bebidas", 1);
            var productosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            
            var producto1 = Producto.Crear("Producto 1", "Descripción 1", new PrecioProducto(10.0m), Guid.NewGuid(), "Otra Categoría");
            
            _mockCategoriaRepository.Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
            
            _mockProductoRepository.Setup(r => r.ObtenerPorIdAsync(productosIds[0], It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto1);
            
            _mockProductoRepository.Setup(r => r.ObtenerPorIdAsync(productosIds[1], It.IsAny<CancellationToken>()))
                .ReturnsAsync((Producto)null);

            // Act
            var result = await _service.ActualizarCategoriaProductosAsync(productosIds, categoriaId);

            // Assert
            result.Should().Be(1); // Solo 1 producto actualizado
            
            producto1.CategoriaId.Should().Be(categoriaId);
            producto1.CategoriaNombre.Should().Be("Bebidas");
            
            _mockCategoriaRepository.Verify(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductoRepository.Verify(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _mockProductoRepository.Verify(r => r.ActualizarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ReorganizarCategoriasAsync_ConCategoriasExistentes_DebeActualizarOrdenes()
        {
            // Arrange
            var categorias = new List<ProductoCategoria>
            {
                ProductoCategoria.Crear("Bebidas", "Categoría de bebidas", 1),
                ProductoCategoria.Crear("Platos Principales", "Platos principales", 2),
                ProductoCategoria.Crear("Postres", "Categoría de postres", 3)
            };
            
            var nuevosOrdenes = new Dictionary<Guid, int>
            {
                { categorias[0].Id, 3 }, // Cambiar orden de Bebidas a 3
                { categorias[1].Id, 1 }, // Cambiar orden de Platos Principales a 1
                { categorias[2].Id, 2 }  // Cambiar orden de Postres a 2
            };
            
            _mockCategoriaRepository.Setup(r => r.ObtenerTodasAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categorias);

            // Act
            var result = await _service.ReorganizarCategoriasAsync(nuevosOrdenes);

            // Assert
            result.Should().Be(3); // 3 categorías actualizadas
            
            categorias[0].Orden.Should().Be(3);
            categorias[1].Orden.Should().Be(1);
            categorias[2].Orden.Should().Be(2);
            
            _mockCategoriaRepository.Verify(r => r.ObtenerTodasAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockCategoriaRepository.Verify(r => r.ActualizarAsync(It.IsAny<ProductoCategoria>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }
        
        [Fact]
        public async Task ReorganizarCategoriasAsync_AlgunosOrdenesNoModificados_DebeActualizarSoloModificados()
        {
            // Arrange
            var categorias = new List<ProductoCategoria>
            {
                ProductoCategoria.Crear("Bebidas", "Categoría de bebidas", 1),
                ProductoCategoria.Crear("Platos Principales", "Platos principales", 2),
                ProductoCategoria.Crear("Postres", "Categoría de postres", 3)
            };
            
            var nuevosOrdenes = new Dictionary<Guid, int>
            {
                { categorias[0].Id, 1 }, // No cambia
                { categorias[1].Id, 3 }, // Cambiar orden de Platos Principales a 3
                { categorias[2].Id, 2 }  // Cambiar orden de Postres a 2
            };
            
            _mockCategoriaRepository.Setup(r => r.ObtenerTodasAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categorias);

            // Act
            var result = await _service.ReorganizarCategoriasAsync(nuevosOrdenes);

            // Assert
            result.Should().Be(2); // Solo 2 categorías actualizadas
            
            categorias[0].Orden.Should().Be(1); // Sin cambio
            categorias[1].Orden.Should().Be(3);
            categorias[2].Orden.Should().Be(2);
            
            _mockCategoriaRepository.Verify(r => r.ObtenerTodasAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockCategoriaRepository.Verify(r => r.ActualizarAsync(It.Is<ProductoCategoria>(c => c.Id == categorias[0].Id), It.IsAny<CancellationToken>()), Times.Never);
            _mockCategoriaRepository.Verify(r => r.ActualizarAsync(It.Is<ProductoCategoria>(c => c.Id == categorias[1].Id), It.IsAny<CancellationToken>()), Times.Once);
            _mockCategoriaRepository.Verify(r => r.ActualizarAsync(It.Is<ProductoCategoria>(c => c.Id == categorias[2].Id), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 