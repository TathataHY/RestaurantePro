namespace RestaurantePro.Domain.UnitTests.Integration
{
    /// <summary>
    /// Tests de integración para verificar el flujo completo de generación automática 
    /// de órdenes de compra cuando el stock de un ingrediente cae por debajo del mínimo.
    /// </summary>
    public class StockBajo_GeneracionOrdenCompraAutomaticaTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock = new();
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock = new();
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock = new();
        private readonly Mock<IDomainEventLog> _eventLogMock = new();
        private readonly Mock<IDateTimeService> _dateTimeServiceMock = new();
        
        private readonly StockBajoMinimo_GenerarOrdenCompraAutomaticaHandler _handler;
        private readonly DateTime _fechaActual = new DateTime(2023, 5, 15, 10, 0, 0);
        
        public StockBajo_GeneracionOrdenCompraAutomaticaTests()
        {
            // Configurar fecha actual
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            // Inicializar handler
            _handler = new StockBajoMinimo_GenerarOrdenCompraAutomaticaHandler(
                _ingredienteRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _eventLogMock.Object,
                _dateTimeServiceMock.Object);
        }
        
        [Fact]
        public async Task DecrementarStock_BajoMinimo_DebeGenerarOrdenCompraAutomatica()
        {
            // Arrange
            // 1. Crear un proveedor
            var proveedorId = Guid.NewGuid();
            var proveedor = Proveedor.Crear(
                "Distribuidora Alimentos SA",
                "Juan Pérez",
                "contacto@distribuidora.com",
                "5551234567",
                "Calle Principal 123",
                "Ciudad Test",
                "12345",
                "México",
                "PROV12345678",
                "BBVA 123456789",
                30); // 30 días de plazo pago
                
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(proveedor, proveedorId);
            
            // 2. Crear un ingrediente con stock encima del mínimo
            var ingredienteId = Guid.NewGuid();
            var stockMinimo = 10m;
            var stockInicial = 15m;
            
            var ingrediente = Ingrediente.Crear(
                "Tomate", 
                "TOM001", 
                "Tomate rojo maduro", 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 
                stockMinimo, 
                stockInicial);
                
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente, ingredienteId);
            
            // Asociar el proveedor al ingrediente
            ingrediente.AsociarProveedorPrincipal(proveedorId);
            
            // 3. Configurar mocks
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrdenCompra>());
                
            _ordenCompraRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            // 1. Decrementar stock por debajo del mínimo
            var cantidadDecrementar = 8m; // 15 - 8 = 7, que es menor que el mínimo 10
            var movimiento = ingrediente.DecrementarStock(cantidadDecrementar, "Consumo para preparación");
            
            // 2. Capturar el evento StockBajoMinimo
            var evento = ingrediente.DomainEvents
                .OfType<RestaurantePro.Domain.Inventario.Ingredientes.Events.StockBajoMinimo>()
                .FirstOrDefault();
                
            // Verificar que se generó el evento
            Assert.NotNull(evento);
            
            // 3. Simular que el ingrediente ya está guardado en la BD
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            // 4. Procesar el evento con el handler
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se llamó a AgregarAsync para crear una orden
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verificar que la orden tiene el ingrediente correcto y cantidad adecuada
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(
                    It.Is<OrdenCompra>(o => 
                        o.ProveedorId == proveedorId &&
                        o.Items.Count == 1 &&
                        o.Items.First().IngredienteId == ingredienteId &&
                        // Cantidad a pedir: (mínimo - actual) * 2 = (10 - 7) * 2 = 6
                        Math.Ceiling(o.Items.First().Cantidad) == 6m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se registró el evento correctamente
            _eventLogMock.Verify(
                l => l.LogEvent(
                    It.IsAny<RestaurantePro.Domain.Inventario.Ingredientes.Events.StockBajoMinimo>(),
                    It.Is<string>(s => s.Contains("Se generó orden de compra automática")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task DecrementarStock_ConOrdenPendienteExistente_NoDebeGenerarNuevaOrden()
        {
            // Arrange
            // 1. Crear un proveedor
            var proveedorId = Guid.NewGuid();
            var proveedor = Proveedor.Crear(
                "Distribuidora Alimentos SA",
                "Juan Pérez",
                "contacto@distribuidora.com",
                "5551234567",
                "Calle Principal 123",
                "Ciudad Test",
                "12345",
                "México",
                "PROV12345678",
                "BBVA 123456789",
                30); // 30 días de plazo pago
                
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(proveedor, proveedorId);
            
            // 2. Crear un ingrediente con stock encima del mínimo
            var ingredienteId = Guid.NewGuid();
            var stockMinimo = 10m;
            var stockInicial = 15m;
            
            var ingrediente = Ingrediente.Crear(
                "Tomate", 
                "TOM001", 
                "Tomate rojo maduro", 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 
                stockMinimo, 
                stockInicial);
                
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente, ingredienteId);
            
            // Asociar el proveedor al ingrediente
            ingrediente.AsociarProveedorPrincipal(proveedorId);
            
            // 3. Crear una orden de compra pendiente que ya incluye el ingrediente
            var ordenExistente = OrdenCompra.Crear(
                proveedorId, 
                "Orden manual previa", 
                _fechaActual.AddDays(-1));
                
            ordenExistente.AgregarItem(
                ingredienteId,
                "Tomate",
                5,
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            // 4. Configurar mocks
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrdenCompra> { ordenExistente });
                
            // Act
            // 1. Decrementar stock por debajo del mínimo
            var cantidadDecrementar = 8m; // 15 - 8 = 7, que es menor que el mínimo 10
            var movimiento = ingrediente.DecrementarStock(cantidadDecrementar, "Consumo para preparación");
            
            // 2. Capturar el evento StockBajoMinimo
            var evento = ingrediente.DomainEvents
                .OfType<RestaurantePro.Domain.Inventario.Ingredientes.Events.StockBajoMinimo>()
                .FirstOrDefault();
                
            // Verificar que se generó el evento
            Assert.NotNull(evento);
            
            // 3. Simular que el ingrediente ya está guardado en la BD
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            // 4. Procesar el evento con el handler
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que NO se llamó a AgregarAsync para crear una orden
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()),
                Times.Never);
                
            // Verificar que se registró un mensaje indicando que ya existe una orden
            _eventLogMock.Verify(
                l => l.LogEvent(
                    It.IsAny<RestaurantePro.Domain.Inventario.Ingredientes.Events.StockBajoMinimo>(),
                    It.Is<string>(s => s.Contains("Ya existe una orden pendiente")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 