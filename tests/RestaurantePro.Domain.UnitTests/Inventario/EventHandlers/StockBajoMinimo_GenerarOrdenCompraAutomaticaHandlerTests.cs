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
        
        private readonly Guid _ingredienteId;
        private readonly Guid _proveedorId;
        private readonly string _nombreIngrediente;
        private readonly decimal _stockActual;
        private readonly decimal _stockMinimo;
        private readonly DateTime _fechaActual;
        
        public StockBajoMinimo_GenerarOrdenCompraAutomaticaHandlerTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _eventLogMock = new Mock<IDomainEventLog>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            
            _handler = new StockBajoMinimo_GenerarOrdenCompraAutomaticaHandler(
                _ingredienteRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _ordenCompraRepositoryMock.Object,
                _eventLogMock.Object,
                _dateTimeServiceMock.Object);
                
            _ingredienteId = Guid.NewGuid();
            _proveedorId = Guid.NewGuid();
            _nombreIngrediente = "Tomate";
            _stockActual = 3.0m;
            _stockMinimo = 5.0m;
            _fechaActual = new DateTime(2023, 1, 1, 12, 0, 0);
            
            // Configurar fecha actual para las pruebas
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
        }
        
        [Fact]
        public async Task Handle_ConIngredienteYProveedorValidos_DebeGenerarOrdenCompra()
        {
            // Arrange
            // Crear ingrediente con stock inicial adecuado y stock por DEBAJO del mínimo
            var stockActualBajo = 3.0m; // Menor que _stockMinimo (5.0)
            var ingrediente = Ingrediente.Crear(
                _nombreIngrediente,
                "TOM-001",
                "Tomate para ensaladas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                _stockMinimo,
                stockActualBajo); // Stock inicial bajo el mínimo
                
            // Establecer ID y proveedor principal usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente, _ingredienteId);
            typeof(Ingrediente).GetProperty("ProveedorPrincipalId").SetValue(ingrediente, _proveedorId);
            
            var proveedor = Proveedor.Crear(
                "Distribuidora de Hortalizas",     // nombre
                "Juan Pérez",                      // nombreContacto
                "info@hortalizas.com",             // email
                "912345678",                       // telefono
                "Calle Principal 123",             // direccion
                "Madrid",                          // ciudad
                "28001",                           // codigoPostal
                "España",                          // pais
                "B12345678901",                    // RFC
                "Cuenta 123456789",                // informacionBancaria
                30);                               // diasCredito
                
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(proveedor, _proveedorId);
            
            // IMPORTANTE: Configurar una fecha fija para evitar problemas con las fechas
            var fechaEmision = new DateTime(2023, 1, 1, 10, 0, 0);
            _dateTimeServiceMock
                .Setup(s => s.Now)
                .Returns(fechaEmision);
            
            Console.WriteLine($"Fecha emisión usada: {fechaEmision}");
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(_proveedorId, It.IsAny<CancellationToken>()))
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
            
            // Capturar logs del evento para depurar
            _eventLogMock
                .Setup(l => l.LogEvent(It.IsAny<DomainEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<DomainEvent, string, CancellationToken>((ev, msg, _) => {
                    Console.WriteLine($"EVENT LOG: {msg}");
                })
                .Returns(Task.CompletedTask);
            
            var evento = new StockBajoMinimo(_ingredienteId, _nombreIngrediente, stockActualBajo, _stockMinimo);
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se consultó el ingrediente
            _ingredienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se consultó el proveedor
            _proveedorRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(_proveedorId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se consultaron órdenes pendientes
            _ordenCompraRepositoryMock.Verify(
                r => r.ObtenerPendientesPorProveedorAsync(_proveedorId, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verificar que se agregó una orden
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_SinProveedorPrincipal_NoDebeGenerarOrden()
        {
            // Arrange
            var ingrediente = Ingrediente.Crear(
                _nombreIngrediente,
                "TOM-001",
                "Tomate para ensaladas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                _stockMinimo,
                _stockActual);
                
            // Establecer ID usando reflexión pero sin proveedor principal
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente, _ingredienteId);
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            var evento = new StockBajoMinimo(_ingredienteId, _nombreIngrediente, _stockActual, _stockMinimo);
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se consultó el ingrediente
            _ingredienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que NO se consultó ningún proveedor
            _proveedorRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
                
            // Verificar que NO se agregó ninguna orden
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        
        [Fact]
        public async Task Handle_ConOrdenPendienteExistente_NoDebeGenerarNuevaOrden()
        {
            // Arrange
            var ingrediente = Ingrediente.Crear(
                _nombreIngrediente,
                "TOM-001",
                "Tomate para ensaladas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                _stockMinimo,
                _stockActual);
                
            // Establecer ID y proveedor principal usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(ingrediente, _ingredienteId);
            typeof(Ingrediente).GetProperty("ProveedorPrincipalId").SetValue(ingrediente, _proveedorId);
            
            var proveedor = Proveedor.Crear(
                "Distribuidora de Hortalizas",     // nombre
                "Juan Pérez",                      // nombreContacto
                "info@hortalizas.com",             // email
                "912345678",                       // telefono
                "Calle Principal 123",             // direccion
                "Madrid",                          // ciudad
                "28001",                           // codigoPostal
                "España",                          // pais
                "B12345678901",                    // RFC
                "Cuenta 123456789",                // informacionBancaria
                30);                               // diasCredito
                
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(proveedor, _proveedorId);
            
            // Crear una orden pendiente existente con fecha de entrega válida
            var ordenExistente = OrdenCompra.Crear(
                _proveedorId,
                "Orden pendiente existente",
                _fechaActual);
                
            // Establecer fecha de entrega estimada (5 días después)
            ordenExistente.EstablecerFechaEntrega(_fechaActual.AddDays(5));
                
            // Establecer un ID predecible para la orden existente
            var ordenExistenteId = Guid.NewGuid();
            typeof(EntityBase).GetProperty("Id").SetValue(ordenExistente, ordenExistenteId);
            
            // Agregar algún otro ingrediente a la orden existente
            var otroIngredienteId = Guid.NewGuid();
            ordenExistente.AgregarItem(
                otroIngredienteId,
                "Cebolla",
                2.0m,
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            // IMPORTANTE: Agregar también el ingrediente actual a la orden existente
            // para que sea detectado como ya incluido
            ordenExistente.AgregarItem(
                _ingredienteId,
                _nombreIngrediente,
                1.0m,
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(_proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPendientesPorProveedorAsync(_proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OrdenCompra> { ordenExistente });
            
            // Capturar logs del evento para comprobar
            _eventLogMock
                .Setup(l => l.LogEvent(It.IsAny<DomainEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<DomainEvent, string, CancellationToken>((ev, msg, _) => {
                    Console.WriteLine($"EVENT LOG: {msg}");
                })
                .Returns(Task.CompletedTask);
            
            var evento = new StockBajoMinimo(_ingredienteId, _nombreIngrediente, _stockActual, _stockMinimo);
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se consultó el ingrediente
            _ingredienteRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(_ingredienteId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se consultó el proveedor
            _proveedorRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(_proveedorId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se consultaron órdenes pendientes
            _ordenCompraRepositoryMock.Verify(
                r => r.ObtenerPendientesPorProveedorAsync(_proveedorId, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verificar que NO se agregó ninguna orden
            _ordenCompraRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<OrdenCompra>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
} 