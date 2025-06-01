namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA PROCESAR PEDIDO COMPLETO VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas del procesamiento de pedidos completos
/// Cobertura: 100% de reglas de negocio del ProcesarPedidoCompletoValidator
/// </summary>
public class ProcesarPedidoCompletoValidatorTests
{
    private readonly ProcesarPedidoCompletoValidator _validator;

    public ProcesarPedidoCompletoValidatorTests()
    {
        _validator = new ProcesarPedidoCompletoValidator();
    }

    #region Validation Command Helper

    private ProcesarPedidoCompletoCommand CrearCommandValido()
    {
        return new ProcesarPedidoCompletoCommand
        {
            MeseroId = Guid.NewGuid(),
            Items = new List<ItemPedidoDto>
            {
                new ItemPedidoDto
                {
                    ProductoId = Guid.NewGuid(),
                    Cantidad = 2,
                    PrecioUnitario = 15.50m,
                    Observaciones = "Sin cebolla"
                },
                new ItemPedidoDto
                {
                    ProductoId = Guid.NewGuid(),
                    Cantidad = 1,
                    PrecioUnitario = 25.00m,
                    Observaciones = "Bien cocido"
                }
            },
            ObservacionesComanda = "Mesa VIP - servicio prioritario",
            ClienteId = Guid.NewGuid(),
            PuntosAUtilizar = 500
        };
    }

    private ItemPedidoDto CrearItemValido()
    {
        return new ItemPedidoDto
        {
            ProductoId = Guid.NewGuid(),
            Cantidad = 1,
            PrecioUnitario = 12.50m,
            Observaciones = "Item válido"
        };
    }

    #endregion

    #region Validación MeseroId

