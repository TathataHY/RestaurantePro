namespace RestaurantePro.Application.UnitTests.Core.Usuarios.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA CAMBIAR PASSWORD USUARIO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de cambio de contraseña
/// Cobertura: 100% de reglas de negocio del CambiarPasswordUsuarioValidator
/// </summary>
public class CambiarPasswordUsuarioValidatorTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CambiarPasswordUsuarioValidator _validator;

    public CambiarPasswordUsuarioValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        
        // Configurar el contexto con el comportamiento básico
        ConfigurarContextoBasico();
        
        _validator = new CambiarPasswordUsuarioValidator(_mockContext.Object);
    }

    private void ConfigurarContextoBasico()
    {
        // Crear usuarios por defecto para satisfacer las validaciones básicas
        var usuarioDefault = Usuario.Crear("usuario.default", "Usuario Default", "usuario.default@test.com", RolUsuario.Mesero);
        var autorizadorDefault = Usuario.Crear("admin.default", "Admin Default", "admin.default@test.com", RolUsuario.Administrador);

        // Configurar IDs específicos para poder referenciarlos en los tests
        var usuarioId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var autorizadorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        
        usuarioDefault.GetType().GetProperty("Id")?.SetValue(usuarioDefault, usuarioId);
        autorizadorDefault.GetType().GetProperty("Id")?.SetValue(autorizadorDefault, autorizadorId);

        // Asegurar que los usuarios estén activos
        usuarioDefault.Activar();
        autorizadorDefault.Activar();

        var usuarios = new List<Usuario> { usuarioDefault, autorizadorDefault };
        var mockUsuarios = MockDbSetHelper.CreateMockDbSet(usuarios.AsQueryable());
        _mockContext.Setup(c => c.Usuarios).Returns(mockUsuarios.Object);
    }

    private void ConfigurarUsuariosExistentes(bool usuarioExiste = true, bool autorizadorExiste = true, Guid? usuarioId = null, Guid? autorizadorId = null)
    {
        var usuarios = new List<Usuario>();

        if (usuarioExiste)
        {
            var usuario = Usuario.Crear("usuario.test", "Usuario Test", "usuario@test.com", RolUsuario.Mesero);
            if (usuarioId.HasValue)
            {
                usuario.GetType().GetProperty("Id")?.SetValue(usuario, usuarioId.Value);
            }
            usuarios.Add(usuario);
        }

        if (autorizadorExiste)
        {
            var autorizador = Usuario.Crear("autorizador.test", "Autorizador Test", "autorizador@test.com", RolUsuario.Administrador);
            if (autorizadorId.HasValue)
            {
                autorizador.GetType().GetProperty("Id")?.SetValue(autorizador, autorizadorId.Value);
            }
            usuarios.Add(autorizador);
        }

        var mockUsuarios = MockDbSetHelper.CreateMockDbSet(usuarios.AsQueryable());
        _mockContext.Setup(c => c.Usuarios).Returns(mockUsuarios.Object);
    }

    #region Validation Command Helper

    private CambiarPasswordUsuarioCommand CrearCommandValido()
    {
        return new CambiarPasswordUsuarioCommand
        {
            UsuarioId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Usuario default que siempre existe
            UsuarioAutorizaId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Autorizador default que siempre existe
            MotivosCambio = "Cambio de contraseña por políticas de seguridad empresarial",
            Prioridad = 2,
            PasswordNueva = "NuevaPassword123!",
            PasswordActual = "PasswordActual123!",
            InvalidarSesionesActivas = true,
            FechaExpiracion = DateTime.UtcNow.AddDays(90),
            ObservacionesAdicionales = "Cambio programado por política de seguridad"
        };
    }

    #endregion

    #region Validación UsuarioId

    [Fact]
    public async Task Validate_ConUsuarioIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.UsuarioId) &&
            e.ErrorMessage.Contains("El ID del usuario es requerido"));
    }

    [Fact]
    public async Task Validate_ConUsuarioIdInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var usuarioIdInexistente = Guid.NewGuid();
        command.UsuarioId = usuarioIdInexistente;

        ConfigurarUsuariosExistentes(usuarioExiste: false, autorizadorExiste: true);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.UsuarioId) &&
            e.ErrorMessage.Contains("El usuario especificado no existe"));
    }

    [Fact]
    public async Task Validate_ConUsuarioIdValido_NoDeberiaRetornarErrorEnUsuarioId()
    {
        // Arrange
        var command = CrearCommandValido();
        ConfigurarUsuariosExistentes(usuarioExiste: true, autorizadorExiste: true, usuarioId: command.UsuarioId, autorizadorId: command.UsuarioAutorizaId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.UsuarioId) &&
            e.ErrorMessage.Contains("El ID del usuario es requerido"));
    }

    #endregion

    #region Validación UsuarioAutorizaId

    [Fact]
    public async Task Validate_ConUsuarioAutorizaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioAutorizaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.UsuarioAutorizaId) &&
            e.ErrorMessage.Contains("El ID del usuario autorizador es requerido"));
    }

    [Fact]
    public async Task Validate_ConUsuarioAutorizadorInexistente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var autorizadorIdInexistente = Guid.NewGuid();
        command.UsuarioAutorizaId = autorizadorIdInexistente;

        ConfigurarUsuariosExistentes(usuarioExiste: true, autorizadorExiste: false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.UsuarioAutorizaId) &&
            e.ErrorMessage.Contains("El usuario autorizador especificado no existe"));
    }

    #endregion

    #region Validación MotivosCambio

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConMotivosCambioVacioONull_DeberiaRetornarError(string? motivoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivosCambio = motivoInvalido ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.MotivosCambio) &&
            e.ErrorMessage.Contains("El motivo del cambio es requerido"));
    }

    [Fact]
    public async Task Validate_ConMotivosCambioMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivosCambio = "Corto"; // Menos de 10 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.MotivosCambio) &&
            e.ErrorMessage.Contains("El motivo debe tener al menos 10 caracteres"));
    }

    [Fact]
    public async Task Validate_ConMotivosCambioMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivosCambio = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.MotivosCambio) &&
            e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres"));
    }

    [Theory]
    [InlineData("Cambio por políticas de seguridad")]
    [InlineData("Actualización programada por administración")]
    [InlineData("1234567890")] // Exactamente 10 caracteres - mínimo válido
    public async Task Validate_ConMotivosCambioValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivosCambio = motivoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.MotivosCambio));
    }

    #endregion

    #region Validación Prioridad

    [Theory]
    [InlineData(0)]  // Menor que 1
    [InlineData(-1)] // Negativo
    public async Task Validate_ConPrioridadMenorAUno_DeberiaRetornarError(int prioridadInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridadInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.Prioridad) &&
            e.ErrorMessage.Contains("La prioridad mínima es 1"));
    }

    [Theory]
    [InlineData(5)]  // Mayor que 4
    [InlineData(10)] // Muy alto
    public async Task Validate_ConPrioridadMayorACuatro_DeberiaRetornarError(int prioridadInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridadInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.Prioridad) &&
            e.ErrorMessage.Contains("La prioridad máxima es 4"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Validate_ConPrioridadValida_NoDeberiaRetornarErrorDePrioridad(int prioridadValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Prioridad = prioridadValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.Prioridad));
    }

    #endregion

    #region Validación PasswordNueva

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConPasswordNuevaVaciaONull_DeberiaRetornarError(string? passwordInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PasswordNueva = passwordInvalida ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordNueva) &&
            e.ErrorMessage.Contains("La nueva contraseña es requerida"));
    }

    [Fact]
    public async Task Validate_ConPasswordNuevaMuyCorta_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PasswordNueva = "Pass1!"; // Menos de 8 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordNueva) &&
            e.ErrorMessage.Contains("La nueva contraseña debe tener al menos 8 caracteres"));
    }

    [Fact]
    public async Task Validate_ConPasswordNuevaMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PasswordNueva = new string('A', 129); // Más de 128 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordNueva) &&
            e.ErrorMessage.Contains("La nueva contraseña no puede exceder 128 caracteres"));
    }

    [Theory]
    [InlineData("password123")] // Sin mayúscula ni carácter especial
    [InlineData("PASSWORD123")] // Sin minúscula ni carácter especial
    [InlineData("Password")] // Sin número ni carácter especial
    [InlineData("Password123")] // Sin carácter especial
    public async Task Validate_ConPasswordNuevaSinComplejidadSuficiente_DeberiaRetornarError(string passwordDebil)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PasswordNueva = passwordDebil;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordNueva) &&
            e.ErrorMessage.Contains("La contraseña debe contener al menos: 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial"));
    }

    [Theory]
    [InlineData("Password123!")] // Contiene "password"
    [InlineData("Admin123!")] // Contiene "admin"
    [InlineData("User123!")] // Contiene "user"
    [InlineData("Abc123!")] // Contiene "abc"
    public async Task Validate_ConPasswordNuevaConPatronesProhibidos_DeberiaRetornarError(string passwordConPatron)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PasswordNueva = passwordConPatron;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordNueva) &&
            e.ErrorMessage.Contains("La contraseña contiene patrones comunes no permitidos"));
    }

    [Theory]
    [InlineData("NuevaPassword123!")]
    [InlineData("Segura2025@")]
    [InlineData("MiClave$456")]
    [InlineData("Complex#789")]
    public async Task Validate_ConPasswordNuevaValida_NoDeberiaRetornarErrorDePasswordNueva(string passwordValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PasswordNueva = passwordValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordNueva) &&
            (e.ErrorMessage.Contains("La nueva contraseña es requerida") ||
             e.ErrorMessage.Contains("La nueva contraseña debe tener al menos 8 caracteres") ||
             e.ErrorMessage.Contains("La contraseña debe contener al menos")));
    }

    #endregion

    #region Validación PasswordActual

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConPasswordActualVaciaONull_DeberiaRetornarError(string? passwordInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PasswordActual = passwordInvalida ?? string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordActual) &&
            e.ErrorMessage.Contains("La contraseña actual es requerida"));
    }

    [Fact]
    public async Task Validate_ConPasswordActualMuyCorta_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PasswordActual = "Pass1!"; // Menos de 8 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordActual) &&
            e.ErrorMessage.Contains("La contraseña actual debe tener al menos 8 caracteres"));
    }

    #endregion

    #region Validación FechaExpiracion

    [Fact]
    public async Task Validate_ConFechaExpiracionMuyProxima_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaExpiracion = DateTime.UtcNow.AddDays(3); // Menos de 7 días

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.FechaExpiracion) &&
            e.ErrorMessage.Contains("La fecha de expiración debe ser al menos 7 días en el futuro"));
    }

    [Fact]
    public async Task Validate_ConFechaExpiracionMuyLejana_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaExpiracion = DateTime.UtcNow.AddDays(400); // Más de 1 año

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.FechaExpiracion) &&
            e.ErrorMessage.Contains("La fecha de expiración no puede ser más de 1 año en el futuro"));
    }

    [Theory]
    [InlineData(7)]   // Mínimo válido
    [InlineData(30)]  // 1 mes
    [InlineData(90)]  // 3 meses
    [InlineData(365)] // 1 año - máximo válido
    public async Task Validate_ConFechaExpiracionValida_NoDeberiaRetornarErrorDeFechaExpiracion(int diasFuturos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaExpiracion = DateTime.UtcNow.AddDays(diasFuturos);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.FechaExpiracion));
    }

    [Fact]
    public async Task Validate_ConFechaExpiracionNull_NoDeberiaRetornarErrorDeFechaExpiracion()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaExpiracion = null; // Opcional

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.FechaExpiracion));
    }

    #endregion

    #region Validación ObservacionesAdicionales

    [Fact]
    public async Task Validate_ConObservacionesAdicionalesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesAdicionales = new string('A', 1001); // Más de 1000 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CambiarPasswordUsuarioCommand.ObservacionesAdicionales) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Observaciones cortas")]
    [InlineData("Esta es una observación de longitud normal para el cambio de contraseña")]
    public async Task Validate_ConObservacionesAdicionalesValidas_NoDeberiaRetornarErrorDeObservaciones(string? observaciones)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesAdicionales = observaciones;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.ObservacionesAdicionales));
    }

    #endregion

    #region Validación InvalidarSesionesActivas

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_ConInvalidarSesionesActivasValido_NoDeberiaRetornarError(bool invalidarSesiones)
    {
        // Arrange
        var command = CrearCommandValido();
        command.InvalidarSesionesActivas = invalidarSesiones;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.InvalidarSesionesActivas));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var autorizadorId = Guid.NewGuid();
        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            UsuarioAutorizaId = autorizadorId,
            MotivosCambio = "Cambio de contraseña por políticas de seguridad empresarial y actualización trimestral",
            Prioridad = 3,
            PasswordNueva = "NuevaPasswordSegura123!",
            PasswordActual = "PasswordActualSegura123!",
            InvalidarSesionesActivas = true,
            FechaExpiracion = DateTime.UtcNow.AddDays(90),
            ObservacionesAdicionales = "Cambio programado por política de seguridad, usuario notificado previamente"
        };

        ConfigurarUsuariosExistentes(usuarioExiste: true, autorizadorExiste: true, usuarioId: usuarioId, autorizadorId: autorizadorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConCommandMinimoValido_DeberiaSerValido()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var autorizadorId = Guid.NewGuid();
        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = usuarioId,
            UsuarioAutorizaId = autorizadorId,
            MotivosCambio = "1234567890", // Exactamente 10 caracteres - mínimo válido
            Prioridad = 1,
            PasswordNueva = "MinPass1!",
            PasswordActual = "OldPass1!",
            InvalidarSesionesActivas = false
        };

        ConfigurarUsuariosExistentes(usuarioExiste: true, autorizadorExiste: true, usuarioId: usuarioId, autorizadorId: autorizadorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var command = new CambiarPasswordUsuarioCommand
        {
            UsuarioId = Guid.Empty, // Error
            UsuarioAutorizaId = Guid.Empty, // Error
            MotivosCambio = "", // Error: vacío
            Prioridad = 0, // Error: menor que 1
            PasswordNueva = "", // Error: vacía
            PasswordActual = "", // Error: vacía
            FechaExpiracion = DateTime.UtcNow.AddDays(2), // Error: muy próxima
            ObservacionesAdicionales = new string('A', 1001) // Error: muy larga
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(6);
        
        // Verificar que tiene errores de diferentes propiedades
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.UsuarioId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.UsuarioAutorizaId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.MotivosCambio));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.Prioridad));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordNueva));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordActual));
    }

    #endregion

    #region Theory Tests para Casos Límite

    [Theory]
    [InlineData(10, true)]   // 10 caracteres - mínimo válido
    [InlineData(250, true)]  // 250 caracteres - válido
    [InlineData(500, true)]  // 500 caracteres - máximo válido
    [InlineData(501, false)] // 501 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesMotivo_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivosCambio = new string('A', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(CambiarPasswordUsuarioCommand.MotivosCambio) &&
                (e.ErrorMessage.Contains("El motivo debe tener al menos 10 caracteres") ||
                 e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres")));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(CambiarPasswordUsuarioCommand.MotivosCambio) &&
                e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres"));
        }
    }

    [Theory]
    [InlineData(8, true)]    // 8 caracteres - mínimo válido
    [InlineData(64, true)]   // 64 caracteres - válido
    [InlineData(128, true)]  // 128 caracteres - máximo válido
    [InlineData(129, false)] // 129 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesPasswordNueva_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        // Crear password que tenga la longitud requerida pero mantenga complejidad
        var passwordBase = "Pass123!";
        command.PasswordNueva = passwordBase + new string('A', Math.Max(0, longitud - passwordBase.Length));

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordNueva) &&
                (e.ErrorMessage.Contains("La nueva contraseña debe tener al menos 8 caracteres") ||
                 e.ErrorMessage.Contains("La nueva contraseña no puede exceder 128 caracteres")));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(CambiarPasswordUsuarioCommand.PasswordNueva) &&
                e.ErrorMessage.Contains("La nueva contraseña no puede exceder 128 caracteres"));
        }
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Fact]
    public async Task Validate_ConCambioAdministrativo_DeberiaSerValido()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var autorizadorId = Guid.NewGuid();
        var command = CrearCommandValido();
        command.UsuarioId = usuarioId;
        command.UsuarioAutorizaId = autorizadorId;
        command.MotivosCambio = "Cambio administrativo por política de seguridad";
        command.Prioridad = 4;

        ConfigurarUsuariosExistentes(usuarioExiste: true, autorizadorExiste: true, usuarioId: usuarioId, autorizadorId: autorizadorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCambioUsuarioRegular_DeberiaSerValido()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var autorizadorId = Guid.NewGuid();
        var command = CrearCommandValido();
        command.UsuarioId = usuarioId;
        command.UsuarioAutorizaId = autorizadorId;
        command.MotivosCambio = "Solicitud de cambio por el usuario";
        command.Prioridad = 1;

        ConfigurarUsuariosExistentes(usuarioExiste: true, autorizadorExiste: true, usuarioId: usuarioId, autorizadorId: autorizadorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Cambio por expiración automática de contraseña")]
    [InlineData("Actualización por política de seguridad empresarial")]
    [InlineData("Solicitud directa del usuario por sospecha de comprometimiento")]
    [InlineData("Cambio programado por administración de sistemas")]
    public async Task Validate_ConDiferentesMotivos_DeberiaSerValido(string motivo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivosCambio = motivo;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CambiarPasswordUsuarioCommand.MotivosCambio));
    }

    #endregion
} 