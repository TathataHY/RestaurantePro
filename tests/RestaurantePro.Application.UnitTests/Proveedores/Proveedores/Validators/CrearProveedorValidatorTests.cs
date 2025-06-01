namespace RestaurantePro.Application.UnitTests.Proveedores.Proveedores.Validators;

/// <summary>
/// Tests unitarios para CrearProveedorValidator
/// Validación completa de reglas de negocio para gestión de proveedores
/// </summary>
public class CrearProveedorValidatorTests
{
    private readonly CrearProveedorValidator _validator;

    public CrearProveedorValidatorTests()
    {
        _validator = new CrearProveedorValidator();
    }

    #region Nombre Validations

    [Fact]
    public void Validator_ConNombreValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Nombre = "Distribuidora ABC S.A.";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConNombreVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Nombre = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Nombre))
            .Which.ErrorMessage.Should().Be("El nombre del proveedor es obligatorio");
    }

    [Fact]
    public void Validator_ConNombreNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Nombre = null!;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Nombre))
            .Which.ErrorMessage.Should().Be("El nombre del proveedor es obligatorio");
    }

    [Fact]
    public void Validator_ConNombreMuyCorto_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Nombre = "AB";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Nombre))
            .Which.ErrorMessage.Should().Be("El nombre debe tener al menos 3 caracteres");
    }

    [Fact]
    public void Validator_ConNombreMuyLargo_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Nombre = new string('A', 201);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Nombre))
            .Which.ErrorMessage.Should().Be("El nombre no puede exceder 200 caracteres");
    }

    #endregion

    #region RazonSocial Validations

    [Fact]
    public void Validator_ConRazonSocialValida_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.RazonSocial = "Distribuidora ABC Sociedad Anónima de Capital Variable";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConRazonSocialVacia_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.RazonSocial = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue(); // Razón social es opcional
    }

    [Fact]
    public void Validator_ConRazonSocialNull_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.RazonSocial = null;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue(); // Razón social es opcional
    }

    [Fact]
    public void Validator_ConRazonSocialMuyLarga_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.RazonSocial = new string('A', 301);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.RazonSocial))
            .Which.ErrorMessage.Should().Be("La razón social no puede exceder 300 caracteres");
    }

    #endregion

    #region RFC Validations

    [Fact]
    public void Validator_ConRfcValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Rfc = "ABC123456789";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConRfcVacio_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Rfc = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue(); // RFC es opcional
    }

    [Fact]
    public void Validator_ConRfcNull_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Rfc = null;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue(); // RFC es opcional
    }

    [Theory]
    [InlineData("ABC123456789")]
    [InlineData("XAXX010101000")]
    [InlineData("GODE561231GR8")]
    public void Validator_ConRfcsValidos_DeberiaSerValido(string rfc)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Rfc = rfc;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("ABC12")]  // Muy corto
    [InlineData("ABC12345678901234")]  // Muy largo
    [InlineData("abc123456789")]  // Minúsculas
    [InlineData("123456789ABC")]  // Formato inválido
    public void Validator_ConRfcsInvalidos_DeberiaFallar(string rfcInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Rfc = rfcInvalido;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Rfc));
    }

    #endregion

    #region Email Validations

    [Fact]
    public void Validator_ConEmailValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Email = "ventas@distribuidoraabc.com.mx";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConEmailVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Email = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Email))
            .Which.ErrorMessage.Should().Be("El email es obligatorio");
    }

    [Fact]
    public void Validator_ConEmailNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Email = null!;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Email))
            .Which.ErrorMessage.Should().Be("El email es obligatorio");
    }

    [Theory]
    [InlineData("email_invalido")]
    [InlineData("@email.com")]
    [InlineData("email@")]
    [InlineData("email.com")]
    [InlineData("email@.com")]
    public void Validator_ConEmailFormatoInvalido_DeberiaFallar(string emailInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Email = emailInvalido;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Email))
            .Which.ErrorMessage.Should().Be("El formato del email no es válido");
    }

    #endregion

    #region Telefono Validations

    [Fact]
    public void Validator_ConTelefonoValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Telefono = "+52-55-1234-5678";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConTelefonoVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Telefono = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Telefono))
            .Which.ErrorMessage.Should().Be("El teléfono es obligatorio");
    }

    [Theory]
    [InlineData("+52-55-1234-5678")]
    [InlineData("5512345678")]
    [InlineData("(55) 1234-5678")]
    [InlineData("55 1234 5678")]
    public void Validator_ConTelefonosValidos_DeberiaSerValido(string telefono)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Telefono = telefono;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Direccion Validations

    [Fact]
    public void Validator_ConDireccionValida_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Direccion = "Av. Insurgentes Sur 1234, Col. Del Valle, CDMX";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConDireccionVacia_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Direccion = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue(); // Dirección es opcional
    }

    [Fact]
    public void Validator_ConDireccionMuyLarga_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Direccion = new string('A', 501);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.Direccion))
            .Which.ErrorMessage.Should().Be("La dirección no puede exceder 500 caracteres");
    }

    #endregion

    #region TipoProveedor Validations

    [Theory]
    [InlineData(TipoProveedor.Nacional)]
    [InlineData(TipoProveedor.Internacional)]
    [InlineData(TipoProveedor.Local)]
    public void Validator_ConTiposProveedorValidos_DeberiaSerValido(TipoProveedor tipo)
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoProveedor = tipo;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConTipoProveedorInvalido_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.TipoProveedor = (TipoProveedor)999;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProveedorCommand.TipoProveedor))
            .Which.ErrorMessage.Should().Be("El tipo de proveedor no es válido");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Validator_ConTodosLosCamposValidos_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validator_ConProveedorCompleto_DeberiaSerValido()
    {
        // Arrange
        var command = new CrearProveedorCommand
        {
            Nombre = "Distribuidora de Alimentos del Bajío S.A. de C.V.",
            RazonSocial = "Distribuidora de Alimentos del Bajío Sociedad Anónima de Capital Variable",
            Rfc = "DAB123456789",
            Email = "ventas@distribuidorabajio.com.mx",
            Telefono = "+52-462-123-4567",
            Direccion = "Carretera Panamericana Km 15.5, Parque Industrial, León, Guanajuato, México",
            TipoProveedor = TipoProveedor.Nacional,
            EstaActivo = true
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validator_ConMultiplesErrores_DeberiaListarTodos()
    {
        // Arrange
        var command = new CrearProveedorCommand
        {
            Nombre = "", // Error: vacío
            RazonSocial = new string('A', 301), // Error: muy larga
            Rfc = "abc123", // Error: formato inválido
            Email = "email_invalido", // Error: formato inválido
            Telefono = "", // Error: vacío
            Direccion = new string('A', 501), // Error: muy larga
            TipoProveedor = (TipoProveedor)999 // Error: inválido
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(5);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearProveedorCommand.Nombre));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearProveedorCommand.RazonSocial));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearProveedorCommand.Rfc));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearProveedorCommand.Email));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearProveedorCommand.Telefono));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearProveedorCommand.Direccion));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearProveedorCommand.TipoProveedor));
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Validator_RendimientoValidacion_DeberiaSerRapido()
    {
        // Arrange
        var command = CrearComandoValido();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _validator.Validate(command);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(150); // Menos de 150ms para 1000 validaciones
    }

    #endregion

    #region Helper Methods

    private CrearProveedorCommand CrearComandoValido()
    {
        return new CrearProveedorCommand
        {
            Nombre = "Distribuidora ABC S.A.",
            RazonSocial = "Distribuidora ABC Sociedad Anónima",
            Rfc = "ABC123456789",
            Email = "ventas@distribuidoraabc.com",
            Telefono = "+52-55-1234-5678",
            Direccion = "Av. Insurgentes Sur 1234, Col. Del Valle, CDMX",
            TipoProveedor = TipoProveedor.Nacional,
            EstaActivo = true
        };
    }

    #endregion
} 