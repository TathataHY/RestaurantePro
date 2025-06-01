namespace RestaurantePro.Application.UnitTests.Core.Usuarios.Commands;

/// <summary>
/// 🔐 Tests para CambiarPasswordUsuarioHandler
/// Validaciones de seguridad, autenticación y autorización
/// </summary>
public class CambiarPasswordUsuarioHandlerTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IPasswordHashingService> _passwordHashingMock;
    private readonly Mock<ISecurityValidationService> _securityValidationMock;
    private readonly Mock<INotificacionService> _notificacionMock;
    private readonly Mock<IAuditingService> _auditingMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CambiarPasswordUsuarioHandler>> _loggerMock;
    private readonly CambiarPasswordUsuarioHandler _handler;

    public CambiarPasswordUsuarioHandlerTests()
    {
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _passwordHashingMock = new Mock<IPasswordHashingService>();
        _securityValidationMock = new Mock<ISecurityValidationService>();
        _notificacionMock = new Mock<INotificacionService>();
        _auditingMock = new Mock<IAuditingService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CambiarPasswordUsuarioHandler>>();

        _handler = new CambiarPasswordUsuarioHandler(
            _usuarioRepositoryMock.Object,
            _passwordHashingMock.Object,
            _securityValidationMock.Object,
            _notificacionMock.Object,
            _auditingMock.Object,
            _currentUserMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
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
        var hashActual = "hash_actual";
        var nuevoHash = "nuevo_hash";

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            NuevaPassword = nuevaPassword,
            ConfirmarPassword = nuevaPassword
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            RolUsuario.Mesero,
            hashActual);
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        // Setup mocks
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);
        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _passwordHashingMock.Setup(x => x.VerifyPassword(passwordActual, hashActual))
            .Returns(true);
        _securityValidationMock.Setup(x => x.ValidarCriteriosSeguridad(nuevaPassword))
            .Returns(Result.Success());
        _passwordHashingMock.Setup(x => x.HashPassword(nuevaPassword))
            .Returns(nuevoHash);
        _usuarioRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Verify password fue actualizada
        _passwordHashingMock.Verify(x => x.HashPassword(nuevaPassword), Times.Once);
        _usuarioRepositoryMock.Verify(x => x.ActualizarAsync(It.Is<Usuario>(u => 
            u.Id == usuarioId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
            NuevaPassword = "NuevaPassword123!",
            ConfirmarPassword = "NuevaPassword123!"
        };

        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Usuario no encontrado");
        
        // Verify no se intentó cambiar password
        _passwordHashingMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
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
        var hashActual = "hash_actual";

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            NuevaPassword = "NuevaPassword123!",
            ConfirmarPassword = "NuevaPassword123!"
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            RolUsuario.Mesero,
            hashActual);

        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId);
        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _passwordHashingMock.Setup(x => x.VerifyPassword(passwordActual, hashActual))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("contraseña actual es incorrecta");

        // Verify se registra intento de cambio fallido
        _auditingMock.Verify(x => x.RegistrarIntentoFallido(
            It.IsAny<string>(), 
            It.IsAny<Guid>(), 
            It.IsAny<string>()), Times.Once);
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
        var hashActual = "hash_actual";

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            NuevaPassword = nuevaPassword,
            ConfirmarPassword = nuevaPassword
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            RolUsuario.Mesero,
            hashActual);

        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId);
        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _passwordHashingMock.Setup(x => x.VerifyPassword(passwordActual, hashActual))
            .Returns(true);
        _securityValidationMock.Setup(x => x.ValidarCriteriosSeguridad(nuevaPassword))
            .Returns(Result.Failure("La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, números y símbolos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("criterios de seguridad");
    }

    /// <summary>
    /// ❌ Test: Confirmación de contraseña no coincide
    /// </summary>
    [Fact]
    public async Task Handle_ConfirmacionNoCoincide_DeberiaRetornarFailure()
    {
        // Arrange
        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = Guid.NewGuid(),
            PasswordActual = "password123",
            NuevaPassword = "NuevaPassword123!",
            ConfirmarPassword = "PasswordDiferente123!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("confirmación de contraseña no coincide");
    }

    /// <summary>
    /// ❌ Test: Usuario sin autorización para cambiar contraseña de otro usuario
    /// </summary>
    [Fact]
    public async Task Handle_SinAutorizacionParaOtroUsuario_DeberiaRetornarFailure()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid(); // Diferente usuario
        
        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "password123",
            NuevaPassword = "NuevaPassword123!",
            ConfirmarPassword = "NuevaPassword123!"
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            RolUsuario.Mesero,
            "hash_actual");

        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);
        _currentUserMock.Setup(x => x.Rol).Returns(RolUsuario.Mesero.ToString());
        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("autorización");
    }

    /// <summary>
    /// ✅ Test: Administrador puede cambiar contraseña de cualquier usuario
    /// </summary>
    [Fact]
    public async Task Handle_AdministradorCambiandoPasswordDeOtroUsuario_DeberiaRetornarSuccess()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid(); // Diferente usuario
        var nuevaPassword = "NuevaPassword123!";
        var hashActual = "hash_actual";
        var nuevoHash = "nuevo_hash";

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = "", // Administrador no necesita password actual
            NuevaPassword = nuevaPassword,
            ConfirmarPassword = nuevaPassword,
            EsResetPorAdmin = true
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            RolUsuario.Mesero,
            hashActual);
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        // Setup mocks para admin
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);
        _currentUserMock.Setup(x => x.Rol).Returns(RolUsuario.Administrador.ToString());
        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _securityValidationMock.Setup(x => x.ValidarCriteriosSeguridad(nuevaPassword))
            .Returns(Result.Success());
        _passwordHashingMock.Setup(x => x.HashPassword(nuevaPassword))
            .Returns(nuevoHash);
        _usuarioRepositoryMock.Setup(x => x.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Verify no se validó password actual (admin reset)
        _passwordHashingMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _passwordHashingMock.Verify(x => x.HashPassword(nuevaPassword), Times.Once);
    }

    /// <summary>
    /// ✅ Test: Notificación de cambio de contraseña enviada
    /// </summary>
    [Fact]
    public async Task Handle_CambioPasswordExitoso_DeberiaEnviarNotificacion()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var passwordActual = "password123";
        var nuevaPassword = "NuevaPassword123!";
        var hashActual = "hash_actual";
        var nuevoHash = "nuevo_hash";

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            NuevaPassword = nuevaPassword,
            ConfirmarPassword = nuevaPassword
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            RolUsuario.Mesero,
            hashActual);
        usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId);

        // Setup successful change
        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId);
        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _passwordHashingMock.Setup(x => x.VerifyPassword(passwordActual, hashActual))
            .Returns(true);
        _securityValidationMock.Setup(x => x.ValidarCriteriosSeguridad(nuevaPassword))
            .Returns(Result.Success());
        _passwordHashingMock.Setup(x => x.HashPassword(nuevaPassword))
            .Returns(nuevoHash);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify notificación de seguridad enviada
        _notificacionMock.Verify(x => x.EnviarNotificacionSeguridadAsync(
            It.Is<Guid>(id => id == usuarioId),
            It.Is<string>(msg => msg.Contains("contraseña ha sido actualizada")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// ✅ Test: Auditoría de cambio de contraseña registrada
    /// </summary>
    [Fact]
    public async Task Handle_CambioPasswordExitoso_DeberiaRegistrarAuditoria()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var currentUserId = usuarioId;
        var passwordActual = "password123";
        var nuevaPassword = "NuevaPassword123!";
        var hashActual = "hash_actual";

        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            PasswordActual = passwordActual,
            NuevaPassword = nuevaPassword,
            ConfirmarPassword = nuevaPassword
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            RolUsuario.Mesero,
            hashActual);

        // Setup successful change
        _currentUserMock.Setup(x => x.UserId).Returns(currentUserId);
        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _passwordHashingMock.Setup(x => x.VerifyPassword(passwordActual, hashActual))
            .Returns(true);
        _securityValidationMock.Setup(x => x.ValidarCriteriosSeguridad(nuevaPassword))
            .Returns(Result.Success());
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify auditoría registrada
        _auditingMock.Verify(x => x.RegistrarCambioPassword(
            It.Is<Guid>(id => id == usuarioId),
            It.Is<Guid>(id => id == currentUserId),
            It.IsAny<DateTime>()), Times.Once);
    }

    /// <summary>
    /// ❌ Test: Error en base de datos
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
            NuevaPassword = "NuevaPassword123!",
            ConfirmarPassword = "NuevaPassword123!"
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            RolUsuario.Mesero,
            "hash_actual");

        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId);
        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _passwordHashingMock.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _securityValidationMock.Setup(x => x.ValidarCriteriosSeguridad(It.IsAny<string>()))
            .Returns(Result.Success());
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error");
    }

    /// <summary>
    /// ✅ Test: Validación de múltiples criterios de seguridad
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
            NuevaPassword = nuevaPassword,
            ConfirmarPassword = nuevaPassword
        };

        var usuario = Usuario.Crear(
            "juan.perez@email.com",
            "Juan Pérez",
            RolUsuario.Mesero,
            "hash_actual");

        _currentUserMock.Setup(x => x.UserId).Returns(usuarioId);
        _usuarioRepositoryMock.Setup(x => x.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _passwordHashingMock.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _securityValidationMock.Setup(x => x.ValidarCriteriosSeguridad(nuevaPassword))
            .Returns(Result.Failure($"Password inválida: {motivoFallo}"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(motivoFallo);
    }
} 