namespace RestaurantePro.Application.UnitTests.Proveedores.ContactosProveedor.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ACTUALIZAR CONTACTO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de actualización de contactos
/// Cobertura: 100% de reglas de negocio del ActualizarContactoValidator
/// </summary>
public class ActualizarContactoValidatorTests
{
    private readonly ActualizarContactoValidator _validator;

    public ActualizarContactoValidatorTests()
    {
        _validator = new ActualizarContactoValidator();
    }

    #region Validation Command Helper

    private ActualizarContactoCommand CrearCommandValido()
    {
        return new ActualizarContactoCommand
        {
            Id = Guid.NewGuid(),
            ProveedorId = Guid.NewGuid(),
            Nombre = "Juan Carlos",
            Apellidos = "García López",
            Cargo = "Gerente de Ventas",
            Email = "juan.garcia@proveedor.com",
            Telefono = "+52-555-123-4567",
            EmailSecundario = "jgarcia.ventas@proveedor.com",
            TelefonoMovil = "+52-555-987-6543",
            Extension = "1234",
            Departamento = "Ventas y Marketing",
            HorarioContacto = "Lunes a Viernes 9:00 AM - 6:00 PM",
            Notas = "Contacto principal para pedidos grandes",
            MotivoActualizacion = "Actualización de información de contacto",
            LimiteAutorizacion = 50000m,
            PuedeAutorizarPedidos = true,
            EsPrincipal = false,
            TiposNotificaciones = new List<string> { "Pedidos", "Pagos", "Generales" }
        };
    }

    #endregion

    #region Validación Id

