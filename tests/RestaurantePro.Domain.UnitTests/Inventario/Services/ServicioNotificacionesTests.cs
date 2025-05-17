namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class ServicioNotificacionesTests
    {
        private readonly Mock<INotificacionRepository> _notificacionRepositoryMock;
        private readonly Mock<IProveedorRepository> _proveedorRepositoryMock;
        private readonly ServicioNotificaciones _servicio;

        public ServicioNotificacionesTests()
        {
            _notificacionRepositoryMock = new Mock<INotificacionRepository>();
            _proveedorRepositoryMock = new Mock<IProveedorRepository>();
            
            _servicio = new ServicioNotificaciones(
                _notificacionRepositoryMock.Object,
                _proveedorRepositoryMock.Object
            );
        }
        
        [Fact]
        public async Task NotificarStockBajo_DadosParametrosValidos_CreaYGuardaNotificacion()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Tomate";
            decimal stockActual = 2;
            decimal stockMinimo = 5;
            
            var notificacionCreada = new Notificacion();
            
            _notificacionRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Notificacion>()))
                .ReturnsAsync(notificacionCreada);
            
            // Act
            var resultado = await _servicio.NotificarStockBajo(ingredienteId, nombreIngrediente, stockActual, stockMinimo);
            
            // Assert
            Assert.NotEqual(Guid.Empty, resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.AddAsync(It.Is<Notificacion>(n => 
                    n.Tipo == TipoNotificacion.StockBajo && 
                    n.EntidadRelacionadaId == ingredienteId &&
                    n.Titulo.Contains(nombreIngrediente)
                )), 
                Times.Once);
        }
        
        [Fact]
        public async Task NotificarOrdenCompraGenerada_DadosParametrosValidos_CreaYGuardaNotificacion()
        {
            // Arrange
            var ordenCompraId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var nombreProveedor = "Proveedor Test";
            
            var notificacionCreada = new Notificacion();
            
            _notificacionRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Notificacion>()))
                .ReturnsAsync(notificacionCreada);
            
            // Act
            var resultado = await _servicio.NotificarOrdenCompraGenerada(ordenCompraId, proveedorId, nombreProveedor);
            
            // Assert
            Assert.NotEqual(Guid.Empty, resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.AddAsync(It.Is<Notificacion>(n => 
                    n.Tipo == TipoNotificacion.OrdenCompraGenerada && 
                    n.EntidadRelacionadaId == ordenCompraId &&
                    n.Titulo.Contains(nombreProveedor)
                )), 
                Times.Once);
        }
        
        [Fact]
        public async Task ObtenerNotificacionesPendientes_LlamaAlRepositorio()
        {
            // Arrange
            var destinatarioId = Guid.NewGuid();
            var notificacionesMock = new List<Notificacion> { new Notificacion() };
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerPendientesPorDestinatarioAsync(destinatarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificacionesMock);
                
            // Act
            var resultado = await _servicio.ObtenerNotificacionesPendientes(destinatarioId);
            
            // Assert
            Assert.Equal(notificacionesMock, resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.ObtenerPendientesPorDestinatarioAsync(destinatarioId, It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarNotificacionComoLeida_NotificacionExiste_MarcaComoLeida()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            var notificacion = Notificacion.Crear(
                "Test", 
                "Test mensaje", 
                TipoNotificacion.Informativa, 
                Guid.NewGuid(), 
                Guid.NewGuid());
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificacion);
                
            // Act
            var resultado = await _servicio.MarcarNotificacionComoLeida(notificacionId);
            
            // Assert
            Assert.True(resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Notificacion>(n => n == notificacion)), 
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarNotificacionComoLeida_NotificacionNoExiste_DevuelveFalse()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Notificacion)null);
                
            // Act
            var resultado = await _servicio.MarcarNotificacionComoLeida(notificacionId);
            
            // Assert
            Assert.False(resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Notificacion>()), 
                Times.Never);
        }
    }
} 