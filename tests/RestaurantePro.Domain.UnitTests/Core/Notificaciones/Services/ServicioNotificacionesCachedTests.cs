using Moq;
using Xunit;
using FluentAssertions;
using RestaurantePro.Domain.Core.Notificaciones.Services;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Domain.Core.Notificaciones.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.UnitTests.Core.Notificaciones.Services
{
    public class ServicioNotificacionesCachedTests
    {
        private readonly Mock<IServicioNotificaciones> _servicioOriginalMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly ServicioNotificacionesCached _servicioCached;
        private readonly Guid _destinatarioId = Guid.NewGuid();
        private readonly TipoNotificacion _tipoNotificacion = TipoNotificacion.StockBajo;
        private readonly string _titulo = "Prueba";
        private readonly string _mensaje = "Mensaje de prueba";
        
        public ServicioNotificacionesCachedTests()
        {
            _servicioOriginalMock = new Mock<IServicioNotificaciones>();
            _cacheServiceMock = new Mock<ICacheService>();
            _servicioCached = new ServicioNotificacionesCached(_servicioOriginalMock.Object, _cacheServiceMock.Object);
        }
        
        [Fact]
        public async Task ObtenerNotificacionesAsync_ShouldUseCache()
        {
            // Arrange
            bool soloNoLeidas = true;
            var notificacionesEsperadas = new List<Notificacion> 
            { 
                Notificacion.Crear(_titulo, _mensaje, _tipoNotificacion, _destinatarioId) 
            };
            var cacheKey = $"ServicioNotificaciones_ObtenerNotificaciones_{_destinatarioId}_{soloNoLeidas}";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<IEnumerable<Notificacion>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificacionesEsperadas);
                
            // Act
            var result = await _servicioCached.ObtenerNotificacionesAsync(_destinatarioId, soloNoLeidas);
            
            // Assert
            result.Should().BeSameAs(notificacionesEsperadas);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<IEnumerable<Notificacion>>>>(),
                    It.Is<int>(ttl => ttl == 30), // Verificamos que se use el tiempo de caché correcto
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task EnviarNotificacionAsync_ShouldInvalidateCache()
        {
            // Arrange
            var notificacionEsperada = Notificacion.Crear(_titulo, _mensaje, _tipoNotificacion, _destinatarioId);
            
            _servicioOriginalMock
                .Setup(s => s.EnviarNotificacionAsync(
                    _titulo, 
                    _mensaje, 
                    _tipoNotificacion, 
                    _destinatarioId, 
                    null, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificacionEsperada);
                
            // Act
            var result = await _servicioCached.EnviarNotificacionAsync(
                _titulo, 
                _mensaje, 
                _tipoNotificacion, 
                _destinatarioId);
            
            // Assert
            result.Should().BeSameAs(notificacionEsperada);
            _servicioOriginalMock.Verify(
                s => s.EnviarNotificacionAsync(
                    _titulo, 
                    _mensaje, 
                    _tipoNotificacion, 
                    _destinatarioId, 
                    null, 
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se invalida la caché para el destinatario
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p.Contains(_destinatarioId.ToString()))),
                Times.Once);
                
            // Verificar que se invalida la caché para el tipo de notificación
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p.Contains("ObtenerNotificaciones_"))),
                Times.Once);
        }
        
        [Fact]
        public async Task EnviarNotificacionMasivaAsync_ShouldInvalidateCache()
        {
            // Arrange
            var destinatarioIds = new List<Guid> { _destinatarioId, Guid.NewGuid() };
            var notificacionesEsperadas = new List<Notificacion> 
            {
                Notificacion.Crear(_titulo, _mensaje, _tipoNotificacion, _destinatarioId),
                Notificacion.Crear(_titulo, _mensaje, _tipoNotificacion, destinatarioIds[1])
            };
            
            _servicioOriginalMock
                .Setup(s => s.EnviarNotificacionMasivaAsync(
                    _titulo, 
                    _mensaje, 
                    _tipoNotificacion, 
                    destinatarioIds, 
                    null, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificacionesEsperadas);
                
            // Act
            var result = await _servicioCached.EnviarNotificacionMasivaAsync(
                _titulo, 
                _mensaje, 
                _tipoNotificacion, 
                destinatarioIds);
            
            // Assert
            result.Should().BeSameAs(notificacionesEsperadas);
            _servicioOriginalMock.Verify(
                s => s.EnviarNotificacionMasivaAsync(
                    _titulo, 
                    _mensaje, 
                    _tipoNotificacion, 
                    destinatarioIds, 
                    null, 
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se invalida la caché para cada destinatario
            foreach (var destinatarioId in destinatarioIds)
            {
                _cacheServiceMock.Verify(
                    s => s.InvalidatePattern(It.Is<string>(p => p.Contains(destinatarioId.ToString()))),
                    Times.Once);
            }
                
            // Verificar que se invalida la caché para el tipo de notificación
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p.Contains("ObtenerNotificaciones_"))),
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarComoLeidaAsync_ShouldInvalidateAllCache()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            
            _servicioOriginalMock
                .Setup(s => s.MarcarComoLeidaAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
                
            // Act
            var result = await _servicioCached.MarcarComoLeidaAsync(notificacionId);
            
            // Assert
            result.Should().BeTrue();
            _servicioOriginalMock.Verify(
                s => s.MarcarComoLeidaAsync(notificacionId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se invalida toda la caché
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern("ServicioNotificaciones_"),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCache_ShouldInvalidateAllCache()
        {
            // Arrange
            var cacheKeyPrefix = "ServicioNotificaciones_";
            
            // Act
            _servicioCached.InvalidarCache();
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPrefix)),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCachePorTipo_ShouldInvalidateTypeCache()
        {
            // Arrange
            var cacheKeyPattern = $"ServicioNotificaciones_ObtenerNotificaciones_";
            
            // Act
            _servicioCached.InvalidarCachePorTipo(_tipoNotificacion);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPattern)),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCachePorDestinatario_ShouldInvalidateRecipientCache()
        {
            // Arrange
            var cacheKeyPattern = $"ServicioNotificaciones_ObtenerNotificaciones_{_destinatarioId}_";
            
            // Act
            _servicioCached.InvalidarCachePorDestinatario(_destinatarioId);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPattern)),
                Times.Once);
        }
    }
} 