    [Fact]
    public async Task Validate_ConIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del contacto es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConIdValido_NoDeberiaRetornarErrorDeId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Id = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Id) &&
            e.ErrorMessage.Contains("El ID del contacto es obligatorio"));
    }

    #endregion

    #region Validación ProveedorId

    [Fact]
    public async Task Validate_ConProveedorIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProveedorId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.ProveedorId) &&
            e.ErrorMessage.Contains("El ID del proveedor es obligatorio"));
    }

    #endregion

    #region Validación Nombre

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConNombreVacioONull_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre del contacto es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConNombreMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = new string('A', 51); // Más de 50 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre no puede exceder 50 caracteres"));
    }

    [Theory]
    [InlineData("Juan123")]
    [InlineData("Juan@")]
    [InlineData("Juan#Carlos")]
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
            e.PropertyName == nameof(ActualizarContactoCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre solo puede contener letras, espacios, guiones y puntos"));
    }

    [Theory]
    [InlineData("Juan")]
    [InlineData("Juan Carlos")]
    [InlineData("María José")]
    [InlineData("José-Luis")]
    [InlineData("Ana.Patricia")]
    [InlineData("José María")]
    public async Task Validate_ConNombreValido_NoDeberiaRetornarErrorDeNombre(string nombreValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombreValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Nombre));
    }

    #endregion

    #region Validación Apellidos

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConApellidosVaciosONull_DeberiaRetornarError(string apellidosInvalidos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Apellidos = apellidosInvalidos;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Apellidos) &&
            e.ErrorMessage.Contains("Los apellidos del contacto son obligatorios"));
    }

    [Fact]
    public async Task Validate_ConApellidosMuyLargos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Apellidos = new string('A', 51); // Más de 50 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Apellidos) &&
            e.ErrorMessage.Contains("Los apellidos no pueden exceder 50 caracteres"));
    }

    [Theory]
    [InlineData("García123")]
    [InlineData("López@")]
    [InlineData("Pérez#")]
    public async Task Validate_ConApellidosConCaracteresInvalidos_DeberiaRetornarError(string apellidosInvalidos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Apellidos = apellidosInvalidos;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Apellidos) &&
            e.ErrorMessage.Contains("Los apellidos solo pueden contener letras, espacios, guiones y puntos"));
    }

    #endregion

    #region Validación Cargo

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConCargoVacioONull_DeberiaRetornarError(string cargoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cargo = cargoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Cargo) &&
            e.ErrorMessage.Contains("El cargo del contacto es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConCargoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cargo = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Cargo) &&
            e.ErrorMessage.Contains("El cargo no puede exceder 100 caracteres"));
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
            e.PropertyName == nameof(ActualizarContactoCommand.Email) &&
            e.ErrorMessage.Contains("El email del contacto es obligatorio"));
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@")]
    [InlineData("usuario@dominio.")]
    [InlineData("usuario@.dominio.com")]
    [InlineData("usuario..punto@dominio.com")]
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
            e.PropertyName == nameof(ActualizarContactoCommand.Email) &&
            e.ErrorMessage.Contains("El email debe tener un formato válido"));
    }

    [Fact]
    public async Task Validate_ConEmailMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = new string('a', 140) + "@dominio.com"; // Más de 150 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Email) &&
            e.ErrorMessage.Contains("El email no puede exceder 150 caracteres"));
    }

    #endregion

    #region Validación Telefono

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConTelefonoVacioONull_DeberiaRetornarError(string telefonoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefonoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Telefono) &&
            e.ErrorMessage.Contains("El teléfono del contacto es obligatorio"));
    }

    [Theory]
    [InlineData("123456")] // Muy corto
    [InlineData("123456789012345678901")] // Muy largo
    [InlineData("555-abc-1234")] // Letras
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
            e.PropertyName == nameof(ActualizarContactoCommand.Telefono) &&
            e.ErrorMessage.Contains("El teléfono debe tener un formato válido"));
    }

    [Theory]
    [InlineData("555-123-4567")]
    [InlineData("+52-555-123-4567")]
    [InlineData("(555) 123-4567")]
    [InlineData("1234567890")]
    public async Task Validate_ConTelefonoValido_NoDeberiaRetornarErrorDeTelefono(string telefonoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefonoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Telefono) &&
            e.ErrorMessage.Contains("El teléfono debe tener un formato válido"));
    }

    #endregion

    #region Validación EmailSecundario

    [Theory]
    [InlineData("email-secundario-invalido")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@")]
    public async Task Validate_ConEmailSecundarioFormatoInvalido_DeberiaRetornarError(string emailInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.EmailSecundario = emailInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.EmailSecundario) &&
            e.ErrorMessage.Contains("El email secundario debe tener un formato válido"));
    }

    [Fact]
    public async Task Validate_ConEmailSecundarioMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.EmailSecundario = new string('b', 140) + "@dominio.com"; // Más de 150 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.EmailSecundario) &&
            e.ErrorMessage.Contains("El email secundario no puede exceder 150 caracteres"));
    }

    [Fact]
    public async Task Validate_ConEmailSecundarioIgualAlPrincipal_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = "juan@proveedor.com";
        command.EmailSecundario = "juan@proveedor.com"; // Igual al principal

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.EmailSecundario) &&
            e.ErrorMessage.Contains("El email secundario debe ser diferente al email principal"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConEmailSecundarioVacioONull_NoDeberiaValidar(string emailVacio)
    {
        // Arrange
        var command = CrearCommandValido();
        command.EmailSecundario = emailVacio;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarContactoCommand.EmailSecundario));
    }

    #endregion

    #region Validación TelefonoMovil

    [Theory]
    [InlineData("123456")] // Muy corto
    [InlineData("123456789012345678901")] // Muy largo
    [InlineData("555-abc-1234")] // Letras
    public async Task Validate_ConTelefonoMovilFormatoInvalido_DeberiaRetornarError(string telefonoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TelefonoMovil = telefonoInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.TelefonoMovil) &&
            e.ErrorMessage.Contains("El teléfono móvil debe tener un formato válido"));
    }

    [Fact]
    public async Task Validate_ConTelefonoMovilIgualAlPrincipal_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = "555-123-4567";
        command.TelefonoMovil = "555-123-4567"; // Igual al principal

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.TelefonoMovil) &&
            e.ErrorMessage.Contains("El teléfono móvil debe ser diferente al teléfono principal"));
    }

    #endregion

    #region Validación Extension

    [Theory]
    [InlineData("12345678")] // Más de 6 dígitos
    [InlineData("abc123")] // Letras
    [InlineData("12-34")] // Guiones
    public async Task Validate_ConExtensionInvalida_DeberiaRetornarError(string extensionInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Extension = extensionInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Extension) &&
            e.ErrorMessage.Contains("La extensión debe ser un número de 1-6 dígitos"));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123")]
    [InlineData("123456")]
    public async Task Validate_ConExtensionValida_NoDeberiaRetornarErrorDeExtension(string extensionValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Extension = extensionValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Extension));
    }

    #endregion

    #region Validación Departamento

    [Fact]
    public async Task Validate_ConDepartamentoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Departamento = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Departamento) &&
            e.ErrorMessage.Contains("El departamento no puede exceder 100 caracteres"));
    }

    #endregion

    #region Validación HorarioContacto

    [Fact]
    public async Task Validate_ConHorarioContactoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.HorarioContacto = new string('A', 201); // Más de 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.HorarioContacto) &&
            e.ErrorMessage.Contains("El horario de contacto no puede exceder 200 caracteres"));
    }

    #endregion

    #region Validación Notas

    [Fact]
    public async Task Validate_ConNotasMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Notas = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Notas) &&
            e.ErrorMessage.Contains("Las notas no pueden exceder 500 caracteres"));
    }

    #endregion

    #region Validación MotivoActualizacion

    [Fact]
    public async Task Validate_ConMotivoActualizacionMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MotivoActualizacion = new string('A', 301); // Más de 300 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.MotivoActualizacion) &&
            e.ErrorMessage.Contains("El motivo de actualización no puede exceder 300 caracteres"));
    }

    #endregion

    #region Validación LimiteAutorizacion

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public async Task Validate_ConLimiteAutorizacionMenorOIgualACero_DeberiaRetornarError(decimal limiteInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.LimiteAutorizacion = limiteInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.LimiteAutorizacion) &&
            e.ErrorMessage.Contains("El límite de autorización debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConLimiteAutorizacionExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.LimiteAutorizacion = 1000001m; // Más de $1,000,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.LimiteAutorizacion) &&
            e.ErrorMessage.Contains("El límite de autorización no puede exceder $1,000,000"));
    }

    [Fact]
    public async Task Validate_ConPuedeAutorizarPedidosSinLimite_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuedeAutorizarPedidos = true;
        command.LimiteAutorizacion = null; // Sin límite cuando puede autorizar

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.LimiteAutorizacion) &&
            e.ErrorMessage.Contains("Si puede autorizar pedidos, debe especificar un límite de autorización"));
    }

    #endregion

    #region Validación TiposNotificaciones

    [Fact]
    public async Task Validate_ConTiposNotificacionesInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TiposNotificaciones = new List<string> { "TipoInvalido", "OtroInvalido" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.TiposNotificaciones) &&
            e.ErrorMessage.Contains("Los tipos de notificaciones contienen valores no válidos"));
    }

    [Fact]
    public async Task Validate_ConTiposNotificacionesPedidos_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TiposNotificaciones = new List<string> { "Pedidos" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarContactoCommand.TiposNotificaciones));
    }

    [Fact]
    public async Task Validate_ConTiposNotificacionesPagosGenerales_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TiposNotificaciones = new List<string> { "Pagos", "Generales" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarContactoCommand.TiposNotificaciones));
    }

    [Fact]
    public async Task Validate_ConTiposNotificacionesTodosValidos_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TiposNotificaciones = new List<string> { "Pedidos", "Pagos", "Generales", "Urgentes", "Promociones", "Facturas", "Emergencias" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(ActualizarContactoCommand.TiposNotificaciones));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarContactoCommand
        {
            Id = Guid.NewGuid(),
            ProveedorId = Guid.NewGuid(),
            Nombre = "Carlos",
            Apellidos = "García López",
            Cargo = "Director de Compras",
            Email = "carlos.garcia@proveedor.com",
            Telefono = "+52-555-123-4567",
            EmailSecundario = "c.garcia@proveedores.com",
            TelefonoMovil = "+52-555-987-6543",
            Extension = "4567",
            Departamento = "Compras y Adquisiciones",
            HorarioContacto = "Lunes a Viernes 8:00 AM - 5:00 PM",
            Notas = "Contacto preferido para negociaciones",
            MotivoActualizacion = "Cambio de departamento y responsabilidades",
            LimiteAutorizacion = 75000m,
            PuedeAutorizarPedidos = true,
            EsPrincipal = true,
            TiposNotificaciones = new List<string> { "Pedidos", "Urgentes", "Facturas" }
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
        var command = new ActualizarContactoCommand
        {
            Id = Guid.NewGuid(),
            ProveedorId = Guid.NewGuid(),
            Nombre = "Ana",
            Apellidos = "Martínez",
            Cargo = "Asistente",
            Email = "ana.martinez@proveedor.com",
            Telefono = "555-111-2222",
            PuedeAutorizarPedidos = false
            // Campos opcionales omitidos
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
        var command = new ActualizarContactoCommand
        {
            Id = Guid.Empty, // Error
            ProveedorId = Guid.Empty, // Error
            Nombre = "", // Error
            Apellidos = "", // Error
            Cargo = "", // Error
            Email = "email-invalido", // Error
            Telefono = "123", // Error
            EmailSecundario = "email-invalido", // Error
            TelefonoMovil = "abc", // Error
            Extension = "1234567", // Error
            LimiteAutorizacion = -100, // Error
            PuedeAutorizarPedidos = true,
            TiposNotificaciones = new List<string> { "TipoInvalido" } // Error
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Gerente de Ventas", 50000)]
    [InlineData("Director Regional", 100000)]
    [InlineData("Supervisor", 25000)]
    public async Task Validate_ConDiferentesCargosYLimites_DeberiaSerValido(string cargo, decimal limite)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cargo = cargo;
        command.LimiteAutorizacion = limite;
        command.PuedeAutorizarPedidos = true;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.Cargo) ||
            e.PropertyName == nameof(ActualizarContactoCommand.LimiteAutorizacion));
    }

    [Fact]
    public async Task Validate_ConContactoSinAutorizacion_NoDeberiaRequirirLimite()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuedeAutorizarPedidos = false;
        command.LimiteAutorizacion = null; // No necesita límite

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ActualizarContactoCommand.LimiteAutorizacion));
    }

    #endregion
} 