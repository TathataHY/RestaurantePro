#nullable disable
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
        private readonly RestaurantePro.Domain.Core.SharedKernel.Validation.NotificationManager _notificationManager;
        private readonly ServicioIntegracionProveedores _sut;

        public ServicioIntegracionProveedoresTests()
        {
            _ordenCompraRepositoryMock = new Mock<IOrdenCompraRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            _servicioFacturacionMock = new Mock<IServicioFacturacion>();
            _facturaRepositoryMock = new Mock<IFacturaRepository>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManager = new RestaurantePro.Domain.Core.SharedKernel.Validation.NotificationManager();
            
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
            var fechaActual = DateTime.Now;
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId, fechaActual, 1500m);
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            var items = new List<ItemOrdenCompra>();
            items.Add(ItemOrdenCompra.Crear(ordenCompraId, Guid.NewGuid(), "Ingrediente 1", 10, UnidadMedida.Kilogramo));
            
            var notificationManager = new NotificationManager();
            var ordenCompraResult = OrdenCompra.Crear(
                proveedorId,
                items,
                fechaActual,
                fechaActual.AddDays(7),
                "Orden de prueba",
                notificationManager);
                
            var ordenCompra = ordenCompraResult.Value;
            
            // Configurar para evitar problemas con Reflection al establecer manualmente el Id
            var propiedadId = ordenCompra.GetType().GetProperty("Id", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
                
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(ordenCompra, ordenCompraId);
            }
            
            var proveedor = Proveedor.Crear(
                "Proveedor de prueba",
                "Contacto Test",
                "proveedor@example.com",
                "123456789",
                "Dirección de prueba",
                "Ciudad Test",
                "12345",
                "País Test",
                "12345678901",
                "Cuenta: 123456789",
                30);
            
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
            
            // Crear una instancia específica para esta prueba con un NotificationManager limpio
            var sut = new ServicioIntegracionProveedores(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _servicioFacturacionMock.Object,
                _facturaRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                new NotificationManager());
            
            // Act
            var result = await sut.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            
            _proveedorRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobadaAsync_EventoNulo_DebeRetornarError()
        {
            // Arrange
            var evento = new OrdenCompraAprobada(Guid.Empty, Guid.Empty, DateTime.MinValue, 0);
            
            // Act
            var result = await _sut.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobadaAsync_OrdenCompraNoEncontrada_DebeRetornarError()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var fechaActual = DateTime.Now;
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId, fechaActual, 1500m);
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrdenCompra?)null);
            
            // Act
            var result = await _sut.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            
            _ordenCompraRepositoryMock.Verify(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobadaAsync_ProveedorNoEncontrado_DebeRetornarError()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var fechaActual = DateTime.Now;
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId, fechaActual, 1500m);
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            var items = new List<ItemOrdenCompra>();
            items.Add(ItemOrdenCompra.Crear(ordenCompraId, Guid.NewGuid(), "Ingrediente 1", 10, UnidadMedida.Kilogramo));
            
            var ordenCompraResult = OrdenCompra.Crear(
                proveedorId,
                items,
                fechaActual,
                fechaActual.AddDays(7),
                "Orden de prueba",
                _notificationManager);
                
            var ordenCompra = ordenCompraResult.Value;
            
            _ordenCompraRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenCompra);
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Proveedor?)null);
            
            // Act
            var result = await _sut.ProcesarOrdenCompraAprobadaAsync(evento);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            
            _ordenCompraRepositoryMock.Verify(r => r.ObtenerPorIdAsync(ordenCompraId, It.IsAny<CancellationToken>()), Times.Once);
            _proveedorRepositoryMock.Verify(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ProcesarOrdenCompraAprobadaAsync_ErrorAlCrearFactura_DebeRetornarError()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var fechaActual = DateTime.Now;
            var evento = new OrdenCompraAprobada(ordenCompraId, proveedorId, fechaActual, 1500m);
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            var items = new List<ItemOrdenCompra>();
            items.Add(ItemOrdenCompra.Crear(ordenCompraId, Guid.NewGuid(), "Ingrediente 1", 10, UnidadMedida.Kilogramo));
            
            var ordenCompraResult = OrdenCompra.Crear(
                proveedorId,
                items,
                fechaActual,
                fechaActual.AddDays(7),
                "Orden de prueba",
                _notificationManager);
                
            var ordenCompra = ordenCompraResult.Value;
            
            var proveedor = Proveedor.Crear(
                "Proveedor de prueba",
                "Contacto Test",
                "proveedor@example.com",
                "123456789",
                "Dirección de prueba",
                "Ciudad Test",
                "12345",
                "País Test",
                "12345678901",
                "Cuenta: 123456789",
                30);
            
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
            
            _proveedorRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Proveedor>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        #region SincronizarInformacionProveedorAsync Tests
        
        [Fact]
        public async Task SincronizarInformacionProveedorAsync_ConProveedorValido_DebeRetornarExito()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var fechaActual = DateTime.Now;
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            var proveedor = Proveedor.Crear(
                "Proveedor de prueba",
                "Contacto Test",
                "proveedor@example.com",
                "123456789",
                "Dirección de prueba",
                "Ciudad Test",
                "12345",
                "País Test",
                "12345678901",
                "Cuenta: 123456789",
                30);
                
            // Establecer ID del proveedor para pruebas
            var propiedadId = proveedor.GetType().GetProperty("Id", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
                
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(proveedor, proveedorId);
            }
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            // Simular que no hay facturas pendientes
            _facturaRepositoryMock
                .Setup(r => r.ObtenerFacturasPendientesPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Factura>());
            
            // Act
            var result = await _sut.SincronizarInformacionProveedorAsync(proveedorId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            
            _proveedorRepositoryMock.Verify(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()), Times.Once);
            _facturaRepositoryMock.Verify(r => r.ObtenerFacturasPendientesPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task SincronizarInformacionProveedorAsync_ConProveedorNoExistente_DebeRetornarError()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Proveedor)null);
            
            // Act
            var result = await _sut.SincronizarInformacionProveedorAsync(proveedorId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("No se encontró el proveedor"));
            
            _proveedorRepositoryMock.Verify(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()), Times.Once);
            _facturaRepositoryMock.Verify(r => r.ObtenerFacturasPendientesPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task SincronizarInformacionProveedorAsync_ConFacturasPendientes_DebeAgregarInformacion()
        {
            // Arrange
            var proveedorId = Guid.NewGuid();
            var fechaActual = DateTime.Now;
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(fechaActual);
            
            var proveedor = Proveedor.Crear(
                "Proveedor de prueba",
                "Contacto Test",
                "proveedor@example.com",
                "123456789",
                "Dirección de prueba",
                "Ciudad Test",
                "12345",
                "País Test",
                "12345678901",
                "Cuenta: 123456789",
                30);
                
            // Establecer ID del proveedor para pruebas
            var propiedadId = proveedor.GetType().GetProperty("Id", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
                
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(proveedor, proveedorId);
            }
            
            _proveedorRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(proveedor);
                
            // Simular que hay facturas pendientes
            var facturasPendientes = new List<Factura>
            {
                Factura.Crear(
                    "FAC-001",
                    TipoFactura.Fiscal,
                    proveedor.Nombre,
                    proveedor.Id,
                    proveedor.RFC,
                    proveedor.Direccion,
                    new List<Guid> { Guid.NewGuid() },
                    "Factura pendiente 1",
                    fechaActual.AddDays(-10),
                    _dateTimeServiceMock.Object),
                    
                Factura.Crear(
                    "FAC-002",
                    TipoFactura.Fiscal,
                    proveedor.Nombre,
                    proveedor.Id,
                    proveedor.RFC,
                    proveedor.Direccion,
                    new List<Guid> { Guid.NewGuid() },
                    "Factura pendiente 2",
                    fechaActual.AddDays(-5),
                    _dateTimeServiceMock.Object)
            };
            
            _facturaRepositoryMock
                .Setup(r => r.ObtenerFacturasPendientesPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(facturasPendientes);
            
            // Crear un NotificationManager específico para esta prueba que podamos verificar
            var notificationManager = new RestaurantePro.Domain.Core.SharedKernel.Validation.NotificationManager();
            
            var sut = new ServicioIntegracionProveedores(
                _ordenCompraRepositoryMock.Object,
                _proveedorRepositoryMock.Object,
                _servicioFacturacionMock.Object,
                _facturaRepositoryMock.Object,
                _dateTimeServiceMock.Object,
                notificationManager);
            
            // Act
            var result = await sut.SincronizarInformacionProveedorAsync(proveedorId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            
            // Verificar que se agregó información sobre las facturas pendientes
            notificationManager.HasErrors.Should().BeFalse();
            _proveedorRepositoryMock.Verify(r => r.ObtenerPorIdAsync(proveedorId, It.IsAny<CancellationToken>()), Times.Once);
            _facturaRepositoryMock.Verify(r => r.ObtenerFacturasPendientesPorProveedorAsync(proveedorId, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        #endregion
    }
} 