namespace RestaurantePro.Domain.UnitTests.Core.Notificaciones.Services
{
    public class ServicioNotificacionesTests
    {
        private readonly Mock<INotificacionRepository> _notificacionRepositoryMock;
        private readonly ServicioNotificaciones _servicio;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public ServicioNotificacionesTests()
        {
            _notificacionRepositoryMock = new Mock<INotificacionRepository>();
            
            _servicio = new ServicioNotificaciones(
                _notificacionRepositoryMock.Object
            );
        }
        
        [Fact]
        public async Task EnviarNotificacionAsync_CreaYGuardaNotificacion()
        {
            // Arrange
            var titulo = "Test Title";
            var mensaje = "Test Message";
            var tipo = TipoNotificacion.Informativa;
            var destinatarioId = Guid.NewGuid();
            var entidadRelacionadaId = Guid.NewGuid();
            
            var notificacionCreada = Notificacion.Crear(
                titulo, 
                mensaje, 
                tipo,
                destinatarioId,
                entidadRelacionadaId);
            
            _notificacionRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(notificacionCreada));
            
            // Act
            var resultado = await _servicio.EnviarNotificacionAsync(
                titulo, 
                mensaje, 
                tipo, 
                destinatarioId, 
                entidadRelacionadaId, 
                _cancellationToken);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tipo, resultado.Tipo);
            
            _notificacionRepositoryMock.Verify(
                r => r.AgregarAsync(It.Is<Notificacion>(n => 
                    n.Tipo == tipo && 
                    n.Titulo == titulo &&
                    n.Mensaje == mensaje &&
                    n.DestinatarioId == destinatarioId &&
                    n.EntidadRelacionadaId == entidadRelacionadaId
                ), It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task EnviarNotificacionMasivaAsync_CreaYGuardaMultiplesNotificaciones()
        {
            // Arrange
            var titulo = "Test Title";
            var mensaje = "Test Message";
            var tipo = TipoNotificacion.Informativa;
            var destinatarioIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var entidadRelacionadaId = Guid.NewGuid();
            
            _notificacionRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((Notificacion)null));
            
            // Act
            var resultados = await _servicio.EnviarNotificacionMasivaAsync(
                titulo, 
                mensaje, 
                tipo, 
                destinatarioIds, 
                entidadRelacionadaId, 
                _cancellationToken);
            
            // Assert
            Assert.NotNull(resultados);
            Assert.Equal(destinatarioIds.Count, resultados.Count());
            
            _notificacionRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()), 
                Times.Exactly(destinatarioIds.Count));
        }
        
        [Fact]
        public async Task ObtenerNotificacionesAsync_ConSoloNoLeidas_LlamaAlRepositorioCorrectamente()
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
            
            _notificacionRepositoryMock
                .Setup(r => r.GetUnreadByRecipientIdAsync(destinatarioId, _cancellationToken))
                .ReturnsAsync(notificaciones);
                
            // Act
            var resultado = await _servicio.ObtenerNotificacionesAsync(destinatarioId, true, _cancellationToken);
            
            // Assert
            Assert.Equal(notificaciones, resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.GetUnreadByRecipientIdAsync(destinatarioId, _cancellationToken), 
                Times.Once);
                
            _notificacionRepositoryMock.Verify(
                r => r.GetByRecipientIdAsync(destinatarioId, _cancellationToken), 
                Times.Never);
        }
        
        [Fact]
        public async Task ObtenerNotificacionesAsync_ConTodasLasNotificaciones_LlamaAlRepositorioCorrectamente()
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
            
            _notificacionRepositoryMock
                .Setup(r => r.GetByRecipientIdAsync(destinatarioId, _cancellationToken))
                .ReturnsAsync(notificaciones);
                
            // Act
            var resultado = await _servicio.ObtenerNotificacionesAsync(destinatarioId, false, _cancellationToken);
            
            // Assert
            Assert.Equal(notificaciones, resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.GetByRecipientIdAsync(destinatarioId, _cancellationToken), 
                Times.Once);
                
            _notificacionRepositoryMock.Verify(
                r => r.GetUnreadByRecipientIdAsync(destinatarioId, _cancellationToken), 
                Times.Never);
        }
        
        [Fact]
        public async Task MarcarComoLeidaAsync_NotificacionExiste_MarcaComoLeida()
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
                .Setup(r => r.GetByIdAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificacion);
                
            // Act
            var resultado = await _servicio.MarcarComoLeidaAsync(notificacionId, _cancellationToken);
            
            // Assert
            Assert.True(resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Notificacion>(n => n == notificacion), It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarComoLeidaAsync_NotificacionNoExiste_DevuelveFalse()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            
            Notificacion? notificacionNull = null;
            _notificacionRepositoryMock
                .Setup(r => r.GetByIdAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificacionNull);
                
            // Act
            var resultado = await _servicio.MarcarComoLeidaAsync(notificacionId, _cancellationToken);
            
            // Assert
            Assert.False(resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }
    }
} 