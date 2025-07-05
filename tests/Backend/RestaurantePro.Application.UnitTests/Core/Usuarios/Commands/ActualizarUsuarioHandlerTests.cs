namespace RestaurantePro.Application.UnitTests.Core.Usuarios.Commands.ActualizarUsuario;

public class ActualizarUsuarioHandlerTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ActualizarUsuarioHandler>> _mockLogger;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<DbSet<Usuario>> _mockUsuarios;
    private readonly ActualizarUsuarioHandler _handler;
    private readonly Usuario _usuarioExistente;
    private readonly Usuario _usuarioAutorizador;
    private readonly UsuarioDto _usuarioDtoEjemplo;

    public ActualizarUsuarioHandlerTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ActualizarUsuarioHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockUsuarios = new Mock<DbSet<Usuario>>();

        _handler = new ActualizarUsuarioHandler(
            _mockContext.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCurrentUserService.Object,
            _mockEmailService.Object,
            _mockNotificationService.Object);

        _usuarioExistente = CrearUsuarioExistente();
        _usuarioAutorizador = CrearUsuarioAutorizador();
        _usuarioDtoEjemplo = CrearUsuarioDtoEjemplo();
    }

    [Fact]
    public async Task Handle_ConActualizacionBasica_DeberiaActualizarCorrectamente()
    {
        // Arrange
        var command = ActualizarUsuarioCommand.ActualizacionInformacionBasica(
            _usuarioExistente.Id,
            "Usuario Actualizado",
            "555-123-4567",
            "Nueva Dirección 123",
            _usuarioAutorizador.Id);

        SetupUsuarioExistenteMock(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConNotificaciones_DeberiaEnviarCorrectamente()
    {
        // Arrange
        var command = ActualizarUsuarioCommand.ActualizacionInformacionBasica(
            _usuarioExistente.Id,
            "Usuario Notificaciones",
            "555-111-2222",
            "Dirección Notificaciones",
            _usuarioAutorizador.Id);

        SetupUsuarioExistenteMock(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que se envían notificaciones (con la firma correcta)
        _mockEmailService.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);

        // Arreglar: usar la firma correcta (Guid usuarioId, string titulo, string mensaje, string tipo)
        _mockNotificationService.Verify(
            n => n.EnviarNotificacionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    #region Helper Methods

    private void SetupUsuarioExistenteMock(Usuario usuario)
    {
        // Crear lista con el usuario y usar MockDbSetHelper
        var usuariosList = new List<Usuario> { usuario };
        var mockUsuariosDbSet = MockDbSetHelper.CreateMockDbSet(usuariosList.AsQueryable());

        // Configurar el contexto para retornar nuestro DbSet mockeado
        _mockContext.Setup(c => c.Usuarios).Returns(mockUsuariosDbSet.Object);
        
        // Configurar SaveChangesAsync
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private Usuario CrearUsuarioExistente()
    {
        return Usuario.Crear("usuario.existente", "Usuario Existente", "existente@restaurantepro.com", RolUsuario.Mesero);
    }

    private Usuario CrearUsuarioAutorizador()
    {
        return Usuario.Crear("admin.autorizador", "Administrador Autorizador", "admin@restaurantepro.com", RolUsuario.Administrador);
    }

    private UsuarioDto CrearUsuarioDtoEjemplo()
    {
        return new UsuarioDto
        {
            Id = Guid.NewGuid(),
            NombreUsuario = "usuario.actualizado",
            NombreCompleto = "Usuario Actualizado",
            Email = "usuario.actualizado@restaurantepro.com",
            Rol = "Mesero",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
    }

    #endregion
} 