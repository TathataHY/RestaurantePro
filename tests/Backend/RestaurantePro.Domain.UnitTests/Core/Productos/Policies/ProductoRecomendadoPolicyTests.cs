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
            _dateTimeServiceMock.Setup(s => s.Now).Returns(DateTime.Now);
            
            _policy = new ProductoRecomendadoPolicy(
                _productoRepositoryMock.Object, 
                _comandaRepositoryMock.Object,
                _dateTimeServiceMock.Object);
        }
        
        [Fact]
        public async Task GenerarRecomendacionesPopulares_DebeRetornarProductosMasVendidos()
        {
            // Arrange
            var productoId1 = Guid.NewGuid();
            var productoId2 = Guid.NewGuid();
            var productoId3 = Guid.NewGuid();
            var productoId4 = Guid.NewGuid();
            
            var fechaActual = DateTime.Now;
            var fechaInicio = fechaActual.AddDays(-30);
            
            // Configurar fecha actual
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            // Crear comandas reales usando el factory method
            var comandasParaRepositorio = new List<Comanda>();
            
            // Crear una comanda con el producto 1
            var mesaId1 = Guid.NewGuid();
            var meseroId1 = Guid.NewGuid();
            var comanda1 = Comanda.Crear(meseroId1, null, mesaId1);
            comanda1.AgregarProducto(productoId1, 3, 100m);
            comandasParaRepositorio.Add(comanda1);
            
            // Crear una comanda con el producto 2
            var mesaId2 = Guid.NewGuid();
            var meseroId2 = Guid.NewGuid();
            var comanda2 = Comanda.Crear(meseroId2, null, mesaId2);
            comanda2.AgregarProducto(productoId2, 2, 100m);
            comandasParaRepositorio.Add(comanda2);
            
            // Crear una comanda con el producto 3
            var mesaId3 = Guid.NewGuid();
            var meseroId3 = Guid.NewGuid();
            var comanda3 = Comanda.Crear(meseroId3, null, mesaId3);
            comanda3.AgregarProducto(productoId3, 1, 100m);
            comandasParaRepositorio.Add(comanda3);
            
            // Crear una comanda con el producto 4
            var mesaId4 = Guid.NewGuid();
            var meseroId4 = Guid.NewGuid();
            var comanda4 = Comanda.Crear(meseroId4, null, mesaId4);
            comanda4.AgregarProducto(productoId4, 1, 100m);
            comandasParaRepositorio.Add(comanda4);
            
            // Configurar productos
            var productos = ConfigurarProductos(productoId1, productoId2, productoId3, productoId4);
            
            // Verificar que los productos estén activos
            foreach (var producto in productos)
            {
                producto.Should().NotBeNull();
                producto.EstaActivo.Should().BeTrue();
                producto.Precio.Should().NotBeNull();
                producto.Precio.Valor.Should().BeGreaterThan(0);
                producto.CategoriaNombre.Should().NotBeNullOrEmpty();
                producto.CategoriaId.Should().NotBe(Guid.Empty);
            }
            
            // Configurar repositorio para devolver comandas
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorRangoFechasAsync(fechaInicio, fechaActual, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comandasParaRepositorio);
            
            // Act
            var resultado = await _policy.GenerarRecomendacionesPopulares(3);
            
            // Assert
            resultado.Should().NotBeNull();
            
            // Depuración: verificar que las comandas fueron proporcionadas correctamente
            _comandaRepositoryMock.Verify(r => r.ObtenerPorRangoFechasAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            
            resultado.Criterios.Should().Contain("popularidad");
            resultado.ProductosRecomendados.Should().HaveCount(3);
            resultado.ProductosRecomendados[0].ProductoId.Should().Be(productoId1);
            resultado.ProductosRecomendados[1].ProductoId.Should().Be(productoId2);
            resultado.ProductosRecomendados[2].ProductoId.Should().Be(productoId3);
        }
        
        [Fact]
        public async Task GenerarRecomendacionesParaCliente_DebeRetornarProductosBasadosEnHistorial()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var productoId1 = Guid.NewGuid();
            var productoId2 = Guid.NewGuid();
            
            // Crear comandas reales usando el factory method
            var comandasParaRepositorio = new List<Comanda>();
            
            // Crear una comanda con el producto 1
            var mesaId1 = Guid.NewGuid();
            var meseroId1 = Guid.NewGuid();
            var comanda1 = Comanda.Crear(meseroId1, clienteId, mesaId1);
            comanda1.AgregarProducto(productoId1, 2, 100m);
            comandasParaRepositorio.Add(comanda1);
            
            // Crear una comanda con el producto 2
            var mesaId2 = Guid.NewGuid();
            var meseroId2 = Guid.NewGuid();
            var comanda2 = Comanda.Crear(meseroId2, clienteId, mesaId2);
            comanda2.AgregarProducto(productoId2, 1, 100m);
            comandasParaRepositorio.Add(comanda2);
            
            // Configurar productos
            var productos = ConfigurarProductos(productoId1, productoId2, Guid.NewGuid(), Guid.NewGuid());
            
            // Verificar que los productos estén activos
            foreach (var producto in productos)
            {
                producto.Should().NotBeNull();
                producto.EstaActivo.Should().BeTrue();
                producto.Precio.Should().NotBeNull();
                producto.Precio.Valor.Should().BeGreaterThan(0);
                producto.CategoriaNombre.Should().NotBeNullOrEmpty();
                producto.CategoriaId.Should().NotBe(Guid.Empty);
            }
            
            // Configurar repositorio para devolver comandas del cliente
            _comandaRepositoryMock
                .Setup(r => r.ObtenerPorClienteAsync(clienteId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(comandasParaRepositorio);
            
            // Act
            var resultado = await _policy.GenerarRecomendacionesParaCliente(clienteId, 2);
            
            // Assert
            resultado.Should().NotBeNull();
            
            // Depuración: verificar que las comandas fueron proporcionadas correctamente
            _comandaRepositoryMock.Verify(r => r.ObtenerPorClienteAsync(
                clienteId, true, It.IsAny<CancellationToken>()), Times.Once);
                
            resultado.Criterios.Should().Contain("historial");
            resultado.ProductosRecomendados.Should().HaveCount(2);
        }
        
        private List<Producto> ConfigurarProductos(params Guid[] productosIds)
        {
            var productos = new List<Producto>();
            
            foreach (var id in productosIds)
            {
                // Usar el factory method para crear el producto
                var producto = Producto.Crear(
                    $"Producto {id.ToString().Substring(0, 8)}", 
                    "Descripción para pruebas", 
                    new PrecioProducto(100m), 
                    Guid.NewGuid(), 
                    "Categoría Test"
                );
                
                // Usar reflexión para reemplazar el ID generado automáticamente
                var idProperty = typeof(EntityBase).GetProperty("Id", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                if (idProperty != null)
                {
                    // Usamos un método no público para establecer el ID
                    var setMethod = idProperty.GetSetMethod(true);
                    setMethod?.Invoke(producto, new object[] { id });
                }
                
                productos.Add(producto);
            }
            
            // Configurar para que devuelva todos los productos
            _productoRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productos);
                 
            // Configurar para que devuelva productos por ID
            foreach (var producto in productos)
            {
                _productoRepositoryMock
                    .Setup(r => r.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(producto);
            }
            
            return productos;
        }
        
        private Producto CrearProductoMock(Guid id, string nombre, decimal precio, bool activo, string categoria = "Test", Guid? categoriaId = null)
        {
            // Usar factory method para crear el producto
            var categId = categoriaId ?? Guid.NewGuid();
            var producto = Producto.Crear(nombre, "Descripción de prueba", new PrecioProducto(precio), categId, categoria);
            
            // Usar reflexión para establecer el ID
            var idProperty = typeof(EntityBase).GetProperty("Id", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
            if (idProperty != null)
            {
                var setMethod = idProperty.GetSetMethod(true);
                setMethod?.Invoke(producto, new object[] { id });
            }
            
            // Si debe estar inactivo, desactivarlo
            if (!activo)
            {
                producto.Desactivar();
            }
            
            return producto;
        }
    }
} 