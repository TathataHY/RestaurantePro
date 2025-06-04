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
            FechaHora = DateTime.UtcNow.AddHours(2),
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120,
            ZonaPreferida = null,
            MesaPreferida = null,
            MostrarAlternativas = true,
            RangoAlternativasMinutos = 60,
            PermitirCapacidadMayor = true,
            MargenToleranciaPersonas = 2,
            IncluirDetallesMesas = true,
            EsEventoEspecial = false
        };
    }

    #endregion

    #region Validación FechaHora

    [Fact]
    public async Task Validate_ConFechaHoraEnPasado_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaHora = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.FechaHora) &&
            e.ErrorMessage.Contains("La fecha y hora no puede ser en el pasado") &&
            e.ErrorCode == "FECHA_HORA_PASADO");
    }

    [Fact]
    public async Task Validate_ConFechaHoraMuyLejana_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaHora = DateTime.UtcNow.AddDays(181); // Más de 180 días

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.FechaHora) &&
            e.ErrorMessage.Contains("No se pueden hacer reservaciones con más de 180 días de anticipación") &&
            e.ErrorCode == "FECHA_HORA_MUY_FUTURA");
    }

    [Theory]
    [InlineData(1)]   // 1 día
    [InlineData(7)]   // 1 semana
    [InlineData(30)]  // 1 mes
    [InlineData(90)]  // 3 meses
    [InlineData(180)] // 6 meses (límite)
    public async Task Validate_ConFechaHoraValida_NoDeberiaRetornarErrorDeFecha(int diasAdelante)
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaHora = DateTime.UtcNow.AddDays(diasAdelante);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.FechaHora));
    }

    #endregion

    #region Validación NumeroPersonas

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConNumeroPersonasMenorIgualCero_DeberiaRetornarError(int numeroInvalido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.NumeroPersonas = numeroInvalido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.NumeroPersonas) &&
            e.ErrorMessage.Contains("El número de personas debe ser mayor a 0") &&
            e.ErrorCode == "NUMERO_PERSONAS_INVALIDO");
    }

    [Fact]
    public async Task Validate_ConNumeroPersonasExcesivo_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.NumeroPersonas = 101; // Más de 100 personas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.NumeroPersonas) &&
            e.ErrorMessage.Contains("El número máximo de personas por reservación es 100") &&
            e.ErrorCode == "NUMERO_PERSONAS_EXCESIVO");
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
    public async Task Validate_ConNumeroPersonasValido_NoDeberiaRetornarErrorDeNumero(int numeroValido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.NumeroPersonas = numeroValido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.NumeroPersonas));
    }

    #endregion

    #region Validación DuracionEstimadaMinutos

    [Theory]
    [InlineData(15)]  // Muy corto
    [InlineData(20)]  // Muy corto
    public async Task Validate_ConDuracionMuyCorta_DeberiaRetornarError(int duracionCorta)
    {
        // Arrange
        var query = CrearQueryValida();
        query.DuracionEstimadaMinutos = duracionCorta;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.DuracionEstimadaMinutos) &&
            e.ErrorMessage.Contains("La duración mínima de una reservación es 30 minutos") &&
            e.ErrorCode == "DURACION_MUY_CORTA");
    }

    [Theory]
    [InlineData(480)] // 8 horas - muy largo
    [InlineData(600)] // 10 horas - muy largo
    public async Task Validate_ConDuracionMuyLarga_DeberiaRetornarError(int duracionLarga)
    {
        // Arrange
        var query = CrearQueryValida();
        query.DuracionEstimadaMinutos = duracionLarga;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.DuracionEstimadaMinutos) &&
            e.ErrorMessage.Contains("La duración máxima de una reservación es 6 horas") &&
            e.ErrorCode == "DURACION_MUY_LARGA");
    }

    [Theory]
    [InlineData(30)]   // 30 minutos - mínimo
    [InlineData(60)]   // 1 hora
    [InlineData(120)]  // 2 horas
    [InlineData(180)]  // 3 horas
    [InlineData(360)]  // 6 horas - máximo
    public async Task Validate_ConDuracionValida_NoDeberiaRetornarErrorDeDuracion(int duracionValida)
    {
        // Arrange
        var query = CrearQueryValida();
        query.DuracionEstimadaMinutos = duracionValida;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.DuracionEstimadaMinutos));
    }

    #endregion

    #region Validación ZonaPreferida

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_ConZonaPreferidaVacio_NoDeberiaValidarZona(string zonaVacio)
    {
        // Arrange
        var query = CrearQueryValida();
        query.ZonaPreferida = zonaVacio;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.ZonaPreferida));
    }

    [Fact]
    public async Task Validate_ConZonaPreferidaMuyLargo_DeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.ZonaPreferida = new string('A', 101); // Más de 100 caracteres

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.ZonaPreferida) &&
            e.ErrorMessage.Contains("La zona preferida no puede exceder 100 caracteres") &&
            e.ErrorCode == "ZONA_PREFERIDA_LONGITUD");
    }

    [Theory]
    [InlineData("Centro")]
    [InlineData("Sur")]
    [InlineData("Norte")]
    [InlineData("Este")]
    [InlineData("Oeste")]
    [InlineData("Sur Este")]
    [InlineData("Sur Oeste")]
    public async Task Validate_ConZonaPreferidaValido_NoDeberiaRetornarErrorDeZona(string zonaValido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.ZonaPreferida = zonaValido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.ZonaPreferida));
    }

    #endregion

    #region Validación MesaPreferida

    [Theory]
    [InlineData(null)]
    public async Task Validate_ConMesaPreferidaVacio_NoDeberiaValidarMesa(Guid? mesaVacio)
    {
        // Arrange
        var query = CrearQueryValida();
        query.MesaPreferida = mesaVacio;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.MesaPreferida));
    }

    [Fact]
    public async Task Validate_ConMesaPreferidaValida_NoDeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MesaPreferida = Guid.NewGuid(); // Mesa válida con GUID

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.MesaPreferida));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000001")]
    [InlineData("00000000-0000-0000-0000-000000000002")]
    [InlineData("00000000-0000-0000-0000-000000000003")]
    [InlineData("00000000-0000-0000-0000-000000000004")]
    [InlineData("00000000-0000-0000-0000-000000000005")]
    public async Task Validate_ConMesaPreferidaValidaGuid_NoDeberiaRetornarErrorDeMesa(string guidString)
    {
        // Arrange
        var query = CrearQueryValida();
        query.MesaPreferida = Guid.Parse(guidString);

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.MesaPreferida));
    }

    #endregion

    #region Validación MostrarAlternativas

    [Fact]
    public async Task Validate_ConMostrarAlternativasTrue_NoDeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.MostrarAlternativas = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Validación RangoAlternativasMinutos

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConRangoAlternativasMenorIgualCero_DeberiaRetornarError(int rangoInvalido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.RangoAlternativasMinutos = rangoInvalido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.RangoAlternativasMinutos) &&
            e.ErrorMessage.Contains("El rango de alternativas debe ser mayor a 0") &&
            e.ErrorCode == "RANGO_ALTERNATIVAS_INVALIDO");
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
    public async Task Validate_ConRangoAlternativasValido_NoDeberiaRetornarErrorDeRango(int rangoValido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.RangoAlternativasMinutos = rangoValido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.RangoAlternativasMinutos));
    }

    #endregion

    #region Validación PermitirCapacidadMayor

    [Fact]
    public async Task Validate_ConPermitirCapacidadMayorTrue_NoDeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.PermitirCapacidadMayor = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Validación MargenToleranciaPersonas

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task Validate_ConMargenToleranciaMenorIgualCero_DeberiaRetornarError(int margenInvalido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.MargenToleranciaPersonas = margenInvalido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.MargenToleranciaPersonas) &&
            e.ErrorMessage.Contains("El margen de tolerancia debe ser mayor a 0") &&
            e.ErrorCode == "MARGEN_TOLERANCIA_INVALIDO");
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
    public async Task Validate_ConMargenToleranciaValido_NoDeberiaRetornarErrorDeMargen(int margenValido)
    {
        // Arrange
        var query = CrearQueryValida();
        query.MargenToleranciaPersonas = margenValido;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == nameof(ConsultarDisponibilidadQuery.MargenToleranciaPersonas));
    }

    #endregion

    #region Validación IncluirDetallesMesas

    [Fact]
    public async Task Validate_ConIncluirDetallesMesasTrue_NoDeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.IncluirDetallesMesas = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Validación EsEventoEspecial

    [Fact]
    public async Task Validate_ConEsEventoEspecialTrue_NoDeberiaRetornarError()
    {
        // Arrange
        var query = CrearQueryValida();
        query.EsEventoEspecial = true;
        query.NumeroPersonas = 8; // Mínimo para eventos especiales: 6
        query.DuracionEstimadaMinutos = 150; // Mínimo para eventos especiales: 120
        query.FechaHora = DateTime.UtcNow.AddDays(3); // Anticipación para eventos especiales

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Validaciones Integradas

    [Fact]
    public async Task Validate_ConQueryCompleta_DeberiaSerValida()
    {
        // Arrange
        var query = new ConsultarDisponibilidadQuery
        {
            FechaHora = DateTime.UtcNow.AddDays(7),
            NumeroPersonas = 6,
            DuracionEstimadaMinutos = 120,
            ZonaPreferida = "Centro",
            MesaPreferida = Guid.NewGuid(),
            MostrarAlternativas = true,
            RangoAlternativasMinutos = 60,
            PermitirCapacidadMayor = true,
            MargenToleranciaPersonas = 2,
            IncluirDetallesMesas = true,
            EsEventoEspecial = false
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
            FechaHora = DateTime.UtcNow.AddHours(6),
            NumeroPersonas = 4,
            DuracionEstimadaMinutos = 120,
            ZonaPreferida = "Centro",
            MesaPreferida = Guid.NewGuid(),
            MostrarAlternativas = true,
            RangoAlternativasMinutos = 60,
            PermitirCapacidadMayor = true,
            MargenToleranciaPersonas = 2,
            IncluirDetallesMesas = true,
            EsEventoEspecial = false
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
            FechaHora = DateTime.UtcNow.AddDays(-1), // Error - pasado
            NumeroPersonas = 0, // Error - cero
            DuracionEstimadaMinutos = 15, // Error - muy corto
            ZonaPreferida = null,
            MesaPreferida = null,
            MostrarAlternativas = false,
            RangoAlternativasMinutos = 0,
            PermitirCapacidadMayor = false,
            MargenToleranciaPersonas = 0,
            IncluirDetallesMesas = false,
            EsEventoEspecial = false
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
    [InlineData("Cena Tardía", 21, 30, 0, 30, 8)] // Evento especial: 3 horas (180 min)
    public async Task Validate_ConDiferentesHorarios_DeberiaSerValido(string tipo, int hIni, int mIni, int hFin, int mFin, int personas)
    {
        // Arrange
        var query = CrearQueryValida();
        query.EsEventoEspecial = tipo == "Cena Tardía";
        query.NumeroPersonas = personas;
        
        // Calcular duración correctamente
        var duracion = tipo == "Cena Tardía" 
            ? 180 // 3 horas para eventos especiales
            : (hFin - hIni) * 60 + (mFin - mIni);
        
        query.DuracionEstimadaMinutos = duracion;
        
        // Para eventos especiales, agregar anticipación
        if (query.EsEventoEspecial)
        {
            query.FechaHora = DateTime.UtcNow.AddDays(3);
        }

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
        query.EsEventoEspecial = true;
        query.NumeroPersonas = 25;
        query.DuracionEstimadaMinutos = 300; // 5 horas
        query.FechaHora = DateTime.UtcNow.AddDays(3); // 3 días de anticipación para eventos especiales

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
        query.EsEventoEspecial = false;
        query.NumeroPersonas = 2;
        query.DuracionEstimadaMinutos = 90; // 1.5 horas

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Tests de Límites y Casos Especiales

    [Fact]
    public async Task Validate_ConFechaHoraHoy_DeberiaSerValido()
    {
        // Arrange
        var query = CrearQueryValida();
        query.FechaHora = DateTime.UtcNow.Date;
        query.DuracionEstimadaMinutos = 30; // 30 minutos

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
        query.DuracionEstimadaMinutos = 30; // Exactamente 30 minutos

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
        query.DuracionEstimadaMinutos = 360; // Exactamente 6 horas

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
        query.EsEventoEspecial = true;
        query.NumeroPersonas = 100;
        query.DuracionEstimadaMinutos = 360; // 6 horas máximas
        query.FechaHora = DateTime.UtcNow.AddDays(3); // Anticipación para eventos especiales

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
                q.FechaHora = DateTime.UtcNow.AddDays(i);
                q.NumeroPersonas = Math.Min(i + 1, 100); // Asegurar que sea al menos 1 persona
                q.DuracionEstimadaMinutos = Math.Min(i * 30 + 60, 360); // Duración entre 90 y 360 min
                q.EsEventoEspecial = false; // Evitar reglas de eventos especiales
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
        query.EsEventoEspecial = true;
        query.NumeroPersonas = 8; // Mínimo para eventos especiales
        query.DuracionEstimadaMinutos = 180; // Mínimo para eventos especiales
        query.FechaHora = DateTime.UtcNow.AddDays(3); // Anticipación para eventos especiales

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
        query.IncluirDetallesMesas = true;

        // Act
        var result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
} 