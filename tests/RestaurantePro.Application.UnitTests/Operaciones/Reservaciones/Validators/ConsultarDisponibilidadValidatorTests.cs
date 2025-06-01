namespace RestaurantePro.Application.UnitTests.Operaciones.Reservaciones.Validators;

/// <summary>
/// 🔥 TESTS EXHAUSTIVOS PARA CONSULTAR DISPONIBILIDAD VALIDATOR - IMPLEMENTACIÓN COMPLETA
/// Tests completos para validar todas las reglas críticas de consulta de disponibilidad
/// Cobertura: 100% de reglas de negocio del ConsultarDisponibilidadValidator
/// </summary>
public class ConsultarDisponibilidadValidatorTests
{
    private readonly ConsultarDisponibilidadValidator _validator;

    public ConsultarDisponibilidadValidatorTests()
    {
        _validator = new ConsultarDisponibilidadValidator();
    }

    #region Validation Query Helper

    private ConsultarDisponibilidadQuery CrearQueryValida()
    {
        return new ConsultarDisponibilidadQuery
        {
            FechaReservacion = DateTime.UtcNow.AddHours(2),
            HoraInicio = TimeSpan.FromHours(12), // 12:00 PM
            HoraFin = TimeSpan.FromHours(14), // 2:00 PM
            CantidadPersonas = 4,
            TipoReservacion = "Estándar",
            RequiereConfirmacion = false,
            IncluirMesasReservadas = false
        };
    }

    #endregion

    #region Validación FechaReservacion

