namespace RestaurantePro.Domain.UnitTests.Integration.WithinContext.Inventario
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
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock = new();
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
                _eventRegistryMock.Object,
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
                "Chile",
                "PROV12345678",
                "BBVA 123456789",
                30); // 30 días de plazo pago
                
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(proveedor, proveedorId);
            
            // 2. Crear un ingrediente con stock inicial cero y un stock mínimo mayor
            var ingredienteId = Guid.NewGuid();
            var stockMinimo = 10m;
            var stockActual = 5m; // Stock actual por debajo del mínimo
            
            var ingrediente = Ingrediente.Crear(
                "Tomate", 
                "TOM001", 
                "Tomate rojo maduro", 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 
                stockMinimo, 
                stockActual); // Ya empezamos con stock bajo el mínimo
                
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente, ingredienteId);
            
            // Asociar el proveedor al ingrediente
            ingrediente.AsociarProveedorPrincipal(proveedorId);
            
            // IMPORTANTE: Configurar una fecha fija para evitar problemas con las fechas
            var fechaEmision = new DateTime(2023, 1, 1, 10, 0, 0);
            _dateTimeServiceMock
                .Setup(s => s.Now)
                .Returns(fechaEmision);
            
            Console.WriteLine($"Fecha emisión usada: {fechaEmision}");
            Console.WriteLine($"Datos Ingrediente: ID={ingredienteId}, Nombre={ingrediente.Nombre}, Stock={ingrediente.Stock}, StockMinimo={ingrediente.StockMinimo}");
            Console.WriteLine($"ProveedorId: {proveedorId}, ProveedorPrincipalId: {ingrediente.ProveedorPrincipalId}");
            
            // 3. Configurar mocks
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrdenCompra>());
                
            // IMPORTANTE: Interceptamos las llamadas a AgregarAsync para modificar la orden y evitar la validación
            OrdenCompra ordenCapturada = null;
            _ordenCompraRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()))
                .Callback<OrdenCompra, CancellationToken>((orden, _) => {
                    ordenCapturada = orden;
                    Console.WriteLine($"OrdenCompra capturada: ID={orden.Id}, ProveedorId={orden.ProveedorId}");
                    Console.WriteLine($"Fechas: Emisión={orden.FechaEmision}, Entrega={orden.FechaEntregaEstimada}");
                    Console.WriteLine($"Items: {orden.Items.Count}");
                    foreach (var item in orden.Items)
                    {
                        Console.WriteLine($"- Item: {item.NombreIngrediente}, Cantidad: {item.Cantidad}");
                    }
                })
                .Returns(Task.CompletedTask);
                
            // Capturar todos los eventos de log para ver qué está ocurriendo
            _eventRegistryMock
                .Setup(l => l.RegisterAsync(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Callback<DomainEvent, CancellationToken>((ev, _) => {
                    Console.WriteLine($"EVENT REGISTERED: {ev.GetType().Name}");
                })
                .Returns(Task.CompletedTask);
                
            // Act
            // 1. Capturar el evento StockBajoMinimo para un stock que ya está bajo
            var evento = new RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente.StockBajoMinimo(
                ingredienteId, "Tomate", stockActual, stockMinimo);
                
            // 2. Simular que el ingrediente ya está guardado en la BD
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            // 3. Procesar el evento con el handler
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se agregó una orden
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verificar que se registró el evento
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente.StockBajoMinimo>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
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
                "Chile",
                "PROV12345678",
                "BBVA 123456789",
                30); // 30 días de plazo pago
                
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(proveedor, proveedorId);
            
            // 2. Crear un ingrediente con stock inicial en cero
            var ingredienteId = Guid.NewGuid();
            var stockMinimo = 10m;
            var stockInicial = 0m; // Inicialmente sin stock
            
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
            
            // Agregar un movimiento de ingreso para llevar el stock a 15
            ingrediente.IncrementarStock(15m, "Stock inicial");
            
            // 3. Crear una orden de compra pendiente que ya incluye el ingrediente
            var ordenExistente = OrdenCompra.Crear(
                proveedorId, 
                "Orden manual previa", 
                _fechaActual.AddDays(-1));
                
            // Establecer fecha de entrega estimada posterior a la fecha de emisión
            ordenExistente.EstablecerFechaEntrega(_fechaActual.AddDays(5));
                
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
                .OfType<RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente.StockBajoMinimo>()
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
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<RestaurantePro.Domain.Inventario.Ingredientes.Events.Ingrediente.StockBajoMinimo>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 