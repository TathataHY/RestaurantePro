using Moq;
using RestaurantePro.Domain.Core.Services;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using System.Linq.Expressions;

namespace RestaurantePro.Domain.UnitTests.Core.Services
{
    public class CoreServiceFacadeTests
    {
        private readonly Mock<Core.Productos.Interfaces.IProductoRepository> _productoRepositoryMock;
        private readonly Mock<Core.Productos.Interfaces.IProductoCategoriaRepository> _productoCategoriaRepositoryMock;
        private readonly Mock<Core.Productos.Interfaces.IRecetaRepository> _recetaRepositoryMock;
        private readonly Mock<Core.Usuarios.Interfaces.IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<Core.Usuarios.Interfaces.IRolRepository> _rolRepositoryMock;
        private readonly Mock<Core.Notificaciones.Interfaces.INotificacionRepository> _notificacionRepositoryMock;
        private readonly Mock<Core.Productos.Services.IProductoCategoriaService> _productoCategoriaServiceMock;
        private readonly Mock<Core.Productos.Services.IRecetaService> _recetaServiceMock;
        private readonly Mock<IEventBasedNotificationService> _notificationServiceMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        
        private readonly CoreServiceFacade _sut; // System Under Test
        
        public CoreServiceFacadeTests()
        {
            _productoRepositoryMock = new Mock<Core.Productos.Interfaces.IProductoRepository>();
            _productoCategoriaRepositoryMock = new Mock<Core.Productos.Interfaces.IProductoCategoriaRepository>();
            _recetaRepositoryMock = new Mock<Core.Productos.Interfaces.IRecetaRepository>();
            _usuarioRepositoryMock = new Mock<Core.Usuarios.Interfaces.IUsuarioRepository>();
            _rolRepositoryMock = new Mock<Core.Usuarios.Interfaces.IRolRepository>();
            _notificacionRepositoryMock = new Mock<Core.Notificaciones.Interfaces.INotificacionRepository>();
            _productoCategoriaServiceMock = new Mock<Core.Productos.Services.IProductoCategoriaService>();
            _recetaServiceMock = new Mock<Core.Productos.Services.IRecetaService>();
            _notificationServiceMock = new Mock<IEventBasedNotificationService>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            
            _sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                _dateTimeServiceMock.Object);
        }
        
        #region Productos Tests
        
        [Fact]
        public async Task ObtenerProductoPorIdAsync_DebeRetornarProducto_CuandoExiste()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var productoEsperado = Producto.Crear(
                "Producto Test", 
                "Descripción Test", 
                PrecioProducto.Crear(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, CancellationToken.None))
                .ReturnsAsync(productoEsperado);
            
