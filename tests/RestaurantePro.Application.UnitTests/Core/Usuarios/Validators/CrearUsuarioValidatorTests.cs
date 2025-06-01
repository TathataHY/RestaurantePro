namespace RestaurantePro.Application.UnitTests.Core.Usuarios.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA CREAR USUARIO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de creación de usuarios
/// Cobertura: 100% de reglas de negocio del CrearUsuarioValidator
/// </summary>
public class CrearUsuarioValidatorTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly CrearUsuarioValidator _validator;

    public CrearUsuarioValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _validator = new CrearUsuarioValidator(_mockContext.Object);
    }

    #region Validation Command Helper

    private CrearUsuarioCommand CrearCommandValido()
    {
        return new CrearUsuarioCommand
        {
            NombreUsuario = "usuario.test",
            NombreCompleto = "Usuario de Prueba",
            Email = "usuario.test@restaurante.com",
            Password = "Password123!",
            ConfirmarPassword = "Password123!",
            Rol = "Empleado",
            RolesAdicionales = new List<string>(),
            NivelAcceso = 3,
            PermisosEspecificos = new List<string> { "VerReportes" },
            Telefono = "+521234567890",
            Departamento = "Cocina",
            Puesto = "Cocinero Junior",
            UsuarioCreadorId = Guid.NewGuid(),
            HorariosTrabajo = new List<HorarioTrabajoDto>
            {
                new HorarioTrabajoDto
                {
                    DiaSemana = "Lunes",
                    HoraInicio = TimeSpan.FromHours(8),
                    HoraFin = TimeSpan.FromHours(16),
                    EsDiaLibre = false
                }
            }
        };
    }

    #endregion

    #region Validación NombreUsuario

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConNombreUsuarioVacioONull_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreUsuario = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario) &&
            e.ErrorMessage.Contains("El nombre de usuario es requerido"));
    }

    [Theory]
    [InlineData("ab")]  // Menos de 3 caracteres
    [InlineData("a")]   // 1 carácter
    public async Task Validate_ConNombreUsuarioMuyCorto_DeberiaRetornarError(string nombreCorto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreUsuario = nombreCorto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario) &&
            e.ErrorMessage.Contains("El nombre de usuario debe tener al menos 3 caracteres"));
    }

    [Fact]
    public async Task Validate_ConNombreUsuarioMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreUsuario = new string('a', 51); // Más de 50 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario) &&
            e.ErrorMessage.Contains("El nombre de usuario no puede exceder 50 caracteres"));
    }

    [Theory]
    [InlineData("usuario con espacios")]  // Espacios no permitidos
    [InlineData("usuario@invalid")]       // @ no permitido
    [InlineData("usuario#invalid")]       // # no permitido
    [InlineData("usuario$invalid")]       // $ no permitido
    public async Task Validate_ConNombreUsuarioConCaracteresInvalidos_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreUsuario = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario) &&
            e.ErrorMessage.Contains("El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos"));
    }

    [Theory]
    [InlineData("usuario.test")]
    [InlineData("usuario_test")]
    [InlineData("usuario-test")]
    [InlineData("usuario123")]
    [InlineData("abc")]  // Mínimo válido
    public async Task Validate_ConNombreUsuarioValido_NoDeberiaRetornarErrorDeNombreUsuario(string nombreValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreUsuario = nombreValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario));
    }

    #endregion

    #region Validación NombreCompleto

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConNombreCompletoVacioONull_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreCompleto = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto) &&
            e.ErrorMessage.Contains("El nombre completo es requerido"));
    }

    [Theory]
    [InlineData("a")]   // 1 carácter
    public async Task Validate_ConNombreCompletoMuyCorto_DeberiaRetornarError(string nombreCorto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreCompleto = nombreCorto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto) &&
            e.ErrorMessage.Contains("El nombre completo debe tener al menos 2 caracteres"));
    }

    [Fact]
    public async Task Validate_ConNombreCompletoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreCompleto = new string('A', 201); // Más de 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto) &&
            e.ErrorMessage.Contains("El nombre completo no puede exceder 200 caracteres"));
    }

    [Theory]
    [InlineData("Juan123")]       // Números no permitidos
    [InlineData("María@")]        // @ no permitido
    [InlineData("José#")]         // # no permitido
    public async Task Validate_ConNombreCompletoConCaracteresInvalidos_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreCompleto = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto) &&
            e.ErrorMessage.Contains("El nombre completo solo puede contener letras y espacios"));
    }

    [Theory]
    [InlineData("María José")]
    [InlineData("Juan Carlos Pérez")]
    [InlineData("Ñuño Álvarez")]
    [InlineData("Ab")]  // Mínimo válido
    public async Task Validate_ConNombreCompletoValido_NoDeberiaRetornarErrorDeNombreCompleto(string nombreValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreCompleto = nombreValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto));
    }

    #endregion

    #region Validación Email

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConEmailVacioONull_DeberiaRetornarError(string emailInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = emailInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Email) &&
            e.ErrorMessage.Contains("El email es requerido"));
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("email@")]
    [InlineData("@domain.com")]
    [InlineData("email.domain.com")]
    [InlineData("email@@domain.com")]
    public async Task Validate_ConEmailFormatoInvalido_DeberiaRetornarError(string emailInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = emailInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Email) &&
            e.ErrorMessage.Contains("El formato del email no es válido"));
    }

    [Fact]
    public async Task Validate_ConEmailMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var nombreLargo = new string('a', 310);
        command.Email = $"{nombreLargo}@domain.com"; // Más de 320 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Email) &&
            e.ErrorMessage.Contains("El email no puede exceder 320 caracteres"));
    }

    [Theory]
    [InlineData("usuario@restaurante.com")]
    [InlineData("test.email@empresa.com.mx")]
    [InlineData("admin+info@sistema.net")]
    [InlineData("a@b.co")]  // Mínimo válido
    public async Task Validate_ConEmailValido_NoDeberiaRetornarErrorDeEmail(string emailValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = emailValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Email));
    }

    #endregion

    #region Validación Password

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConPasswordVaciaONull_DeberiaRetornarError(string passwordInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Password = passwordInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Password) &&
            e.ErrorMessage.Contains("La contraseña es requerida"));
    }

    [Theory]
    [InlineData("Pass1!")]   // 7 caracteres - menos de 8
    [InlineData("1234567")]  // 7 caracteres numéricos
    public async Task Validate_ConPasswordMuyCorta_DeberiaRetornarError(string passwordCorta)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Password = passwordCorta;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Password) &&
            e.ErrorMessage.Contains("La contraseña debe tener al menos 8 caracteres"));
    }

    [Fact]
    public async Task Validate_ConPasswordMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Password = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Password) &&
            e.ErrorMessage.Contains("La contraseña no puede exceder 100 caracteres"));
    }

    [Theory]
    [InlineData("password123")]    // Sin mayúscula ni carácter especial
    [InlineData("PASSWORD123")]    // Sin minúscula ni carácter especial
    [InlineData("Password")]       // Sin número ni carácter especial
    [InlineData("Password123")]    // Sin carácter especial
    public async Task Validate_ConPasswordSinComplejidadSuficiente_DeberiaRetornarError(string passwordDebil)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Password = passwordDebil;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Password) &&
            e.ErrorMessage.Contains("La contraseña debe contener al menos: 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial"));
    }

    [Theory]
    [InlineData("Password123!")]
    [InlineData("MiClave2025@")]
    [InlineData("Segura$456")]
    [InlineData("Test123#")]
    public async Task Validate_ConPasswordValida_NoDeberiaRetornarErrorDePassword(string passwordValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Password = passwordValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Password) &&
            (e.ErrorMessage.Contains("La contraseña es requerida") ||
             e.ErrorMessage.Contains("La contraseña debe tener al menos 8 caracteres") ||
             e.ErrorMessage.Contains("La contraseña debe contener al menos")));
    }

    #endregion

    #region Validación ConfirmarPassword

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConConfirmarPasswordVaciaONull_DeberiaRetornarError(string confirmacionInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ConfirmarPassword = confirmacionInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.ConfirmarPassword) &&
            e.ErrorMessage.Contains("La confirmación de contraseña es requerida"));
    }

    [Fact]
    public async Task Validate_ConConfirmarPasswordDiferente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Password = "Password123!";
        command.ConfirmarPassword = "DiferentePassword123!";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.ConfirmarPassword) &&
            e.ErrorMessage.Contains("La confirmación de contraseña no coincide"));
    }

    [Fact]
    public async Task Validate_ConConfirmarPasswordIgual_NoDeberiaRetornarErrorDeConfirmacion()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Password = "Password123!";
        command.ConfirmarPassword = "Password123!";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.ConfirmarPassword) &&
            e.ErrorMessage.Contains("La confirmación de contraseña no coincide"));
    }

    #endregion

    #region Validación Rol

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConRolVacioONull_DeberiaRetornarError(string rolInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Rol = rolInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Rol) &&
            e.ErrorMessage.Contains("El rol principal es requerido"));
    }

    [Theory]
    [InlineData("RolInexistente")]
    [InlineData("Admin")]  // No está en la lista de roles válidos
    [InlineData("User")]   // No está en la lista de roles válidos
    public async Task Validate_ConRolInvalido_DeberiaRetornarError(string rolInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Rol = rolInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Rol) &&
            e.ErrorMessage.Contains("El rol debe ser uno de"));
    }

    [Theory]
    [InlineData("Empleado")]
    [InlineData("Supervisor")]
    [InlineData("Gerente")]
    [InlineData("Administrador")]
    [InlineData("SuperAdministrador")]
    public async Task Validate_ConRolValido_NoDeberiaRetornarErrorDeRol(string rolValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Rol = rolValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Rol));
    }

    #endregion

    #region Validación RolesAdicionales

    [Fact]
    public async Task Validate_ConRolesAdicionalesInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.RolesAdicionales = new List<string> { "Supervisor", "RolInexistente" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.RolesAdicionales) &&
            e.ErrorMessage.Contains("Todos los roles adicionales deben ser válidos"));
    }

    [Fact]
    public async Task Validate_ConMuchosRolesAdicionales_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.RolesAdicionales = new List<string> { "Supervisor", "Gerente", "Administrador", "SuperAdministrador" }; // Más de 3

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.RolesAdicionales) &&
            e.ErrorMessage.Contains("No se pueden asignar más de 3 roles adicionales"));
    }

    [Theory]
    [InlineData(new string[] { })]  // Sin roles adicionales - válido
    [InlineData(new string[] { "Supervisor" })]
    [InlineData(new string[] { "Supervisor", "Gerente" })]
    [InlineData(new string[] { "Supervisor", "Gerente", "Administrador" })]  // 3 roles - máximo válido
    public async Task Validate_ConRolesAdicionalesValidos_NoDeberiaRetornarErrorDeRolesAdicionales(string[] roles)
    {
        // Arrange
        var command = CrearCommandValido();
        command.RolesAdicionales = roles.ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.RolesAdicionales));
    }

    #endregion

    #region Validación NivelAcceso

    [Theory]
    [InlineData(0)]   // Menor que 1
    [InlineData(-1)]  // Negativo
    public async Task Validate_ConNivelAccesoMenorAUno_DeberiaRetornarError(int nivelInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NivelAcceso = nivelInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NivelAcceso) &&
            e.ErrorMessage.Contains("El nivel de acceso mínimo es 1"));
    }

    [Theory]
    [InlineData(11)]  // Mayor que 10
    [InlineData(15)]  // Muy alto
    public async Task Validate_ConNivelAccesoMayorADiez_DeberiaRetornarError(int nivelInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NivelAcceso = nivelInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.NivelAcceso) &&
            e.ErrorMessage.Contains("El nivel de acceso máximo es 10"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Validate_ConNivelAccesoValido_NoDeberiaRetornarErrorDeNivelAcceso(int nivelValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NivelAcceso = nivelValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.NivelAcceso));
    }

    #endregion

    #region Validación PermisosEspecificos

    [Fact]
    public async Task Validate_ConPermisosEspecificosInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PermisosEspecificos = new List<string> { "VerReportes", "PermisoInexistente" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.PermisosEspecificos) &&
            e.ErrorMessage.Contains("Todos los permisos deben ser válidos"));
    }

    [Fact]
    public async Task Validate_ConMuchosPermisosEspecificos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PermisosEspecificos = new List<string>
        {
            "GestionarUsuarios", "GestionarRoles", "VerReportes", "VerTodosReportes",
            "ConfigurarSistema", "GestionarSucursales", "GestionarEmpleados",
            "AprobarDescuentos", "GestionarInventario", "GestionarProveedores",
            "PermisoExtra" // Más de 10
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.PermisosEspecificos) &&
            e.ErrorMessage.Contains("No se pueden asignar más de 10 permisos específicos"));
    }

    [Theory]
    [InlineData(new string[] { })]  // Sin permisos - válido
    [InlineData(new string[] { "VerReportes" })]
    [InlineData(new string[] { "GestionarUsuarios", "VerReportes", "ConfigurarSistema" })]
    public async Task Validate_ConPermisosEspecificosValidos_NoDeberiaRetornarErrorDePermisos(string[] permisos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PermisosEspecificos = permisos.ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.PermisosEspecificos));
    }

    #endregion

    #region Validación Telefono

    [Theory]
    [InlineData("123")]           // Muy corto
    [InlineData("abcdefgh")]      // Letras
    [InlineData("++123456789")]   // Doble +
    public async Task Validate_ConTelefonoFormatoInvalido_DeberiaRetornarError(string telefonoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefonoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Telefono) &&
            e.ErrorMessage.Contains("El formato del teléfono no es válido"));
    }

    [Theory]
    [InlineData(null)]              // Opcional - válido
    [InlineData("")]                // Opcional - válido
    [InlineData("+521234567890")]   // Con código país
    [InlineData("1234567890")]      // Sin código país
    [InlineData("12345678901234")]  // Hasta 14 dígitos
    public async Task Validate_ConTelefonoValido_NoDeberiaRetornarErrorDeTelefono(string telefono)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefono;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Telefono));
    }

    #endregion

    #region Validación Departamento

    [Theory]
    [InlineData("DepartamentoInexistente")]
    [InlineData("Ventas")]  // No está en la lista válida
    public async Task Validate_ConDepartamentoInvalido_DeberiaRetornarError(string departamentoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Departamento = departamentoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Departamento) &&
            e.ErrorMessage.Contains("El departamento debe ser uno de"));
    }

    [Theory]
    [InlineData(null)]              // Opcional - válido
    [InlineData("")]                // Opcional - válido
    [InlineData("Cocina")]
    [InlineData("Servicio")]
    [InlineData("Administración")]
    [InlineData("Limpieza")]
    [InlineData("Seguridad")]
    [InlineData("Sistemas")]
    public async Task Validate_ConDepartamentoValido_NoDeberiaRetornarErrorDeDepartamento(string departamento)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Departamento = departamento;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Departamento));
    }

    #endregion

    #region Validación Puesto

    [Fact]
    public async Task Validate_ConPuestoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Puesto = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.Puesto) &&
            e.ErrorMessage.Contains("El puesto no puede exceder 100 caracteres"));
    }

    [Theory]
    [InlineData(null)]              // Opcional - válido
    [InlineData("")]                // Opcional - válido
    [InlineData("Cocinero Junior")]
    [InlineData("Mesero")]
    [InlineData("Supervisor de Turno")]
    public async Task Validate_ConPuestoValido_NoDeberiaRetornarErrorDePuesto(string puesto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Puesto = puesto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Puesto));
    }

    #endregion

    #region Validación UsuarioCreadorId

    [Fact]
    public async Task Validate_ConUsuarioCreadorIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioCreadorId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CrearUsuarioCommand.UsuarioCreadorId) &&
            e.ErrorMessage.Contains("El ID del usuario creador es requerido"));
    }

    [Fact]
    public async Task Validate_ConUsuarioCreadorIdValido_NoDeberiaRetornarErrorDeUsuarioCreador()
    {
        // Arrange
        var command = CrearCommandValido();
        var usuarioCreadorId = Guid.NewGuid();
        command.UsuarioCreadorId = usuarioCreadorId;

        var usuarioCreador = new Usuario { Id = usuarioCreadorId, Estado = EstadoUsuario.Activo };
        _mockContext.Setup(c => c.Usuarios
            .AnyAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.UsuarioCreadorId));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var usuarioCreadorId = Guid.NewGuid();
        _mockContext.Setup(c => c.Usuarios
            .AnyAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "usuario.completo",
            NombreCompleto = "Usuario Completo de Prueba",
            Email = "usuario.completo@restaurante.com.mx",
            Password = "PasswordCompleto123!",
            ConfirmarPassword = "PasswordCompleto123!",
            Rol = "Supervisor",
            RolesAdicionales = new List<string> { "Empleado" },
            NivelAcceso = 5,
            PermisosEspecificos = new List<string> { "VerReportes", "GestionarEmpleados" },
            Telefono = "+521234567890",
            Departamento = "Servicio",
            Puesto = "Supervisor de Meseros",
            UsuarioCreadorId = usuarioCreadorId,
            HorariosTrabajo = new List<HorarioTrabajoDto>
            {
                new HorarioTrabajoDto
                {
                    DiaSemana = "Lunes",
                    HoraInicio = TimeSpan.FromHours(8),
                    HoraFin = TimeSpan.FromHours(16),
                    EsDiaLibre = false
                },
                new HorarioTrabajoDto
                {
                    DiaSemana = "Martes",
                    HoraInicio = TimeSpan.FromHours(9),
                    HoraFin = TimeSpan.FromHours(17),
                    EsDiaLibre = false
                }
            }
        };

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
        var usuarioCreadorId = Guid.NewGuid();
        _mockContext.Setup(c => c.Usuarios
            .AnyAsync(It.IsAny<Expression<Func<Usuario, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "min", // Mínimo válido
            NombreCompleto = "Ab", // Mínimo válido
            Email = "a@b.co", // Mínimo válido
            Password = "Pass123!",
            ConfirmarPassword = "Pass123!",
            Rol = "Empleado",
            RolesAdicionales = new List<string>(),
            NivelAcceso = 1,
            PermisosEspecificos = new List<string>(),
            UsuarioCreadorId = usuarioCreadorId,
            HorariosTrabajo = new List<HorarioTrabajoDto>()
        };

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
        var command = new CrearUsuarioCommand
        {
            NombreUsuario = "", // Error: vacío
            NombreCompleto = "", // Error: vacío
            Email = "email-invalido", // Error: formato
            Password = "", // Error: vacía
            ConfirmarPassword = "", // Error: vacía
            Rol = "", // Error: vacío
            NivelAcceso = 0, // Error: menor que 1
            UsuarioCreadorId = Guid.Empty, // Error: vacío
            RolesAdicionales = new List<string> { "RolInvalido" }, // Error: rol inválido
            PermisosEspecificos = new List<string> { "PermisoInvalido" } // Error: permiso inválido
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(7);
        
        // Verificar que tiene errores de diferentes propiedades
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearUsuarioCommand.NombreUsuario));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearUsuarioCommand.NombreCompleto));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearUsuarioCommand.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearUsuarioCommand.Password));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearUsuarioCommand.Rol));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearUsuarioCommand.NivelAcceso));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CrearUsuarioCommand.UsuarioCreadorId));
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Empleado", 1)]
    [InlineData("Supervisor", 3)]
    [InlineData("Gerente", 6)]
    [InlineData("Administrador", 8)]
    [InlineData("SuperAdministrador", 10)]
    public async Task Validate_ConDiferentesCombinacionesRolNivel_DeberiaSerValido(string rol, int nivel)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Rol = rol;
        command.NivelAcceso = nivel;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Rol));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.NivelAcceso));
    }

    [Theory]
    [InlineData("Cocina", "Cocinero")]
    [InlineData("Servicio", "Mesero")]
    [InlineData("Administración", "Contador")]
    [InlineData("Sistemas", "Desarrollador")]
    public async Task Validate_ConDiferentesCombinacionesDepartamentoPuesto_DeberiaSerValido(string departamento, string puesto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Departamento = departamento;
        command.Puesto = puesto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Departamento));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearUsuarioCommand.Puesto));
    }

    #endregion
} 