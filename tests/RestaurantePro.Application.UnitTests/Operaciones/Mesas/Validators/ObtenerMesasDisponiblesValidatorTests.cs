namespace RestaurantePro.Application.UnitTests.Operaciones.Mesas.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA OBTENER MESAS DISPONIBLES VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de consulta de mesas disponibles
/// Cobertura: 100% de reglas de negocio del ObtenerMesasDisponiblesValidator
/// </summary>
public class ObtenerMesasDisponiblesValidatorTests
{
    private readonly ObtenerMesasDisponiblesValidator _validator;

    public ObtenerMesasDisponiblesValidatorTests()
    {
        _validator = new ObtenerMesasDisponiblesValidator();
    }

    #region Validation Query Helper

    private ObtenerMesasDisponiblesQuery CrearQueryValida()
    {
        return new ObtenerMesasDisponiblesQuery
        {
            FechaInicio = DateTime.UtcNow.AddHours(1),
            FechaFin = DateTime.UtcNow.AddHours(3),
            CapacidadMinima = 2,
            CapacidadMaxima = 8,
            IncluirReservadas = false,
            TipoMesa = "Estándar"
        };
    }

    #endregion

    #region Validación FechaInicio

    [Fact]
    public async Task Validate_ConFechaInicioEnPasado_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow.AddHours(-2);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.FechaInicio) &&
            e.ErrorMessage.Contains("La fecha de inicio no puede ser en el pasado") &&
            e.ErrorCode == "FECHA_INICIO_PASADO");
    }

    [Fact]
    public async Task Validate_ConFechaInicioMuyLejana_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow.AddDays(91); // Más de 90 días

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.FechaInicio) &&
            e.ErrorMessage.Contains("La fecha de inicio no puede ser mayor a 90 días en el futuro") &&
            e.ErrorCode == "FECHA_INICIO_MUY_FUTURA");
    }

    [Theory]
    [InlineData(1)]   // 1 hora
    [InlineData(24)]  // 1 día
    [InlineData(168)] // 1 semana
    [InlineData(720)] // 1 mes
    public async Task Validate_ConFechaInicioValida_NoDeberiaRetornarErrorDeFecha(int horasAdelante)
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow.AddHours(horasAdelante);
        query.FechaFin = query.FechaInicio.AddHours(2);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.FechaInicio));
    }

    #endregion

    #region Validación FechaFin

    [Fact]
    public async Task Validate_ConFechaFinAnteriorAInicio_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow.AddHours(3);
        query.FechaFin = DateTime.UtcNow.AddHours(1); // Anterior a inicio

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.FechaFin) &&
            e.ErrorMessage.Contains("La fecha de fin debe ser posterior a la fecha de inicio") &&
            e.ErrorCode == "FECHA_FIN_ANTERIOR");
    }

    [Fact]
    public async Task Validate_ConRangoMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow.AddHours(1);
        query.FechaFin = query.FechaInicio.AddHours(25); // Más de 24 horas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.FechaFin) &&
            e.ErrorMessage.Contains("El rango de fechas no puede exceder 24 horas") &&
            e.ErrorCode == "RANGO_FECHAS_EXCESIVO");
    }

    [Theory]
    [InlineData(1)]   // 1 hora
    [InlineData(2)]   // 2 horas
    [InlineData(4)]   // 4 horas
    [InlineData(8)]   // 8 horas
    [InlineData(12)]  // 12 horas
    [InlineData(24)]  // 24 horas (límite)
    public async Task Validate_ConRangoValido_NoDeberiaRetornarErrorDeRango(int horasDuracion)
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow.AddHours(1);
        query.FechaFin = query.FechaInicio.AddHours(horasDuracion);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorCode == "RANGO_FECHAS_EXCESIVO");
    }

    #endregion

    #region Validación CapacidadMinima

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConCapacidadMinimaMenorIgualCero_DeberiaRetornarError(int capacidadInvalida)
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = capacidadInvalida;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMinima) &&
            e.ErrorMessage.Contains("La capacidad mínima debe ser mayor a 0") &&
            e.ErrorCode == "CAPACIDAD_MINIMA_INVALIDA");
    }

    [Fact]
    public async Task Validate_ConCapacidadMinimaExcesiva_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = 51; // Más de 50 personas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMinima) &&
            e.ErrorMessage.Contains("La capacidad mínima no puede exceder 50 personas") &&
            e.ErrorCode == "CAPACIDAD_MINIMA_EXCESIVA");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task Validate_ConCapacidadMinimaValida_NoDeberiaRetornarErrorDeCapacidad(int capacidadValida)
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = capacidadValida;
        query.CapacidadMaxima = Math.Max(capacidadValida, query.CapacidadMaxima);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMinima));
    }

    #endregion

    #region Validación CapacidadMaxima

    [Fact]
    public async Task Validate_ConCapacidadMaximaMenorAMinima_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = 6;
        query.CapacidadMaxima = 4; // Menor que mínima

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMaxima) &&
            e.ErrorMessage.Contains("La capacidad máxima debe ser mayor o igual a la capacidad mínima") &&
            e.ErrorCode == "CAPACIDAD_MAXIMA_MENOR");
    }

    [Fact]
    public async Task Validate_ConCapacidadMaximaExcesiva_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMaxima = 101; // Más de 100 personas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMaxima) &&
            e.ErrorMessage.Contains("La capacidad máxima no puede exceder 100 personas") &&
            e.ErrorCode == "CAPACIDAD_MAXIMA_EXCESIVA");
    }

    [Theory]
    [InlineData(2, 2)]   // Iguales
    [InlineData(2, 4)]   // Máxima mayor
    [InlineData(4, 8)]   // Diferencia normal
    [InlineData(6, 12)]  // Diferencia amplia
    [InlineData(10, 100)] // Límite máximo
    public async Task Validate_ConCapacidadesValidas_NoDeberiaRetornarErrorDeCapacidades(int minima, int maxima)
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = minima;
        query.CapacidadMaxima = maxima;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMaxima) ||
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.CapacidadMinima));
    }

    #endregion

    #region Validación TipoMesa

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConTipoMesaVacio_NoDeberiaValidarTipo(string tipoVacio)
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoMesa = tipoVacio;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.TipoMesa));
    }

    [Fact]
    public async Task Validate_ConTipoMesaMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoMesa = new string('A', 51); // Más de 50 caracteres

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.TipoMesa) &&
            e.ErrorMessage.Contains("El tipo de mesa no puede exceder 50 caracteres") &&
            e.ErrorCode == "TIPO_MESA_LONGITUD");
    }

    [Theory]
    [InlineData("Estándar")]
    [InlineData("VIP")]
    [InlineData("Terraza")]
    [InlineData("Bar")]
    [InlineData("Reservada")]
    [InlineData("Familiar")]
    public async Task Validate_ConTipoMesaValido_NoDeberiaRetornarErrorDeTipo(string tipoValido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoMesa = tipoValido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ObtenerMesasDisponiblesQuery.TipoMesa));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConQueryCompleta_DeberiaSerValida()
    {
        // Arrange
        var query = new ObtenerMesasDisponiblesQuery
        {
            FechaInicio = DateTime.UtcNow.AddHours(2),
            FechaFin = DateTime.UtcNow.AddHours(4),
            CapacidadMinima = 4,
            CapacidadMaxima = 8,
            IncluirReservadas = false,
            TipoMesa = "Estándar"
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
        var query = new ObtenerMesasDisponiblesQuery
        {
            FechaInicio = DateTime.UtcNow.AddHours(1),
            FechaFin = DateTime.UtcNow.AddHours(2),
            CapacidadMinima = 1,
            CapacidadMaxima = 1
            // TipoMesa e IncluirReservadas opcionales
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ConMultiplesErrores_DeberiaRetornarTodosLosErrores()
    {
        // Arrange
        var query = new ObtenerMesasDisponiblesQuery
        {
            FechaInicio = DateTime.UtcNow.AddHours(-1), // Error - pasado
            FechaFin = DateTime.UtcNow.AddHours(-2), // Error - anterior a inicio
            CapacidadMinima = 0, // Error - cero
            CapacidadMaxima = 101, // Error - excesiva
            TipoMesa = new string('X', 51) // Error - muy largo
        };

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    #endregion

    #region Tests de Escenarios de Negocio

    [Theory]
    [InlineData("Almuerzo", 2, 4)]
    [InlineData("Cena", 4, 8)]
    [InlineData("Brunch", 2, 6)]
    [InlineData("Eventos", 10, 50)]
    public async Task Validate_ConDiferentesEscenarios_DeberiaSerValido(string tipoMesa, int capMin, int capMax)
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoMesa = tipoMesa;
        query.CapacidadMinima = capMin;
        query.CapacidadMaxima = capMax;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConBusquedaParaEventoEspecial_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow.AddDays(7); // Una semana adelante
        query.FechaFin = query.FechaInicio.AddHours(6);
        query.CapacidadMinima = 20;
        query.CapacidadMaxima = 50;
        query.TipoMesa = "Evento Especial";
        query.IncluirReservadas = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConBusquedaRapida_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow.AddMinutes(30);
        query.FechaFin = query.FechaInicio.AddHours(1);
        query.CapacidadMinima = 2;
        query.CapacidadMaxima = 2;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Fact]
    public async Task Validate_ConFechaInicioExactamenteAhora_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow;
        query.FechaFin = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConRangoExactamente24Horas_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaInicio = DateTime.UtcNow.AddHours(1);
        query.FechaFin = query.FechaInicio.AddHours(24);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConCapacidadesEnLimites_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.CapacidadMinima = 1; // Límite inferior
        query.CapacidadMaxima = 100; // Límite superior

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConTipoMesaEnLimiteMaximo_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoMesa = new string('M', 50); // Exactamente 50 caracteres

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Performance y Concurrencia

    [Fact]
    public async Task Validate_ConMultiplesValidacionesConcurrentes_DeberiaSerConsistente()
    {
        // Arrange
        var queries = Enumerable.Range(1, 10)
            .Select(i => 
            {
                var q = CrearQueryValida();
                q.FechaInicio = DateTime.UtcNow.AddHours(i);
                q.FechaFin = q.FechaInicio.AddHours(2);
                return q;
            })
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
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    #endregion

    #region Tests de Casos Edge

    [Fact]
    public async Task Validate_ConIncluirReservadasTrue_NoDeberiaAfectarValidacion()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirReservadas = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConIncluirReservadasFalse_NoDeberiaAfectarValidacion()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirReservadas = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
} 