            // Act
            var resultado = await _sut.ObtenerProductoPorIdAsync(productoId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(productoEsperado, resultado);
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, CancellationToken.None), Times.Once);
        }
        
        [Fact]
        public async Task RegistrarProductoAsync_DebeCrearProducto_ConDatosValidos()
        {
            // Arrange
            var nombre = "Producto Test";
            var descripcion = "Descripción Test";
            var precio = 10.99m;
            var categoriaId = Guid.NewGuid();
            var categoriaNombre = "Categoría Test";
            
            var categoria = ProductoCategoria.Crear(categoriaNombre);
            
            _productoCategoriaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
                
            _productoCategoriaRepositoryMock
                .Setup(r => r.ObtenerNombreCategoriaAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoriaNombre);
            
            // Act
            var resultado = await _sut.RegistrarProductoAsync(nombre, descripcion, precio, categoriaId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(nombre, resultado.Nombre);
            Assert.Equal(descripcion, resultado.Descripcion);
            Assert.Equal(precio, resultado.Precio.Valor);
            Assert.Equal(categoriaId, resultado.CategoriaId);
            
            _productoRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Once);
            _productoRepositoryMock.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        #endregion
        
        #region Recetas Tests
        
        [Fact]
        public async Task VerificarDisponibilidadProductoAsync_DebeUsarRecetaService()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            
            _recetaServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            // Act
            var resultado = await _sut.VerificarDisponibilidadProductoAsync(productoId, cantidad);
            
            // Assert
            Assert.True(resultado);
            _recetaServiceMock.Verify(s => s.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ObtenerIngredientesFaltantesProductoAsync_DebeUsarRecetaService()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var ingredientesFaltantes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 1.5m },
                { Guid.NewGuid(), 2.0m }
            };
            
            _recetaServiceMock
                .Setup(s => s.ObtenerIngredientesFaltantesAsync(productoId, cantidad, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientesFaltantes);
            
            // Act
            var resultado = await _sut.ObtenerIngredientesFaltantesProductoAsync(productoId, cantidad);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(ingredientesFaltantes.Count, resultado.Count);
            _recetaServiceMock.Verify(s => s.ObtenerIngredientesFaltantesAsync(productoId, cantidad, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        #endregion
        
        #region Usuarios Tests
        
        [Fact]
        public async Task RegistrarUsuarioAsync_DebeCrearUsuario_ConDatosValidos()
        {
            // Arrange
            var nombreUsuario = "usuario_test";
            var nombre = "Usuario Test";
            var emailString = "usuario@test.com";
            var email = new Email(emailString);
            
            // Configurar mocks para verificar que no existe un usuario con el mismo nombre o email
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Core.Usuarios.Entities.Usuario)null);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorEmailAsync(emailString, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Core.Usuarios.Entities.Usuario)null);
            
            // Mock para el usuario creado
            var usuarioCreado = Core.Usuarios.Entities.Usuario.Crear(nombreUsuario, nombre, email);
            
            // Setup para métodos utilizados en la implementación
            _usuarioRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Core.Usuarios.Entities.Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.RegistrarUsuarioAsync(nombreUsuario, nombre, emailString);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(nombreUsuario, resultado.NombreUsuario);
            Assert.Equal(nombre, resultado.Nombre);
            Assert.Equal(emailString, resultado.Email.Value);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorEmailAsync(emailString, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Core.Usuarios.Entities.Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task RegistrarUsuarioAsync_DebeLanzarExcepcion_CuandoNombreUsuarioExiste()
        {
            // Arrange
            var nombreUsuario = "usuario_existente";
            var nombre = "Usuario Test";
            var emailString = "usuario@test.com";
            var email = new Email(emailString);
            
            var usuarioExistente = Core.Usuarios.Entities.Usuario.Crear(nombreUsuario, "Otro Usuario", email);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuarioExistente);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _sut.RegistrarUsuarioAsync(nombreUsuario, nombre, emailString));
            
            Assert.Contains("ya está en uso", exception.Message);
            Assert.Equal("nombreUsuario", exception.ParamName);
        }
        
        [Fact]
        public async Task ActualizarUsuarioAsync_DebeActualizarUsuario_ConDatosValidos()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var nombreOriginal = "Usuario Original";
            var nombreNuevo = "Usuario Actualizado";
            var emailOriginal = "original@test.com";
            var emailNuevo = "actualizado@test.com";
            
            var emailVO = new Email(emailOriginal);
            var usuario = Core.Usuarios.Entities.Usuario.Crear("usuario_test", nombreOriginal, emailVO);
            
            // Establecer ID manualmente para pruebas (normalmente lo hace EF Core)
            var propiedadId = usuario.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(usuario, usuarioId);
            }
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorEmailAsync(emailNuevo, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Core.Usuarios.Entities.Usuario)null);
            
            _usuarioRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Core.Usuarios.Entities.Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.ActualizarUsuarioAsync(usuarioId, nombreNuevo, emailNuevo, true);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(nombreNuevo, resultado.Nombre);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Core.Usuarios.Entities.Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task AsignarRolesUsuarioAsync_DebeAsignarRoles_CuandoUsuarioExiste()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var emailVO = new Email("usuario@test.com");
            var usuario = Core.Usuarios.Entities.Usuario.Crear("usuario_test", "Usuario Test", emailVO);
            
            // Establecer ID manualmente para pruebas
            var propiedadId = usuario.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(usuario, usuarioId);
            }
            
            var rolId1 = Guid.NewGuid();
            var rolId2 = Guid.NewGuid();
            var rolesIds = new List<Guid> { rolId1, rolId2 };
            
            var rol1 = new Core.Usuarios.Entities.Rol("Rol1", "Descripción Rol 1");
            var rol2 = new Core.Usuarios.Entities.Rol("Rol2", "Descripción Rol 2");
            
            // Establecer IDs manualmente para pruebas
            var propiedadRolId = rol1.GetType().GetProperty("Id");
            if (propiedadRolId != null && propiedadRolId.CanWrite)
            {
                propiedadRolId.SetValue(rol1, rolId1);
                propiedadRolId.SetValue(rol2, rolId2);
            }
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            _rolRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(rolId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rol1);
            
            _rolRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(rolId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rol2);
            
            // Act
            var resultado = await _sut.AsignarRolesUsuarioAsync(usuarioId, rolesIds);
            
            // Assert
            Assert.True(resultado);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _rolRepositoryMock.Verify(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Core.Usuarios.Entities.Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task AsignarRolesUsuarioAsync_DebeRetornarFalse_CuandoUsuarioNoExiste()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var rolesIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Core.Usuarios.Entities.Usuario)null);
            
            // Act
            var resultado = await _sut.AsignarRolesUsuarioAsync(usuarioId, rolesIds);
            
            // Assert
            Assert.False(resultado);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _rolRepositoryMock.Verify(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Core.Usuarios.Entities.Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        #endregion
        
        #region Notificaciones Tests
        
        [Fact]
        public async Task EnviarNotificacionAsync_DebeCrearYPersistirNotificacion()
        {
            // Arrange
            var destinatarioId = Guid.NewGuid();
            var tipo = "Informativa";
            var titulo = "Título Test";
            var mensaje = "Mensaje Test";
            var datos = "Datos Test";
            var prioridad = 1;
            
            var tipoNotificacion = Core.Notificaciones.Enums.TipoNotificacion.Informativa;
            
            // Configurar mock para el repositorio de notificaciones
            _notificacionRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Configurar mock para el servicio de notificaciones
            _notificationServiceMock
                .Setup(s => s.EnviarNotificacionAUsuarioAsync(
                    destinatarioId,
                    titulo,
                    mensaje,
                    tipoNotificacion,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.EnviarNotificacionAsync(destinatarioId, tipo, titulo, mensaje, datos, prioridad);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(destinatarioId, resultado.DestinatarioId);
            Assert.Equal(titulo, resultado.Titulo);
            Assert.Equal(mensaje, resultado.Mensaje);
            
            _notificacionRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()), 
                Times.Once);
            
            _notificationServiceMock.Verify(
                s => s.EnviarNotificacionAUsuarioAsync(
                    destinatarioId,
                    titulo,
                    mensaje,
                    tipoNotificacion,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarNotificacionComoLeidaAsync_DebeMarcarComoLeida_CuandoNotificacionExiste()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            var notificacion = Notificacion.Crear(
                "Título Test",
                "Mensaje Test",
                Core.Notificaciones.Enums.TipoNotificacion.Informativa,
                Guid.NewGuid());
            
            // Establecer ID manualmente para pruebas
            var propiedadId = notificacion.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(notificacion, notificacionId);
            }
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificacion);
            
            _notificacionRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.MarcarNotificacionComoLeidaAsync(notificacionId);
            
            // Assert
            Assert.True(resultado);
            Assert.True(notificacion.EstaLeida);
            
            _notificacionRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()), 
                Times.Once);
            
            _notificacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarNotificacionComoLeidaAsync_DebeRetornarFalse_CuandoNotificacionNoExiste()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Notificacion)null);
            
            // Act
            var resultado = await _sut.MarcarNotificacionComoLeidaAsync(notificacionId);
            
            // Assert
            Assert.False(resultado);
            
            _notificacionRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()), 
                Times.Once);
            
            _notificacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task ObtenerNotificacionesUsuarioAsync_DebeRetornarNotificacionesDeUsuario()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var otroUsuarioId = Guid.NewGuid();
            
            // Crear algunas notificaciones de prueba
            var notificacionesUsuario = new List<Notificacion>
            {
                Notificacion.Crear("Título 1", "Mensaje 1", Core.Notificaciones.Enums.TipoNotificacion.Informativa, usuarioId),
                Notificacion.Crear("Título 2", "Mensaje 2", Core.Notificaciones.Enums.TipoNotificacion.Advertencia, usuarioId)
            };
            
            var notificacionesOtroUsuario = new List<Notificacion>
            {
                Notificacion.Crear("Título 3", "Mensaje 3", Core.Notificaciones.Enums.TipoNotificacion.Error, otroUsuarioId)
            };
            
            var todasNotificaciones = notificacionesUsuario.Concat(notificacionesOtroUsuario).ToList();
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(todasNotificaciones);
            
            // Act
            var resultado = await _sut.ObtenerNotificacionesUsuarioAsync(usuarioId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
            Assert.All(resultado, n => Assert.Equal(usuarioId, n.DestinatarioId));
            
            _notificacionRepositoryMock.Verify(
                r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        #endregion
    }
} 