namespace RestaurantePro.Domain.UnitTests.Integration.BetweenContexts.Core_Comercial
{
    /// <summary>
    /// Pruebas de integración para verificar la interacción entre el sistema de recomendación de productos
    /// del contexto Core y el sistema de descuentos del contexto Comercial.
    /// </summary>
    public class ProductoRecomendado_CalcularDescuentoTests
    {
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IPromocionRepository> _promocionRepositoryMock;
        private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaFidelizacionRepositoryMock;
        private readonly Mock<IComandaRepository> _comandaRepositoryMock;
        private readonly Mock<IEventRegistry> _eventRegistryMock;
        
        public ProductoRecomendado_CalcularDescuentoTests()
        {
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _promocionRepositoryMock = new Mock<IPromocionRepository>();
            _tarjetaFidelizacionRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
            _comandaRepositoryMock = new Mock<IComandaRepository>();
            _eventRegistryMock = new Mock<IEventRegistry>();
            
            // Configurar fecha actual
            _dateTimeServiceMock.Setup(d => d.Now).Returns(new DateTime(2023, 7, 15));
        }

        [Fact]
        public async Task CalcularDescuento_ClienteSinTarjetaFidelizacion_DebeRetornarCero()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            
            // Cliente sin tarjeta de fidelización
            var cliente = Cliente.Crear(clienteId, "Juan Pérez", "juan@example.com", "555-123456");
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            // Configurar tarjeta de fidelización (inexistente)
            _tarjetaFidelizacionRepositoryMock
                .Setup(r => r.ObtenerPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TarjetaFidelizacion?)null);
            
            // Producto recomendado
            var producto = Producto.Crear(
                "Pizza Supreme",
                "Pizza con todos los ingredientes",
                new PrecioProducto(19.99m),
                Guid.NewGuid(),
                "Pizzas");
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
            
            // Promoción para productos recomendados
            var promocion = Promocion.Crear(
                "PROMO-REC-10",
                "Promoción Productos Recomendados",
                "10% de descuento en productos recomendados",
                TipoPromocion.ProductoRecomendado,
                10, // porcentaje descuento
                DateTime.Now.AddDays(-10),
                DateTime.Now.AddDays(10),
                100, // monto mínimo
                1, // cantidad mínima
                null, // categoría
                true // activo
            );
            
            _promocionRepositoryMock
                .Setup(r => r.ObtenerPromocionesActivasPorTipoAsync(
                    TipoPromocion.ProductoRecomendado, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Promocion> { promocion });
            
            // Configurar el servicio de política de productos recomendados
            var productoRecomendadoPolicy = new ProductoRecomendadoPolicy(
                _productoRepositoryMock.Object,
                _comandaRepositoryMock.Object,
                _dateTimeServiceMock.Object);
            
            // Configurar el servicio de fidelización
            var servicioFidelizacion = new ServicioFidelizacion(
                _tarjetaFidelizacionRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _dateTimeServiceMock.Object);
            
            // Configurar el servicio de descuentos
            var servicioDescuentos = new ServicioDescuentos(
                _promocionRepositoryMock.Object,
                productoRecomendadoPolicy,
                servicioFidelizacion,
                _dateTimeServiceMock.Object);
            
            // Act
            var descuento = await servicioDescuentos.CalcularDescuentoAsync(
                cliente,
                productoId,
                1, // cantidad
                CancellationToken.None);
            
            // Assert
            Assert.Equal(0, descuento); // Cliente sin tarjeta no recibe descuento
        }
        
        [Fact]
        public async Task CalcularDescuento_ClienteConTarjetaPlatinum_DebeRetornarDescuentoAumentado()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            
            // Cliente con tarjeta platinum
            var cliente = Cliente.Crear(clienteId, "María López", "maria@example.com", "555-987654");
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            // Configurar tarjeta de fidelización (platinum)
            var tarjeta = TarjetaFidelizacion.Crear(
                clienteId,
                "TF-PLAT-789"); // Código de tarjeta
                
