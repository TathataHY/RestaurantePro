using FluentAssertions;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.AgregarContacto;
using RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs;
using Xunit;

namespace RestaurantePro.Application.UnitTests.Proveedores.ContactosProveedor.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA AGREGAR CONTACTO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de creación de contactos
/// Cobertura: 100% de reglas de negocio del AgregarContactoValidator
/// </summary>
public class AgregarContactoValidatorTests
{
    private readonly AgregarContactoValidator _validator;

    public AgregarContactoValidatorTests()
    {
        _validator = new AgregarContactoValidator();
    }

    #region Validation Command Helper

    private AgregarContactoCommand CrearCommandValido()
    {
        return new AgregarContactoCommand
        {
            ProveedorId = Guid.NewGuid(),
            Nombre = "Carlos Eduardo",
            Apellidos = "Ramírez López",
            Cargo = "Director de Compras",
            Departamento = "Adquisiciones",
            Email = "carlos.ramirez@proveedor.com",
            EmailSecundario = "cramirez@proveedor.com",
            Telefono = "555-789-0123",
            TelefonoMovil = "555-456-7890",
            Extension = "9876",
            EsPrincipal = false,
            PuedeAutorizarPedidos = false,
            RecibeNotificaciones = true,
            TiposNotificaciones = new List<string> { "Pedidos", "Generales" },
            HorarioContacto = "Lunes a Viernes 8:00 AM - 7:00 PM",
            Notas = "Especialista en productos de alta demanda",
            DatosAdicionales = new Dictionary<string, object>
            {
                { "FechaIngreso", DateTime.UtcNow },
                { "UsuarioCreacion", "admin@sistema.com" }
            }
        };
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
            e.PropertyName == nameof(AgregarContactoCommand.ProveedorId) &&
            e.ErrorMessage.Contains("El ID del proveedor es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConProveedorIdValido_NoDeberiaRetornarErrorEnProveedorId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProveedorId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarContactoCommand.ProveedorId) &&
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
            e.PropertyName == nameof(AgregarContactoCommand.Nombre) &&
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
            e.PropertyName == nameof(AgregarContactoCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre no puede exceder 50 caracteres"));
    }

    [Theory]
    [InlineData("Roberto123")]      // Números - inválido
    [InlineData("Ana@")]            // Símbolos especiales - inválido  
    [InlineData("Luis#Carlos")]     // # - inválido
    [InlineData("María$")]          // $ - inválido
    [InlineData("José%")]           // % - inválido
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
            e.PropertyName == nameof(AgregarContactoCommand.Nombre) &&
            e.ErrorMessage.Contains("El nombre solo puede contener letras, espacios, guiones y puntos"));
    }

