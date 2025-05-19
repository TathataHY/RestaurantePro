namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class ServicioNotificacionesInventarioTests
    {
        private readonly Mock<RestaurantePro.Domain.Core.Notificaciones.Services.IServicioNotificaciones> _coreServicioMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly IServicioNotificacionesInventario _servicio;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public ServicioNotificacionesInventarioTests()
        {
            _coreServicioMock = new Mock<RestaurantePro.Domain.Core.Notificaciones.Services.IServicioNotificaciones>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            
            _servicio = new ServicioNotificacionesInventario(
                _coreServicioMock.Object,
                _proveedorRepositoryMock.Object
            );
        }
        
        [Fact]
        public async Task NotificarStockBajo_DadosParametrosValidos_EnviaNotificacionMasiva()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Tomate";
            decimal stockActual = 2;
            decimal stockMinimo = 5;
            
            var notificacionId = Guid.NewGuid();
            var notificaciones = new List<Notificacion> { 
                Notificacion.Crear(
                    "Stock bajo: Tomate", 
                    "El ingrediente Tomate tiene un stock actual de 2 unidades, por debajo del mínimo recomendado (5 unidades).", 
                    TipoNotificacion.StockBajo,
                    Guid.NewGuid(),
                    ingredienteId)
            };
            
            _coreServicioMock
                .Setup(r => r.EnviarNotificacionMasivaAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    TipoNotificacion.StockBajo,
                    It.IsAny<IEnumerable<Guid>>(),
                    ingredienteId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificaciones);
            
            // Act
            var resultado = await _servicio.NotificarStockBajo(ingredienteId, nombreIngrediente, stockActual, stockMinimo);
            
            // Assert
            Assert.NotEqual(Guid.Empty, resultado);
            
            _coreServicioMock.Verify(
                r => r.EnviarNotificacionMasivaAsync(
                    It.Is<string>(s => s.Contains(nombreIngrediente)),
                    It.Is<string>(s => s.Contains(nombreIngrediente) && 
                                      s.Contains(stockActual.ToString()) && 
                                      s.Contains(stockMinimo.ToString())),
                    TipoNotificacion.StockBajo,
                    It.IsAny<IEnumerable<Guid>>(),
                    ingredienteId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task NotificarOrdenCompraGenerada_DadosParametrosValidos_EnviaNotificacionMasiva()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var nombreProveedor = "Proveedor Test";
            
            var notificaciones = new List<Notificacion> { 
                Notificacion.Crear(
                    "Orden de compra generada: Proveedor Test", 
                    "Se ha generado automáticamente una orden de compra para el proveedor Proveedor Test debido a stock bajo.", 
                    TipoNotificacion.OrdenCompraGenerada,
                    Guid.NewGuid(),
                    ordenCompraId)
            };
            
            _coreServicioMock
                .Setup(r => r.EnviarNotificacionMasivaAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    TipoNotificacion.OrdenCompraGenerada,
                    It.IsAny<IEnumerable<Guid>>(),
                    ordenCompraId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificaciones);
            
            // Act
            var resultado = await _servicio.NotificarOrdenCompraGenerada(ordenCompraId, proveedorId, nombreProveedor);
            
            // Assert
            Assert.NotEqual(Guid.Empty, resultado);
            
            _coreServicioMock.Verify(
                r => r.EnviarNotificacionMasivaAsync(
                    It.Is<string>(s => s.Contains(nombreProveedor)),
                    It.Is<string>(s => s.Contains(nombreProveedor)),
                    TipoNotificacion.OrdenCompraGenerada,
                    It.IsAny<IEnumerable<Guid>>(),
                    ordenCompraId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ObtenerNotificacionesPendientes_LlamaAlServicioCore()
        {
            // Arrange
            var destinatarioId = Guid.NewGuid();
            var notificaciones = new List<Notificacion> { 
                Notificacion.Crear(
                    "Test", 
                    "Mensaje de prueba", 
                    TipoNotificacion.Informativa,
                    destinatarioId,
                    Guid.NewGuid())
            };
            
            _coreServicioMock
                .Setup(r => r.ObtenerNotificacionesAsync(destinatarioId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificaciones);
                
            // Act
            var resultado = await _servicio.ObtenerNotificacionesPendientes(destinatarioId);
            
            // Assert
            Assert.Equal(notificaciones, resultado);
            
            _coreServicioMock.Verify(
                r => r.ObtenerNotificacionesAsync(destinatarioId, true, It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarNotificacionComoLeida_LlamaAlServicioCore()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            
            _coreServicioMock
                .Setup(r => r.MarcarComoLeidaAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
                
            // Act
            var resultado = await _servicio.MarcarNotificacionComoLeida(notificacionId);
            
            // Assert
            Assert.True(resultado);
            
            _coreServicioMock.Verify(
                r => r.MarcarComoLeidaAsync(notificacionId, It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task EnviarNotificacionAsync_LlamaAlServicioCore()
        {
            // Arrange
            var destinatarioId = Guid.NewGuid();
            var titulo = "Test Title";
            var mensaje = "Test Message";
            
            // Act
            await _servicio.EnviarNotificacionAsync(destinatarioId, titulo, mensaje, _cancellationToken);
            
            // Assert
            _coreServicioMock.Verify(
                r => r.EnviarNotificacionAsync(
                    titulo,
                    mensaje,
                    TipoNotificacion.Personalizada,
                    destinatarioId,
                    null,
                    _cancellationToken), 
                Times.Once);
        }
        
        [Fact]
        public async Task EnviarNotificacionMasivaAsync_LlamaAlServicioCore()
        {
            // Arrange
            var destinatariosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var titulo = "Test Title";
            var mensaje = "Test Message";
            
            // Act
            await _servicio.EnviarNotificacionMasivaAsync(destinatariosIds, titulo, mensaje, _cancellationToken);
            
            // Assert
            _coreServicioMock.Verify(
                r => r.EnviarNotificacionMasivaAsync(
                    titulo,
                    mensaje,
                    TipoNotificacion.Personalizada,
                    destinatariosIds,
                    null,
                    _cancellationToken), 
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarComoLeidaAsync_LlamaAlServicioCore()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            
            // Act
            await _servicio.MarcarComoLeidaAsync(notificacionId, _cancellationToken);
            
            // Assert
            _coreServicioMock.Verify(
                r => r.MarcarComoLeidaAsync(notificacionId, _cancellationToken), 
                Times.Once);
        }
    }
} 