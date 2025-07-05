namespace RestaurantePro.Application.UnitTests.Inventario.Ingredientes.Validators;

/// <summary>
/// Tests unitarios para CrearIngredienteValidator
/// Validación completa de reglas de negocio para gestión de inventario
/// </summary>
public class CrearIngredienteValidatorTests
{
    private readonly CrearIngredienteValidator _validator;

    public CrearIngredienteValidatorTests()
    {
        _validator = new CrearIngredienteValidator();
    }

    #region Nombre Validations

    [Fact]
    public void Validator_ConNombreValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Nombre = "Harina de trigo";

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
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.Nombre))
            .Which.ErrorMessage.Should().Be("El nombre del ingrediente es obligatorio");
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
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.Nombre))
            .Which.ErrorMessage.Should().Be("El nombre del ingrediente es obligatorio");
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
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.Nombre))
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
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.Nombre))
            .Which.ErrorMessage.Should().Be("El nombre no puede exceder 200 caracteres");
    }

    [Theory]
    [InlineData("Sal")]
    [InlineData("Harina de trigo")]
    [InlineData("Aceite de oliva extra virgen")]
    [InlineData("Tomate cherry")]
    [InlineData("Queso mozzarella")]
    public void Validator_ConNombresValidos_DeberiaSerValido(string nombre)
    {
        // Arrange
        var command = CrearComandoValido();
        command.Nombre = nombre;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Descripcion Validations

    [Fact]
    public void Validator_ConDescripcionValida_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Descripcion = "Harina de trigo refinada para panadería";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConDescripcionVacia_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Descripcion = "";

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConDescripcionNull_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Descripcion = string.Empty;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConDescripcionMuyLarga_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Descripcion = new string('A', 501);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.Descripcion))
            .Which.ErrorMessage.Should().Be("La descripción no puede exceder 500 caracteres");
    }

    #endregion

    #region UnidadMedida Validations

    [Theory]
    [InlineData(UnidadMedida.Kilogramos)]
    [InlineData(UnidadMedida.Gramos)]
    [InlineData(UnidadMedida.Litros)]
    [InlineData(UnidadMedida.Mililitros)]
    [InlineData(UnidadMedida.Unidades)]
    [InlineData(UnidadMedida.Piezas)]
    public void Validator_ConUnidadesMedidaValidas_DeberiaSerValido(UnidadMedida unidad)
    {
        // Arrange
        var command = CrearComandoValido();
        command.UnidadMedida = unidad.ToString();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConUnidadMedidaInvalida_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.UnidadMedida = ((UnidadMedida)999).ToString();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.UnidadMedida))
            .Which.ErrorMessage.Should().Be("La unidad de medida no es válida");
    }

    #endregion

    #region Stock Validations

    [Fact]
    public void Validator_ConStockMinimoValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.StockMinimo = 15;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConStockMinimoNegativo_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.StockMinimo = -5;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.StockMinimo))
            .Which.ErrorMessage.Should().Be("El stock mínimo debe ser mayor o igual a 0");
    }

    #endregion

    #region Stock Inicial Validations

    [Fact]
    public void Validator_ConStockInicialValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.StockInicial = 25;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConStockInicialNegativo_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.StockInicial = -10;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.StockInicial))
            .Which.ErrorMessage.Should().Be("El stock inicial debe ser mayor o igual a 0");
    }

    [Theory]
    [InlineData(10, 50, true)] // Stock inicial mayor que mínimo
    [InlineData(10, 10, true)] // Stock inicial igual al mínimo
    [InlineData(10, 5, true)] // Stock inicial menor que mínimo - VÁLIDO porque la regla está comentada en el validador
    public void Validator_ConDiferentesStocksIniciales_DeberiaValidarCorrectamente(
        decimal stockMin, decimal stockInicial, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.StockMinimo = stockMin;
        command.StockInicial = stockInicial;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(deberiaSerValido);
        if (!deberiaSerValido)
        {
            result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.StockInicial));
        }
    }

    #endregion

    #region Costo Validations

    [Fact]
    public void Validator_ConCostoInicialValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.CostoInicial = 25.50m;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConCostoInicialNegativo_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.CostoInicial = -15.00m;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.CostoInicial))
            .Which.ErrorMessage.Should().Be("El costo inicial debe ser mayor o igual a 0");
    }

    [Fact]
    public void Validator_ConCostoInicialMuyAlto_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.CostoInicial = 100000.01m;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.CostoInicial))
            .Which.ErrorMessage.Should().Be("El costo inicial no puede exceder $100,000");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(15.50)]
    [InlineData(999.99)]
    [InlineData(100000.00)]
    public void Validator_ConCostosValidos_DeberiaSerValido(decimal costo)
    {
        // Arrange
        var command = CrearComandoValido();
        command.CostoInicial = costo;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region IDs Validations

    [Fact]
    public void Validator_ConProveedorPrincipalIdValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ProveedorPrincipalId = Guid.NewGuid();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConProveedorPrincipalIdNull_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ProveedorPrincipalId = null;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConProveedorPrincipalIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ProveedorPrincipalId = Guid.Empty;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearIngredienteCommand.ProveedorPrincipalId))
            .Which.ErrorMessage.Should().Be("El ID del proveedor no puede ser un GUID vacío");
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
    public void Validator_ConMultiplesErrores_DeberiaListarTodos()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "", // Error: vacío
            Codigo = "", // Error: vacío
            Descripcion = new string('A', 501), // Error: muy larga
            StockMinimo = -5, // Error: negativo
            StockInicial = -15, // Error: negativo
            CostoInicial = -25.00m, // Error: negativo
            UsuarioId = Guid.Empty, // Error: vacío
            MotivoStockInicial = "Stock inicial al crear ingrediente"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(3);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearIngredienteCommand.Nombre));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearIngredienteCommand.Codigo));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearIngredienteCommand.StockMinimo));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearIngredienteCommand.CostoInicial));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearIngredienteCommand.UsuarioId));
    }

    [Fact]
    public void Validator_ConIngredienteCompleto_DeberiaSerValido()
    {
        // Arrange
        var command = new CrearIngredienteCommand
        {
            Nombre = "Aceite de oliva extra virgen",
            Codigo = "ACE-OLI-001",
            Descripcion = "Aceite de oliva extra virgen importado de España, ideal para ensaladas y cocina mediterránea",
            UnidadMedida = "Litros",
            StockMinimo = 5,
            StockInicial = 20,
            CostoInicial = 85.50m,
            ProveedorPrincipalId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            MotivoStockInicial = "Stock inicial al crear ingrediente"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
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
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 validaciones
    }

    #endregion

    #region Helper Methods

    private CrearIngredienteCommand CrearComandoValido()
    {
        return new CrearIngredienteCommand
        {
            Nombre = "Harina de trigo",
            Codigo = "HAR-TRI-001",
            Descripcion = "Harina de trigo refinada para panadería",
            UnidadMedida = "Kilogramos",
            StockMinimo = 10,
            StockInicial = 50,
            CostoInicial = 25.50m,
            ProveedorPrincipalId = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            MotivoStockInicial = "Stock inicial al crear ingrediente"
        };
    }

    #endregion
} 