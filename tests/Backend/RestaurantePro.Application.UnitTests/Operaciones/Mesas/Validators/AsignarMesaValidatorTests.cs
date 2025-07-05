namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA ASIGNAR MESA VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de asignación de mesas
/// Cobertura: 100% de reglas de negocio del AsignarMesaValidator
/// </summary>
public class AsignarMesaValidatorTests
{
    private readonly AsignarMesaValidator _validator;

    public AsignarMesaValidatorTests()
    {
        _validator = new AsignarMesaValidator();
    }

    #region Validation Command Helper

    private AsignarMesaCommand CrearCommandValido()
    {
        return new AsignarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = "Mesa asignada para clientes VIP",
            MeseroId = Guid.NewGuid()
        };
    }

    #endregion

    #region Validación MesaId

    [Fact]
    public async Task Validate_ConMesaIdVacio_DeberiaRetornarError()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MesaId = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(AsignarMesaCommand.MesaId) &&
            e.ErrorMessage.Contains("El ID de la mesa es obligatorio"));
    }

    [Fact]
    public async Task Validate_ConMesaIdValido_NoDeberiaRetornarErrorDeMesaId()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MesaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AsignarMesaCommand.MesaId) &&
            e.ErrorMessage.Contains("El ID de la mesa es obligatorio"));
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
            e.PropertyName == nameof(AsignarMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
    }

    [Theory]
    [InlineData("Observaciones cortas")]
    [InlineData("Mesa asignada para clientes VIP requiriendo atención especial")]
    [InlineData("Reservación para evento empresarial importante con configuración especial")]
    public async Task Validate_ConObservacionesLongitudValida_NoDeberiaRetornarErrorDeObservaciones(string observacionesValidas)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observacionesValidas;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AsignarMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
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
            e.PropertyName == nameof(AsignarMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
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
            e.PropertyName == nameof(AsignarMesaCommand.Observaciones) &&
            e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
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
            e.PropertyName == nameof(AsignarMesaCommand.MeseroId) &&
            e.ErrorMessage.Contains("El ID del mesero no puede ser un GUID vacío"));
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
            e.PropertyName == nameof(AsignarMesaCommand.MeseroId) &&
            e.ErrorMessage.Contains("El ID del mesero no puede ser un GUID vacío"));
    }

    [Fact]
    public async Task Validate_ConMeseroIdNull_NoDeberiaValidar()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MeseroId = null;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(AsignarMesaCommand.MeseroId));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConCommandCompletoValido_DeberiaSerValido()
    {
        // Arrange
        var command = new AsignarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = "Mesa asignada para clientes especiales, requiere atención prioritaria",
            MeseroId = Guid.NewGuid()
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
        var command = new AsignarMesaCommand
        {
            MesaId = Guid.NewGuid()
            // Campos opcionales omitidos
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConSoloMeseroId_DeberiaSerValido()
    {
        // Arrange
        var command = new AsignarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid()
            // Observaciones omitidas
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConSoloObservaciones_DeberiaSerValido()
    {
        // Arrange
        var command = new AsignarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = "Mesa para evento especial"
            // MeseroId omitido
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
        var command = new AsignarMesaCommand
        {
            MesaId = Guid.Empty, // Error
            Observaciones = new string('A', 501), // Error  
            MeseroId = Guid.Empty // Error
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Mesa VIP para clientes especiales")]
    [InlineData("Reservación para evento corporativo")]
    [InlineData("Mesa familiar - requiere silla alta para bebé")]
    [InlineData("Cliente habitual - mesa preferida")]
    public async Task Validate_ConDiferentesObservaciones_DeberiaSerValido(string observaciones)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = observaciones;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConAsignacionAutomatica_DeberiaSerValido()
    {
        // Arrange - Simulando asignación automática sin mesero específico
        var command = new AsignarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            Observaciones = "Asignación automática del sistema"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConAsignacionManual_DeberiaSerValido()
    {
        // Arrange - Simulando asignación manual con mesero específico
        var command = new AsignarMesaCommand
        {
            MesaId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            Observaciones = "Asignación manual - mesero especializado"
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData(1, true)]    // 1 carácter - válido
    [InlineData(250, true)]  // 250 caracteres - válido
    [InlineData(500, true)]  // 500 caracteres - límite válido
    [InlineData(501, false)] // 501 caracteres - inválido
    public async Task Validate_ConDiferentesLongitudesObservaciones_DeberiaValidarCorrectamente(int longitud, bool deberiaSerValido)
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = new string('O', longitud);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        if (deberiaSerValido)
        {
            result.Errors.Should().NotContain(e => 
                e.PropertyName == nameof(AsignarMesaCommand.Observaciones) &&
                e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
        }
        else
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => 
                e.PropertyName == nameof(AsignarMesaCommand.Observaciones) &&
                e.ErrorMessage.Contains("Las observaciones no pueden exceder los 500 caracteres"));
        }
    }

    [Fact]
    public async Task Validate_ConObservacionesCaracteresEspeciales_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = "Mesa para Pérez & Co. - Evento #1 (VIP) - Req. especiales: 50% desc.";

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
        command.Observaciones = "Mesa especial 🍽️ Cliente VIP ⭐ Atención prioritaria 🥇";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConObservacionesNumeros_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.Observaciones = "Mesa 15 - Reservación #12345 - 8 personas - Hora: 19:30";

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Rendimiento y Concurrencia

    [Fact]
    public async Task Validate_ConValidacionConcurrente_DeberiaSerThreadSafe()
    {
        // Arrange
        var commands = Enumerable.Range(0, 100)
            .Select(_ => CrearCommandValido())
            .ToList();

        // Act
        var tasks = commands.Select(command => _validator.ValidateAsync(command));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
    }

    [Fact]
    public async Task Validate_ConMultiplesInstanciasValidator_DeberiaFuncionarCorrectamente()
    {
        // Arrange
        var validators = Enumerable.Range(0, 10)
            .Select(_ => new AsignarMesaValidator())
            .ToList();

        var command = CrearCommandValido();

        // Act
        var tasks = validators.Select(validator => validator.ValidateAsync(command));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
    }

    #endregion

    #region Tests de Casos Límite

    [Fact]
    public async Task Validate_ConGuidNuevoParaMesa_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MesaId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConGuidNuevoParaMesero_DeberiaSerValido()
    {
        // Arrange
        var command = CrearCommandValido();
        command.MeseroId = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConMismoGuidParaMesaYMesero_DeberiaSerValido()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = CrearCommandValido();
        command.MesaId = guid;
        command.MeseroId = guid; // Mismo GUID (caso hipotético)

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
} 