namespace RestaurantePro.Application.UnitTests.Core.Productos.Validators;

/// <summary>
/// Pruebas unitarias para CrearProductoValidator
/// Valida todas las reglas de validación de FluentValidation
/// </summary>
public class CrearProductoValidatorTests
{
    private readonly CrearProductoValidator _validator;

    public CrearProductoValidatorTests()
    {
        _validator = new CrearProductoValidator();
    }

    [Fact]
    public void Validate_ConComandoValido_DeberiaRetornarExito()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Margherita",
            Descripcion = "Deliciosa pizza italiana con tomate y mozzarella",
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.Should().NotBeNull();
        resultado.IsValid.Should().BeTrue();
        resultado.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_ConNombreInvalido_DeberiaRetornarError(string nombre)
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = nombre,
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(x => 
            x.PropertyName == nameof(CrearProductoCommand.Nombre) &&
            x.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void Validate_ConNombreMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var nombreMuyLargo = new string('a', 101); // Más de 100 caracteres
        var command = new CrearProductoCommand
        {
            Nombre = nombreMuyLargo,
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(x => 
            x.PropertyName == nameof(CrearProductoCommand.Nombre) &&
            x.ErrorMessage.Contains("100 caracteres"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.99)]
    public void Validate_ConPrecioInvalido_DeberiaRetornarError(decimal precio)
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Test",
            Precio = precio,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(x => 
            x.PropertyName == nameof(CrearProductoCommand.Precio) &&
            x.ErrorMessage.Contains("mayor a 0"));
    }

    [Theory]
    [InlineData(10000.01)]
    [InlineData(50000)]
    public void Validate_ConPrecioMuyAlto_DeberiaRetornarError(decimal precio)
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Test",
            Precio = precio,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(x => 
            x.PropertyName == nameof(CrearProductoCommand.Precio) &&
            x.ErrorMessage.Contains("10,000"));
    }

    [Fact]
    public void Validate_ConCategoriaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Test",
            Precio = 15.99m,
            CategoriaId = Guid.Empty
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(x => 
            x.PropertyName == nameof(CrearProductoCommand.CategoriaId) &&
            x.ErrorMessage.Contains("obligatorio"));
    }

    [Fact]
    public void Validate_ConDescripcionMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var descripcionMuyLarga = new string('a', 501); // Más de 500 caracteres
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Test",
            Descripcion = descripcionMuyLarga,
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().ContainSingle(x => 
            x.PropertyName == nameof(CrearProductoCommand.Descripcion) &&
            x.ErrorMessage.Contains("500 caracteres"));
    }

    [Fact]
    public void Validate_ConDescripcionVacia_DeberiaSerValido()
    {
        // Arrange - La descripción es opcional
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Test",
            Descripcion = "", // Vacía pero válida
            Precio = 15.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.IsValid.Should().BeTrue();
        resultado.Errors.Should().NotContain(x => 
            x.PropertyName == nameof(CrearProductoCommand.Descripcion));
    }

    [Theory]
    [InlineData(0.01)] // Precio mínimo válido
    [InlineData(1.00)]
    [InlineData(99.99)]
    [InlineData(999.99)]
    [InlineData(10000.00)] // Precio máximo válido
    public void Validate_ConPreciosValidos_DeberiaSerValido(decimal precio)
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Producto Test",
            Precio = precio,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.Errors.Should().NotContain(x => 
            x.PropertyName == nameof(CrearProductoCommand.Precio));
    }

    [Fact]
    public void Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "", // Error: vacío
            Descripcion = new string('a', 501), // Error: muy largo
            Precio = -10m, // Error: negativo
            CategoriaId = Guid.Empty // Error: vacío
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().HaveCount(4);
        
        resultado.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProductoCommand.Nombre));
        resultado.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProductoCommand.Descripcion));
        resultado.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProductoCommand.Precio));
        resultado.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearProductoCommand.CategoriaId));
    }

    [Fact]
    public async Task ValidateAsync_ConComandoValido_DeberiaRetornarExitoAsync()
    {
        // Arrange
        var command = new CrearProductoCommand
        {
            Nombre = "Pizza Napolitana",
            Descripcion = "Pizza tradicional napolitana",
            Precio = 18.99m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var resultado = await _validator.ValidateAsync(command);

        // Assert
        resultado.Should().NotBeNull();
        resultado.IsValid.Should().BeTrue();
        resultado.Errors.Should().BeEmpty();
    }
} 