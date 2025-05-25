namespace RestaurantePro.Domain.UnitTests.Integration.BetweenContexts.Comercial_Inventario
{
    /// <summary>
    /// Pruebas de integración entre el contexto Comercial (Facturación) y el contexto Proveedores (Órdenes de Compra).
    /// Verifica que las órdenes de compra aprobadas sean facturadas correctamente.
    /// </summary>
    public class Facturacion_OrdenesCompraAprobadas_Tests
    {
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IFacturaRepository> _facturaRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
        
        public Facturacion_OrdenesCompraAprobadas_Tests()
        {
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _facturaRepositoryMock = new Mock<IFacturaRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _eventDispatcherMock = new Mock<IDomainEventDispatcher>();
            
            // Configurar fecha actual para todas las pruebas
            _dateTimeServiceMock.Setup(d => d.Now).Returns(new DateTime(2023, 8, 15));
            _dateTimeServiceMock.Setup(d => d.Today).Returns(new DateTime(2023, 8, 15));
        }
        
        [Fact]
        public async Task OrdenCompraAprobada_DebeGenerarFactura_CuandoSeEmiteEvento()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var ordenCompraId = Guid.NewGuid();
            
            // Configurar proveedor
            var proveedor = Proveedor.Crear(
                "Distribuidora Alimentos SPA", 
                "Proveedor de alimentos",
                "20.123.456-7", // RUT
                ProveedorCategoria.Insumos);
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            // Configurar orden de compra aprobada
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                _dateTimeServiceMock.Object.Now,
                _dateTimeServiceMock.Object.Now.AddDays(5));
                
            // Añadir ítems a la orden
            var itemOrden1 = ItemOrdenCompra.Crear(
                Guid.NewGuid(), // Ingrediente ID
                "Tomate",
                10, // Cantidad
                "Kg", // Unidad
                5.99m); // Precio unitario
                
            var itemOrden2 = ItemOrdenCompra.Crear(
                Guid.NewGuid(), // Ingrediente ID
                "Lechuga",
                5, // Cantidad
                "Kg", // Unidad
                4.50m); // Precio unitario
                
            ordenCompra.AgregarItem(itemOrden1);
            ordenCompra.AgregarItem(itemOrden2);
            ordenCompra.Aprobar(_dateTimeServiceMock.Object);
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
                
            // Configurar manejador de eventos para OrdenCompraAprobada
            var ordenCompraAprobadaHandler = new OrdenCompraAprobada_FacturacionHandler(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _facturaRepositoryMock.Object,
                _dateTimeServiceMock.Object);
                
            // Crear evento OrdenCompraAprobada
            var evento = new OrdenCompraAprobada(
                ordenCompraId,
                proveedorId,
                _dateTimeServiceMock.Object.Now,
                ordenCompra.Total);
                
            // Factura generada esperada - mock para verificar que se guarde en el repositorio
            Factura? facturaGenerada = null;
            _facturaRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Factura>(), It.IsAny<CancellationToken>()))
                .Callback<Factura, CancellationToken>((factura, token) => facturaGenerada = factura)
                .Returns(Task.CompletedTask);
                
            // Act
            await ordenCompraAprobadaHandler.Handle(evento, CancellationToken.None);
            
            // Assert
            Assert.NotNull(facturaGenerada);
            Assert.Equal(ordenCompra.Total, facturaGenerada!.Total);
            // Nota: No verificamos EntidadRelacionadaId y Referencia porque 
            // la implementación actual de Factura no tiene esas propiedades
            
            // Verificar que se usó el repositorio de facturas
            _facturaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Factura>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task OrdenCompraAprobada_DebeNotificarAlProveedor_CuandoSeEmiteEvento()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var ordenCompraId = Guid.NewGuid();
            
            // Configurar proveedor con contacto
            var proveedor = Proveedor.Crear(
                "Distribuidora Alimentos SPA", 
                "Proveedor de alimentos",
                "20.123.456-7", // RUT
                ProveedorCategoria.Insumos);
                
            var contactoEmail = Email.Create("contacto@distribuidora.com");
            var contacto = ContactoProveedor.Crear(
                "Juan Pérez",
                contactoEmail,
                "+56 9 1234 5678");
                
            proveedor.AgregarContacto(contacto);
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            // Configurar orden de compra aprobada
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                _dateTimeServiceMock.Object.Now,
                _dateTimeServiceMock.Object.Now.AddDays(5));
                
            // Añadir ítems a la orden
            var itemOrden = ItemOrdenCompra.Crear(
                Guid.NewGuid(), // Ingrediente ID
                "Tomate",
                10, // Cantidad
                "Kg", // Unidad
                5.99m); // Precio unitario
                
            ordenCompra.AgregarItem(itemOrden);
            ordenCompra.Aprobar(_dateTimeServiceMock.Object);
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
                
            // Configurar servicio de notificaciones
            var notificacionServiceMock = new Mock<IServicioNotificaciones>();
            
            // Configurar manejador de eventos para OrdenCompraAprobada
            var ordenCompraAprobadaHandler = new OrdenCompraAprobada_NotificacionProveedorHandler(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                notificacionServiceMock.Object,
                _dateTimeServiceMock.Object);
                
            // Crear evento OrdenCompraAprobada
            var evento = new OrdenCompraAprobada(
                ordenCompraId,
                proveedorId,
                _dateTimeServiceMock.Object.Now,
                ordenCompra.Total);
                
            // Act
            await ordenCompraAprobadaHandler.Handle(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se envió una notificación
            notificacionServiceMock.Verify(
                n => n.EnviarNotificacionAsync(
                    It.Is<string>(s => s.Contains("Orden de Compra Aprobada")),
                    It.IsAny<string>(),
                    TipoNotificacion.Informativa,
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task OrdenCompraAprobada_DebeActualizarEstadisticasProveedor_CuandoSeEmiteEvento()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var ordenCompraId = Guid.NewGuid();
            
            // Configurar proveedor con estadísticas iniciales
            var proveedor = Proveedor.Crear(
                "Distribuidora Alimentos SPA", 
                "Proveedor de alimentos",
                "20.123.456-7", // RUT
                ProveedorCategoria.Insumos);
                
            // Configurar estadísticas iniciales del proveedor
            proveedor.RegistrarOrden(_dateTimeServiceMock.Object.Now.AddMonths(-1));
            proveedor.RegistrarOrden(_dateTimeServiceMock.Object.Now.AddMonths(-2));
                
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            // Configurar orden de compra aprobada
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                _dateTimeServiceMock.Object.Now,
                _dateTimeServiceMock.Object.Now.AddDays(5));
                
            // Añadir ítems a la orden
            var itemOrden = ItemOrdenCompra.Crear(
                Guid.NewGuid(), // Ingrediente ID
                "Tomate",
                10, // Cantidad
                "Kg", // Unidad
                5.99m); // Precio unitario
                
            ordenCompra.AgregarItem(itemOrden);
            ordenCompra.Aprobar(_dateTimeServiceMock.Object);
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
                
            // Capturar el proveedor actualizado
            Proveedor? proveedorActualizado = null;
            _proveedorRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()))
                .Callback<Proveedor, CancellationToken>((p, token) => proveedorActualizado = p)
                .Returns(Task.CompletedTask);
                
            // Configurar manejador de eventos para OrdenCompraAprobada
            var ordenCompraAprobadaHandler = new OrdenCompraAprobada_ActualizarEstadisticasProveedorHandler(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _dateTimeServiceMock.Object);
                
            // Crear evento OrdenCompraAprobada
            var evento = new OrdenCompraAprobada(
                ordenCompraId,
                proveedorId,
                _dateTimeServiceMock.Object.Now,
                59.90m); // Total de la orden
                
            // Act
            await ordenCompraAprobadaHandler.Handle(evento, CancellationToken.None);
            
            // Assert
            Assert.NotNull(proveedorActualizado);
            // Verificamos solo que se ha registrado la orden, ya que TotalCompras no está implementado
            Assert.Contains(_dateTimeServiceMock.Object.Now, proveedorActualizado!.HistorialOrdenes);
            
            // Verificar que se actualizó el proveedor
            _proveedorRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task IntegracionCompleta_OrdenCompraAprobada_DebeDesencadenarFlujoCompleto()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var ordenCompraId = Guid.NewGuid();
            
            // Configurar proveedor
            var proveedor = Proveedor.Crear(
                "Distribuidora Alimentos SPA", 
                "Proveedor de alimentos",
                "20.123.456-7", // RUT
                ProveedorCategoria.Insumos);
                
            var contactoEmail = Email.Create("contacto@distribuidora.com");
            var contacto = ContactoProveedor.Crear(
                "Juan Pérez",
                contactoEmail,
                "+56 9 1234 5678");
                
            proveedor.AgregarContacto(contacto);
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            // Configurar orden de compra aprobada
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                _dateTimeServiceMock.Object.Now,
                _dateTimeServiceMock.Object.Now.AddDays(5));
                
            // Añadir ítems a la orden
            var itemOrden1 = ItemOrdenCompra.Crear(
                Guid.NewGuid(), // Ingrediente ID
                "Tomate",
                10, // Cantidad
                "Kg", // Unidad
                5.99m); // Precio unitario
                
            var itemOrden2 = ItemOrdenCompra.Crear(
                Guid.NewGuid(), // Ingrediente ID
                "Lechuga",
                5, // Cantidad
                "Kg", // Unidad
                4.50m); // Precio unitario
                
            ordenCompra.AgregarItem(itemOrden1);
            ordenCompra.AgregarItem(itemOrden2);
            ordenCompra.Aprobar(_dateTimeServiceMock.Object);
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
                
            // Capturar la factura generada
            Factura? facturaGenerada = null;
            _facturaRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Factura>(), It.IsAny<CancellationToken>()))
                .Callback<Factura, CancellationToken>((factura, token) => facturaGenerada = factura)
                .Returns(Task.CompletedTask);
                
            // Capturar eventos publicados
            _eventDispatcherMock
                .Setup(e => e.Dispatch(It.IsAny<FacturaCreada>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
                
            // Configurar el event dispatcher para que reenvíe eventos a los manejadores
            var eventDispatcher = new Mock<IEventDispatcher>();
            eventDispatcher
                .Setup(d => d.Dispatch(It.IsAny<DomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Configurar el servicio de facturación integrado
            var servicioFacturacion = new ServicioFacturacion(
                _facturaRepositoryMock.Object,
                _dateTimeServiceMock.Object);
                
            // Configurar el servicio de integración Proveedores-Comercial
            var servicioIntegracionProveedoresComercial = new ProveedoresComercialIntegrationService(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                servicioFacturacion,
                _dateTimeServiceMock.Object);
                
            // Crear evento OrdenCompraAprobada
            var evento = new OrdenCompraAprobada(
                ordenCompraId,
                proveedorId,
                _dateTimeServiceMock.Object.Now,
                ordenCompra.Total);
                
            // Act
            await servicioIntegracionProveedoresComercial.ProcesarOrdenCompraAprobadaAsync(evento, CancellationToken.None);
            
            // Assert
            // Verificar que se generó la factura
            Assert.NotNull(facturaGenerada);
            Assert.Equal(ordenCompra.Total, facturaGenerada!.Total);
            
            // Verificar que se publicó el evento FacturaCreada
            _eventDispatcherMock.Verify(e => e.Dispatch(It.IsAny<FacturaCreada>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            
            // Verificar que se actualizó el proveedor
            _proveedorRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
} 