namespace RestaurantePro.Application.UnitTests.Core.Usuarios.Commands;

/// <summary>
/// Pruebas unitarias para ActualizarUsuarioHandler
/// Tests que cubren todos los escenarios de actualización de usuarios con lógica empresarial
/// </summary>
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

        // Setup de datos de prueba
        _usuarioExistente = CrearUsuarioExistente();
        _usuarioAutorizador = CrearUsuarioAutorizador();
        _usuarioDtoEjemplo = CrearUsuarioDtoEjemplo();

        ConfigurarMockContext();
    }

    [Fact]
    public async Task Handle_ConActualizacionBasica_DeberiaActualizarCorrectamente()
    {
        // Arrange
        var command = ActualizarUsuarioCommand.ActualizacionInformacionBasica(
            _usuarioExistente.Id,
            "Juan Carlos Pérez",
            "555-999-8888",
            "Calle Nueva 123",
            _usuarioAutorizador.Id);

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(_usuarioDtoEjemplo);

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Verificar que no es crítico por lo que no requiere backup
        command.CrearBackup.Should().BeFalse();
        command.RequiereAprobacion.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ConUsuarioInexistente_DeberiaRetornarError()
    {
        // Arrange
        var usuarioInexistenteId = Guid.NewGuid();
        var command = ActualizarUsuarioCommand.ActualizacionInformacionBasica(
            usuarioInexistenteId,
            "Usuario Inexistente",
            "555-000-0000",
            "Dirección inexistente",
            _usuarioAutorizador.Id);

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("El usuario especificado no existe");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConCambiosCriticos_DeberiaCreaBackupYValidar()
    {
        // Arrange
        var command = ActualizarUsuarioCommand.ActualizacionRolPermisos(
            _usuarioExistente.Id,
            "Gerente", // Cambio de rol es crítico
            8, // Nivel de acceso alto
            new List<string> { "GestionarUsuarios", "VerReportes" },
            _usuarioAutorizador.Id,
            "Promoción a gerente por buen desempeño");

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar configuración de cambios críticos
        command.TieneCambiosCriticos().Should().BeTrue();
        command.CrearBackup.Should().BeTrue();
        command.RequiereAprobacion.Should().BeTrue();
        command.InvalidarSesionesActivas.Should().BeTrue();

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConActualizacionProgramada_DeberiaCrearProgramacion()
    {
        // Arrange
        var fechaFutura = DateTime.UtcNow.AddDays(7);
        var cambiosBase = ActualizarUsuarioCommand.ActualizacionInformacionBasica(
            _usuarioExistente.Id,
            "Nombre Futuro",
            "555-777-9999",
            "Dirección futura",
            _usuarioAutorizador.Id);

        var command = ActualizarUsuarioCommand.ActualizacionProgramada(
            _usuarioExistente.Id,
            fechaFutura,
            cambiosBase,
            _usuarioAutorizador.Id,
            "Actualización programada de datos");

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        command.FechaEfectivacambios.Should().Be(fechaFutura);
        command.RequiereAprobacion.Should().BeTrue();

        // Verificar que se loggea la programación
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando actualización")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConActualizacionJerarquica_DeberiaConfiguraCorrectamente()
    {
        // Arrange
        var nuevoSupervisor = Guid.NewGuid();
        var command = ActualizarUsuarioCommand.CambioJerarquico(
            _usuarioExistente.Id,
            nuevoSupervisor,
            "Nuevo Departamento",
            "Supervisor Junior",
            _usuarioAutorizador.Id,
            "Reestructuración organizacional");

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        command.Departamento.Should().Be("Nuevo Departamento");
        command.Posicion.Should().Be("Supervisor Junior");
        command.SupervisorId.Should().Be(nuevoSupervisor);
        command.NotificarSupervisor.Should().BeTrue();

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConActualizacionConfiguraciones_DeberiaIncluirPreferencias()
    {
        // Arrange
        var notificaciones = new ConfiguracionNotificacionesDto
        {
            EmailHabilitado = true,
            PushHabilitado = true,
            SmsHabilitado = false
        };

        var preferencias = new Dictionary<string, object>
        {
            { "idioma", "es-ES" },
            { "tema", "claro" },
            { "zona_horaria", "GMT-5" }
        };

        var command = ActualizarUsuarioCommand.ActualizacionConfiguraciones(
            _usuarioExistente.Id,
            notificaciones,
            preferencias,
            _usuarioAutorizador.Id);

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        command.ConfiguracionNotificaciones.Should().NotBeNull();
        command.ConfiguracionNotificaciones!.EmailHabilitado.Should().BeTrue();
        command.ConfiguracionNotificaciones.PushHabilitado.Should().BeTrue();
        command.ConfiguracionNotificaciones.SmsHabilitado.Should().BeFalse();
        
        command.Preferencias.Should().ContainKey("idioma");
        command.Preferencias.Should().ContainKey("tema");
        command.Preferencias["idioma"].Should().Be("es-ES");

        // Este tipo de actualización no debería ser crítica
        command.Prioridad.Should().Be(1);
        command.RequiereAprobacion.Should().BeFalse();
        command.NotificarUsuario.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ConCambioEstado_DeberiaInvalidarSesiones()
    {
        // Arrange
        var command = ActualizarUsuarioCommand.CambioEstadoActivacion(
            _usuarioExistente.Id,
            false, // Desactivar (suspender)
            _usuarioAutorizador.Id,
            "Suspensión por violación de políticas");

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        command.Activo.Should().BeFalse(); // Usar Activo en lugar de Estado
        command.InvalidarSesionesActivas.Should().BeTrue();
        command.NotificarUsuario.Should().BeTrue();
        command.NotificarSupervisor.Should().BeTrue();

        // Cambio de estado es crítico
        command.TieneCambiosCriticos().Should().BeTrue();
        command.Prioridad.Should().Be(4); // CambioEstadoActivacion usa prioridad 4 para desactivar

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConErrorEnBaseDatos_DeberiaRetornarErrorGenerico()
    {
        // Arrange
        var command = ActualizarUsuarioCommand.ActualizacionInformacionBasica(
            _usuarioExistente.Id,
            "Nombre Error",
            "555-000-0000",
            "Dirección Error",
            _usuarioAutorizador.Id);

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("Error de base de datos"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Error interno al actualizar el usuario");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeberiaLoggearInformacionCompleta()
    {
        // Arrange
        var command = ActualizarUsuarioCommand.ActualizacionInformacionBasica(
            _usuarioExistente.Id,
            "Nombre para Logging",
            "555-777-8888",
            "Dirección para Logging",
            _usuarioAutorizador.Id);

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar logging de inicio
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando actualización")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verificar logging de éxito
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("actualizado exitosamente")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConNotificaciones_DeberiaEnviarCorrectamente()
    {
        // Arrange
        var command = ActualizarUsuarioCommand.ActualizacionRolPermisos(
            _usuarioExistente.Id,
            "Administrador",
            9, // Nivel de acceso alto
            new List<string> { "GestionarUsuarios", "ConfigurarSistema" },
            _usuarioAutorizador.Id,
            "Promoción a administrador");

        command.NotificarUsuario = true;
        command.NotificarSupervisor = true;

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que se envían notificaciones por email
        _mockEmailService.Verify(
            e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.AtLeastOnce);

        // Verificar que se envían notificaciones del sistema
        _mockNotificationService.Verify(
            n => n.EnviarNotificacionAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Theory]
    [InlineData(true, "Administrador")]
    [InlineData(false, "Empleado")]
    public async Task Handle_ConDiferentesRoles_DeberiaAplicarCorrectamente(bool esAdmin, string rolEsperado)
    {
        // Arrange
        var command = new ActualizarUsuarioCommand
        {
            UsuarioId = _usuarioExistente.Id,
            Rol = rolEsperado,
            NivelAcceso = esAdmin ? 9 : 1, // Usar NivelAcceso en lugar de EsAdministrador
            UsuarioAutorizaId = _usuarioAutorizador.Id,
            MotivoActualizacion = $"Cambio a {rolEsperado}"
        };

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        command.Rol.Should().Be(rolEsperado);
        command.NivelAcceso.Should().Be(esAdmin ? 9 : 1); // Verificar NivelAcceso en lugar de EsAdministrador

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConValidacionDeCampos_DeberiaValidarCorrectamente()
    {
        // Arrange
        var command = ActualizarUsuarioCommand.ActualizacionInformacionBasica(
            _usuarioExistente.Id,
            "Nombre Test",
            "555-123-4567",
            "Dirección Test",
            _usuarioAutorizador.Id);

        // Agregar email manualmente ya que no está en ActualizacionInformacionBasica
        command.Email = "test@restaurantepro.com";
        command.MotivoActualizacion = "Test de validación";

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        // Verificar que el command es válido
        command.EsValido().Should().BeTrue();
        command.TieneAlMenosUnCambio().Should().BeTrue();

        var camposModificados = command.ObtenerCamposAModificar();
        camposModificados.Should().Contain("Nombre"); // Usar "Nombre" en lugar de "NombreCompleto"
        camposModificados.Should().Contain("Email");
        camposModificados.Should().Contain("Telefono");

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, false)] // Prioridad baja, no crítico
    [InlineData(3, true)]  // Prioridad alta, crítico
    [InlineData(4, true)]  // Prioridad máxima, crítico
    public async Task Handle_ConDiferentesPrioridades_DeberiaConfigurarseCriticidadCorrectamente(int prioridad, bool esCriticoEsperado)
    {
        // Arrange
        var command = new ActualizarUsuarioCommand
        {
            UsuarioId = _usuarioExistente.Id,
            Nombre = "Test Prioridad", // Usar Nombre en lugar de NombreCompleto
            Rol = esCriticoEsperado ? "Administrador" : "Empleado", // Rol crítico vs no crítico
            UsuarioAutorizaId = _usuarioAutorizador.Id,
            MotivoActualizacion = $"Test prioridad {prioridad}",
            Prioridad = prioridad
        };

        _mockUsuarios.Setup(u => u.FirstOrDefaultAsync(
                       It.IsAny<System.Linq.Expressions.Expression<Func<Usuario, bool>>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(_usuarioExistente);

        _mockMapper.Setup(m => m.Map<UsuarioDto>(It.IsAny<Usuario>()))
                   .Returns(_usuarioDtoEjemplo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        command.Prioridad.Should().Be(prioridad);
        command.TieneCambiosCriticos().Should().Be(esCriticoEsperado);

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #region Helper Methods

    private void ConfigurarMockContext()
    {
        _mockContext.Setup(c => c.Usuarios)
                   .Returns(_mockUsuarios.Object);
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
            Id = _usuarioExistente?.Id ?? Guid.NewGuid(),
            NombreUsuario = "usuario.actualizado",
            Nombre = "Usuario",
            Apellido = "Actualizado",
            Email = "actualizado@restaurantepro.com",
            Rol = "Empleado",
            Activo = true,
            FechaCreacion = DateTime.UtcNow.AddMonths(-3),
            FechaUltimaActualizacion = DateTime.UtcNow
        };
    }

    #endregion
} 