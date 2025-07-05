#nullable disable
namespace RestaurantePro.Domain.UnitTests.Integration.BetweenContexts.Comercial_Proveedores
{
    /// <summary>
    /// Pruebas de integración entre los contextos de Comercial y Proveedores
    /// Verifica que cuando se aprueba una orden de compra, se genere correctamente la factura
    /// </summary>
    public class OrdenCompraAprobada_CreacionFacturaTests
    {
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IServicioFacturacion> _servicioFacturacionMock;
        private readonly Mock<IFacturaRepository> _facturaRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock;
        private readonly Mock<IEventSubscriptionManager> _subscriptionManagerMock;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly NotificationManager _notificationManager;

        public OrdenCompraAprobada_CreacionFacturaTests()
        {
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _servicioFacturacionMock = new Mock<IServicioFacturacion>();
            _facturaRepositoryMock = new Mock<IFacturaRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _eventRegistryMock = new Mock<IDomainEventRegistry>();
            _subscriptionManagerMock = new Mock<IEventSubscriptionManager>();
            _notificationManager = new NotificationManager();
            
            // Crear el despachador de eventos con las dependencias simuladas
            _eventDispatcher = new DomainEventDispatcher(
                _serviceProviderMock.Object,
                _eventRegistryMock.Object,
                _subscriptionManagerMock.Object);
        }

        [Fact]
        public async Task OrdenCompraAprobada_DebeGenerarFactura_YActualizarProveedor()
        {
            // Arrange
            var fechaActual = new DateTime(2024, 1, 1, 12, 0, 0);
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);

            // Crear proveedor
            var proveedor = Proveedor.Crear(
                "Proveedor Test",
                "Contacto Test",
                "proveedor@test.com",
                "123456789",
                "Dirección Test",
                "Ciudad Test",
                "12345",
                "País Test",
                "RFC12345678901",
                "Cuenta 123456789",
                30);