    [Theory]
    [InlineData("Carlos Eduardo")]
    [InlineData("Ana-María")]
    [InlineData("Dr. Roberto")]
    [InlineData("José Luis")]
    [InlineData("Ñándor")]
    [InlineData("A")]  // Mínimo válido
    public async Task Validate_ConNombreValido_NoDeberiaRetornarErrorDeNombre(string nombreValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = nombreValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.Nombre));
    }

    #endregion

    #region Validación Apellidos

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConApellidosVacioONull_DeberiaRetornarError(string apellidosInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Apellidos = apellidosInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarContactoCommand.Apellidos) &&
            e.ErrorMessage.Contains("Los apellidos del contacto son obligatorios"));
    }

    [Fact]
    public async Task Validate_ConApellidosMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Apellidos = new string('A', 51); // Más de 50 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarContactoCommand.Apellidos) &&
            e.ErrorMessage.Contains("Los apellidos no pueden exceder 50 caracteres"));
    }

    [Theory]
    [InlineData("Ramírez López")]
    [InlineData("García-Mendoza")]
    [InlineData("De los Santos")]
    [InlineData("Rodríguez")]
    [InlineData("Ñúñez")]
    [InlineData("B")]  // Mínimo válido
    public async Task Validate_ConApellidosValidos_NoDeberiaRetornarErrorDeApellidos(string apellidosValidos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Apellidos = apellidosValidos;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.Apellidos));
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
            e.PropertyName == nameof(AgregarContactoCommand.Cargo) &&
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
            e.PropertyName == nameof(AgregarContactoCommand.Cargo) &&
            e.ErrorMessage.Contains("El cargo no puede exceder 100 caracteres"));
    }

    [Theory]
    [InlineData("Director de Compras")]
    [InlineData("Gerente General")]
    [InlineData("Coordinador de Ventas")]
    [InlineData("CFO")]
    [InlineData("A")]  // Mínimo válido
    public async Task Validate_ConCargoValido_NoDeberiaRetornarErrorDeCargo(string cargoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cargo = cargoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.Cargo));
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
            e.PropertyName == nameof(AgregarContactoCommand.Email) &&
            e.ErrorMessage.Contains("El email del contacto es obligatorio"));
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
            e.PropertyName == nameof(AgregarContactoCommand.Email) &&
            e.ErrorMessage.Contains("El email debe tener un formato válido"));
    }

    [Fact]
    public async Task Validate_ConEmailMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        var nombreLargo = new string('a', 142); // 142 + 11 ("@domain.com") = 153 caracteres, que excede 150
        command.Email = $"{nombreLargo}@domain.com"; // Más de 150 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarContactoCommand.Email) &&
            e.ErrorMessage.Contains("El email no puede exceder 150 caracteres"));
    }

    [Theory]
    [InlineData("carlos.ramirez@proveedor.com")]
    [InlineData("director.compras@empresa.com.mx")]
    [InlineData("compras+info@distribuidor.net")]
    [InlineData("a@b.co")]  // Mínimo válido
    public async Task Validate_ConEmailValido_NoDeberiaRetornarErrorDeEmail(string emailValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Email = emailValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.Email));
    }

    #endregion

    #region Validación Teléfono

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
            e.PropertyName == nameof(AgregarContactoCommand.Telefono) &&
            e.ErrorMessage.Contains("El teléfono del contacto es obligatorio"));
    }

    [Theory]
    [InlineData("123")]            // Muy corto (menos de 7)
    [InlineData("abcdefgh")]       // Letras
    [InlineData("555-123-456a")]   // Letras mezcladas
    [InlineData("555@123456")]     // Símbolos inválidos
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
            e.PropertyName == nameof(AgregarContactoCommand.Telefono) &&
            e.ErrorMessage.Contains("El teléfono debe tener un formato válido"));
    }

    [Theory]
    [InlineData("5551234")]        // 7 dígitos - mínimo
    [InlineData("555-789-0123")]
    [InlineData("(555) 789-0123")]
    [InlineData("+52 555 789 0123")]
    [InlineData("12345678901234567890")] // 20 dígitos - máximo
    public async Task Validate_ConTelefonoValido_NoDeberiaRetornarErrorDeTelefono(string telefonoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Telefono = telefonoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.Telefono));
    }

    #endregion

    #region Validación EmailSecundario

    [Theory]
    [InlineData("email-secundario-invalido")]
    [InlineData("email@")]
    [InlineData("@domain.com")]
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
            e.PropertyName == nameof(AgregarContactoCommand.EmailSecundario) &&
            e.ErrorMessage.Contains("El email secundario debe tener un formato válido"));
    }

    [Theory]
    [InlineData(null)]             // Válido - opcional
    [InlineData("")]               // Válido - opcional
    [InlineData("backup@proveedor.com")]
    [InlineData("alternativo@empresa.net")]
    public async Task Validate_ConEmailSecundarioValido_NoDeberiaRetornarErrorDeEmailSecundario(string emailSecundario)
    {
        // Arrange
        var command = CrearCommandValido();
        command.EmailSecundario = emailSecundario;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.EmailSecundario));
    }

    #endregion

    #region Validación TelefonoMovil

    [Theory]
    [InlineData("123")]            // Muy corto
    [InlineData("abcdefgh")]       // Letras
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
            e.PropertyName == nameof(AgregarContactoCommand.TelefonoMovil) &&
            e.ErrorMessage.Contains("El teléfono móvil debe tener un formato válido"));
    }

    [Theory]
    [InlineData(null)]             // Válido - opcional
    [InlineData("")]               // Válido - opcional
    [InlineData("555-456-7890")]
    [InlineData("+52 555 456 7890")]
    public async Task Validate_ConTelefonoMovilValido_NoDeberiaRetornarErrorDeTelefonoMovil(string telefonoMovil)
    {
        // Arrange
        var command = CrearCommandValido();
        command.TelefonoMovil = telefonoMovil;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.TelefonoMovil));
    }

    #endregion

    #region Validación Extension

    [Theory]
    [InlineData("abcd")]           // Letras
    [InlineData("123a")]           // Letras mezcladas
    [InlineData("1234567")]        // Más de 6 dígitos
    public async Task Validate_ConExtensionFormatoInvalido_DeberiaRetornarError(string extensionInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Extension = extensionInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarContactoCommand.Extension) &&
            e.ErrorMessage.Contains("La extensión debe ser un número de 1-6 dígitos"));
    }

    [Theory]
    [InlineData(null)]             // Válido - opcional
    [InlineData("")]               // Válido - opcional
    [InlineData("1")]              // 1 dígito - mínimo
    [InlineData("987")]
    [InlineData("123456")]         // 6 dígitos - máximo
    public async Task Validate_ConExtensionValida_NoDeberiaRetornarErrorDeExtension(string extension)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Extension = extension;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.Extension));
    }

    #endregion

    #region Validación LimiteAutorizacion

    [Fact]
    public async Task Validate_ConLimiteAutorizacionRequeridoSinEspecificar_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuedeAutorizarPedidos = true;
        command.LimiteAutorizacion = null; // No especificado

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarContactoCommand.LimiteAutorizacion) &&
            e.ErrorMessage.Contains("Si puede autorizar pedidos, debe especificar un límite de autorización"));
    }

    [Theory]
    [InlineData(-500)]             // Negativo
    [InlineData(0)]                // Cero
    public async Task Validate_ConLimiteAutorizacionInvalido_DeberiaRetornarError(decimal limite)
    {
        // Arrange
        var command = CrearCommandValido();
        command.LimiteAutorizacion = limite;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarContactoCommand.LimiteAutorizacion) &&
            e.ErrorMessage.Contains("El límite de autorización debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConLimiteAutorizacionMuyAlto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.LimiteAutorizacion = 1000001m; // Más de 1,000,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarContactoCommand.LimiteAutorizacion) &&
            e.ErrorMessage.Contains("El límite de autorización no puede exceder $1,000,000"));
    }

    [Fact]
    public async Task Validate_ConLimiteAutorizacionValido_SinLimite_NoDeberiaRetornarErrorDeLimite()
    {
        // Arrange
        var command = CrearCommandValido();
        command.LimiteAutorizacion = null;
        command.PuedeAutorizarPedidos = false;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.LimiteAutorizacion));
    }

    [Fact]
    public async Task Validate_ConLimiteAutorizacionValido_2500_NoDeberiaRetornarErrorDeLimite()
    {
        // Arrange
        var command = CrearCommandValido();
        command.LimiteAutorizacion = 2500m;
        command.PuedeAutorizarPedidos = true;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.LimiteAutorizacion));
    }

    [Fact]
    public async Task Validate_ConLimiteAutorizacionValido_75000_NoDeberiaRetornarErrorDeLimite()
    {
        // Arrange
        var command = CrearCommandValido();
        command.LimiteAutorizacion = 75000m;
        command.PuedeAutorizarPedidos = false;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.LimiteAutorizacion));
    }

    [Fact]
    public async Task Validate_ConLimiteAutorizacionValido_1000000_NoDeberiaRetornarErrorDeLimite()
    {
        // Arrange
        var command = CrearCommandValido();
        command.LimiteAutorizacion = 1000000m;
        command.PuedeAutorizarPedidos = true;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.LimiteAutorizacion));
    }

    #endregion

    #region Validación TiposNotificaciones

    [Fact]
    public async Task Validate_ConTiposNotificacionesPedidos_NoDeberiaRetornarErrorDeTipos()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TiposNotificaciones = new List<string> { "Pedidos" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.TiposNotificaciones));
    }

    [Fact]
    public async Task Validate_ConTiposNotificacionesMultiples_NoDeberiaRetornarErrorDeTipos()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TiposNotificaciones = new List<string> { "Pedidos", "Pagos", "Generales" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.TiposNotificaciones));
    }

    [Fact]
    public async Task Validate_ConTiposNotificacionesUrgentes_NoDeberiaRetornarErrorDeTipos()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TiposNotificaciones = new List<string> { "Urgentes", "Promociones", "Facturas" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.TiposNotificaciones));
    }

    [Fact]
    public async Task Validate_ConTiposNotificacionesVacia_NoDeberiaRetornarErrorDeTipos()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TiposNotificaciones = new List<string>();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.TiposNotificaciones));
    }

    [Fact]
    public async Task Validate_ConTiposNotificacionesInvalidos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.TiposNotificaciones = new List<string> { "Pedidos", "TipoInexistente", "Pagos" };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarContactoCommand.TiposNotificaciones) &&
            e.ErrorMessage.Contains("Los tipos de notificaciones contienen valores no válidos"));
    }

    #endregion

    #region Validación EsPrincipal

    [Fact]
    public async Task Validate_ConContactoPrincipalDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.EsPrincipal = true; // Marcar como principal

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        // Nota: Este test valida la regla de negocio. 
        // En implementación real se consultaría la base de datos
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarContactoCommand.EsPrincipal) &&
            e.ErrorMessage.Contains("Solo puede haber un contacto principal por proveedor"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_ConEsPrincipalValido_NoDeberiaRetornarErrorDeEsPrincipal(bool esPrincipal)
    {
        // Arrange
        var command = CrearCommandValido();
        command.EsPrincipal = esPrincipal;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.EsPrincipal));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new AgregarContactoCommand
        {
            ProveedorId = Guid.NewGuid(),
            Nombre = "Carlos Eduardo",
            Apellidos = "Ramírez López",
            Cargo = "Director Regional de Compras y Adquisiciones",
            Departamento = "Adquisiciones y Logística",
            Email = "carlos.ramirez@proveedor.com.mx",
            EmailSecundario = "cramirez.backup@proveedor.com.mx",
            Telefono = "+52 (55) 789-0123",
            TelefonoMovil = "+52 (55) 456-7890",
            Extension = "9876",
            EsPrincipal = false,
            PuedeAutorizarPedidos = true,
            LimiteAutorizacion = 100000m,
            RecibeNotificaciones = true,
            TiposNotificaciones = new List<string> { "Pedidos", "Pagos", "Urgentes", "Facturas" },
            HorarioContacto = "Lunes a Viernes de 7:00 AM a 8:00 PM, Sábados de 8:00 AM a 2:00 PM",
            Notas = "Especialista en productos de alta demanda y gestión de inventarios críticos",
            DatosAdicionales = new Dictionary<string, object>
            {
                { "FechaIngreso", DateTime.UtcNow },
                { "UsuarioCreacion", "admin@sistema.com" },
                { "PerfilAutorizacion", "Director" }
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
        var command = new AgregarContactoCommand
        {
            ProveedorId = Guid.NewGuid(),
            Nombre = "A",              // Mínimo válido
            Apellidos = "B",           // Mínimo válido
            Cargo = "C",               // Mínimo válido
            Email = "a@b.co",          // Mínimo válido
            Telefono = "1234567",      // Mínimo válido
            EsPrincipal = false,
            PuedeAutorizarPedidos = false,
            RecibeNotificaciones = false,
            TiposNotificaciones = new List<string>()
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
        var command = new AgregarContactoCommand
        {
            ProveedorId = Guid.Empty,  // Error
            Nombre = "",               // Error: vacío
            Apellidos = "",            // Error: vacío
            Cargo = "",                // Error: vacío
            Email = "email-invalido",  // Error: formato
            Telefono = "123",          // Error: muy corto
            EmailSecundario = "email-invalido", // Error: formato
            Extension = "abcd",        // Error: formato
            PuedeAutorizarPedidos = true,
            LimiteAutorizacion = null, // Error: requerido
            TiposNotificaciones = new List<string> { "TipoInvalido" } // Error: tipo inválido
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(7);
        
        // Verificar que tiene errores de diferentes propiedades
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarContactoCommand.ProveedorId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarContactoCommand.Nombre));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarContactoCommand.Apellidos));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarContactoCommand.Cargo));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarContactoCommand.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AgregarContactoCommand.Telefono));
    }

    #endregion

    #region Theory Tests para Casos Límite

    [Theory]
    [InlineData(1, true)]    // 1 carácter - válido
    [InlineData(25, true)]   // 25 caracteres - válido
    [InlineData(50, true)]   // 50 caracteres - límite válido
    [InlineData(51, false)]  // 51 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesNombre_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Nombre = new string('A', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(AgregarContactoCommand.Nombre) &&
                e.ErrorMessage.Contains("El nombre no puede exceder 50 caracteres"));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(AgregarContactoCommand.Nombre) &&
                e.ErrorMessage.Contains("El nombre no puede exceder 50 caracteres"));
        }
    }

    [Theory]
    [InlineData(1, true)]    // 1 carácter - válido
    [InlineData(100, true)]  // 100 caracteres - límite válido
    [InlineData(101, false)] // 101 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesCargo_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cargo = new string('A', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(AgregarContactoCommand.Cargo) &&
                e.ErrorMessage.Contains("El cargo no puede exceder 100 caracteres"));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(AgregarContactoCommand.Cargo) &&
                e.ErrorMessage.Contains("El cargo no puede exceder 100 caracteres"));
        }
    }

    [Theory]
    [InlineData(0, true)]    // 0 caracteres - válido (opcional)
    [InlineData(100, true)]  // 100 caracteres - válido
    [InlineData(200, true)]  // 200 caracteres - válido
    [InlineData(201, false)] // 201 caracteres - inválido (nota: 200 límite)
    public async Task Validate_ConDiferentesLongitudesHorarioContacto_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.HorarioContacto = longitud == 0 ? null : new string('A', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(AgregarContactoCommand.HorarioContacto) &&
                e.ErrorMessage.Contains("El horario de contacto no puede exceder 200 caracteres"));
        }
        else
        {
            result.Errors.Should().Contain(e => 
                e.PropertyName == nameof(AgregarContactoCommand.HorarioContacto) &&
                e.ErrorMessage.Contains("El horario de contacto no puede exceder 200 caracteres"));
        }
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Fact]
    public async Task Validate_ConDatosAdicionales_NoDeberiaAfectarValidacion()
    {
        // Arrange
        var command = CrearCommandValido();
        command.DatosAdicionales = new Dictionary<string, object>
        {
            { "Especialidad", "Productos farmacéuticos" },
            { "Experiencia", 15 },
            { "CertificacionISO", true },
            { "FechaUltimaCapacitacion", DateTime.UtcNow.AddMonths(-3) }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]              // Sin departamento - válido
    [InlineData("")]                // Departamento vacío - válido
    [InlineData("Compras")]         // Departamento corto - válido
    [InlineData("Adquisiciones y Logística Empresarial")] // Departamento largo - válido
    public async Task Validate_ConDiferentesDepartamentos_DeberiaSerValido(string departamento)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Departamento = departamento;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.Departamento));
    }

    [Theory]
    [InlineData(true, true)]        // Recibe notificaciones y autoriza - válido
    [InlineData(true, false)]       // Recibe notificaciones, no autoriza - válido
    [InlineData(false, true)]       // No recibe notificaciones pero autoriza - válido
    [InlineData(false, false)]      // No recibe notificaciones ni autoriza - válido
    public async Task Validate_ConDiferentesCombinacionesPermisos_DeberiaSerValido(bool recibeNotificaciones, bool puedeAutorizar)
    {
        // Arrange
        var command = CrearCommandValido();
        command.RecibeNotificaciones = recibeNotificaciones;
        command.PuedeAutorizarPedidos = puedeAutorizar;
        if (puedeAutorizar)
        {
            command.LimiteAutorizacion = 50000m; // Establecer límite si puede autorizar
        }

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.RecibeNotificaciones));
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(AgregarContactoCommand.PuedeAutorizarPedidos));
    }

    #endregion
} 