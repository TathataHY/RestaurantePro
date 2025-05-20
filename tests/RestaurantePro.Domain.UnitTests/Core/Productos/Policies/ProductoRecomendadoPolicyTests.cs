namespace RestaurantePro.Domain.UnitTests.Core.Productos.Policies
{
    public class ProductoRecomendadoPolicyTests
    {
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly ProductoRecomendadoPolicy _policy;
        
        public ProductoRecomendadoPolicyTests()
        {
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            
            // Configurar fecha actual para pruebas
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2024, 4, 15));
            
            _policy = new ProductoRecomendadoPolicy(
                _productoRepositoryMock.Object,
                _comandaRepositoryMock.Object,
                _dateTimeServiceMock.Object
            );
        }
        
        [Fact]
        public async Task GenerarRecomendacionesPopulares_DebeRetornarProductosMasVendidos()
        {
            // Arrange
            var fechaActual = _dateTimeServiceMock.Object.Now;
            var fechaInicio = fechaActual.AddDays(-30);
            
            var productos = new List<Producto>
            {
                CrearProductoMock(Guid.NewGuid(), "Producto 1", 100, true),
                CrearProductoMock(Guid.NewGuid(), "Producto 2", 150, true),
                CrearProductoMock(Guid.NewGuid(), "Producto 3", 120, true),
                CrearProductoMock(Guid.NewGuid(), "Producto 4", 200, false), // inactivo, no debe aparecer
                CrearProductoMock(Guid.NewGuid(), "Producto 5", 180, true)
            };
            
            var comandas = new List<Comanda>
            {
                CrearComandaMock(productos[0].Id, 10), // 10 veces Producto 1
                CrearComandaMock(productos[1].Id, 5),  // 5 veces Producto 2
                CrearComandaMock(productos[2].Id, 8),  // 8 veces Producto 3
                CrearComandaMock(productos[4].Id, 3)   // 3 veces Producto 5
            };
            
            _productoRepositoryMock.Setup(repo => repo.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productos.Where(p => p.EstaActivo).ToList());
                
            _comandaRepositoryMock.Setup(repo => repo.ObtenerPorRangoFechasAsync(fechaInicio, fechaActual, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comandas);
            
            // Act
            var resultado = await _policy.GenerarRecomendacionesPopulares(3, 30);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.ProductosRecomendados.Should().HaveCount(3);
            resultado.ProductosRecomendados[0].ProductoId.Should().Be(productos[0].Id); // Producto 1 (10 veces)
            resultado.ProductosRecomendados[1].ProductoId.Should().Be(productos[2].Id); // Producto 3 (8 veces)
            resultado.ProductosRecomendados[2].ProductoId.Should().Be(productos[1].Id); // Producto 2 (5 veces)
            resultado.Criterios.Should().Contain("popularidad");
        }
        
        [Fact]
        public async Task GenerarRecomendacionesParaCliente_DebeRetornarProductosBasadosEnHistorial()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            var productos = new List<Producto>
            {
                CrearProductoMock(Guid.NewGuid(), "Producto 1", 100, true, "Categoría 1"),
                CrearProductoMock(Guid.NewGuid(), "Producto 2", 150, true, "Categoría 1"),
                CrearProductoMock(Guid.NewGuid(), "Producto 3", 120, true, "Categoría 2"),
                CrearProductoMock(Guid.NewGuid(), "Producto 4", 200, true, "Categoría 2"),
                CrearProductoMock(Guid.NewGuid(), "Producto 5", 180, true, "Categoría 3")
            };
            
            var comandasCliente = new List<Comanda>
            {
                CrearComandaMockParaCliente(clienteId, productos[0].Id, 3),  // Compró 3 veces Producto 1
                CrearComandaMockParaCliente(clienteId, productos[2].Id, 2)   // Compró 2 veces Producto 3
            };
            
            _productoRepositoryMock.Setup(repo => repo.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productos);
                
            _comandaRepositoryMock.Setup(repo => repo.ObtenerPorClienteAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comandasCliente);
            
            // Act
            var resultado = await _policy.GenerarRecomendacionesParaCliente(clienteId, 4);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.ProductosRecomendados.Should().HaveCount(4);
            
            // Verificar que hay productos de las mismas categorías que el cliente ha comprado antes
            var categorias = resultado.ProductosRecomendados.Select(p => p.CategoriaNombre).Distinct();
            categorias.Should().Contain("Categoría 1");
            categorias.Should().Contain("Categoría 2");
            
            resultado.Criterios.Should().Contain("historial personal");
        }
        
        [Fact]
        public async Task GenerarRecomendacionesComplementarias_DebeRecomendarProductosComplementarios()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var categoriaComida = Guid.NewGuid();
            var categoriaBebida = Guid.NewGuid();
            var categoriaPostre = Guid.NewGuid();
            
            var comidaEnComanda = CrearProductoMock(Guid.NewGuid(), "Hamburguesa", 150, true, "Comidas", categoriaComida);
            
            var productos = new List<Producto>
            {
                comidaEnComanda,
                CrearProductoMock(Guid.NewGuid(), "Refresco", 50, true, "Bebidas", categoriaBebida),
                CrearProductoMock(Guid.NewGuid(), "Agua", 30, true, "Bebidas", categoriaBebida),
                CrearProductoMock(Guid.NewGuid(), "Pastel", 80, true, "Postres", categoriaPostre),
                CrearProductoMock(Guid.NewGuid(), "Helado", 60, true, "Postres", categoriaPostre)
            };
            
            // Crear una comanda real usando el factory method
            var comanda = Comanda.Crear(mesaId, meseroId);
            
            // Usar reflexión para establecer el ID
            typeof(EntityBase).GetProperty("Id").SetValue(comanda, comandaId);
            
            // Agregar un ítem de comida a la comanda
            comanda.AgregarProducto(comidaEnComanda.Id, 1, comidaEnComanda.Precio.Valor);
            
            _productoRepositoryMock.Setup(repo => repo.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productos);
                
            _comandaRepositoryMock.Setup(repo => repo.ObtenerPorIdAsync(comandaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comanda);
                
            _productoRepositoryMock.Setup(repo => repo.ObtenerPorIdAsync(comidaEnComanda.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comidaEnComanda);
            
            // Act
            var resultado = await _policy.GenerarRecomendacionesComplementarias(comandaId, 2);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.ProductosRecomendados.Should().HaveCount(2);
            
            // Verificar que se recomiendan productos de categorías distintas a la que ya está en la comanda
            resultado.ProductosRecomendados.All(p => p.CategoriaId != categoriaComida).Should().BeTrue();
            
            resultado.Criterios.Should().Contain("complementario");
        }
        
        #region Métodos de ayuda para crear mocks
        
        private Producto CrearProductoMock(Guid id, string nombre, decimal precio, bool activo, string categoria = "Test", Guid? categoriaId = null)
        {
            var producto = Producto.Crear(
                nombre,
                $"Descripción de {nombre}",
                new PrecioProducto(precio),
                categoriaId ?? Guid.NewGuid(),
                categoria
            );
            
            // Usar reflexión para establecer Id
            typeof(Producto).GetProperty("Id").SetValue(producto, id);
            
            if (!activo)
            {
                producto.Desactivar();
            }
            
            return producto;
        }
        
        private Comanda CrearComandaMock(Guid productoId, int cantidad)
        {
            var comandaMock = new Mock<Comanda>();
            var items = new List<ItemComanda>();
            
            for (int i = 0; i < cantidad; i++)
            {
                items.Add(new ItemComanda(Guid.NewGuid(), productoId, 1, 100m));
            }
            
            comandaMock.Setup(c => c.Items).Returns(items);
            return comandaMock.Object;
        }
        
        private Comanda CrearComandaMockParaCliente(Guid clienteId, Guid productoId, int cantidad)
        {
            var comandaMock = new Mock<Comanda>();
            var items = new List<ItemComanda>();
            
            for (int i = 0; i < cantidad; i++)
            {
                items.Add(new ItemComanda(Guid.NewGuid(), productoId, 1, 100m));
            }
            
            comandaMock.Setup(c => c.ClienteId).Returns(clienteId);
            comandaMock.Setup(c => c.Items).Returns(items);
            return comandaMock.Object;
        }
        
        #endregion
    }
} 