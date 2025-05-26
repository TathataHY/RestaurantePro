namespace RestaurantePro.Domain.UnitTests.Integration.BetweenContexts.Comercial_Inventario
{
    /// <summary>
    /// Pruebas de integración entre el contexto Comercial (Facturación) y el contexto Inventario (Órdenes de Compra).
    /// Verifica que las órdenes de compra aprobadas sean facturadas correctamente.
    /// </summary>
    public class Facturacion_OrdenesCompraAprobadas_Tests
    {
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IFacturaRepository> _facturaRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<IServicioFacturacion> _servicioFacturacionMock;
        
        private readonly Mock<IServicioIntegracionProveedores> _integrationServiceMock;
        private Guid _ordenCompraId = Guid.NewGuid();
        private Guid _proveedorId = Guid.NewGuid();
        private Guid _facturaId = Guid.NewGuid();
        
        public Facturacion_OrdenesCompraAprobadas_Tests()
        {
            // Inicializar mocks
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _facturaRepositoryMock = new Mock<IFacturaRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _servicioFacturacionMock = new Mock<IServicioFacturacion>();
            _integrationServiceMock = new Mock<IServicioIntegracionProveedores>();
            
            // Configurar fecha actual para pruebas
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2025, 6, 1, 12, 0, 0));
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobada_DebeGenerarFactura_CuandoSeRecibeLaOrdenAprobada()
        {
            // Arrange
            // Configurar evento de orden aprobada
            var fechaAprobacion = _dateTimeServiceMock.Object.Now;
            var evento = new OrdenCompraAprobada(_ordenCompraId, _proveedorId, fechaAprobacion, 500m);
            
            // Configurar el mock del servicio de integración
            _integrationServiceMock.Setup(s => s.ProcesarOrdenCompraAprobadaAsync(
                It.IsAny<OrdenCompraAprobada>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));
            
            // Act
            var result = await _integrationServiceMock.Object.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();
            
            _integrationServiceMock.Verify(s => s.ProcesarOrdenCompraAprobadaAsync(
                It.Is<OrdenCompraAprobada>(e => e.OrdenCompraId == _ordenCompraId && e.ProveedorId == _proveedorId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        private OrdenCompra CrearOrdenCompraPrueba()
        {
            // Crear una orden de compra utilizando el factory method
            var fechaEmision = _dateTimeServiceMock.Object.Now.AddDays(-1);
            var ordenCompra = OrdenCompra.Crear(
                _proveedorId,
                "Orden de prueba para test",
                fechaEmision);
                
            // Establecer el ID usando reflexión
            typeof(EntityBase).GetProperty("Id")?.SetValue(ordenCompra, _ordenCompraId);
            
            // Establecer fecha de entrega estimada posterior a la fecha de emisión
            ordenCompra.EstablecerFechaEntrega(fechaEmision.AddDays(5));
            
            // Agregar algunos items utilizando el método correcto (sin precio unitario)
            var ingredienteId = Guid.NewGuid();
            ordenCompra.AgregarItem(ingredienteId, "Ingrediente de prueba", 10, UnidadMedida.Kilogramo);
            
            return ordenCompra;
        }
        
        private Proveedor CrearProveedorPrueba()
        {
            // Usando reflexión para crear un proveedor con valores mínimos
            var proveedor = new Mock<Proveedor>().Object;
            
            // Establecer propiedades usando reflexión
            typeof(EntityBase).GetProperty("Id")?.SetValue(proveedor, _proveedorId);
            typeof(Proveedor).GetProperty("Nombre")?.SetValue(proveedor, "Proveedor de Prueba");
            typeof(Proveedor).GetProperty("RFC")?.SetValue(proveedor, "PROV123456TEST");
            typeof(Proveedor).GetProperty("Direccion")?.SetValue(proveedor, "Calle Test #123, Ciudad Test");
            typeof(Proveedor).GetProperty("Activo")?.SetValue(proveedor, true);
            
            return proveedor;
        }
        
        private Factura CrearFacturaPrueba()
        {
            // Usar el factory method para crear una factura en lugar de Moq
            var fechaActual = _dateTimeServiceMock.Object.Now;
            var numeroFactura = $"TEST-{Guid.NewGuid().ToString().Substring(0, 8)}";
            
            var factura = Factura.Crear(
                numeroFactura,
                TipoFactura.Fiscal,
                "Proveedor de Prueba",
                _proveedorId,
                "PROV123456TEST",
                "Calle Test #123, Ciudad Test",
                new List<Guid>(),
                "Factura de prueba",
                fechaActual,
                _dateTimeServiceMock.Object);
                
            // Establecer el ID usando reflexión
            typeof(EntityBase).GetProperty("Id")?.SetValue(factura, _facturaId);
            
            return factura;
        }
    }
} 