            // Configurar para evitar problemas con Reflection al establecer manualmente el Id
            var proveedorId = Guid.NewGuid();
            var propiedadProveedorId = proveedor.GetType().GetProperty("Id", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
                
            if (propiedadProveedorId != null && propiedadProveedorId.CanWrite)
            {
                propiedadProveedorId.SetValue(proveedor, proveedorId);
            }

            // Crear items de orden de compra
            var ordenCompraId = Guid.NewGuid();
            var items = new List<ItemOrdenCompra>();
            var ingredienteId = Guid.NewGuid();
            
            // Usar la sobrecarga correcta de ItemOrdenCompra.Crear basada en el código existente
            items.Add(ItemOrdenCompra.Crear(ordenCompraId, ingredienteId, "Tomate", 10, UnidadMedida.Kilogramo));
            items.Add(ItemOrdenCompra.Crear(ordenCompraId, Guid.NewGuid(), "Cebolla", 5, UnidadMedida.Kilogramo));

            // Crear orden de compra
            var ordenCompraResult = OrdenCompra.Crear(
                proveedorId,
                items,
                fechaActual,
                fechaActual.AddDays(7),
                "Orden de prueba para integración",
                new NotificationManager());
            
            var ordenCompra = ordenCompraResult.Value;
            
            // Configurar ID de la orden de compra
            var propiedadOrdenCompraId = ordenCompra.GetType().GetProperty("Id", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
                
            if (propiedadOrdenCompraId != null && propiedadOrdenCompraId.CanWrite)
            {
                propiedadOrdenCompraId.SetValue(ordenCompra, ordenCompraId);
            }

            // Crear factura que se devolverá desde el servicio de facturación
            var factura = Factura.Crear(
                $"FC-OC-{ordenCompraId.ToString().Substring(0, 8)}",
                TipoFactura.Fiscal,
                proveedor.Nombre,
                proveedor.Id,
                proveedor.RFC,
                proveedor.Direccion,
                new List<Guid> { ordenCompraId },
                "Factura de prueba",
                fechaActual,
                _dateTimeServiceMock.Object);

            // Configurar mocks de repositorios
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
            
            _proveedorRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback<Proveedor, CancellationToken>((p, _) => {
                    // Verificar que se actualizó la fecha de última orden
                    p.UltimaOrden.Should().Be(fechaActual);
                });
            
            _servicioFacturacionMock
                .Setup(s => s.GenerarFacturaParaComandaAsync(
                    ordenCompraId,
                    TipoFactura.Fiscal,
                    proveedor.Nombre,
                    proveedor.Id,
                    proveedor.RFC,
                    proveedor.Direccion,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(factura));
            
            _facturaRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Factura>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Configurar manejadores de eventos
            var facturacionHandler = new OrdenCompraAprobada_FacturacionHandler(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _facturaRepositoryMock.Object,
                _dateTimeServiceMock.Object);
                
            var estadisticasHandler = new OrdenCompraAprobada_ActualizarEstadisticasProveedorHandler(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);
                
            // Configurar el ServiceProvider para que devuelva los manejadores
            var handlers = new List<IDomainEventHandler<OrdenCompraAprobada>> 
            { 
                facturacionHandler, 
                estadisticasHandler 
            };
            
            _serviceProviderMock
                .Setup(sp => sp.GetService(It.Is<Type>(t => 
                    t.IsGenericType && 
                    t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                    t.GetGenericArguments()[0].IsGenericType &&
                    t.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(IDomainEventHandler<>) &&
                    t.GetGenericArguments()[0].GetGenericArguments()[0] == typeof(OrdenCompraAprobada))))
                .Returns(handlers);

            // Crear servicio de integración
            var servicioIntegracion = new ServicioIntegracionProveedores(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _servicioFacturacionMock.Object,
                _facturaRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManager);

            // Act
            // 1. Crear evento de orden de compra aprobada
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId, fechaActual, 65m);
            
            // 2. Procesar el evento mediante el servicio de integración
            var result = await servicioIntegracion.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // 3. Alternativamente, disparar el evento para que lo manejen los handlers
            await _eventDispatcher.Dispatch(evento);

            // Assert
            // Verificar que el procesamiento fue exitoso
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            
            // Verificar que se llamó al servicio de facturación
            _servicioFacturacionMock.Verify(
                s => s.GenerarFacturaParaComandaAsync(
                    ordenCompraId,
                    TipoFactura.Fiscal,
                    proveedor.Nombre,
                    proveedor.Id,
                    proveedor.RFC,
                    proveedor.Direccion,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verificar que se actualizó el proveedor
            _proveedorRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce); // Podría ser llamado múltiples veces si hay varios handlers
        }

        [Fact]
        public async Task OrdenCompraAprobada_ConErrorEnCreacionFactura_NoDebeActualizarProveedor()
        {
            // Arrange
            var fechaActual = new DateTime(2024, 1, 1, 12, 0, 0);
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);

            // Crear proveedor
            var proveedor = Proveedor.Crear(
                "Proveedor Test",
                "Contacto Test",
                "proveedor@test.com",
                "123456789",
                "Dirección Test",
                "Ciudad Test",
                "12345",
                "País Test",
                "RFC12345678901",
                "Cuenta 123456789",
                30);

            var proveedorId = Guid.NewGuid();
            var propiedadProveedorId = proveedor.GetType().GetProperty("Id", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
                
            if (propiedadProveedorId != null && propiedadProveedorId.CanWrite)
            {
                propiedadProveedorId.SetValue(proveedor, proveedorId);
            }

            // Crear orden de compra
            var ordenCompraId = Guid.NewGuid();
            var items = new List<ItemOrdenCompra>();
            items.Add(ItemOrdenCompra.Crear(ordenCompraId, Guid.NewGuid(), "Tomate", 10, UnidadMedida.Kilogramo));
            
            var ordenCompraResult = OrdenCompra.Crear(
                proveedorId,
                items,
                fechaActual,
                fechaActual.AddDays(7),
                "Orden de prueba para integración",
                new NotificationManager());
            
            var ordenCompra = ordenCompraResult.Value;
            
            var propiedadOrdenCompraId = ordenCompra.GetType().GetProperty("Id", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
                
            if (propiedadOrdenCompraId != null && propiedadOrdenCompraId.CanWrite)
            {
                propiedadOrdenCompraId.SetValue(ordenCompra, ordenCompraId);
            }

            // Configurar mocks
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
            
            // Simular error en la generación de factura
            _servicioFacturacionMock
                .Setup(s => s.GenerarFacturaParaComandaAsync(
                    ordenCompraId,
                    TipoFactura.Fiscal,
                    proveedor.Nombre,
                    proveedor.Id,
                    proveedor.RFC,
                    proveedor.Direccion,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<Factura>("Error al crear factura"));

            // Crear servicio
            var servicioIntegracion = new ServicioIntegracionProveedores(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _servicioFacturacionMock.Object,
                _facturaRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManager);

            // Act
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId, fechaActual, 50m);
            var result = await servicioIntegracion.ProcesarOrdenCompraAprobadaAsync(evento);

            // Assert
            // Verificar que el procesamiento falló
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            
            // Verificar que el error está relacionado con la facturación
            result.Errors.Should().Contain(e => e.Contains("Error al crear factura"));
            
            // Verificar que no se actualizó el proveedor
            _proveedorRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
} 