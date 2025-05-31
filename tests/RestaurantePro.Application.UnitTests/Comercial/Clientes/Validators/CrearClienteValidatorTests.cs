namespace RestaurantePro.Application.UnitTests.Comercial.Clientes.Validators;

public class CrearClienteValidatorTests
{
    private readonly CrearClienteValidator _validator;

    public CrearClienteValidatorTests()
    {
        _validator = new CrearClienteValidator();
    }

    [Fact]
    public async Task Validate_CommandValido_DeberiaSerValido()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "juan.perez@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25),
            EstaActivo = true
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_NombreVacio_DeberiaSerInvalido(string? nombreInvalido)
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = nombreInvalido!,
            Email = "test@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre del cliente es obligatorio");
    }

    [Fact]
    public async Task Validate_NombreMuyCorto_DeberiaSerInvalido()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "A", // Solo 1 carácter
            Email = "test@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre debe tener al menos 2 caracteres");
    }

    [Fact]
    public async Task Validate_NombreMuyLargo_DeberiaSerInvalido()
    {
        // Arrange
        var nombreLargo = new string('A', 201); // 201 caracteres
        var command = new CrearClienteCommand
        {
            Nombre = nombreLargo,
            Email = "test@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre no puede exceder 200 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_EmailVacio_DeberiaSerInvalido(string? emailInvalido)
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = emailInvalido!,
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El email es obligatorio");
    }

    [Theory]
    [InlineData("email_invalido")]
    [InlineData("@email.com")]
    [InlineData("email@")]
    [InlineData("email.com")]
    public async Task Validate_EmailFormatoInvalido_DeberiaSerInvalido(string emailInvalido)
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = emailInvalido,
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El formato del email no es válido");
    }

    [Fact]
    public async Task Validate_EmailMuyLargo_DeberiaSerInvalido()
    {
        // Arrange
        var emailLargo = new string('a', 315) + "@email.com"; // 325 caracteres total
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = emailLargo,
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El email no puede exceder 320 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Validate_TelefonoVacio_DeberiaSerInvalido(string? telefonoInvalido)
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "test@email.com",
            Telefono = telefonoInvalido!,
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El teléfono es obligatorio");
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("123-456-789")]
    [InlineData("0300123456")] // No puede empezar con 0
    public async Task Validate_TelefonoFormatoInvalido_DeberiaSerInvalido(string telefonoInvalido)
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "test@email.com",
            Telefono = telefonoInvalido,
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El formato del teléfono no es válido");
    }

    [Theory]
    [InlineData("+57300123456")]
    [InlineData("573001234567")]
    [InlineData("12345678901")]
    public async Task Validate_TelefonoValido_DeberiaSerValido(string telefonoValido)
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "test@email.com",
            Telefono = telefonoValido,
            FechaNacimiento = DateTime.Now.AddYears(-25)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CrearClienteCommand.Telefono));
    }

    [Fact]
    public async Task Validate_FechaNacimientoVacia_DeberiaSerInvalido()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "test@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = default(DateTime) // Fecha vacía
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "La fecha de nacimiento es obligatoria");
    }

    [Fact]
    public async Task Validate_FechaNacimientoFutura_DeberiaSerInvalido()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "test@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddDays(1) // Fecha futura
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "La fecha de nacimiento debe ser anterior a hoy");
    }

    [Fact]
    public async Task Validate_ClienteMenorDeEdad_DeberiaSerInvalido()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "test@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-17) // Menor de 18 años
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El cliente debe ser mayor de 18 años");
    }

    [Fact]
    public async Task Validate_FechaNacimientoMuyAntigua_DeberiaSerInvalido()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "test@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-121) // Más de 120 años
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "La fecha de nacimiento no puede ser hace más de 120 años");
    }

    [Fact]
    public async Task Validate_MultipleErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "", // Error: vacío
            Email = "email_invalido", // Error: formato inválido
            Telefono = "0300123456", // Error: formato inválido (empieza con 0)
            FechaNacimiento = DateTime.Now.AddYears(-17) // Error: menor de edad
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(3);

        var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
        errorMessages.Should().Contain("El nombre del cliente es obligatorio");
        errorMessages.Should().Contain("El formato del email no es válido");
        errorMessages.Should().Contain("El formato del teléfono no es válido");
        errorMessages.Should().Contain("El cliente debe ser mayor de 18 años");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_EstadoActivo_DeberiaSerValido(bool estadoActivo)
    {
        // Arrange
        var command = new CrearClienteCommand
        {
            Nombre = "Juan Pérez",
            Email = "test@email.com",
            Telefono = "+57300123456",
            FechaNacimiento = DateTime.Now.AddYears(-25),
            EstaActivo = estadoActivo
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
} 