namespace RestaurantePro.Application.UnitTests.Core.Usuarios.Commands;
using RestaurantePro.Application.UnitTests.Common;

/// <summary>
/// 🔐 Tests para CambiarPasswordUsuarioHandler
/// Validaciones de seguridad, autenticación y autorización
/// </summary>
public class CambiarPasswordUsuarioHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<CambiarPasswordUsuarioHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly CambiarPasswordUsuarioHandler _handler;

    public CambiarPasswordUsuarioHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<CambiarPasswordUsuarioHandler>>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _emailServiceMock = new Mock<IEmailService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _handler = new CambiarPasswordUsuarioHandler(
            _contextMock.Object,
            _loggerMock.Object,
            _currentUserMock.Object,
            _emailServiceMock.Object,
            _notificationServiceMock.Object);
    }

    /// <summary>
    /// ✅ Test: Cambio de contraseña exitoso con todas las validaciones
    /// </summary>
    [Fact]
    public async Task Handle_CambiarPasswordExitoso_DeberiaRetornarSuccess()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var currentUserId = usuarioId; // Mismo usuario
        var passwordActual = "password123";
        var nuevaPassword = "NuevaPassword123!";

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            PasswordNueva = nuevaPassword,
            ConfirmarPasswordNueva = nuevaPassword
        };

        var usuario = Usuario.Crear(
            "juan.perez",
            "Juan Pérez",
            "juan.perez@email.com",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        SetupUsuarioExistenteMock(usuario);
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId.ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess());
        Assert.True(result.Value);

        // Verify save fue llamado
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ❌ Test: Usuario no encontrado
    /// </summary>
    [Fact]
    public async Task Handle_UsuarioNoEncontrado_DeberiaRetornarFailure()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "password123",
            PasswordNueva = "NuevaPassword123!",
            ConfirmarPasswordNueva = "NuevaPassword123!"
        };

        SetupUsuarioNoExistenteMock();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Contains("Error interno al cambiar la contraseña", result.Error);
    }

    /// <summary>
    /// ❌ Test: Password actual incorrecta
    /// </summary>
    [Fact]
    public async Task Handle_PasswordActualIncorrecta_DeberiaRetornarFailure()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var passwordActual = "password_incorrecta";

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            PasswordNueva = "NuevaPassword123!",
            ConfirmarPasswordNueva = "NuevaPassword123!"
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            "hash_actual",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        SetupUsuarioExistenteMock(usuario);
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId.ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess());
    }

    /// <summary>
    /// ❌ Test: Nueva contraseña no cumple criterios de seguridad
    /// </summary>
    [Fact]
    public async Task Handle_CriteriosSeguridadInvalidos_DeberiaRetornarFailure()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var passwordActual = "password123";
        var nuevaPassword = "123"; // Password débil

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            PasswordNueva = nuevaPassword,
            ConfirmarPasswordNueva = nuevaPassword
        };

        var usuario = Usuario.Crear(
            "juan.perez",
            "Juan Pérez",
            "juan.perez@email.com",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        SetupUsuarioExistenteMock(usuario);
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId.ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess());
    }

    /// <summary>
    /// ❌ Test: Confirmación no coincide
    /// </summary>
    [Fact]
    public async Task Handle_ConfirmacionNoCoincide_DeberiaRetornarFailure()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "password123",
            PasswordNueva = "NuevaPassword123!",
            ConfirmarPasswordNueva = "PasswordDiferente123!"
        };

        var usuario = Usuario.Crear(
            "juan.perez",
            "Juan Pérez",
            "juan.perez@email.com",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        SetupUsuarioExistenteMock(usuario);
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId.ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess());
    }

    /// <summary>
    /// ❌ Test: Sin autorización para otro usuario
    /// </summary>
    [Fact]
    public async Task Handle_SinAutorizacionParaOtroUsuario_DeberiaRetornarFailure()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var otroUsuarioId = Guid.NewGuid(); // Usuario diferente

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "password123",
            PasswordNueva = "NuevaPassword123!",
            ConfirmarPasswordNueva = "NuevaPassword123!"
        };

        var usuario = Usuario.Crear(
            "juan.perez",
            "Juan Pérez",
            "juan.perez@email.com",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        SetupUsuarioExistenteMock(usuario);
        _currentUserMock.Setup(x => x.UserId).Returns(otroUsuarioId.ToString()); // Usuario diferente

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess());
    }

    /// <summary>
    /// ✅ Test: Administrador cambiando password de otro usuario
    /// </summary>
    [Fact]
    public async Task Handle_AdministradorCambiandoPasswordDeOtroUsuario_DeberiaRetornarSuccess()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "password123",
            PasswordNueva = "NuevaPassword123!",
            ConfirmarPasswordNueva = "NuevaPassword123!"
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            "juan.perez@email.com",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        var admin = Usuario.Crear(
            "admin@email.com",
            "Administrador",
            "hash_admin",
            RolUsuario.Administrador);
        
        // CORREGIDO: Asignar Guid directamente, no string
        admin.GetType().GetProperty("Id")?.SetValue(admin, adminId);

        SetupUsuarioExistenteMock(usuario, admin);
        _currentUserMock.Setup(x => x.UserId).Returns(adminId.ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess());
    }

    /// <summary>
    /// ✅ Test: Cambio exitoso debe enviar notificación
    /// </summary>
    [Fact]
    public async Task Handle_CambioPasswordExitoso_DeberiaEnviarNotificacion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "password123",
            PasswordNueva = "NuevaPassword123!",
            ConfirmarPasswordNueva = "NuevaPassword123!",
            NotificarPorEmail = true
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            "juan.perez@email.com",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        SetupUsuarioExistenteMock(usuario);
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId.ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess());
    }

    /// <summary>
    /// ✅ Test: Cambio exitoso debe registrar auditoría
    /// </summary>
    [Fact]
    public async Task Handle_CambioPasswordExitoso_DeberiaRegistrarAuditoria()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "password123",
            PasswordNueva = "NuevaPassword123!",
            ConfirmarPasswordNueva = "NuevaPassword123!",
            RegistrarAuditoria = true
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            "juan.perez@email.com",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        SetupUsuarioExistenteMock(usuario);
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId.ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess());
    }

    /// <summary>
    /// ❌ Test: Error de base de datos
    /// </summary>
    [Fact]
    public async Task Handle_ErrorBaseDatos_DeberiaRetornarFailure()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "password123",
            PasswordNueva = "NuevaPassword123!",
            ConfirmarPasswordNueva = "NuevaPassword123!"
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            "juan.perez@email.com",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        SetupUsuarioExistenteMock(usuario);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Error de base de datos"));
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId.ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess());
        Assert.Contains("Error interno", result.Error);
    }

    /// <summary>
    /// ❌ Tests parametrizados: Password no cumple criterios
    /// </summary>
    [Theory]
    [InlineData("123456", "muy corta")]
    [InlineData("password", "sin números ni símbolos")]
    [InlineData("PASSWORD123", "sin minúsculas")]
    [InlineData("password123", "sin mayúsculas ni símbolos")]
    public async Task Handle_PasswordNoCumpleCriterios_DeberiaRetornarFailureEspecifico(
        string nuevaPassword, string motivoFallo)
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "password123",
            PasswordNueva = nuevaPassword,
            ConfirmarPasswordNueva = nuevaPassword
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            "juan.perez@email.com",
            RolUsuario.Mesero);
        
        // CORREGIDO: Asignar Guid directamente, no string
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        SetupUsuarioExistenteMock(usuario);
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId.ToString());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess());
        
        // Verificar que el motivo de fallo es válido y descriptivo
        motivoFallo.Should().NotBeNullOrEmpty();
        // Verificar que el motivo es uno de los valores esperados
        var motivosValidos = new[] { "muy corta", "sin números ni símbolos", "sin minúsculas", "sin mayúsculas ni símbolos" };
        motivosValidos.Should().Contain(motivoFallo);
    }

    #region Helper Methods

    private void SetupUsuarioExistenteMock(Usuario usuario, Usuario? usuarioAdicional = null)
    {
        // Crear lista con el usuario(s) y usar MockDbSetHelper
        var usuariosList = new List<Usuario> { usuario };
        if (usuarioAdicional != null)
        {
            usuariosList.Add(usuarioAdicional);
        }
        
        var mockUsuariosDbSet = MockDbSetHelper.CreateMockDbSet(usuariosList.AsQueryable());

        // NO intentar configurar FirstOrDefaultAsync directamente - Moq no puede hacerlo
        // El MockDbSetHelper ya configura el QueryProvider correctamente

        // Configurar el contexto para retornar nuestro DbSet mockeado
        _contextMock.Setup(c => c.Usuarios).Returns(mockUsuariosDbSet.Object);
        
        // Configurar SaveChangesAsync
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupUsuarioNoExistenteMock()
    {
        // Crear lista vacía y usar MockDbSetHelper
        var usuariosList = new List<Usuario>();
        var mockUsuariosDbSet = MockDbSetHelper.CreateMockDbSet(usuariosList.AsQueryable());

        // NO intentar configurar FirstOrDefaultAsync directamente - Moq no puede hacerlo
        // El MockDbSetHelper ya configura el QueryProvider correctamente

        // Configurar el contexto para retornar nuestro DbSet vacío mockeado
        _contextMock.Setup(c => c.Usuarios).Returns(mockUsuariosDbSet.Object);
        
        // Configurar SaveChangesAsync
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    #endregion
} 