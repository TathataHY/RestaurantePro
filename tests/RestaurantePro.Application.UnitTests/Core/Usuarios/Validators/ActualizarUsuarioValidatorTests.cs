namespace RestaurantePro.Application.UnitTests.Core.Usuarios.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ACTUALIZAR USUARIO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de actualización de usuarios
/// Cobertura: 100% de reglas de negocio del ActualizarUsuarioValidator
/// </summary>
public class ActualizarUsuarioValidatorTests
{
    private readonly Mock<IApplicationDbContext> _mockContext;
    private readonly ActualizarUsuarioValidator _validator;

    public ActualizarUsuarioValidatorTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _validator = new ActualizarUsuarioValidator(_mockContext.Object);
    }

    #region Validation Command Helper

    private ActualizarUsuarioCommand CrearCommandValido()
    {
        return new ActualizarUsuarioCommand
        {
            UsuarioId = Guid.NewGuid(),
            UsuarioAutorizaId = Guid.NewGuid(),
            MotivoActualizacion = "Actualización de datos de usuario por cambio organizacional",
            Prioridad = 2,
            Nombre = "Usuario Actualizado",
            Identificacion = "ID123456789",
            Email = "usuario.actualizado@restaurante.com",
            Telefono = "+521234567890",
            Direccion = "Nueva Dirección 123, Ciudad",
            Rol = "Supervisor",
            PermisosEspecificos = new List<string> { "VerReportes" },
            Departamento = "Servicio",
            Posicion = "Supervisor de Meseros",
            FechaIngreso = DateTime.Today.AddYears(-2),
            SalarioBase = 25000m,
            ObservacionesAdicionales = "Actualización por promoción interna"
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
            e.PropertyName == nameof(ActualizarUsuarioCommand.UsuarioId) &&
            e.ErrorMessage.Contains("El ID del usuario es requerido"));
    }

    [Fact]
    public async Task Validate_ConUsuarioIdValido_NoDeberiaRetornarErrorEnUsuarioId()
    {
        // Arrange
        var command = CrearCommandValido();
        var usuario = new Usuario { Id = command.UsuarioId, Estado = EstadoUsuario.Activo };

        _mockContext.Setup(c => c.Usuarios.FindAsync(command.UsuarioId))
            .ReturnsAsync(usuario);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.UsuarioId) &&
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
            e.PropertyName == nameof(ActualizarUsuarioCommand.UsuarioAutorizaId) &&
            e.ErrorMessage.Contains("El ID del usuario que autoriza es requerido"));
    }

    #endregion

    #region Validación MotivoActualizacion

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConMotivoActualizacionVacioONull_DeberiaRetornarError(string motivoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoActualizacion = motivoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.MotivoActualizacion) &&
            e.ErrorMessage.Contains("El motivo de actualización es requerido"));
    }

    [Fact]
    public async Task Validate_ConMotivoActualizacionMuyCorto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoActualizacion = "Corto"; // Menos de 10 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.MotivoActualizacion) &&
            e.ErrorMessage.Contains("El motivo debe tener al menos 10 caracteres"));
    }

    [Fact]
    public async Task Validate_ConMotivoActualizacionMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoActualizacion = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.MotivoActualizacion) &&
            e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres"));
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
            e.PropertyName == nameof(ActualizarUsuarioCommand.Prioridad) &&
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
            e.PropertyName == nameof(ActualizarUsuarioCommand.Prioridad) &&
            e.ErrorMessage.Contains("La prioridad máxima es 4"));
    }

    #endregion

    #region Validación AlMenosUnCambio

    [Fact]
    public async Task Validate_SinNingunCambio_DeberiaRetornarError()
    {
        // Arrange
        var command = new ActualizarUsuarioCommand
        {
            UsuarioId = Guid.NewGuid(),
            UsuarioAutorizaId = Guid.NewGuid(),
            MotivoActualizacion = "Intento de actualización sin cambios",
            Prioridad = 1
            // Sin ningún campo para actualizar
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "AlMenosUnCambio" &&
            e.ErrorMessage.Contains("Se debe especificar al menos un campo para actualizar"));
    }

    [Fact]
    public async Task Validate_ConAlMenosUnCambio_NoDeberiaRetornarErrorDeAlMenosUnCambio()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = "Nuevo Nombre"; // Al menos un campo

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == "AlMenosUnCambio" &&
            e.ErrorMessage.Contains("Se debe especificar al menos un campo para actualizar"));
    }

    #endregion

    #region Validación Nombre

    [Theory]
    [InlineData("a")]   // 1 carácter
    public async Task Validate_ConNombreMuyCorto_DeberiaRetornarError(string nombreCorto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombreCorto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre debe tener al menos 2 caracteres"));
    }

    [Fact]
    public async Task Validate_ConNombreMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre no puede exceder 100 caracteres"));
    }

    [Theory]
    [InlineData("Juan123")]       // Números no permitidos
    [InlineData("María@")]        // @ no permitido
    public async Task Validate_ConNombreConCaracteresInvalidos_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre solo puede contener letras y espacios"));
    }

    [Theory]
    [InlineData("María José")]
    [InlineData("Juan Carlos")]
    [InlineData("Ñuño")]
    [InlineData("Ab")]  // Mínimo válido
    public async Task Validate_ConNombreValido_NoDeberiaRetornarErrorDeNombre(string nombreValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombreValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Nombre));
    }

    #endregion

    #region Validación Identificacion

    [Theory]
    [InlineData("12345")]   // Menos de 6 caracteres
    [InlineData("abc")]     // Muy corto
    public async Task Validate_ConIdentificacionMuyCorta_DeberiaRetornarError(string identificacionCorta)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Identificacion = identificacionCorta;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Identificacion) &&
            e.ErrorMessage.Contains("La identificación debe tener al menos 6 caracteres"));
    }

    [Fact]
    public async Task Validate_ConIdentificacionMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Identificacion = new string('1', 21); // Más de 20 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Identificacion) &&
            e.ErrorMessage.Contains("La identificación no puede exceder 20 caracteres"));
    }

    [Theory]
    [InlineData("ID@123")]        // @ no permitido
    [InlineData("ID#456")]        // # no permitido
    [InlineData("ID$789")]        // $ no permitido
    public async Task Validate_ConIdentificacionConCaracteresInvalidos_DeberiaRetornarError(string identificacionInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Identificacion = identificacionInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Identificacion) &&
            e.ErrorMessage.Contains("La identificación solo puede contener números, letras y guiones"));
    }

    [Theory]
    [InlineData("ID123456")]
    [InlineData("ABC-123")]
    [InlineData("123456")]
    [InlineData("ABC123")]  // Mínimo válido
    public async Task Validate_ConIdentificacionValida_NoDeberiaRetornarErrorDeIdentificacion(string identificacionValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Identificacion = identificacionValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Identificacion));
    }

    #endregion

    #region Validación Email

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("email@")]
    [InlineData("@domain.com")]
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
            e.PropertyName == nameof(ActualizarUsuarioCommand.Email) &&
            e.ErrorMessage.Contains("El formato del email es inválido"));
    }

    [Fact]
    public async Task Validate_ConEmailMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var nombreLargo = new string('a', 85);
        command.Email = $"{nombreLargo}@domain.com"; // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Email) &&
            e.ErrorMessage.Contains("El email no puede exceder 100 caracteres"));
    }

    [Theory]
    [InlineData("usuario@restaurante.com")]
    [InlineData("test.email@empresa.net")]
    [InlineData("a@b.co")]  // Mínimo válido
    public async Task Validate_ConEmailValido_NoDeberiaRetornarErrorDeEmail(string emailValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = emailValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Email));
    }

    #endregion

    #region Validación Telefono

    [Theory]
    [InlineData("123456")]        // Menos de 7 dígitos
    [InlineData("12345")]         // Muy corto
    public async Task Validate_ConTelefonoMuyCorto_DeberiaRetornarError(string telefonoCorto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefonoCorto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Telefono) &&
            e.ErrorMessage.Contains("El teléfono debe tener al menos 7 dígitos"));
    }

    [Fact]
    public async Task Validate_ConTelefonoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = new string('1', 16); // Más de 15 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Telefono) &&
            e.ErrorMessage.Contains("El teléfono no puede exceder 15 caracteres"));
    }

    [Theory]
    [InlineData("abcdefgh")]       // Letras
    [InlineData("123@456")]        // @ no permitido
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
            e.PropertyName == nameof(ActualizarUsuarioCommand.Telefono) &&
            e.ErrorMessage.Contains("El formato del teléfono es inválido"));
    }

    [Theory]
    [InlineData("+521234567")]
    [InlineData("1234567")]
    [InlineData("(555) 123-4567")]
    [InlineData("555-123-4567")]
    public async Task Validate_ConTelefonoValido_NoDeberiaRetornarErrorDeTelefono(string telefonoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefonoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Telefono));
    }

    #endregion

    #region Validación Direccion

    [Fact]
    public async Task Validate_ConDireccionMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Direccion = new string('A', 201); // Más de 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Direccion) &&
            e.ErrorMessage.Contains("La dirección no puede exceder 200 caracteres"));
    }

    [Theory]
    [InlineData("Calle Principal 123")]
    [InlineData("Av. Reforma 456, Col. Centro")]
    [InlineData("A")]  // Mínimo válido
    public async Task Validate_ConDireccionValida_NoDeberiaRetornarErrorDeDireccion(string direccionValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Direccion = direccionValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Direccion));
    }

    #endregion

    #region Validación Rol

    [Theory]
    [InlineData("RolInexistente")]
    [InlineData("Admin")]  // No está en la lista de roles válidos
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
            e.PropertyName == nameof(ActualizarUsuarioCommand.Rol) &&
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
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Rol));
    }

    #endregion

    #region Validación PermisosEspecificos

    [Fact]
    public async Task Validate_ConPermisosEspecificosSinPermisos_NoDeberiaRetornarErrorDePermisos()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PermisosEspecificos = new List<string>();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.PermisosEspecificos));
    }

    [Fact]
    public async Task Validate_ConPermisosEspecificosVerReportes_NoDeberiaRetornarErrorDePermisos()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PermisosEspecificos = new List<string> { "VerReportes" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.PermisosEspecificos));
    }

    [Fact]
    public async Task Validate_ConPermisosEspecificosMultiples_NoDeberiaRetornarErrorDePermisos()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PermisosEspecificos = new List<string> { "GestionarUsuarios", "VerReportes" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.PermisosEspecificos));
    }

    [Fact]
    public async Task Validate_ConMuchosPermisosEspecificos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PermisosEspecificos = Enumerable.Range(1, 21).Select(i => $"Permiso{i}").ToList(); // Más de 20

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.PermisosEspecificos) &&
            e.ErrorMessage.Contains("No se pueden asignar más de 20 permisos específicos"));
    }

    [Fact]
    public async Task Validate_ConPermisosEspecificosConValoresInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PermisosEspecificos = new List<string> { "PermisoValido", "", "   " }; // Valores vacíos

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.PermisosEspecificos) &&
            e.ErrorMessage.Contains("Todos los permisos específicos deben ser válidos"));
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
            e.PropertyName == nameof(ActualizarUsuarioCommand.Departamento) &&
            e.ErrorMessage.Contains("El departamento debe ser uno de"));
    }

    [Theory]
    [InlineData("Cocina")]
    [InlineData("Servicio")]
    [InlineData("Administración")]
    [InlineData("Gerencia")]
    [InlineData("Mantenimiento")]
    public async Task Validate_ConDepartamentoValido_NoDeberiaRetornarErrorDeDepartamento(string departamentoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Departamento = departamentoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Departamento));
    }

    #endregion

    #region Validación Posicion

    [Fact]
    public async Task Validate_ConPosicionMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Posicion = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.Posicion) &&
            e.ErrorMessage.Contains("La posición no puede exceder 100 caracteres"));
    }

    [Theory]
    [InlineData("Supervisor")]
    [InlineData("Cocinero Senior")]
    [InlineData("Gerente de Operaciones")]
    [InlineData("A")]  // Mínimo válido
    public async Task Validate_ConPosicionValida_NoDeberiaRetornarErrorDePosicion(string posicionValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Posicion = posicionValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Posicion));
    }

    #endregion

    #region Validación FechaIngreso

    [Fact]
    public async Task Validate_ConFechaIngresoFutura_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaIngreso = DateTime.Today.AddDays(1); // Fecha futura

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.FechaIngreso) &&
            e.ErrorMessage.Contains("La fecha de ingreso no puede ser futura"));
    }

    [Fact]
    public async Task Validate_ConFechaIngresoMuyAntigua_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaIngreso = new DateTime(1979, 12, 31); // Antes de 1980

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.FechaIngreso) &&
            e.ErrorMessage.Contains("La fecha de ingreso debe ser posterior a 1980"));
    }

    [Theory]
    [InlineData(-365)]  // 1 año atrás
    [InlineData(-30)]   // 1 mes atrás
    [InlineData(0)]     // Hoy
    public async Task Validate_ConFechaIngresoValida_NoDeberiaRetornarErrorDeFechaIngreso(int diasAtras)
    {
        // Arrange
        var command = CrearCommandValido();
        command.FechaIngreso = DateTime.Today.AddDays(diasAtras);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.FechaIngreso));
    }

    #endregion

    #region Validación SalarioBase

    [Theory]
    [InlineData(0)]     // Cero
    [InlineData(-1000)] // Negativo
    public async Task Validate_ConSalarioBaseMenorOIgualACero_DeberiaRetornarError(decimal salarioInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.SalarioBase = salarioInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.SalarioBase) &&
            e.ErrorMessage.Contains("El salario base debe ser mayor que cero"));
    }

    [Fact]
    public async Task Validate_ConSalarioBaseMuyAlto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.SalarioBase = 50000001m; // Más de 50,000,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarUsuarioCommand.SalarioBase) &&
            e.ErrorMessage.Contains("El salario base no puede exceder $50,000,000"));
    }

    [Theory]
    [InlineData(15000)]
    [InlineData(25000)]
    [InlineData(50000000)]  // Máximo válido
    public async Task Validate_ConSalarioBaseValido_NoDeberiaRetornarErrorDeSalarioBase(decimal salarioValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.SalarioBase = salarioValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.SalarioBase));
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
            e.PropertyName == nameof(ActualizarUsuarioCommand.ObservacionesAdicionales) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres"));
    }

    [Theory]
    [InlineData("Observaciones cortas")]
    [InlineData("Esta es una observación de longitud normal")]
    [InlineData("")]  // Vacío - válido
    public async Task Validate_ConObservacionesAdicionalesValidas_NoDeberiaRetornarErrorDeObservaciones(string observaciones)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesAdicionales = observaciones;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.ObservacionesAdicionales));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var autorizadorId = Guid.NewGuid();
        
        var usuario = new Usuario { Id = usuarioId, Estado = EstadoUsuario.Activo };
        var autorizador = new Usuario { Id = autorizadorId, Estado = EstadoUsuario.Activo };

        _mockContext.Setup(c => c.Usuarios.FindAsync(usuarioId))
            .ReturnsAsync(usuario);
        _mockContext.Setup(c => c.Usuarios.FindAsync(autorizadorId))
            .ReturnsAsync(autorizador);

        var command = new ActualizarUsuarioCommand
        {
            UsuarioId = usuarioId,
            UsuarioAutorizaId = autorizadorId,
            MotivoActualizacion = "Actualización completa de datos por promoción a supervisor",
            Prioridad = 3,
            Nombre = "Usuario Completamente Actualizado",
            Identificacion = "ID987654321",
            Email = "usuario.completo@restaurante.com.mx",
            Telefono = "+521234567890",
            Direccion = "Av. Principal 123, Col. Centro, Ciudad de México",
            Rol = "Supervisor",
            PermisosEspecificos = new List<string> { "VerReportes", "GestionarEmpleados" },
            Departamento = "Servicio",
            Posicion = "Supervisor de Área de Servicio",
            FechaIngreso = DateTime.Today.AddYears(-3),
            SalarioBase = 35000m,
            ObservacionesAdicionales = "Promoción por excelente desempeño y liderazgo demostrado"
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
        var usuarioId = Guid.NewGuid();
        var autorizadorId = Guid.NewGuid();
        
        var usuario = new Usuario { Id = usuarioId, Estado = EstadoUsuario.Activo };
        var autorizador = new Usuario { Id = autorizadorId, Estado = EstadoUsuario.Activo };

        _mockContext.Setup(c => c.Usuarios.FindAsync(usuarioId))
            .ReturnsAsync(usuario);
        _mockContext.Setup(c => c.Usuarios.FindAsync(autorizadorId))
            .ReturnsAsync(autorizador);

        var command = new ActualizarUsuarioCommand
        {
            UsuarioId = usuarioId,
            UsuarioAutorizaId = autorizadorId,
            MotivoActualizacion = "1234567890", // Exactamente 10 caracteres - mínimo válido
            Prioridad = 1,
            Nombre = "Ab" // Solo cambio mínimo para cumplir "al menos un cambio"
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
        var command = new ActualizarUsuarioCommand
        {
            UsuarioId = Guid.Empty, // Error
            UsuarioAutorizaId = Guid.Empty, // Error
            MotivoActualizacion = "", // Error: vacío
            Prioridad = 0, // Error: menor que 1
            Nombre = "a", // Error: muy corto
            Identificacion = "12", // Error: muy corta
            Email = "email-invalido", // Error: formato
            Telefono = "123", // Error: muy corto
            Direccion = new string('A', 201), // Error: muy larga
            Rol = "RolInvalido", // Error: rol inválido
            SalarioBase = -1000, // Error: negativo
            ObservacionesAdicionales = new string('A', 1001) // Error: muy larga
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(8);
        
        // Verificar que tiene errores de diferentes propiedades
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.UsuarioId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.UsuarioAutorizaId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.MotivoActualizacion));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Prioridad));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Nombre));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Email));
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Empleado", "Cocina", "Cocinero")]
    [InlineData("Supervisor", "Servicio", "Supervisor de Meseros")]
    [InlineData("Gerente", "Administración", "Gerente General")]
    public async Task Validate_ConDiferentesCombinacionesRolDepartamentoPosicion_DeberiaSerValido(string rol, string departamento, string posicion)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Rol = rol;
        command.Departamento = departamento;
        command.Posicion = posicion;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Rol));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Departamento));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Posicion));
    }

    [Theory]
    [InlineData(15000, 1)]   // Salario bajo, prioridad baja
    [InlineData(25000, 2)]   // Salario medio, prioridad media
    [InlineData(50000, 3)]   // Salario alto, prioridad alta
    public async Task Validate_ConDiferentesCombinacionesSalarioPrioridad_DeberiaSerValido(decimal salario, int prioridad)
    {
        // Arrange
        var command = CrearCommandValido();
        command.SalarioBase = salario;
        command.Prioridad = prioridad;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.SalarioBase));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarUsuarioCommand.Prioridad));
    }

    #endregion
} 