            tarjeta.AsignarNivel(NivelFidelizacion.Platinum);
            tarjeta.AgregarPuntos(5000); // Suficientes puntos para ser platinum
            
            _tarjetaFidelizacionRepositoryMock
                .Setup(r => r.ObtenerPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
            
            // Producto recomendado con precio 200
            var producto = Producto.Crear(
                "Parrillada Premium",
                "Parrillada para 4 personas con cortes premium",
                new PrecioProducto(200m),
                Guid.NewGuid(),
                "Parrilladas");
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
            
            // Configurar política de productos recomendados para que considere este producto como recomendado
            var productoRecomendadoPolicy = new ProductoRecomendadoPolicy(
                _productoRepositoryMock.Object,
                _comandaRepositoryMock.Object,
                _dateTimeServiceMock.Object);
                
            // Mockear el método de verificación para que retorne true
            var productoRecomendadoPolicyMock = new Mock<IProductoRecomendadoPolicy>();
            productoRecomendadoPolicyMock
                .Setup(p => p.EsProductoRecomendadoAsync(productoId, clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            // Promoción para productos recomendados (15% de descuento)
            var promocion = Promocion.Crear(
                "PROMO-REC-15",
                "Promoción Productos Recomendados Premium",
                "15% de descuento en productos recomendados premium",
                TipoPromocion.ProductoRecomendado,
                15, // porcentaje descuento
                DateTime.Now.AddDays(-10),
                DateTime.Now.AddDays(10),
                150, // monto mínimo
                1, // cantidad mínima
                null, // categoría
                true // activo
            );
            
            _promocionRepositoryMock
                .Setup(r => r.ObtenerPromocionesActivasPorTipoAsync(
                    TipoPromocion.ProductoRecomendado, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Promocion> { promocion });
            
            // Configurar el servicio de fidelización
            var servicioFidelizacion = new ServicioFidelizacion(
                _tarjetaFidelizacionRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _dateTimeServiceMock.Object);
            
            // Configurar el servicio de descuentos
            var servicioDescuentos = new ServicioDescuentos(
                _promocionRepositoryMock.Object,
                productoRecomendadoPolicyMock.Object,
                servicioFidelizacion,
                _dateTimeServiceMock.Object);
            
            // Act
            var descuento = await servicioDescuentos.CalcularDescuentoAsync(
                cliente,
                productoId,
                1, // cantidad
                CancellationToken.None);
            
            // Assert
            Assert.Equal(50m, descuento); // 15% de 200 = 30, más 20 de bonificación platinum = 50
        }
        
        [Fact]
        public async Task CalcularDescuento_ProductoNoRecomendado_DebeRetornarSoloDescuentoFidelizacion()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            
            // Cliente con tarjeta gold
            var cliente = Cliente.Crear(clienteId, "Carlos Ruiz", "carlos@example.com", "555-456789");
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            // Configurar tarjeta de fidelización (gold)
            var tarjeta = TarjetaFidelizacion.Crear(
                clienteId,
                "TF-GOLD-456"); // Código de tarjeta
                
            tarjeta.AsignarNivel(NivelFidelizacion.Gold);
            tarjeta.AgregarPuntos(2500); // Suficientes puntos para ser gold
            
            _tarjetaFidelizacionRepositoryMock
                .Setup(r => r.ObtenerPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
            
            // Producto NO recomendado
            var producto = Producto.Crear(
                "Ensalada César",
                "Ensalada César tradicional",
                new PrecioProducto(80m),
                Guid.NewGuid(),
                "Ensaladas");
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
            
            // Configurar política de productos recomendados para que NO considere este producto como recomendado
            var productoRecomendadoPolicyMock = new Mock<IProductoRecomendadoPolicy>();
            productoRecomendadoPolicyMock
                .Setup(p => p.EsProductoRecomendadoAsync(productoId, clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            
            // Configurar el servicio de fidelización
            var servicioFidelizacion = new ServicioFidelizacion(
                _tarjetaFidelizacionRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _dateTimeServiceMock.Object);
            
            // Configurar el servicio de descuentos
            var servicioDescuentos = new ServicioDescuentos(
                _promocionRepositoryMock.Object,
                productoRecomendadoPolicyMock.Object,
                servicioFidelizacion,
                _dateTimeServiceMock.Object);
            
            // Act
            var descuento = await servicioDescuentos.CalcularDescuentoAsync(
                cliente,
                productoId,
                1, // cantidad
                CancellationToken.None);
            
            // Assert
            Assert.Equal(8m, descuento); // 10% de descuento gold sobre 80 = 8
        }

        [Fact]
        public async Task IntegracionCompleta_DescuentoEnProductoRecomendado_DebeEmitirEventos()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            
            // Cliente con tarjeta gold
            var cliente = Cliente.Crear(clienteId, "Ana Torres", "ana@example.com", "555-123789");
            _clienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cliente);
            
            // Configurar tarjeta de fidelización (gold)
            var tarjeta = TarjetaFidelizacion.Crear(
                clienteId,
                "TF-GOLD-789"); // Código de tarjeta
                
            tarjeta.AsignarNivel(NivelFidelizacion.Gold);
            tarjeta.AgregarPuntos(2500); // Suficientes puntos para ser gold
            
            _tarjetaFidelizacionRepositoryMock
                .Setup(r => r.ObtenerPorClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
            
            // Producto recomendado
            var producto = Producto.Crear(
                "Combo Familiar",
                "Combo para familia con 4 platos principales y postre",
                new PrecioProducto(150m),
                Guid.NewGuid(),
                "Combos");
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
            
            // Configurar política de productos recomendados
            var productoRecomendadoPolicy = new ProductoRecomendadoPolicy(
                _productoRepositoryMock.Object,
                _comandaRepositoryMock.Object,
                _dateTimeServiceMock.Object);
                
            // Mockear el método de verificación para que retorne true
            var productoRecomendadoPolicyMock = new Mock<IProductoRecomendadoPolicy>();
            productoRecomendadoPolicyMock
                .Setup(p => p.EsProductoRecomendadoAsync(productoId, clienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
                
            // Capturar evento de descuento aplicado
            _eventRegistryMock
                .Setup(e => e.Publish(It.IsAny<DescuentoAplicado>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            
            // Promoción para productos recomendados (15% de descuento)
            var promocion = Promocion.Crear(
                "PROMO-REC-15",
                "Promoción Productos Recomendados Premium",
                "15% de descuento en productos recomendados premium",
                TipoPromocion.ProductoRecomendado,
                15, // porcentaje descuento
                DateTime.Now.AddDays(-10),
                DateTime.Now.AddDays(10),
                100, // monto mínimo
                1, // cantidad mínima
                null, // categoría
                true // activo
            );
            
            _promocionRepositoryMock
                .Setup(r => r.ObtenerPromocionesActivasPorTipoAsync(
                    TipoPromocion.ProductoRecomendado, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Promocion> { promocion });
            
            // Configurar el servicio de fidelización
            var servicioFidelizacion = new ServicioFidelizacion(
                _tarjetaFidelizacionRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _dateTimeServiceMock.Object);
            
            // Configurar el servicio de descuentos con eventos
            var servicioDescuentos = new ServicioDescuentos(
                _promocionRepositoryMock.Object,
                productoRecomendadoPolicyMock.Object,
                servicioFidelizacion,
                _dateTimeServiceMock.Object,
                _eventRegistryMock.Object);
            
            // Act
            var descuento = await servicioDescuentos.CalcularDescuentoAsync(
                cliente,
                productoId,
                2, // cantidad (2 unidades)
                CancellationToken.None);
            
            // Assert
            // Esperamos: 15% de descuento promocional (22.5 por unidad, 45 por 2) + 10% de gold (15 por unidad, 30 por 2) = 75
            Assert.Equal(75m, descuento);
            
            // Verificar que se emitió el evento de descuento aplicado
            _eventRegistryMock.Verify(e => e.Publish(It.IsAny<DescuentoAplicado>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 