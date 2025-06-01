namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA AGREGAR ITEM COMANDA VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de agregación de items a comandas
/// Cobertura: 100% de reglas de negocio del AgregarItemComandaValidator
/// </summary>
public class AgregarItemComandaValidatorTests
{
    private readonly AgregarItemComandaValidator _validator;

    public AgregarItemComandaValidatorTests()
    {
        _validator = new AgregarItemComandaValidator();
    }

    #region Validation Command Helper

    private AgregarItemComandaCommand CrearCommandValido()
    {
        return new AgregarItemComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            ProductoId = Guid.NewGuid(),
            NombreProducto = "Pizza Margarita",
            Cantidad = 2,
            PrecioUnitario = 18.50m,
            Observaciones = "Sin cebolla, extra queso",
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Extra",
                    IngredienteId = Guid.NewGuid(),
                    PrecioAdicional = 2.50m
                }
            }
        };
    }

    private PersonalizacionCreateDto CrearPersonalizacionValida(string tipo = "Extra")
    {
        return new PersonalizacionCreateDto
        {
            Tipo = tipo,
            IngredienteId = Guid.NewGuid(),
            PrecioAdicional = 1.50m,
            IngredienteSustitucionId = tipo == "Sustituir" ? Guid.NewGuid() : null
        };
    }

    #endregion

    #region Validación ComandaId

    [Fact]
    public async Task Validate_ConComandaIdVacia_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ComandaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.ComandaId) &&
            e.ErrorMessage.Contains("El ID de la comanda no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConComandaIdValida_NoDeberiaRetornarErrorDeComandaId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ComandaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.ComandaId));
    }

    #endregion

    #region Validación ProductoId

    [Fact]
    public async Task Validate_ConProductoIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProductoId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.ProductoId) &&
            e.ErrorMessage.Contains("El ID del producto no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConProductoIdValido_NoDeberiaRetornarErrorDeProductoId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ProductoId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.ProductoId));
    }

    #endregion

    #region Validación NombreProducto

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConNombreProductoVacioONull_DeberiaRetornarError(string nombreInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreProducto = nombreInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.NombreProducto) &&
            e.ErrorMessage.Contains("El nombre del producto es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConNombreProductoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreProducto = new string('A', 201); // Más de 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.NombreProducto) &&
            e.ErrorMessage.Contains("El nombre del producto no puede exceder 200 caracteres"));
    }

    [Theory]
    [InlineData("Pizza con <script>")]
    [InlineData("Hamburguesa > Especial")]
    [InlineData("Pasta \"Carbonara\"")]
    [InlineData("Ensalada 'Verde'")]
    [InlineData("Pollo & Verduras")]
    [InlineData("Arroz; Pollo")]
    public async Task Validate_ConNombreProductoCaracteresInvalidos_DeberiaRetornarError(string nombreConCaracteresInvalidos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreProducto = nombreConCaracteresInvalidos;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.NombreProducto) &&
            e.ErrorMessage.Contains("El nombre del producto contiene caracteres no válidos"));
    }

    [Theory]
    [InlineData("Pizza Margarita")]
    [InlineData("Hamburguesa Especial")]
    [InlineData("Pasta Carbonara")]
    [InlineData("Ensalada César")]
    [InlineData("Pollo con Verduras")]
    [InlineData("Arroz con Pollo")]
    public async Task Validate_ConNombreProductoValido_NoDeberiaRetornarErrorDeNombre(string nombreValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreProducto = nombreValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.NombreProducto));
    }

    #endregion

    #region Validación Cantidad

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task Validate_ConCantidadMenorOIgualACero_DeberiaRetornarError(int cantidadInvalida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cantidad = cantidadInvalida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Cantidad) &&
            e.ErrorMessage.Contains("La cantidad debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConCantidadExcesiva_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cantidad = 51; // Más de 50 unidades

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Cantidad) &&
            e.ErrorMessage.Contains("La cantidad máxima por item es 50 unidades"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public async Task Validate_ConCantidadValida_NoDeberiaRetornarErrorDeCantidad(int cantidadValida)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Cantidad = cantidadValida;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Cantidad));
    }

    #endregion

    #region Validación PrecioUnitario

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.50)]
    public async Task Validate_ConPrecioUnitarioMenorOIgualACero_DeberiaRetornarError(decimal precioInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PrecioUnitario = precioInvalido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.PrecioUnitario) &&
            e.ErrorMessage.Contains("El precio unitario debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConPrecioUnitarioExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PrecioUnitario = 10001m; // Más de $10,000

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.PrecioUnitario) &&
            e.ErrorMessage.Contains("El precio unitario no puede exceder $10,000"));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(5.99)]
    [InlineData(18.50)]
    [InlineData(500.00)]
    [InlineData(10000.00)]
    public async Task Validate_ConPrecioUnitarioValido_NoDeberiaRetornarErrorDePrecio(decimal precioValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PrecioUnitario = precioValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.PrecioUnitario));
    }

    #endregion

    #region Validación Observaciones

    [Fact]
    public async Task Validate_ConObservacionesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('A', 501); // Más de 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 500 caracteres"));
    }

    [Theory]
    [InlineData("Sin cebolla")]
    [InlineData("Extra queso, sin tomate")]
    [InlineData("Punto medio, sin sal")]
    [InlineData("Para llevar, empaque especial")]
    public async Task Validate_ConObservacionesValidas_NoDeberiaRetornarErrorDeObservaciones(string observacionesValidas)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacionesValidas;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Observaciones));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConObservacionesVacias_NoDeberiaValidarLongitud(string observacionesVacias)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacionesVacias;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Observaciones));
    }

    [Fact]
    public async Task Validate_ConObservacionesEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('A', 500); // Exactamente 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Observaciones));
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
            e.PropertyName == nameof(AgregarItemComandaCommand.UsuarioId) &&
            e.ErrorMessage.Contains("El ID del usuario no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConUsuarioIdValido_NoDeberiaRetornarErrorDeUsuarioId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.UsuarioId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.UsuarioId));
    }

    #endregion

    #region Validación Personalizaciones - Cantidad

    [Fact]
    public async Task Validate_ConDemasiadasPersonalizaciones_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = Enumerable.Range(1, 11) // 11 personalizaciones (más de 10)
            .Select(_ => CrearPersonalizacionValida())
            .ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Personalizaciones) &&
            e.ErrorMessage.Contains("Máximo 10 personalizaciones por producto"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Validate_ConCantidadValidaDePersonalizaciones_NoDeberiaRetornarErrorDeCantidad(int cantidadPersonalizaciones)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = Enumerable.Range(1, cantidadPersonalizaciones)
            .Select(_ => CrearPersonalizacionValida())
            .ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Personalizaciones) &&
            e.ErrorMessage.Contains("Máximo 10 personalizaciones por producto"));
    }

    #endregion

    #region Validación PersonalizacionCreateDto - Tipo

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Validate_ConPersonalizacionTipoVacioONull_DeberiaRetornarError(string tipoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = tipoInvalido,
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 1.50m
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Personalizaciones[0].Tipo" &&
            e.ErrorMessage.Contains("El tipo de personalización es obligatorio"));
    }

    [Theory]
    [InlineData("Invalido")]
    [InlineData("Agregar")]
    [InlineData("Eliminar")]
    [InlineData("EXTRA")]
    [InlineData("quitar")]
    public async Task Validate_ConPersonalizacionTipoInvalido_DeberiaRetornarError(string tipoInvalido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = tipoInvalido,
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 1.50m
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Personalizaciones[0].Tipo" &&
            e.ErrorMessage.Contains("Tipo de personalización inválido. Valores válidos: Extra, Quitar, Sustituir"));
    }

    [Theory]
    [InlineData("Extra")]
    [InlineData("Quitar")]
    [InlineData("Sustituir")]
    public async Task Validate_ConPersonalizacionTipoValido_NoDeberiaRetornarErrorDeTipo(string tipoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = tipoValido,
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 1.50m,
                IngredienteSustitucionId = tipoValido == "Sustituir" ? Guid.NewGuid() : null
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == "Personalizaciones[0].Tipo");
    }

    #endregion

    #region Validación PersonalizacionCreateDto - IngredienteId

    [Fact]
    public async Task Validate_ConPersonalizacionIngredienteIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = "Extra",
                IngredienteId = Guid.Empty,
                PrecioAdicional = 1.50m
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Personalizaciones[0].IngredienteId" &&
            e.ErrorMessage.Contains("El ID del ingrediente no puede ser un GUID vacío"));
    }

    #endregion

    #region Validación PersonalizacionCreateDto - PrecioAdicional

    [Theory]
    [InlineData(-1)]
    [InlineData(-10.50)]
    public async Task Validate_ConPersonalizacionPrecioNegativo_DeberiaRetornarError(decimal precioNegativo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = "Extra",
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = precioNegativo
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Personalizaciones[0].PrecioAdicional" &&
            e.ErrorMessage.Contains("El precio adicional no puede ser negativo"));
    }

    [Fact]
    public async Task Validate_ConPersonalizacionPrecioExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = "Extra",
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 1001m // Más de $1,000
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Personalizaciones[0].PrecioAdicional" &&
            e.ErrorMessage.Contains("El precio adicional no puede exceder $1,000"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1.50)]
    [InlineData(50.00)]
    [InlineData(1000.00)]
    public async Task Validate_ConPersonalizacionPrecioValido_NoDeberiaRetornarErrorDePrecio(decimal precioValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = "Extra",
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = precioValido
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == "Personalizaciones[0].PrecioAdicional");
    }

    #endregion

    #region Validación PersonalizacionCreateDto - IngredienteSustitucionId

    [Fact]
    public async Task Validate_ConSustituirSinIngredienteSustituto_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = "Sustituir",
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 0m,
                IngredienteSustitucionId = null // Sin ingrediente sustituto
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Personalizaciones[0].IngredienteSustitucionId" &&
            e.ErrorMessage.Contains("El ingrediente sustituto es obligatorio para personalizaciones de tipo 'Sustituir'"));
    }

    [Fact]
    public async Task Validate_ConSustituirConIngredienteSustitutoVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = "Sustituir",
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 0m,
                IngredienteSustitucionId = Guid.Empty // GUID vacío
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Personalizaciones[0].IngredienteSustitucionId" &&
            e.ErrorMessage.Contains("El ID del ingrediente sustituto no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConSustituirConIngredienteSustitutoValido_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = "Sustituir",
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 0m,
                IngredienteSustitucionId = Guid.NewGuid()
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == "Personalizaciones[0].IngredienteSustitucionId");
    }

    [Theory]
    [InlineData("Extra")]
    [InlineData("Quitar")]
    public async Task Validate_ConNoSustituirPeroConIngredienteSustituto_DeberiaRetornarError(string tipoNoSustituir)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = tipoNoSustituir,
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 1.50m,
                IngredienteSustitucionId = Guid.NewGuid() // No debería tener sustituto
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == "Personalizaciones[0].IngredienteSustitucionId" &&
            e.ErrorMessage.Contains("No se debe especificar ingrediente sustituto para personalizaciones que no son de tipo 'Sustituir'"));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new AgregarItemComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            ProductoId = Guid.NewGuid(),
            NombreProducto = "Pizza Napolitana",
            Cantidad = 3,
            PrecioUnitario = 22.50m,
            Observaciones = "Extra queso mozzarella, sin aceitunas negras",
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                new PersonalizacionCreateDto
                {
                    Tipo = "Extra",
                    IngredienteId = Guid.NewGuid(),
                    PrecioAdicional = 3.00m
                },
                new PersonalizacionCreateDto
                {
                    Tipo = "Quitar",
                    IngredienteId = Guid.NewGuid(),
                    PrecioAdicional = 0m
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
        var command = new AgregarItemComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            ProductoId = Guid.NewGuid(),
            NombreProducto = "Hamburguesa Simple",
            Cantidad = 1,
            PrecioUnitario = 12.00m,
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>()
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
        var command = new AgregarItemComandaCommand
        {
            ComandaId = Guid.Empty, // Error
            ProductoId = Guid.Empty, // Error
            NombreProducto = "", // Error
            Cantidad = 0, // Error
            PrecioUnitario = -5m, // Error
            Observaciones = new string('A', 501), // Error
            UsuarioId = Guid.Empty, // Error
            Personalizaciones = Enumerable.Range(1, 11) // Error - demasiadas
                .Select(_ => new PersonalizacionCreateDto
                {
                    Tipo = "Invalido", // Error anidado
                    IngredienteId = Guid.Empty, // Error anidado
                    PrecioAdicional = -1m // Error anidado
                })
                .ToList()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(7);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Fact]
    public async Task Validate_ConPersonalizacionesComplejas_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            new PersonalizacionCreateDto
            {
                Tipo = "Extra",
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 2.50m
            },
            new PersonalizacionCreateDto
            {
                Tipo = "Quitar",
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 0m
            },
            new PersonalizacionCreateDto
            {
                Tipo = "Sustituir",
                IngredienteId = Guid.NewGuid(),
                PrecioAdicional = 1.00m,
                IngredienteSustitucionId = Guid.NewGuid()
            }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConProductoComplejoSinPersonalizaciones_DeberiaSerValido()
    {
        // Arrange
        var command = new AgregarItemComandaCommand
        {
            ComandaId = Guid.NewGuid(),
            ProductoId = Guid.NewGuid(),
            NombreProducto = "Pizza Suprema Familiar",
            Cantidad = 1,
            PrecioUnitario = 35.99m,
            Observaciones = "Masa gruesa, extra cocida, cortada en 12 porciones",
            UsuarioId = Guid.NewGuid(),
            Personalizaciones = new List<PersonalizacionCreateDto>()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Pizza Margarita")]
    [InlineData("Hamburguesa Clásica")]
    [InlineData("Ensalada César")]
    [InlineData("Pasta Alfredo")]
    [InlineData("Pollo Asado")]
    public async Task Validate_ConDiferentesProductos_DeberiaSerValido(string nombreProducto)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreProducto = nombreProducto;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData(1, true)]    // 1 carácter - válido
    [InlineData(100, true)]  // 100 caracteres - válido
    [InlineData(200, true)]  // 200 caracteres - límite válido
    [InlineData(201, false)] // 201 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesNombreProducto_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreProducto = new string('P', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(AgregarItemComandaCommand.NombreProducto) &&
                e.ErrorMessage.Contains("El nombre del producto no puede exceder 200 caracteres"));
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => 
                e.PropertyName == nameof(AgregarItemComandaCommand.NombreProducto) &&
                e.ErrorMessage.Contains("El nombre del producto no puede exceder 200 caracteres"));
        }
    }

    [Fact]
    public async Task Validate_ConNombreProductoEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.NombreProducto = new string('P', 200); // Exactamente 200 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.NombreProducto) &&
            e.ErrorMessage.Contains("El nombre del producto no puede exceder 200 caracteres"));
    }

    [Fact]
    public async Task Validate_ConPersonalizacionesEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Personalizaciones = Enumerable.Range(1, 10) // Exactamente 10 personalizaciones
            .Select(_ => CrearPersonalizacionValida())
            .ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AgregarItemComandaCommand.Personalizaciones) &&
            e.ErrorMessage.Contains("Máximo 10 personalizaciones por producto"));
    }

    #endregion
} 