    [Fact]
    public async Task Validate_ConMeseroIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MeseroId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.MeseroId) &&
            e.ErrorMessage.Contains("El ID del mesero es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConMeseroIdValido_NoDeberiaRetornarErrorDeMeseroId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MeseroId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.MeseroId) &&
            e.ErrorMessage.Contains("El ID del mesero es obligatorio"));
    }

    #endregion

    #region Validación Items

    [Fact]
    public async Task Validate_ConItemsVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Items = new List<ItemPedidoDto>();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.Items) &&
            e.ErrorMessage.Contains("El pedido debe contener al menos un item"));
    }

    [Fact]
    public async Task Validate_ConItemsNull_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Items = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.Items) &&
            e.ErrorMessage.Contains("El pedido debe contener al menos un item"));
    }

    [Fact]
    public async Task Validate_ConDemasiadosItems_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Items = Enumerable.Range(1, 51) // 51 items (más del máximo de 50)
            .Select(_ => CrearItemValido())
            .ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.Items) &&
            e.ErrorMessage.Contains("El pedido no puede tener más de 50 items"));
    }

    [Theory]
    [InlineData(1)]  // 1 item - válido
    [InlineData(25)] // 25 items - válido
    [InlineData(50)] // 50 items - límite válido
    public async Task Validate_ConCantidadValidaDeItems_NoDeberiaRetornarErrorDeItems(int cantidadItems)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Items = Enumerable.Range(1, cantidadItems)
            .Select(_ => CrearItemValido())
            .ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.Items) &&
            (e.ErrorMessage.Contains("El pedido debe contener al menos un item") ||
             e.ErrorMessage.Contains("El pedido no puede tener más de 50 items")));
    }

    #endregion

    #region Validación ObservacionesComanda

    [Fact]
    public async Task Validate_ConObservacionesMuyLargas_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesComanda = new string('A', 1001); // Más de 1000 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.ObservacionesComanda) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres"));
    }

    [Theory]
    [InlineData("Observaciones cortas")]
    [InlineData("Mesa VIP - servicio prioritario y atención especial")]
    [InlineData("Cliente habitual - conoce sus preferencias")]
    public async Task Validate_ConObservacionesLongitudValida_NoDeberiaRetornarErrorDeObservaciones(string observacionesValidas)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesComanda = observacionesValidas;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.ObservacionesComanda) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres"));
    }

    [Fact]
    public async Task Validate_ConObservacionesEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesComanda = new string('A', 1000); // Exactamente 1000 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.ObservacionesComanda) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConObservacionesVacias_NoDeberiaValidarLongitud(string observacionesVacias)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesComanda = observacionesVacias;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.ObservacionesComanda) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres"));
    }

    #endregion

    #region Validación PuntosAUtilizar

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-1)]
    public async Task Validate_ConPuntosAUtilizarMenorOIgualACero_DeberiaRetornarError(int puntosInvalidos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = puntosInvalidos;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.PuntosAUtilizar) &&
            e.ErrorMessage.Contains("Los puntos a utilizar deben ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConPuntosAUtilizarExcesivos_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 10001; // Más de 10,000 puntos

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.PuntosAUtilizar) &&
            e.ErrorMessage.Contains("No se pueden utilizar más de 10,000 puntos en un pedido"));
    }

    [Theory]
    [InlineData(1)]    // 1 punto - válido
    [InlineData(500)]  // 500 puntos - válido
    [InlineData(10000)] // 10,000 puntos - límite válido
    public async Task Validate_ConPuntosAUtilizarValidos_NoDeberiaRetornarErrorDePuntos(int puntosValidos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = puntosValidos;
        command.ClienteId = Guid.NewGuid(); // Necesario cuando se usan puntos

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.PuntosAUtilizar));
    }

    [Fact]
    public async Task Validate_ConPuntosAUtilizarNull_NoDeberiaValidar()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.PuntosAUtilizar));
    }

    #endregion

    #region Validación Lógica de Negocio - Cliente para Puntos

    [Fact]
    public async Task Validate_ConPuntosSinCliente_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 500;
        command.ClienteId = Guid.Empty; // Sin cliente

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ProcesarPedidoCompletoCommand.ClienteId) &&
            e.ErrorMessage.Contains("Debe especificar un cliente para utilizar puntos de fidelización"));
    }

    [Fact]
    public async Task Validate_ConPuntosYClienteValido_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 500;
        command.ClienteId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("Debe especificar un cliente para utilizar puntos de fidelización"));
    }

    [Fact]
    public async Task Validate_SinPuntosYSinCliente_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = null;
        command.ClienteId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorMessage.Contains("Debe especificar un cliente para utilizar puntos de fidelización"));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new ProcesarPedidoCompletoCommand
        {
            MeseroId = Guid.NewGuid(),
            Items = new List<ItemPedidoDto>
            {
                new ItemPedidoDto
                {
                    ProductoId = Guid.NewGuid(),
                    Cantidad = 3,
                    PrecioUnitario = 18.50m,
                    Observaciones = "Extra queso"
                },
                new ItemPedidoDto
                {
                    ProductoId = Guid.NewGuid(),
                    Cantidad = 2,
                    PrecioUnitario = 12.00m,
                    Observaciones = "Sin picante"
                }
            },
            ObservacionesComanda = "Mesa número 5 - cliente frecuente",
            ClienteId = Guid.NewGuid(),
            PuntosAUtilizar = 750
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
        var command = new ProcesarPedidoCompletoCommand
        {
            MeseroId = Guid.NewGuid(),
            Items = new List<ItemPedidoDto>
            {
                new ItemPedidoDto
                {
                    ProductoId = Guid.NewGuid(),
                    Cantidad = 1,
                    PrecioUnitario = 10.00m
                }
            }
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
        var command = new ProcesarPedidoCompletoCommand
        {
            MeseroId = Guid.Empty, // Error
            Items = new List<ItemPedidoDto>(), // Error - lista vacía
            ObservacionesComanda = new string('A', 1001), // Error - muy largo
            PuntosAUtilizar = -100, // Error
            ClienteId = Guid.Empty // Error - puntos sin cliente
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Mesa VIP - atención prioritaria")]
    [InlineData("Cliente alérgico a mariscos")]
    [InlineData("Pedido para llevar - empaque especial")]
    [InlineData("Evento corporativo - factura empresarial")]
    public async Task Validate_ConDiferentesObservaciones_DeberiaSerValido(string observaciones)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesComanda = observaciones;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConPedidoGrande_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Items = Enumerable.Range(1, 30) // 30 items (dentro del límite)
            .Select(i => new ItemPedidoDto
            {
                ProductoId = Guid.NewGuid(),
                Cantidad = 2,
                PrecioUnitario = 15.00m + i,
                Observaciones = $"Item {i}"
            })
            .ToList();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConClienteFrecuente_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ClienteId = Guid.NewGuid();
        command.PuntosAUtilizar = 2500; // Cliente frecuente con muchos puntos
        command.ObservacionesComanda = "Cliente VIP - descuento por fidelidad";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConPedidoSinPuntos_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = null;
        command.ClienteId = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData(1, true)]     // 1 carácter - válido
    [InlineData(500, true)]   // 500 caracteres - válido
    [InlineData(1000, true)]  // 1000 caracteres - límite válido
    [InlineData(1001, false)] // 1001 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesObservaciones_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesComanda = new string('O', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(ProcesarPedidoCompletoCommand.ObservacionesComanda) &&
                e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres"));
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => 
                e.PropertyName == nameof(ProcesarPedidoCompletoCommand.ObservacionesComanda) &&
                e.ErrorMessage.Contains("Las observaciones no pueden exceder 1000 caracteres"));
        }
    }

    [Theory]
    [InlineData(1, true)]     // 1 punto - válido
    [InlineData(5000, true)]  // 5000 puntos - válido
    [InlineData(10000, true)] // 10000 puntos - límite válido
    [InlineData(10001, false)] // 10001 puntos - inválido
    public async Task Validate_ConDiferentesCantidadesPuntos_DeberiaValidarCorrectamente(int puntos, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = puntos;
        command.ClienteId = Guid.NewGuid(); // Necesario para usar puntos

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(ProcesarPedidoCompletoCommand.PuntosAUtilizar) &&
                e.ErrorMessage.Contains("No se pueden utilizar más de 10,000 puntos en un pedido"));
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => 
                e.PropertyName == nameof(ProcesarPedidoCompletoCommand.PuntosAUtilizar) &&
                e.ErrorMessage.Contains("No se pueden utilizar más de 10,000 puntos en un pedido"));
        }
    }

    [Fact]
    public async Task Validate_ConObservacionesCaracteresEspeciales_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesComanda = "Mesa #15 - Cliente: Pérez & Co. - Descuento: 10% - Nota: ¡Excelente servicio!";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConObservacionesEmojis_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ObservacionesComanda = "Pedido especial 🍕 Cliente feliz 😊 Servicio premium ⭐";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
} 