namespace RestaurantePro.Domain.UnitTests.Core.Usuarios.Services
{
    public class UsuarioServiceCachedTests
    {
        private readonly Mock<IUsuarioService> _mockUsuarioService;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly UsuarioServiceCached _usuarioServiceCached;
        
        public UsuarioServiceCachedTests()
        {
            _mockUsuarioService = new Mock<IUsuarioService>();
            _mockCacheService = new Mock<ICacheService>();
            _usuarioServiceCached = new UsuarioServiceCached(_mockUsuarioService.Object, _mockCacheService.Object);
        }
        
        [Fact]
        public async Task ObtenerPorIdAsync_DebeUsarCache()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var usuario = CrearUsuarioEjemplo(usuarioId);
            
            _mockCacheService
                .Setup(s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Usuario?>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            // Act
            var resultado = await _usuarioServiceCached.ObtenerPorIdAsync(usuarioId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(usuarioId);
            
            _mockCacheService.Verify(s => s.GetOrAddAsync(
                It.Is<string>(key => key.Contains(usuarioId.ToString())),
                It.IsAny<Func<CancellationToken, Task<Usuario?>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CrearUsuarioAsync_DebeInvalidarCache()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var usuario = CrearUsuarioEjemplo(usuarioId);
            
            _mockUsuarioService
                .Setup(s => s.CrearUsuarioAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<RolUsuario>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            // Act
            var resultado = await _usuarioServiceCached.CrearUsuarioAsync(
                "usuario", "Usuario Ejemplo", "usuario@ejemplo.com", RolUsuario.Cajero);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(usuarioId);
            
            _mockCacheService.Verify(s => s.InvalidatePattern(It.IsAny<string>()), Times.Once);
        }
        
        [Fact]
        public async Task ActualizarUsuarioAsync_DebeInvalidarCacheEspecifica()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var usuario = CrearUsuarioEjemplo(usuarioId);
            
            _mockUsuarioService
                .Setup(s => s.ActualizarUsuarioAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            // Act
            var resultado = await _usuarioServiceCached.ActualizarUsuarioAsync(
                usuarioId, "Nuevo Nombre", null);
            
            // Assert
            resultado.Should().NotBeNull();
            
            _mockCacheService.Verify(s => s.Remove(It.IsAny<string>()), Times.Once);
        }
        
        [Fact]
        public async Task ActivarUsuarioAsync_DebeInvalidarCacheEspecificaYListas()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            
            _mockUsuarioService
                .Setup(s => s.ActivarUsuarioAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            // Act
            var resultado = await _usuarioServiceCached.ActivarUsuarioAsync(usuarioId);
            
            // Assert
            resultado.Should().BeTrue();
            
            _mockCacheService.Verify(s => s.Remove(It.IsAny<string>()), Times.Once);
            _mockCacheService.Verify(s => s.InvalidatePattern(It.IsAny<string>()), Times.Once);
        }
        
        [Fact]
        public async Task ObtenerPorRolAsync_DebeUsarCache()
        {
            // Arrange
            var usuarios = new List<Usuario>
            {
                CrearUsuarioEjemplo(Guid.NewGuid()),
                CrearUsuarioEjemplo(Guid.NewGuid())
            };
            
            _mockCacheService
                .Setup(s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<IEnumerable<Usuario>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuarios);
            
            // Act
            var resultado = await _usuarioServiceCached.ObtenerPorRolAsync(RolUsuario.Administrador);
            
            // Assert
            resultado.Should().HaveCount(2);
            
            _mockCacheService.Verify(s => s.GetOrAddAsync(
                It.Is<string>(key => key.Contains("Usuarios_Rol_")),
                It.IsAny<Func<CancellationToken, Task<IEnumerable<Usuario>>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ExisteNombreUsuarioAsync_DebeUsarCache()
        {
            // Arrange
            const string nombreUsuario = "usuario_test";
            
            _mockCacheService
                .Setup(s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<bool>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            // Act
            var resultado = await _usuarioServiceCached.ExisteNombreUsuarioAsync(nombreUsuario);
            
            // Assert
            resultado.Should().BeTrue();
            
            _mockCacheService.Verify(s => s.GetOrAddAsync(
                It.Is<string>(key => key.Contains(nombreUsuario)),
                It.IsAny<Func<CancellationToken, Task<bool>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public void InvalidarCacheUsuario_DebeEliminarCacheEspecifica()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            
            // Act
            _usuarioServiceCached.InvalidarCacheUsuario(usuarioId);
            
            // Assert
            _mockCacheService.Verify(s => s.Remove(It.Is<string>(key => key.Contains(usuarioId.ToString()))), Times.Once);
        }
        
        [Fact]
        public void InvalidarCacheUsuarios_DebeEliminarTodasLasCachesDeUsuarios()
        {
            // Act
            _usuarioServiceCached.InvalidarCacheUsuarios();
            
            // Assert
            _mockCacheService.Verify(s => s.InvalidatePattern("Usuario_"), Times.Once);
        }
        
        [Fact]
        public void ObtenerEstadisticasCache_DebeRetornarDiccionarioConEstadisticas()
        {
            // Arrange
            var mockTelemetry = new Mock<ICacheTelemetry>();
            var cacheMetrics = new CacheMetrics
            {
                TotalAccesses = 100,
                TotalHits = 75,
                AverageAccessTimeMs = 5,
                RecentInvalidations = 10
            };
            mockTelemetry.Setup(t => t.GetMetrics()).Returns(cacheMetrics);

            var mockCacheServiceWithTelemetry = mockTelemetry.As<ICacheService>();

            var serviceCached = new UsuarioServiceCached(_mockUsuarioService.Object, mockCacheServiceWithTelemetry.Object);
            
            // Act
            var estadisticas = serviceCached.ObtenerEstadisticasCache();
            
            // Assert
            estadisticas.Should().ContainKey("TotalAccesos");
            estadisticas.Should().ContainKey("TotalAciertos");
            estadisticas.Should().ContainKey("TasaAciertos");
            estadisticas.Should().ContainKey("TiempoPromedioAcceso");
            
            estadisticas["TotalAccesos"].Should().Be(100);
            estadisticas["TotalAciertos"].Should().Be(75);
            estadisticas["TasaAciertos"].Should().Be(0.75);
        }
        
        private Usuario CrearUsuarioEjemplo(Guid id)
        {
            var usuario = Usuario.Crear("usuario", "Usuario Ejemplo", "usuario@ejemplo.com", RolUsuario.Cajero);
            
            // Acceder a la propiedad Id utilizando reflection para establecer un ID específico
            var propInfo = typeof(Usuario).GetProperty("Id");
            if (propInfo != null && propInfo.CanWrite)
            {
                propInfo.SetValue(usuario, id);
            }
            
            return usuario;
        }
    }
} 