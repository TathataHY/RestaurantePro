namespace RestaurantePro.Domain.UnitTests.Core.Services
{
    public class CoreServiceFacadeTests
    {
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IProductoCategoriaRepository> _productoCategoriaRepositoryMock;
        private readonly Mock<IRecetaRepository> _recetaRepositoryMock;
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<IRolRepository> _rolRepositoryMock;
        private readonly Mock<INotificacionRepository> _notificacionRepositoryMock;
        private readonly Mock<IProductoCategoriaService> _productoCategoriaServiceMock;
        private readonly Mock<IRecetaService> _recetaServiceMock;
        private readonly Mock<IEventBasedNotificationService> _notificationServiceMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly INotificationManager _notificationManager;
        
        private readonly CoreServiceFacade _sut; // System Under Test
        
        public CoreServiceFacadeTests()
        {
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _productoCategoriaRepositoryMock = new Mock<IProductoCategoriaRepository>();
            _recetaRepositoryMock = new Mock<IRecetaRepository>();
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _rolRepositoryMock = new Mock<IRolRepository>();
            _notificacionRepositoryMock = new Mock<INotificacionRepository>();
            _productoCategoriaServiceMock = new Mock<IProductoCategoriaService>();
            _recetaServiceMock = new Mock<IRecetaService>();
            _notificationServiceMock = new Mock<IEventBasedNotificationService>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManagerMock = new Mock<INotificationManager>();
            
            // Usamos una implementación real para evitar problemas con los mocks
            _notificationManager = new NotificationManager();
            
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
                _dateTimeServiceMock.Object,
                _notificationManager);
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
                new PrecioProducto(10.99m), 
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
            
            var categoria = ProductoCategoria.Crear(categoriaNombre, "Descripción de categoría", 1);
            
            _productoCategoriaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
            
            // Act
            var resultado = await _sut.RegistrarProductoAsync(nombre, descripcion, precio, categoriaId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(nombre, resultado.Value.Nombre);
            Assert.Equal(descripcion, resultado.Value.Descripcion);
            Assert.Equal(precio, resultado.Value.Precio.Valor);
            Assert.Equal(categoriaId, resultado.Value.CategoriaId);
            
            _productoRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Once);
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
                .ReturnsAsync(Result.Success(true));
            
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
                .ReturnsAsync(Result.Success(ingredientesFaltantes));
            
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
        public async Task CrearUsuarioAsync_DebeCrearUsuario_ConDatosValidos()
        {
            // Arrange
            var nombreUsuario = "usuario_test";
            var nombre = "Usuario Test";
            var emailString = "usuario@test.com";
            var email = Email.Create(emailString);
            
            // Configurar mocks para verificar que no existe un usuario con el mismo nombre o email
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario)null);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorEmailAsync(emailString, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario)null);
            
            // Mock para el usuario creado
            var usuarioCreado = Usuario.Crear(nombreUsuario, nombre, email, RolUsuario.Cajero);
            
            // Setup para métodos utilizados en la implementación
            _usuarioRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.CrearUsuarioAsync(nombreUsuario, nombre, emailString, "Cajero");
            
            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(nombreUsuario, resultado.Value.NombreUsuario);
            Assert.Equal(nombre, resultado.Value.NombreCompleto);
            Assert.Equal(emailString, resultado.Value.Email);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CrearUsuarioAsync_DebeRetornarError_CuandoNombreUsuarioExiste()
        {
            // Arrange
            var nombreUsuario = "usuario_existente";
            var nombre = "Usuario Test";
            var emailString = "usuario@test.com";
            var email = Email.Create(emailString);
            
            var usuarioExistente = Usuario.Crear(nombreUsuario, "Otro Usuario", email, RolUsuario.Cajero);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuarioExistente);
            
            // Crear un NotificationManager real específico para esta prueba
            var notificationManager = new NotificationManager();
            
            // Crear una instancia específica de CoreServiceFacade para esta prueba
            var sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                _dateTimeServiceMock.Object,
                notificationManager);
            
            // Act
            var resultado = await sut.CrearUsuarioAsync(nombreUsuario, nombre, emailString, "Cajero");
            
            // Assert
            Console.WriteLine($"Resultado: {resultado?.Succeeded}");
            Assert.NotNull(resultado);
            Assert.False(resultado.Succeeded);
            
            // Verificar que se llamó al repositorio para comprobar si existe el usuario
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()), Times.Once);
            // Verificar que no se llamó al método AgregarAsync
            _usuarioRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task ActualizarNombreUsuarioAsync_DebeActualizarUsuario_ConDatosValidos()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var nombreOriginal = "Usuario Original";
            var nombreNuevo = "Usuario Actualizado";
            var emailOriginal = "original@test.com";
            
            var usuario = Usuario.Crear("usuario_test", nombreOriginal, emailOriginal, RolUsuario.Cajero);
            
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
                .Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.ActualizarNombreUsuarioAsync(usuarioId, nombreNuevo);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(nombreNuevo, resultado.Value.NombreCompleto);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task AsignarRolUsuarioAsync_DebeAsignarRoles_CuandoUsuarioExiste()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var emailVO = Email.Create("usuario@test.com");
            var usuario = Usuario.Crear("usuario_test", "Usuario Test", emailVO, RolUsuario.Cajero);
            
            // Establecer ID manualmente para pruebas
            var propiedadId = usuario.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(usuario, usuarioId);
            }
            
            var rolId1 = Guid.NewGuid();
            var rol = "Administrador";
            
            // Usar el método factory de Rol
            var rol1 = Rol.Crear("Rol1", "Descripción Rol 1", TipoUsuario.Administrador);
            
            // Establecer IDs manualmente para pruebas
            typeof(EntityBase).GetProperty("Id")!.SetValue(rol1, rolId1);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            _rolRepositoryMock
                .Setup(r => r.ObtenerPorTipoUsuarioAsync(TipoUsuario.Administrador, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rol1);
            
            // Act
            var resultado = await _sut.AsignarRolUsuarioAsync(usuarioId, rol);
            
            // Assert
            Assert.NotNull(resultado);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task AsignarRolUsuarioAsync_DebeRetornarFalse_CuandoUsuarioNoExiste()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var rol = "Administrador";
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario)null);
            
            // Crear un NotificationManager real específico para esta prueba
            var notificationManager = new NotificationManager();
            
            // Crear una instancia específica de CoreServiceFacade para esta prueba
            var sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                _dateTimeServiceMock.Object,
                notificationManager);
            
            // Act
            var resultado = await sut.AsignarRolUsuarioAsync(usuarioId, rol);
            
            // Assert
            Console.WriteLine($"Resultado: {resultado?.Succeeded}");
            Assert.NotNull(resultado);
            Assert.False(resultado.Succeeded);
            
            // Verificar que se consultó el repositorio
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            // Verificar que no se llamó al método ActualizarAsync
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
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
            
            _notificacionRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Create a test for conversion from string to TipoNotificacion enum
            var tipoNotificacion = Enum.Parse<RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion>("Informativa");

            await _sut.EnviarNotificacionAsync(
                destinatarioId, 
                tipo, 
                titulo, 
                mensaje, 
                datos, 
                prioridad);
            
            // Assert
            _notificacionRepositoryMock.Verify(
                r => r.AgregarAsync(
                    It.Is<Notificacion>(n => 
                        n.DestinatarioId == destinatarioId && 
                        n.Titulo == titulo && 
                        n.Mensaje == mensaje),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarNotificacionComoLeidaAsync_DebeMarcarComoLeida_CuandoNotificacionExiste()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            var destinatarioId = Guid.NewGuid();
            var ahora = new DateTime(2023, 1, 1, 12, 0, 0);
            
            _dateTimeServiceMock.Setup(d => d.Now).Returns(ahora);
            
            var notificacion = Notificacion.Crear(
                "Título Test",
                "Mensaje Test",
                RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion.Informativa,
                destinatarioId);
                
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
            Assert.True(resultado.Succeeded);
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
            Assert.False(resultado.Succeeded);
            
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
            
            // Configurar el mock para que devuelva un usuario válido
            var emailVO = Email.Create("usuario@test.com");
            var usuario = Usuario.Crear("usuario_test", "Usuario Test", emailVO, RolUsuario.Cajero);
            
            // Establecer ID manualmente para pruebas
            var propiedadId = usuario.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(usuario, usuarioId);
            }
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            // Convertir el tipo a TipoNotificacion
            var tipoNotificacionInformativa = RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion.Informativa;
            var tipoNotificacionAlerta = RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion.Alerta;
            var tipoNotificacionError = RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion.Error;

            // Crear algunas notificaciones de prueba
            var notificaciones = new List<Notificacion>();
            
            var notificacion1 = Notificacion.Crear(
                "Título 1",
                "Mensaje 1",
                tipoNotificacionInformativa,
                usuarioId);
                
            var notificacion2 = Notificacion.Crear(
                "Título 2",
                "Mensaje 2",
                tipoNotificacionAlerta,
                usuarioId);
                
            var notificacion3 = Notificacion.Crear(
                "Título 3",
                "Mensaje 3", 
                tipoNotificacionError,
                otroUsuarioId);
                
            notificaciones.Add(notificacion1);
            notificaciones.Add(notificacion2);
            notificaciones.Add(notificacion3);
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificaciones);
            
            // Act
            var resultado = await _sut.ObtenerNotificacionesUsuarioAsync(usuarioId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.Succeeded);
            Assert.Equal(2, resultado.Value.Count);
            Assert.All(resultado.Value, n => Assert.Equal(usuarioId, n.DestinatarioId));
            
            _notificacionRepositoryMock.Verify(
                r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()), 
                Times.Once);
            
            _usuarioRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        #endregion
    }
} 