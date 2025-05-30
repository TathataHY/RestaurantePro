namespace RestaurantePro.Application.UnitTests.Core.Productos.Validators;

public class ActualizarProductoValidatorTests
{
    private readonly ActualizarProductoValidator _validator;

    public ActualizarProductoValidatorTests()
    {
        _validator = new ActualizarProductoValidator();
    }

    [Fact]
    public async Task Validate_CommandValido_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Margherita",
            Descripcion = "Deliciosa pizza con tomate, mozzarella y albahaca",
            Precio = 250.50m,
            CategoriaId = Guid.NewGuid(),
            Activo = true
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
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = nombreInvalido!,
            Descripcion = "Descripción válida",
            Precio = 100m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("El nombre del producto es obligatorio");
    }

    [Fact]
    public async Task Validate_NombreMuyLargo_DeberiaSerInvalido()
    {
        // Arrange
        var nombreLargo = new string('A', 101); // 101 caracteres
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = nombreLargo,
            Descripcion = "Descripción válida",
            Precio = 100m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("El nombre no puede exceder 100 caracteres");
    }

    [Fact]
    public async Task Validate_DescripcionMuyLarga_DeberiaSerInvalido()
    {
        // Arrange
        var descripcionLarga = new string('D', 501); // 501 caracteres
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Nombre válido",
            Descripcion = descripcionLarga,
            Precio = 100m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("La descripción no puede exceder 500 caracteres");
    }

    [Fact]
    public async Task Validate_DescripcionVacia_DeberiaSerValido()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Margherita",
            Descripcion = "", // Descripción vacía debería ser válida
            Precio = 100m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_PrecioInvalido_DeberiaSerInvalido(decimal precioInvalido)
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Margherita",
            Descripcion = "Descripción válida",
            Precio = precioInvalido,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("El precio debe ser mayor a 0");
    }

    [Fact]
    public async Task Validate_PrecioMuyAlto_DeberiaSerInvalido()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Margherita",
            Descripcion = "Descripción válida",
            Precio = 1000000m, // Precio mayor al límite de 999,999
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("El precio no puede exceder 999,999");
    }

    [Fact]
    public async Task Validate_IdVacio_DeberiaSerInvalido()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.Empty, // ID vacío
            Nombre = "Pizza Margherita",
            Descripcion = "Descripción válida",
            Precio = 100m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("El ID del producto es obligatorio");
    }

    [Fact]
    public async Task Validate_CategoriaIdVacio_DeberiaSerInvalido()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Margherita",
            Descripcion = "Descripción válida",
            Precio = 100m,
            CategoriaId = Guid.Empty // CategoriaId vacío
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("La categoría es obligatoria");
    }

    [Fact]
    public async Task Validate_MultipleErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.Empty,                        // Error: ID vacío
            Nombre = "",                            // Error: Nombre vacío
            Descripcion = new string('D', 501),     // Error: Descripción muy larga
            Precio = -10m,                          // Error: Precio negativo
            CategoriaId = Guid.Empty                // Error: CategoriaId vacío
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(5);

        var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
        errorMessages.Should().Contain("El ID del producto es obligatorio");
        errorMessages.Should().Contain("El nombre del producto es obligatorio");
        errorMessages.Should().Contain("La descripción no puede exceder 500 caracteres");
        errorMessages.Should().Contain("El precio debe ser mayor a 0");
        errorMessages.Should().Contain("La categoría es obligatoria");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)] // Precio máximo válido
    public async Task Validate_PreciosValidos_DeberiaSerValido(decimal precioValido)
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Margherita",
            Descripcion = "Descripción válida",
            Precio = precioValido,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("A")]       // 1 carácter (válido)
    [InlineData("AB")]      // 2 caracteres (válido)
    [InlineData("ABC")]     // 3 caracteres (válido)
    [InlineData("Pizza Margherita Suprema")]  // Nombre normal (válido)
    public async Task Validate_NombresValidos_DeberiaSerValido(string nombreValido)
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = nombreValido,
            Descripcion = "Descripción válida",
            Precio = 100m,
            CategoriaId = Guid.NewGuid()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validate_EstadoActivo_DeberiaSerValido(bool estadoActivo)
    {
        // Arrange
        var command = new ActualizarProductoCommand
        {
            Id = Guid.NewGuid(),
            Nombre = "Pizza Margherita",
            Descripcion = "Descripción válida",
            Precio = 100m,
            CategoriaId = Guid.NewGuid(),
            Activo = estadoActivo
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
} 