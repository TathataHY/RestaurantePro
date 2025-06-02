namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Validators;

/// <summary>
/// Tests unitarios para CrearComandaValidator  
/// Validación SÚPER COMPLEJA de reglas de negocio con ítems anidados
/// </summary>
public class CrearComandaValidatorTests
{
    private readonly CrearComandaValidator _validator;

    public CrearComandaValidatorTests()
    {
        _validator = new CrearComandaValidator();
    }

    #region MesaId Validations

    [Fact]
    public void Validator_ConMesaIdValida_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MesaId = Guid.NewGuid();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConMesaIdVacia_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MesaId = Guid.Empty;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearComandaCommand.MesaId))
            .Which.ErrorMessage.Should().Be("El ID de la mesa es obligatorio");
    }

    #endregion

    #region Items Validations

    [Fact]
    public void Validator_ConItemsValidos_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Items = new List<AgregarProductoDto>
        {
            CrearItemValido(),
            CrearItemValido()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConItemsVacia_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Items = new List<AgregarProductoDto>();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearComandaCommand.Items))
            .Which.ErrorMessage.Should().Be("La comanda debe tener al menos un ítem");
    }

    [Fact]
    public void Validator_ConItemsNull_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Items = null!;

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == nameof(CrearComandaCommand.Items))
            .Which.ErrorMessage.Should().Be("La comanda debe tener al menos un ítem");
    }

    #endregion

    #region ItemComanda - ProductoId Validations

    [Fact]
    public void Validator_ConItemProductoIdValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.ProductoId = Guid.NewGuid();
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConItemProductoIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.ProductoId = Guid.Empty;
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.Contains("ProductoId"));
    }

    #endregion

    #region ItemComanda - Cantidad Validations

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void Validator_ConCantidadValida_DeberiaSerValido(int cantidad)
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Cantidad = cantidad;
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validator_ConCantidadInvalida_DeberiaFallar(int cantidadInvalida)
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Cantidad = cantidadInvalida;
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.Contains("Cantidad") && 
            x.ErrorMessage == "La cantidad debe ser mayor a 0");
    }

    [Fact]
    public void Validator_ConCantidadMuyAlta_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Cantidad = 101; // Máximo 100
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.Contains("Cantidad") && 
            x.ErrorMessage == "La cantidad no puede exceder 100 unidades");
    }

    #endregion

    #region ItemComanda - Observaciones Validations

    [Fact]
    public void Validator_ConObservacionesValidas_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Observaciones = "Sin cebolla, con extra queso";
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConObservacionesVacias_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Observaciones = "";
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConObservacionesNull_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Observaciones = null;
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConObservacionesMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Observaciones = new string('A', 501); // Máximo 500
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.Contains("Observaciones"));
    }

    #endregion

    #region Personalizaciones Validations

    [Fact]
    public void Validator_ConPersonalizacionesValidas_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Personalizaciones = new List<PersonalizacionCreateDto>
        {
            CrearPersonalizacionValida(),
            CrearPersonalizacionValida()
        };
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConPersonalizacionesVacias_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Personalizaciones = new List<PersonalizacionCreateDto>();
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConPersonalizacionesNull_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Personalizaciones = null;
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Personalizacion - Nombre Validations

    [Fact]
    public void Validator_ConPersonalizacionNombreValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        var personalizacion = CrearPersonalizacionValida();
        personalizacion.Tipo = "Extra queso";
        item.Personalizaciones = new List<PersonalizacionCreateDto> { personalizacion };
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConPersonalizacionNombreVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        var personalizacion = CrearPersonalizacionValida();
        personalizacion.Tipo = "";
        item.Personalizaciones = new List<PersonalizacionCreateDto> { personalizacion };
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.Contains("Tipo"));
    }

    #endregion

    #region Personalizacion - Costo Validations

    [Theory]
    [InlineData(0)]
    [InlineData(5.50)]
    [InlineData(15.00)]
    [InlineData(25.75)]
    public void Validator_ConPersonalizacionCostoValido_DeberiaSerValido(decimal costo)
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        var personalizacion = CrearPersonalizacionValida();
        personalizacion.PrecioAdicional = costo;
        item.Personalizaciones = new List<PersonalizacionCreateDto> { personalizacion };
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConPersonalizacionCostoNegativo_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        var personalizacion = CrearPersonalizacionValida();
        personalizacion.PrecioAdicional = -5.00m;
        item.Personalizaciones = new List<PersonalizacionCreateDto> { personalizacion };
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.Contains("PrecioAdicional"));
    }

    #endregion

    #region Complex Integration Tests

    [Fact]
    public void Validator_ConComandaCompleta_DeberiaSerValido()
    {
        // Arrange
        var command = new CrearComandaCommand
        {
            MesaId = Guid.NewGuid(),
            Items = new List<AgregarProductoDto>
            {
                new AgregarProductoDto
                {
                    ProductoId = Guid.NewGuid(),
                    Cantidad = 2,
                    Observaciones = "Término medio",
                    Personalizaciones = new List<PersonalizacionCreateDto>
                    {
                        new PersonalizacionCreateDto
                        {
                            Tipo = "Extra salsa",
                            Detalles = "Salsa BBQ extra",
                            PrecioAdicional = 3.50m
                        }
                    }
                },
                new AgregarProductoDto
                {
                    ProductoId = Guid.NewGuid(),
                    Cantidad = 1,
                    Observaciones = "Sin cebolla",
                    Personalizaciones = null
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validator_ConMultiplesErroresAnidados_DeberiaListarTodos()
    {
        // Arrange
        var command = new CrearComandaCommand
        {
            MesaId = Guid.Empty, // Error: mesa vacía
            Items = new List<AgregarProductoDto>
            {
                new AgregarProductoDto
                {
                    ProductoId = Guid.Empty, // Error: producto vacío
                    Cantidad = 0, // Error: cantidad inválida
                    Observaciones = new string('A', 501), // Error: muy largo
                    Personalizaciones = new List<PersonalizacionCreateDto>
                    {
                        new PersonalizacionCreateDto
                        {
                            Tipo = "", // Error: nombre vacío
                            PrecioAdicional = -5.00m // Error: costo negativo
                        }
                    }
                }
            }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(4);
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CrearComandaCommand.MesaId));
        result.Errors.Should().Contain(x => x.PropertyName.Contains("ProductoId"));
        result.Errors.Should().Contain(x => x.PropertyName.Contains("Cantidad"));
        result.Errors.Should().Contain(x => x.PropertyName.Contains("Observaciones"));
        result.Errors.Should().Contain(x => x.PropertyName.Contains("Tipo"));
        result.Errors.Should().Contain(x => x.PropertyName.Contains("PrecioAdicional"));
    }

    [Fact]
    public void Validator_ConMuchosItems_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        command.Items = Enumerable.Range(1, 20).Select(_ => CrearItemValido()).ToList();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_ConItemsConMuchasPersonalizaciones_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Personalizaciones = Enumerable.Range(1, 10)
            .Select(i => new PersonalizacionCreateDto
            {
                Tipo = $"Personalización {i}",
                Detalles = $"Descripción {i}",
                PrecioAdicional = i * 2.50m
            }).ToList();
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Validator_RendimientoValidacionCompleja_DeberiaSerRapido()
    {
        // Arrange
        var command = CrearComandoComplejo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 100; i++)
        {
            _validator.Validate(command);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(300); // Menos de 300ms para 100 validaciones complejas
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validator_ConPersonalizacionDescripcionMuyLarga_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        var personalizacion = CrearPersonalizacionValida();
        personalizacion.Detalles = new string('A', 1001); // Máximo 1000
        item.Personalizaciones = new List<PersonalizacionCreateDto> { personalizacion };
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.Contains("Detalles"));
    }

    [Fact]
    public void Validator_ConCaracteresEspecialesEnObservaciones_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        var item = CrearItemValido();
        item.Observaciones = "Sin cebolla, extra queso & salsa BBQ (muy picante) - término 3/4";
        command.Items = new List<AgregarProductoDto> { item };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private CrearComandaCommand CrearComandoValido()
    {
        return new CrearComandaCommand
        {
            MesaId = Guid.NewGuid(),
            Items = new List<AgregarProductoDto>
            {
                CrearItemValido()
            }
        };
    }

    private AgregarProductoDto CrearItemValido()
    {
        return new AgregarProductoDto
        {
            ProductoId = Guid.NewGuid(),
            Cantidad = 1,
            Observaciones = "Observaciones test",
            Personalizaciones = new List<PersonalizacionCreateDto>
            {
                CrearPersonalizacionValida()
            }
        };
    }

    private PersonalizacionCreateDto CrearPersonalizacionValida()
    {
        return new PersonalizacionCreateDto
        {
            Tipo = "Extra",
            Detalles = "Queso extra mozzarella",
            PrecioAdicional = 5.50m
        };
    }

    private CrearComandaCommand CrearComandoComplejo()
    {
        return new CrearComandaCommand
        {
            MesaId = Guid.NewGuid(),
            Items = Enumerable.Range(1, 5).Select(i => new AgregarProductoDto
            {
                ProductoId = Guid.NewGuid(),
                Cantidad = i,
                Observaciones = $"Observaciones para item {i}",
                Personalizaciones = Enumerable.Range(1, 3).Select(j => new PersonalizacionCreateDto
                {
                    Tipo = $"Personalización {j}",
                    Detalles = $"Descripción detallada {j}",
                    PrecioAdicional = j * 2.5m
                }).ToList()
            }).ToList()
        };
    }

    #endregion
} 