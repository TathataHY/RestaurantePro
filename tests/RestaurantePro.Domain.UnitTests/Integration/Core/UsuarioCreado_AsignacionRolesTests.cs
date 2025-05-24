namespace RestaurantePro.Domain.UnitTests.Integration.Core
{
    /// <summary>
    /// Tests de integración para verificar la asignación automática de roles
    /// cuando se crea un nuevo usuario.
    /// </summary>
    public class UsuarioCreado_AsignacionRolesTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
        private readonly Mock<IRolRepository> _rolRepositoryMock = new();
        private readonly Mock<IDomainEventRegistry> _eventRegistryMock = new();
        private readonly Mock<IDateTimeService> _dateTimeServiceMock = new();
        
        private readonly UsuarioCreado_AsignarRolPredeterminadoHandler _handler;
        private readonly DateTime _fechaActual = new DateTime(2023, 5, 15, 10, 0, 0);
        
        public UsuarioCreado_AsignacionRolesTests()
        {
            // Configurar fecha actual
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fechaActual);
            
            // Inicializar handler
            _handler = new UsuarioCreado_AsignarRolPredeterminadoHandler(
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _eventRegistryMock.Object,
                _dateTimeServiceMock.Object);
        }
        
        [Fact]
        public async Task UsuarioCreado_DebeAsignarRolPredeterminado()
        {
            // Arrange
            // 1. Crear un usuario nuevo
            var usuarioId = Guid.NewGuid();
            var email = "nuevo.usuario@example.com";
            var usuario = Usuario.Crear(
                "NuevoUsuario",
                "Juan Pérez",
                email,
                RolUsuario.Mesero);
                
            // Establecer ID usando reflexión para simular entidad guardada
            typeof(EntityBase).GetProperty("Id").SetValue(usuario, usuarioId);
            
            // 2. Crear un rol predeterminado
            var rolId = Guid.NewGuid();
            var rolEmpleado = Rol.Crear(
                "Empleado", 
                "Rol básico para empleados", 
                TipoUsuario.Empleado, 
                true, 
                false);
            
            // Establecer ID usando reflexión
            typeof(EntityBase).GetProperty("Id").SetValue(rolEmpleado, rolId);
            
            // 3. Configurar mocks
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            _rolRepositoryMock
                .Setup(r => r.ObtenerPredeterminadoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(rolEmpleado);
            
            _usuarioRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // 4. Capturar el evento UsuarioCreado
            var eventoUsuarioCreado = new UsuarioCreado(
                usuarioId,
                "NuevoUsuario",
                email,
                EstadoUsuario.PendienteConfirmacion,
                TipoUsuario.Empleado);
            
            // Act
            await _handler.Handle(eventoUsuarioCreado, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se consultó al usuario
            _usuarioRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // 2. Verificar que se consultó el rol predeterminado
            _rolRepositoryMock.Verify(
                r => r.ObtenerPredeterminadoAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            
            // 3. Verificar que se guardó el usuario con el rol asignado
            _usuarioRepositoryMock.Verify(
                r => r.ActualizarAsync(It.Is<Usuario>(u => 
                    u.Roles.Count == 1),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            
            // 4. Verificar que se registró el evento en el log
            _eventRegistryMock.Verify(
                l => l.RegisterAsync(
                    It.IsAny<UsuarioCreado>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task UsuarioCreado_ConTipoAdmin_DebeAsignarRolAdministrador()
        {
            // Arrange
            // 1. Crear un usuario administrativo nuevo
            var usuarioId = Guid.NewGuid();
            var email = "admin@example.com";
            var usuario = Usuario.Crear(
                "AdminUser",
                "Admin User",
                email,
                RolUsuario.Administrador);
                
            // Establecer como administrador
            usuario.EstablecerTipo(TipoUsuario.Administrador);
                
            // Establecer ID usando reflexión para simular entidad guardada
            typeof(EntityBase).GetProperty("Id").SetValue(usuario, usuarioId);
            
            // 2. Crear roles disponibles
            var rolEmpleadoId = Guid.NewGuid();
            var rolEmpleado = Rol.Crear(
                "Empleado", 
                "Rol básico para empleados", 
                TipoUsuario.Empleado, 
                true, 
                false);
            typeof(EntityBase).GetProperty("Id").SetValue(rolEmpleado, rolEmpleadoId);
            
            var rolAdminId = Guid.NewGuid();
            var rolAdmin = Rol.Crear(
                "Administrador", 
                "Rol con todos los permisos", 
                TipoUsuario.Administrador, 
                true, 
                true);
            typeof(EntityBase).GetProperty("Id").SetValue(rolAdmin, rolAdminId);
            
            // 3. Configurar mocks
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            _rolRepositoryMock
                .Setup(r => r.ObtenerPorTipoUsuarioAsync(TipoUsuario.Administrador, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rolAdmin);
            
            _usuarioRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // 4. Capturar el evento UsuarioCreado con tipo administrador
            var eventoUsuarioCreado = new UsuarioCreado(
                usuarioId,
                "AdminUser",
                email,
                EstadoUsuario.PendienteConfirmacion,
                TipoUsuario.Administrador);
            
            // Act
            await _handler.Handle(eventoUsuarioCreado, CancellationToken.None);
            
            // Assert
            // 1. Verificar que se consultó al usuario
            _usuarioRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // 2. Verificar que se consultó el rol para administradores
            _rolRepositoryMock.Verify(
                r => r.ObtenerPorTipoUsuarioAsync(TipoUsuario.Administrador, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // 3. Verificar que se guardó el usuario con el rol de administrador asignado
            _usuarioRepositoryMock.Verify(
                r => r.ActualizarAsync(It.Is<Usuario>(u => 
                    u.Roles.Count == 1),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
} 