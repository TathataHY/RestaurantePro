namespace RestaurantePro.Application.UnitTests.Comercial.Fidelizacion.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA CANJEAR PUNTOS VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de canje de puntos de fidelización
/// Cobertura: 100% de reglas de negocio del CanjearPuntosValidator
/// </summary>
public class CanjearPuntosValidatorTests
{
    private readonly CanjearPuntosValidator _validator;

    public CanjearPuntosValidatorTests()
    {
        _validator = new CanjearPuntosValidator();
    }

    #region Validation Command Helper

    private CanjearPuntosCommand CrearCommandValido()
    {
        return new CanjearPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            PuntosAUtilizar = 100,
            ComandaId = Guid.NewGuid(),
            Motivo = "Canje de puntos por descuento en comanda"
        };
    }

    #endregion

    #region Validación ClienteId

    [Fact]
    public async Task Validate_ConClienteIdVacio_DeberiaRetornarError()
    {
        // Arrange - Usar inicializador de objeto para propiedades init-only
        var command = new CanjearPuntosCommand
        {
            ClienteId = Guid.Empty,
            PuntosAUtilizar = 100,
            ComandaId = Guid.NewGuid(),
            Motivo = "Canje de puntos por descuento en comanda"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.ClienteId) &&
            e.ErrorMessage.Contains("El ID del cliente es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConClienteIdValido_NoDeberiaRetornarErrorDeClienteId()
    {
        // Arrange - Usar inicializador de objeto para propiedades init-only
        var command = new CanjearPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            PuntosAUtilizar = 100,
            ComandaId = Guid.NewGuid(),
            Motivo = "Canje de puntos por descuento en comanda"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.ClienteId) &&
            e.ErrorMessage.Contains("El ID del cliente es obligatorio"));
    }

    #endregion

    #region Validación PuntosAUtilizar

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_ConPuntosAUtilizarMenorOIgualACero_DeberiaRetornarError(int puntosInvalidos)
    {
        // Arrange - Usar inicializador de objeto para propiedades init-only
        var command = new CanjearPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            PuntosAUtilizar = puntosInvalidos,
            ComandaId = Guid.NewGuid(),
            Motivo = "Canje de puntos por descuento en comanda"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.PuntosAUtilizar) &&
            e.ErrorMessage.Contains("La cantidad de puntos debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConPuntosAUtilizarExcesivos_DeberiaRetornarError()
    {
        // Arrange - Usar inicializador de objeto para propiedades init-only
        var command = new CanjearPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            PuntosAUtilizar = 10001, // Más de 10,000 puntos
            ComandaId = Guid.NewGuid(),
            Motivo = "Canje de puntos por descuento en comanda"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.PuntosAUtilizar) &&
            e.ErrorMessage.Contains("No se pueden canjear más de 10,000 puntos en una sola operación"));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(5000)]
    [InlineData(10000)]
    public async Task Validate_ConPuntosAUtilizarValidos_NoDeberiaRetornarErrorDePuntos(int puntosValidos)
    {
        // Arrange - Usar inicializador de objeto para propiedades init-only
        var command = new CanjearPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            PuntosAUtilizar = puntosValidos,
            ComandaId = Guid.NewGuid(),
            Motivo = "Canje de puntos por descuento en comanda"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.PuntosAUtilizar) &&
            (e.ErrorMessage.Contains("debe ser mayor a 0") || e.ErrorMessage.Contains("más de 10,000 puntos")));
    }

    #endregion

    #region Validación Múltiplos de Puntos

    [Theory]
    [InlineData(15)]  // No es múltiplo de 10
    [InlineData(23)]  // No es múltiplo de 10
    [InlineData(47)]  // No es múltiplo de 10
    [InlineData(99)]  // No es múltiplo de 10
    [InlineData(101)] // No es múltiplo de 10
    public async Task Validate_ConPuntosNoMultiplosDeDiez_DeberiaRetornarError(int puntosNoMultiplos)
    {
        // Arrange
        var command = CanjearPuntosCommand.Crear(Guid.NewGuid(), puntosNoMultiplos);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.PuntosAUtilizar) &&
            e.ErrorMessage.Contains("Los puntos deben canjearse en múltiplos de 10"));
    }

    [Theory]
    [InlineData(10)]   // Múltiplo de 10
    [InlineData(20)]   // Múltiplo de 10
    [InlineData(50)]   // Múltiplo de 10
    [InlineData(100)]  // Múltiplo de 10
    [InlineData(500)]  // Múltiplo de 10
    [InlineData(1000)] // Múltiplo de 10
    [InlineData(10000)] // Múltiplo de 10
    public async Task Validate_ConPuntosMultiplosDeDiez_NoDeberiaRetornarErrorDeMultiplos(int puntosMultiplos)
    {
        // Arrange
        var command = CanjearPuntosCommand.Crear(Guid.NewGuid(), puntosMultiplos);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.PuntosAUtilizar) &&
            e.ErrorMessage.Contains("Los puntos deben canjearse en múltiplos de 10"));
    }

    #endregion

    #region Validación ComandaId

    [Fact]
    public async Task Validate_ConComandaIdVaciaCuandoSeEspecifica_DeberiaRetornarError()
    {
        // Arrange
        var command = CanjearPuntosCommand.Crear(Guid.NewGuid(), 100, Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.ComandaId) &&
            e.ErrorMessage.Contains("El ID de la comanda es obligatorio cuando se especifica"));
    }

    [Fact]
    public async Task Validate_ConComandaIdValidaCuandoSeEspecifica_NoDeberiaRetornarErrorDeComandaId()
    {
        // Arrange
        var command = CanjearPuntosCommand.Crear(Guid.NewGuid(), 100, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.ComandaId));
    }

    [Fact]
    public async Task Validate_ConComandaIdNull_NoDeberiaValidarComandaId()
    {
        // Arrange
        var command = CanjearPuntosCommand.Crear(Guid.NewGuid(), 100, null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.ComandaId));
    }

    #endregion

    #region Validación Motivo

    [Fact]
    public async Task Validate_ConMotivoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var motivoLargo = new string('A', 501); // Más de 500 caracteres
        var command = CanjearPuntosCommand.Crear(Guid.NewGuid(), 100, null, motivoLargo);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.Motivo) &&
            e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres"));
    }

    [Theory]
    [InlineData("Canje por descuento en comanda")]
    [InlineData("Redención de puntos acumulados por fidelidad del cliente")]
    [InlineData("Aplicación de beneficio por aniversario")]
    [InlineData("Descuento especial por cliente frecuente")]
    public async Task Validate_ConMotivoValido_NoDeberiaRetornarErrorDeMotivo(string motivoValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = motivoValido;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.Motivo));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConMotivoVacio_NoDeberiaValidarLongitud(string motivoVacio)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = motivoVacio;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.Motivo));
    }

    [Fact]
    public async Task Validate_ConMotivoEnLimiteMaximo_NoDeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = new string('M', 500); // Exactamente 500 caracteres

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.Motivo));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new CanjearPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            PuntosAUtilizar = 500,
            ComandaId = Guid.NewGuid(),
            Motivo = "Canje de puntos por descuento del 20% en comanda especial de aniversario"
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
        var command = new CanjearPuntosCommand
        {
            ClienteId = Guid.NewGuid(),
            PuntosAUtilizar = 10
            // ComandaId y Motivo omitidos (opcionales)
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
        var command = new CanjearPuntosCommand
        {
            ClienteId = Guid.Empty, // Error
            PuntosAUtilizar = -50, // Error - negativo
            ComandaId = Guid.Empty, // Error - vacío
            Motivo = new string('A', 501) // Error - muy largo
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
    }

    [Fact]
    public async Task Validate_ConPuntosExcesivosYNoMultiplos_DeberiaRetornarAmbosErrores()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 10005; // Más de 10,000 Y no múltiplo de 10

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => 
            e.ErrorMessage.Contains("No se pueden canjear más de 10,000 puntos"));
        result.Errors.Should().Contain(e => 
            e.ErrorMessage.Contains("Los puntos deben canjearse en múltiplos de 10"));
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData(10, "Descuento mínimo")]
    [InlineData(100, "Descuento básico en bebida")]
    [InlineData(500, "Descuento medio en comanda")]
    [InlineData(1000, "Descuento grande - cliente frecuente")]
    [InlineData(5000, "Descuento premium por aniversario")]
    [InlineData(10000, "Descuento máximo - cliente VIP")]
    public async Task Validate_ConDiferentesCantidadesPuntos_DeberiaSerValido(int puntos, string motivo)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = puntos;
        command.Motivo = motivo;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCanjeParaDescuentoEspecial_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 2000;
        command.ComandaId = Guid.NewGuid();
        command.Motivo = "Canje especial por cumpleaños del cliente - descuento del 25%";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCanjeSinComanda_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 1500;
        command.ComandaId = null;
        command.Motivo = "Canje de puntos por productos promocionales para llevar";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCanjeMinimo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 10; // Mínimo canje posible
        command.ComandaId = Guid.NewGuid();
        command.Motivo = "Canje mínimo por descuento en postre";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCanjeMaximo_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 10000; // Máximo canje posible
        command.ComandaId = Guid.NewGuid();
        command.Motivo = "Canje máximo por evento especial - cliente platino";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData(1, true)]     // 1 carácter - válido
    [InlineData(50, true)]    // 50 caracteres - válido
    [InlineData(250, true)]   // 250 caracteres - válido
    [InlineData(500, true)]   // 500 caracteres - límite válido
    [InlineData(501, false)]  // 501 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesMotivo_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = new string('M', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(CanjearPuntosCommand.Motivo) &&
                e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres"));
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => 
                e.PropertyName == nameof(CanjearPuntosCommand.Motivo) &&
                e.ErrorMessage.Contains("El motivo no puede exceder 500 caracteres"));
        }
    }

    [Fact]
    public async Task Validate_ConMotivoConCaracteresEspeciales_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = "Canje: ñáéíóú @#$%^&*()_+ cliente VIP 😊 - 50% descuento";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConMotivoConEmojis_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = "Canje especial 🎉🎂 por cumpleaños del cliente 🥳 - descuento 25% 💰";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(30)]
    [InlineData(40)]
    [InlineData(50)]
    [InlineData(60)]
    [InlineData(70)]
    [InlineData(80)]
    [InlineData(90)]
    [InlineData(100)]
    public async Task Validate_ConMultiplosDeDiezEnRangoBajo_DeberiaSerValido(int puntos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = puntos;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(2000)]
    [InlineData(3000)]
    [InlineData(4000)]
    [InlineData(5000)]
    [InlineData(6000)]
    [InlineData(7000)]
    [InlineData(8000)]
    [InlineData(9000)]
    [InlineData(10000)]
    public async Task Validate_ConMultiplosDeDiezEnRangoAlto_DeberiaSerValido(int puntos)
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = puntos;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Performance y Concurrencia

    [Fact]
    public async Task Validate_ConMultiplesValidacionesConcurrentes_DeberiaSerConsistente()
    {
        // Arrange
        var commands = Enumerable.Range(1, 10)
            .Select(i => 
            {
                var cmd = CrearCommandValido();
                cmd.ClienteId = Guid.NewGuid();
                cmd.PuntosAUtilizar = i * 10; // Múltiplos de 10
                return cmd;
            })
            .ToList();

        // Act
        var tasks = commands.Select(cmd => _validator.ValidateAsync(cmd));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
    }

    [Fact]
    public async Task Validate_ConDiferentesPuntosEnParalelo_DeberiaValidarTodos()
    {
        // Arrange
        var puntosValidos = new[] { 10, 50, 100, 500, 1000, 5000, 10000 };
        var commands = puntosValidos.Select(puntos => 
        {
            var cmd = CrearCommandValido();
            cmd.PuntosAUtilizar = puntos;
            cmd.ClienteId = Guid.NewGuid();
            return cmd;
        }).ToList();

        // Act
        var tasks = commands.Select(cmd => _validator.ValidateAsync(cmd));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
    }

    #endregion

    #region Tests de Casos Edge

    [Fact]
    public async Task Validate_ConPuntosEnLimiteSuperior_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 10000; // Exactamente en el límite

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConPuntosEnLimiteInferior_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 10; // Mínimo múltiplo de 10

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConPuntosCero_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 0;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.ErrorMessage.Contains("La cantidad de puntos debe ser mayor a 0"));
    }

    [Fact]
    public async Task Validate_ConPuntosJustoSobreLimite_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.PuntosAUtilizar = 10010; // 10 puntos sobre el límite

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.ErrorMessage.Contains("No se pueden canjear más de 10,000 puntos"));
    }

    #endregion

    #region Tests de Validaciones Condicionales

    [Fact]
    public async Task Validate_ConComandaIdCuandoEsOpcional_NoDeberiaRequerirla()
    {
        // Arrange
        var command = CrearCommandValido();
        command.ComandaId = null; // Opcional

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.ComandaId));
    }

    [Fact]
    public async Task Validate_ConMotivoCuandoEsOpcional_NoDeberiaRequerirlo()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Motivo = null; // Opcional

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(CanjearPuntosCommand.Motivo));
    }

    #endregion
} 