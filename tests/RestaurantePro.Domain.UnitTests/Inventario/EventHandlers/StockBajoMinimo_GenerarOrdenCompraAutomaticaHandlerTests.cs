namespace RestaurantePro.Domain.UnitTests.Inventario.EventHandlers
{
    public class StockBajoMinimo_GenerarOrdenCompraAutomaticaHandlerTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IDomainEventLog> _eventLogMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        
        private readonly StockBajoMinimo_GenerarOrdenCompraAutomaticaHandler _handler;
        
        // Datos de prueba
        private readonly Guid _ingredienteId = Guid.NewGuid();
        private readonly Guid _proveedorId = Guid.NewGuid();
        private readonly DateTime _fechaActual = new DateTime(2023, 1, 1, 12, 0, 0);
        
        public StockBajoMinimo_GenerarOrdenCompraAutomaticaHandlerTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _eventLogMock = new Mock<IDomainEventLog>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            
            // Configurar fecha actual para pruebas
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            _handler = new StockBajoMinimo_GenerarOrdenCompraAutomaticaHandler(
                _ingredienteRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _eventLogMock.Object,
                _dateTimeServiceMock.Object);
        }
        
        [Fact]
        public async Task Handle_ConIngredienteYProveedorValidos_DebeGenerarOrdenCompra()
        {
            // Arrange
            var evento = new RestaurantePro.Domain.Inventario.Ingredientes.Events.StockBajoMinimo(_ingredienteId, "Tomate", 5m, 10m);
            
            // Crear ingrediente usando reflexión para simular uno existente
            var ingrediente = Ingrediente.Crear("Tomate", "TOM001", "Tomate rojo", RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 10m, 5m);
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente, _ingredienteId);
            
            // Configurar proveedor principal
            typeof(Ingrediente).GetProperty("ProveedorPrincipalId", BindingFlags.Public | BindingFlags.Instance)
                .SetValue(ingrediente, _proveedorId);
            
            // Crear proveedor usando método de fábrica correcto
            var proveedor = Proveedor.Crear(
                "Proveedor Test", 
                "Juan Pérez", 
                "contacto@proveedor.com", 
                "5551234567", 
                "Calle Principal 123", 
                "Ciudad Test", 
                "12345", 
                "País Test", 
                "RFC12345678", 
                "Banco Test Cuenta 123456", 
                30);
            
            // Establecer ID del proveedor usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(proveedor, _proveedorId);
            
            // Configurar mocks
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(_proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrdenCompra>());
                
            _ordenCompraRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se llamó al repository para agregar la orden
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que la orden contiene los parámetros correctos
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(
                    It.Is<OrdenCompra>(o => 
                        o.ProveedorId == _proveedorId &&
                        o.Items.Count == 1 &&
                        o.Items.First().IngredienteId == _ingredienteId &&
                        o.Items.First().Cantidad == 10m), // 2 * (10-5) = 10
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se registró el evento correctamente
            _eventLogMock.Verify(
                l => l.LogEvent(
                    evento,
                    It.Is<string>(s => s.Contains("Se generó orden de compra")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_SinProveedorPrincipal_NoDebeGenerarOrden()
        {
            // Arrange
            var evento = new RestaurantePro.Domain.Inventario.Ingredientes.Events.StockBajoMinimo(_ingredienteId, "Tomate", 5m, 10m);
            
            // Crear ingrediente usando método de fábrica
            var ingrediente = Ingrediente.Crear("Tomate", "TOM001", "Tomate rojo", RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 10m, 5m);
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente, _ingredienteId);
            
            // ProveedorPrincipalId es null por defecto
            
            // Configurar mocks
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que NO se llamó al repository para agregar la orden
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()),
                Times.Never);
                
            // Verificar que se registró un mensaje de error
            _eventLogMock.Verify(
                l => l.LogEvent(
                    evento,
                    It.Is<string>(s => s.Contains("no tiene proveedor principal")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_ConOrdenPendienteExistente_NoDebeGenerarNuevaOrden()
        {
            // Arrange
            var evento = new RestaurantePro.Domain.Inventario.Ingredientes.Events.StockBajoMinimo(_ingredienteId, "Tomate", 5m, 10m);
            
            // Crear ingrediente usando método de fábrica
            var ingrediente = Ingrediente.Crear("Tomate", "TOM001", "Tomate rojo", RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, 10m, 5m);
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente, _ingredienteId);
            
            // Configurar proveedor principal
            typeof(Ingrediente).GetProperty("ProveedorPrincipalId", BindingFlags.Public | BindingFlags.Instance)
                .SetValue(ingrediente, _proveedorId);
            
            // Crear proveedor usando método de fábrica correcto
            var proveedor = Proveedor.Crear(
                "Proveedor Test", 
                "Juan Pérez", 
                "contacto@proveedor.com", 
                "5551234567", 
                "Calle Principal 123", 
                "Ciudad Test", 
                "12345", 
                "País Test", 
                "RFC12345678", 
                "Banco Test Cuenta 123456", 
                30);
            
            // Establecer ID del proveedor usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(proveedor, _proveedorId);
            
            // Crear una orden pendiente que ya incluye el ingrediente
            var ordenExistente = OrdenCompra.Crear(_proveedorId, "Orden existente", _fechaActual);
            ordenExistente.AgregarItem(_ingredienteId, "Tomate", 5, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            // Configurar mocks
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(_proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrdenCompra> { ordenExistente });
                
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que NO se llamó al repository para agregar la orden
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()),
                Times.Never);
                
            // Verificar que se registró un mensaje indicando que ya existe una orden
            _eventLogMock.Verify(
                l => l.LogEvent(
                    evento,
                    It.Is<string>(s => s.Contains("Ya existe una orden pendiente")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 