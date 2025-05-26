namespace RestaurantePro.Domain.UnitTests.Comercial.Services
{
    /// <summary>
    /// Pruebas unitarias para ServicioIntegracionProveedores
    /// </summary>
    public class ServicioIntegracionProveedoresTests
    {
        private readonly Mock<IOrdenCompraRepository> _ordenCompraRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly Mock<IServicioFacturacion> _servicioFacturacionMock;
        private readonly Mock<IFacturaRepository> _facturaRepositoryMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly NotificationManager _notificationManager;
        private readonly ServicioIntegracionProveedores _sut;

        public ServicioIntegracionProveedoresTests()
        {
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _servicioFacturacionMock = new Mock<IServicioFacturacion>();
            _facturaRepositoryMock = new Mock<IFacturaRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManager = new NotificationManager();
            
            _sut = new ServicioIntegracionProveedores(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _servicioFacturacionMock.Object,
                _facturaRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManager);
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobadaAsync_ConDatosValidos_DebeRetornarExito()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId);
            
            var fechaActual = DateTime.Now;
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                "Orden de prueba",
                "Observaciones de prueba",
                fechaActual,
                fechaActual.AddDays(7));
            ordenCompra.AgregarItem(Guid.NewGuid(), "Ingrediente 1", 10, 100);
            ordenCompra.AgregarItem(Guid.NewGuid(), "Ingrediente 2", 5, 200);
            
            var proveedor = Proveedor.Crear(
                "Proveedor de prueba",
                "12345678901",
                "Dirección de prueba",
                "123456789",
                "proveedor@example.com",
                ProveedorCategoria.Crear("Ingredientes"));
            
            var factura = Factura.Crear(
                "FAC-001",
                TipoFactura.Fiscal,
                proveedor.Nombre,
                proveedor.Id,
                proveedor.RFC,
                proveedor.Direccion,
                new List<Guid> { ordenCompraId },
                "Factura de prueba",
                fechaActual,
                _dateTimeServiceMock.Object);
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
            
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
            
            _proveedorRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();
            
            _proveedorRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobadaAsync_EventoNulo_DebeRetornarError()
        {
            // Arrange
            OrdenCompraAprobada evento = null;
            
            // Act
            var result = await _sut.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Should().Contain("no puede ser nulo");
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobadaAsync_OrdenCompraNoEncontrada_DebeRetornarError()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId);
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrdenCompra)null);
            
            // Act
            var result = await _sut.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Should().Contain("No se encontró la orden");
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobadaAsync_ProveedorNoEncontrado_DebeRetornarError()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId);
            
            var fechaActual = DateTime.Now;
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                "Orden de prueba",
                "Observaciones de prueba",
                fechaActual,
                fechaActual.AddDays(7));
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Proveedor)null);
            
            // Act
            var result = await _sut.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Should().Contain("No se encontró el proveedor");
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobadaAsync_ErrorAlCrearFactura_DebeRetornarError()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId);
            
            var fechaActual = DateTime.Now;
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                "Orden de prueba",
                "Observaciones de prueba",
                fechaActual,
                fechaActual.AddDays(7));
            
            var proveedor = Proveedor.Crear(
                "Proveedor de prueba",
                "12345678901",
                "Dirección de prueba",
                "123456789",
                "proveedor@example.com",
                ProveedorCategoria.Crear("Ingredientes"));
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
            
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
                .ReturnsAsync(Result.Failure<Factura>("Error al generar factura"));
            
            // Act
            var result = await _sut.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Value.Should().BeFalse();
            
            _proveedorRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
} 