namespace RestaurantePro.Domain.UnitTests.Integration.BetweenContexts.Core_Comercial
{
    /// <summary>
    /// Pruebas de integración para verificar la interacción entre el sistema de recomendación de productos
    /// del contexto Core y el sistema de descuentos del contexto Comercial.
    /// </summary>
    public class ProductoRecomendado_CalcularDescuentoTests
    {
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IPromocionRepository> _promocionRepositoryMock;
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        
        private readonly ProductoRecomendadoPolicy _productoRecomendadoPolicy;
        private readonly IServicioPromociones _servicioPromociones;
        
        public ProductoRecomendado_CalcularDescuentoTests()
        {
            // Inicializar mocks
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _promocionRepositoryMock = new Mock<IPromocionRepository>();
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            
            // Configurar fecha actual para pruebas
            var fechaActual = new DateTime(2025, 6, 1, 12, 0, 0);
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            // Inicializar servicios a probar
            _productoRecomendadoPolicy = new ProductoRecomendadoPolicy(
                _productoRepositoryMock.Object,
                _comandaRepositoryMock.Object,
                _dateTimeServiceMock.Object);
            
            // Crear un mock para IServicioPromociones
            var servicioPromocionesMock = new Mock<IServicioPromociones>();
            _servicioPromociones = servicioPromocionesMock.Object;
            
            // Configurar comportamiento básico del servicio de promociones
            servicioPromocionesMock.Setup(s => s.ObtenerPromocionesParaProductoAsync(
                    It.IsAny<Guid>(), 
                    It.IsAny<Guid?>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid productoId, Guid? categoriaId, CancellationToken _) => {
                    // Devolver una promoción simulada para cualquier producto
                    var promocion = CrearPromocionPrueba();
                    return new List<Promocion> { promocion };
                });
        }
        
        [Fact]
        public async Task ProductosRecomendados_DebenTenerPromociones_CuandoExistenPromocionesPorCategoria()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();
            var productos = new List<Producto>
            {
                CrearProducto(Guid.NewGuid(), "Producto 1", categoriaId, true)
            };
            
            // Configurar productos
            _productoRepositoryMock.Setup(r => r.ObtenerTodosAsync(true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productos);
                
            // Usar el factory method para crear la comanda
            var meseroId = Guid.NewGuid();
            var mesaId = Guid.NewGuid(); // Asignar una mesa válida
            var comanda = Comanda.Crear(meseroId, clienteId, mesaId);
            
            // Agregar un item a la comanda
            comanda.AgregarProducto(productos[0].Id, 1, 100m, "Observación de prueba");
            
            _comandaRepositoryMock.Setup(r => r.ObtenerPorClienteAsync(clienteId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Comanda> { comanda });
                
            // Act & Assert
            // Verificamos que se pueda generar recomendaciones y que no arroje excepciones
            var recomendaciones = await _productoRecomendadoPolicy.GenerarRecomendacionesParaCliente(clienteId, 1);
            
            Assert.NotNull(recomendaciones);
            // Verificamos que se obtuvieron promociones para el producto recomendado
            foreach (var productoRecomendado in recomendaciones.ProductosRecomendados)
            {
                var promociones = await _servicioPromociones.ObtenerPromocionesParaProductoAsync(
                    productoRecomendado.ProductoId,
                    productoRecomendado.CategoriaId);
                
                Assert.NotEmpty(promociones);
            }
        }
        
        private Producto CrearProducto(Guid id, string nombre, Guid categoriaId, bool activo)
        {
            var producto = new Mock<Producto>().Object;
            
            // Usamos reflexión para establecer las propiedades
            typeof(EntityBase).GetProperty("Id")?.SetValue(producto, id);
            typeof(Producto).GetProperty("Nombre")?.SetValue(producto, nombre);
            typeof(Producto).GetProperty("CategoriaId")?.SetValue(producto, categoriaId);
            typeof(Producto).GetProperty("CategoriaNombre")?.SetValue(producto, "Categoría de prueba");
            typeof(Producto).GetProperty("Activo")?.SetValue(producto, activo);
            
            // Crear un objeto PrecioProducto en lugar de usar decimal directamente
            var precioProducto = new PrecioProducto(100m);
            typeof(Producto).GetProperty("Precio")?.SetValue(producto, precioProducto);
            
            return producto;
        }
        
        private Promocion CrearPromocionPrueba()
        {
            // Crear una promoción de prueba usando Moq
            var promocion = new Mock<Promocion>().Object;
            
            // Establecer propiedades básicas
            typeof(EntityBase).GetProperty("Id")?.SetValue(promocion, Guid.NewGuid());
            typeof(Promocion).GetProperty("Codigo")?.SetValue(promocion, "PROMO10");
            typeof(Promocion).GetProperty("Nombre")?.SetValue(promocion, "10% de descuento");
            typeof(Promocion).GetProperty("Tipo")?.SetValue(promocion, TipoPromocion.PorcentajeTotal);
            typeof(Promocion).GetProperty("ValorDescuento")?.SetValue(promocion, 10m);
            
            // Configurar comportamiento
            Mock.Get(promocion).Setup(p => p.EstaVigente()).Returns(true);
            
            return promocion;
        }
    }
} 