    [Fact]
    public async Task Validate_ConFechaReservacionEnPasado_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReservacion = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.FechaReservacion) &&
            e.ErrorMessage.Contains("La fecha de reservación no puede ser en el pasado") &&
            e.ErrorCode == "FECHA_RESERVACION_PASADO");
    }

    [Fact]
    public async Task Validate_ConFechaReservacionMuyLejana_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReservacion = DateTime.UtcNow.AddDays(181); // Más de 180 días

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.FechaReservacion) &&
            e.ErrorMessage.Contains("No se pueden hacer reservaciones con más de 180 días de anticipación") &&
            e.ErrorCode == "FECHA_RESERVACION_MUY_FUTURA");
    }

    [Theory]
    [InlineData(1)]   // 1 día
    [InlineData(7)]   // 1 semana
    [InlineData(30)]  // 1 mes
    [InlineData(90)]  // 3 meses
    [InlineData(180)] // 6 meses (límite)
    public async Task Validate_ConFechaReservacionValida_NoDeberiaRetornarErrorDeFecha(int diasAdelante)
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReservacion = DateTime.UtcNow.AddDays(diasAdelante);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.FechaReservacion));
    }

    #endregion

    #region Validación HoraInicio

    [Theory]
    [InlineData(5, 0)]   // 5:00 AM - muy temprano
    [InlineData(2, 30)]  // 2:30 AM - madrugada
    public async Task Validate_ConHoraInicioMuyTemprana_DeberiaRetornarError(int horas, int minutos)
    {
        // Arrange
        var query = CrearQueryValida();
        query.HoraInicio = new TimeSpan(horas, minutos, 0);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.HoraInicio) &&
            e.ErrorMessage.Contains("La hora de inicio debe estar entre las 6:00 AM y las 11:00 PM") &&
            e.ErrorCode == "HORA_INICIO_FUERA_HORARIO");
    }

    [Theory]
    [InlineData(23, 30)] // 11:30 PM - muy tarde
    [InlineData(23, 59)] // 11:59 PM - límite
    public async Task Validate_ConHoraInicioMuyTarde_DeberiaRetornarError(int horas, int minutos)
    {
        // Arrange
        var query = CrearQueryValida();
        query.HoraInicio = new TimeSpan(horas, minutos, 0);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.HoraInicio) &&
            e.ErrorCode == "HORA_INICIO_FUERA_HORARIO");
    }

    [Theory]
    [InlineData(6, 0)]   // 6:00 AM - límite inferior
    [InlineData(8, 30)]  // 8:30 AM - desayuno
    [InlineData(12, 0)]  // 12:00 PM - almuerzo
    [InlineData(18, 30)] // 6:30 PM - cena
    [InlineData(22, 30)] // 10:30 PM - cena tardía
    [InlineData(23, 0)]  // 11:00 PM - límite superior
    public async Task Validate_ConHoraInicioValida_NoDeberiaRetornarErrorDeHora(int horas, int minutos)
    {
        // Arrange
        var query = CrearQueryValida();
        query.HoraInicio = new TimeSpan(horas, minutos, 0);
        query.HoraFin = query.HoraInicio.Add(TimeSpan.FromHours(2));
        if (query.HoraFin.TotalHours >= 24)
            query.HoraFin = new TimeSpan(23, 0, 0);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.HoraInicio) &&
            e.ErrorCode == "HORA_INICIO_FUERA_HORARIO");
    }

    #endregion

    #region Validación HoraFin

    [Fact]
    public async Task Validate_ConHoraFinAnteriorAInicio_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.HoraInicio = TimeSpan.FromHours(14); // 2:00 PM
        query.HoraFin = TimeSpan.FromHours(12); // 12:00 PM - anterior

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.HoraFin) &&
            e.ErrorMessage.Contains("La hora de fin debe ser posterior a la hora de inicio") &&
            e.ErrorCode == "HORA_FIN_ANTERIOR");
    }

    [Fact]
    public async Task Validate_ConDuracionMuyCorta_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.HoraInicio = TimeSpan.FromHours(12); // 12:00 PM
        query.HoraFin = TimeSpan.FromMinutes(720 + 20); // 12:20 PM - solo 20 minutos

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.HoraFin) &&
            e.ErrorMessage.Contains("La reservación debe tener una duración mínima de 30 minutos") &&
            e.ErrorCode == "DURACION_RESERVACION_CORTA");
    }

    [Fact]
    public async Task Validate_ConDuracionMuyLarga_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.HoraInicio = TimeSpan.FromHours(10); // 10:00 AM
        query.HoraFin = TimeSpan.FromHours(18); // 6:00 PM - 8 horas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.HoraFin) &&
            e.ErrorMessage.Contains("La reservación no puede exceder 6 horas de duración") &&
            e.ErrorCode == "DURACION_RESERVACION_LARGA");
    }

    [Theory]
    [InlineData(12, 0, 12, 30)] // 30 minutos - mínimo
    [InlineData(12, 0, 13, 0)]  // 1 hora
    [InlineData(12, 0, 14, 0)]  // 2 horas
    [InlineData(12, 0, 15, 0)]  // 3 horas
    [InlineData(12, 0, 18, 0)]  // 6 horas - máximo
    public async Task Validate_ConDuracionValida_NoDeberiaRetornarErrorDeDuracion(int horaIni, int minIni, int horaFin, int minFin)
    {
        // Arrange
        var query = CrearQueryValida();
        query.HoraInicio = new TimeSpan(horaIni, minIni, 0);
        query.HoraFin = new TimeSpan(horaFin, minFin, 0);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.ErrorCode == "DURACION_RESERVACION_CORTA" ||
            e.ErrorCode == "DURACION_RESERVACION_LARGA");
    }

    #endregion

    #region Validación CantidadPersonas

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConCantidadPersonasMenorIgualCero_DeberiaRetornarError(int cantidadInvalida)
    {
        // Arrange
        var query = CrearQueryValida();
        query.CantidadPersonas = cantidadInvalida;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.CantidadPersonas) &&
            e.ErrorMessage.Contains("La cantidad de personas debe ser mayor a 0") &&
            e.ErrorCode == "CANTIDAD_PERSONAS_INVALIDA");
    }

    [Fact]
    public async Task Validate_ConCantidadPersonasExcesiva_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.CantidadPersonas = 101; // Más de 100 personas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.CantidadPersonas) &&
            e.ErrorMessage.Contains("La cantidad de personas no puede exceder 100") &&
            e.ErrorCode == "CANTIDAD_PERSONAS_EXCESIVA");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(15)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Validate_ConCantidadPersonasValida_NoDeberiaRetornarErrorDeCantidad(int cantidadValida)
    {
        // Arrange
        var query = CrearQueryValida();
        query.CantidadPersonas = cantidadValida;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.CantidadPersonas));
    }

    #endregion

    #region Validación TipoReservacion

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConTipoReservacionVacio_NoDeberiaValidarTipo(string tipoVacio)
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoReservacion = tipoVacio;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.TipoReservacion));
    }

    [Fact]
    public async Task Validate_ConTipoReservacionMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoReservacion = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.TipoReservacion) &&
            e.ErrorMessage.Contains("El tipo de reservación no puede exceder 100 caracteres") &&
            e.ErrorCode == "TIPO_RESERVACION_LONGITUD");
    }

    [Theory]
    [InlineData("Estándar")]
    [InlineData("VIP")]
    [InlineData("Evento Especial")]
    [InlineData("Cumpleaños")]
    [InlineData("Aniversario")]
    [InlineData("Corporativo")]
    [InlineData("Familiar")]
    public async Task Validate_ConTipoReservacionValido_NoDeberiaRetornarErrorDeTipo(string tipoValido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoReservacion = tipoValido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.TipoReservacion));
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConQueryCompleta_DeberiaSerValida()
    {
        // Arrange
        var query = new ConsultarDisponibilidadQuery
        {
            FechaReservacion = DateTime.UtcNow.AddDays(7),
            HoraInicio = TimeSpan.FromHours(19), // 7:00 PM
            HoraFin = TimeSpan.FromHours(21), // 9:00 PM
            CantidadPersonas = 6,
            TipoReservacion = "Cena Especial",
            RequiereConfirmacion = true,
            IncluirMesasReservadas = false
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
        var query = new ConsultarDisponibilidadQuery
        {
            FechaReservacion = DateTime.UtcNow.AddHours(6),
            HoraInicio = TimeSpan.FromHours(12), // 12:00 PM
            HoraFin = TimeSpan.FromMinutes(12 * 60 + 30), // 12:30 PM
            CantidadPersonas = 1
            // Campos opcionales omitidos
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
        var query = new ConsultarDisponibilidadQuery
        {
            FechaReservacion = DateTime.UtcNow.AddDays(-1), // Error - pasado
            HoraInicio = TimeSpan.FromHours(2), // Error - muy temprano
            HoraFin = TimeSpan.FromHours(1), // Error - anterior a inicio
            CantidadPersonas = 0, // Error - cero
            TipoReservacion = new string('X', 101) // Error - muy largo
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
    [InlineData("Desayuno", 8, 0, 9, 30, 2)]
    [InlineData("Almuerzo", 12, 30, 14, 0, 4)]
    [InlineData("Cena", 19, 0, 21, 30, 6)]
    [InlineData("Cena Tardía", 21, 30, 23, 0, 2)]
    public async Task Validate_ConDiferentesHorarios_DeberiaSerValido(string tipo, int hIni, int mIni, int hFin, int mFin, int personas)
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoReservacion = tipo;
        query.HoraInicio = new TimeSpan(hIni, mIni, 0);
        query.HoraFin = new TimeSpan(hFin, mFin, 0);
        query.CantidadPersonas = personas;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConEventoEspecial_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReservacion = DateTime.UtcNow.AddDays(30);
        query.HoraInicio = TimeSpan.FromHours(18); // 6:00 PM
        query.HoraFin = TimeSpan.FromHours(23); // 11:00 PM - 5 horas
        query.CantidadPersonas = 25;
        query.TipoReservacion = "Evento Corporativo";
        query.RequiereConfirmacion = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConReservacionUrgente_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReservacion = DateTime.UtcNow.AddHours(2);
        query.HoraInicio = TimeSpan.FromHours(DateTime.UtcNow.Hour + 3);
        query.HoraFin = query.HoraInicio.Add(TimeSpan.FromMinutes(90));
        query.CantidadPersonas = 2;
        query.RequiereConfirmacion = false;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Fact]
    public async Task Validate_ConFechaReservacionHoy_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaReservacion = DateTime.UtcNow.Date;
        query.HoraInicio = TimeSpan.FromHours(DateTime.UtcNow.Hour + 3);
        query.HoraFin = query.HoraInicio.Add(TimeSpan.FromHours(1));

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConDuracionExactamente30Minutos_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.HoraInicio = TimeSpan.FromHours(12);
        query.HoraFin = TimeSpan.FromMinutes(12 * 60 + 30); // Exactamente 30 minutos

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConDuracionExactamente6Horas_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.HoraInicio = TimeSpan.FromHours(12);
        query.HoraFin = TimeSpan.FromHours(18); // Exactamente 6 horas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConTipoReservacionEnLimiteMaximo_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.TipoReservacion = new string('R', 100); // Exactamente 100 caracteres

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
                q.FechaReservacion = DateTime.UtcNow.AddDays(i);
                q.CantidadPersonas = i;
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
    public async Task Validate_ConRequiereConfirmacionTrue_NoDeberiaAfectarValidacion()
    {
        // Arrange
        var query = CrearQueryValida();
        query.RequiereConfirmacion = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ConIncluirMesasReservadasTrue_NoDeberiaAfectarValidacion()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirMesasReservadas = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
} 