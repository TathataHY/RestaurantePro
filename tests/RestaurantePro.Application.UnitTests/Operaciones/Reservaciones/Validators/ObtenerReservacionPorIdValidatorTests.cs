namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA OBTENER RESERVACION POR ID VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de consulta de reservación por ID
/// Cobertura: 100% de reglas de negocio del ObtenerReservacionPorIdValidator
/// </summary>
public class ObtenerReservacionPorIdValidatorTests
{
    private readonly ObtenerReservacionPorIdValidator _validator;

    public ObtenerReservacionPorIdValidatorTests()
    {
        _validator = new ObtenerReservacionPorIdValidator();
    }

    #region Validation Query Helper

    private ObtenerReservacionPorIdQuery CrearQueryValida()
    {
        return new ObtenerReservacionPorIdQuery
        {
            Id = Guid.NewGuid(),
            IncluirDetalles = true,
            IncluirHistorial = false
        };
    }

    #endregion

    #region Validación ReservacionId

    [Fact]
    public async Task Validate_ConReservacionIdVacia_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerReservacionPorIdQuery.Id) &&
            e.ErrorMessage.Contains("El ID de la reservación es requerido") &&
            e.ErrorCode == "RESERVACION_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConReservacionIdValida_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConQueryCompleta_DeberiaSerValida()
    {
        // Arrange
        var query = new ObtenerReservacionPorIdQuery
        {
            Id = Guid.NewGuid(),
            IncluirDetalles = true,
            IncluirHistorial = true
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConQueryMinima_DeberiaSerValida()
    {
        // Arrange
        var query = new ObtenerReservacionPorIdQuery
        {
            Id = Guid.NewGuid()
            // Campos opcionales omitidos
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConDiferentesGuids_DeberiaSerValido()
    {
        // Arrange
        var queries = new[]
        {
            new ObtenerReservacionPorIdQuery { Id = Guid.NewGuid() },
            new ObtenerReservacionPorIdQuery { Id = Guid.NewGuid() },
            new ObtenerReservacionPorIdQuery { Id = Guid.NewGuid() }
        };

        // Act & Assert
        foreach (var query in queries)
        {
            var result = await _validator.ValidateAsync(query);
            result.IsValid.Should().BeTrue();
        }
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Fact]
    public async Task Validate_ConBusquedaReservacionActiva_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = new Guid("12345678-1234-1234-1234-123456789012");
        query.IncluirDetalles = true;
        query.IncluirHistorial = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConBusquedaReservacionHistorial_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.NewGuid();
        query.IncluirDetalles = true;
        query.IncluirHistorial = true; // Incluir historial completo

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConConsultaBasica_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.NewGuid();
        query.IncluirDetalles = false; // Sin detalles adicionales
        query.IncluirHistorial = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConConsultaCompleta_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.NewGuid();
        query.IncluirDetalles = true; // Con todos los detalles
        query.IncluirHistorial = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Guid.Empty
    public async Task Validate_ConGuidEspecial_DeberiaRetornarError(string guidString)
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.Parse(guidString);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.ErrorCode == "RESERVACION_ID_REQUERIDO");
    }

    [Theory]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("87654321-4321-4321-4321-210987654321")]
    [InlineData("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE")]
    [InlineData("11111111-2222-3333-4444-555555555555")]
    public async Task Validate_ConGuidValido_DeberiaSerValido(string guidString)
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.Parse(guidString);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConGuidMinValue_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.Empty; // Equivalent to MinValue

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_ConGuidMaxValue_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Opciones de Consulta

    [Theory]
    [InlineData(true, true)]   // Todo incluido
    [InlineData(true, false)]  // Solo mesa
    [InlineData(false, true)]  // Solo historial
    [InlineData(false, false)] // Básico
    public async Task Validate_ConDiferentesOpciones_DeberiaSerValido(bool incluirMesa, bool incluirHistorial)
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirDetalles = incluirMesa;
        query.IncluirHistorial = incluirHistorial;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConIncluirDetallesTrue_NoDeberiaAfectarValidacion()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirDetalles = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        query.IncluirDetalles.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConIncluirHistorialTrue_NoDeberiaAfectarValidacion()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirHistorial = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        query.IncluirHistorial.Should().BeTrue();
    }

    #endregion

    #region Tests de Performance y Concurrencia

    [Fact]
    public async Task Validate_ConMultiplesValidacionesConcurrentes_DeberiaSerConsistente()
    {
        // Arrange
        var queries = Enumerable.Range(1, 10)
            .Select(_ => new ObtenerReservacionPorIdQuery { Id = Guid.NewGuid() })
            .ToList();

        // Act
        var tasks = queries.Select(q => _validator.ValidateAsync(q));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
    }

    [Fact]
    public async Task Validate_ConValidacionRapida_DeberiaCompletarseRapidamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _validator.ValidateAsync(query);
        stopwatch.Stop();

        // Assert
        result.IsValid.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Debe ser muy rápido
    }

    [Fact]
    public async Task Validate_ConDiferentesReservacionesEnParalelo_DeberiaValidarTodas()
    {
        // Arrange
        var reservacionIds = new[]
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var queries = reservacionIds.Select(id => new ObtenerReservacionPorIdQuery { Id = id }).ToList();

        // Act
        var tasks = queries.Select(q => _validator.ValidateAsync(q));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(result => result.IsValid.Should().BeTrue());
    }

    #endregion

    #region Tests de Casos Edge

    [Fact]
    public async Task Validate_ConQueryNueva_DeberiaSerValida()
    {
        // Arrange
        var query = new ObtenerReservacionPorIdQuery();
        query.Id = Guid.NewGuid();

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConAsignacionDirecta_DeberiaSerValida()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query = new ObtenerReservacionPorIdQuery { Id = reservacionId };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        query.Id.Should().Be(reservacionId);
    }

    [Fact]
    public async Task Validate_ConOpcionesDefault_DeberiaSerValida()
    {
        // Arrange
        var query = new ObtenerReservacionPorIdQuery
        {
            Id = Guid.NewGuid()
            // IncluirDetalles e IncluirHistorial tendrán valores default
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Mensajes de Error

    [Fact]
    public async Task Validate_ConReservacionIdVacia_DeberiaRetornarMensajeEspecifico()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        var error = result.Errors.Single();
        error.PropertyName.Should().Be(nameof(ObtenerReservacionPorIdQuery.Id));
        error.ErrorMessage.Should().Be("El ID de la reservación es requerido para realizar la consulta");
        error.ErrorCode.Should().Be("RESERVACION_ID_REQUERIDO");
    }

    [Fact]
    public async Task Validate_ConErrorDeValidacion_DeberiaIncluirPropertyName()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.Empty;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().AllSatisfy(error =>
        {
            error.PropertyName.Should().NotBeNullOrWhiteSpace();
            error.PropertyName.Should().Be(nameof(ObtenerReservacionPorIdQuery.Id));
        });
    }

    #endregion

    #region Tests de Validador Instance

    [Fact]
    public void Validator_DeberiaImplementarAbstractValidator()
    {
        // Assert
        _validator.Should().BeAssignableTo<AbstractValidator<ObtenerReservacionPorIdQuery>>();
    }

    [Fact]
    public void Validator_DeberiaInicializarseCorrectamente()
    {
        // Arrange & Act
        var validator = new ObtenerReservacionPorIdValidator();

        // Assert
        validator.Should().NotBeNull();
    }

    [Fact]
    public async Task Validator_ConQueryNull_NoDeberiaLanzarExcepcion()
    {
        // Arrange
        ObtenerReservacionPorIdQuery query = null;

        // Act
        Func<Task> act = async () => await _validator.ValidateAsync(query);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Tests de Factory Methods (si existen)

    [Fact]
    public void Query_DeberiaCrearseConFactoryMethod()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();

        // Act
        var query = ObtenerReservacionPorIdQuery.Completa(reservacionId);

        // Assert
        query.Id.Should().Be(reservacionId);
    }

    [Fact]
    public void Query_FactoryMethodConOpciones_DeberiaCrearseCorrectamente()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();

        // Act
        var query = ObtenerReservacionPorIdQuery.Basica(reservacionId, incluirDetalles: true, incluirHistorial: true);

        // Assert
        query.Id.Should().Be(reservacionId);
        query.IncluirDetalles.Should().BeTrue();
        query.IncluirHistorial.Should().BeTrue();
    }

    [Fact]
    public void Query_FactoryMethod_ConGuidVacio_DeberiaLanzarExcepcion()
    {
        // Act & Assert
        var act = () => ObtenerReservacionPorIdQuery.Basica(Guid.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Id*");
    }

    #endregion

    #region Tests de Inmutabilidad y Comportamiento

    [Fact]
    public void Query_DeberiaSerRecord()
    {
        // Arrange
        var query1 = new ObtenerReservacionPorIdQuery { Id = Guid.NewGuid() };
        var query2 = query1 with { };

        // Act & Assert
        query1.Should().BeEquivalentTo(query2);
    }

    [Fact]
    public void Query_ConMismoId_DeberiaSerIgual()
    {
        // Arrange
        var reservacionId = Guid.NewGuid();
        var query1 = new ObtenerReservacionPorIdQuery { Id = reservacionId };
        var query2 = new ObtenerReservacionPorIdQuery { Id = reservacionId };

        // Act & Assert
        query1.Should().BeEquivalentTo(query2);
    }

    [Fact]
    public void Query_ConDiferentesOpciones_DeberiaModificarseCorrectamente()
    {
        // Arrange
        var query = CrearQueryValida();
        var originalIncluirMesa = query.IncluirDetalles;

        // Act
        var queryModificada = query with { IncluirDetalles = !originalIncluirMesa };

        // Assert
        queryModificada.IncluirDetalles.Should().Be(!originalIncluirMesa);
        queryModificada.Id.Should().Be(query.Id);
    }

    #endregion

    #region Tests de Scenarios Específicos de Reservaciones

    [Fact]
    public async Task Validate_ConBusquedaReservacionVIP_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.NewGuid();
        query.IncluirDetalles = true; // VIP necesita detalles de mesa
        query.IncluirHistorial = true; // VIP necesita historial completo

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConBusquedaReservacionCancelada_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.Id = Guid.NewGuid();
        query.IncluirDetalles = false; // No necesita mesa para cancelada
        query.IncluirHistorial = true; // Sí necesita historial para cancelada

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
} 