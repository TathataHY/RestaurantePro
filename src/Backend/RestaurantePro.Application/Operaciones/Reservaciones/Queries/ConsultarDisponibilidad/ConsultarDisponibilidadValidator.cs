namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ConsultarDisponibilidad;

public class ConsultarDisponibilidadValidator : AbstractValidator<ConsultarDisponibilidadQuery>
{
    private readonly string[] _zonasValidas = { "Centro", "Sur", "Norte", "Este", "Oeste", "Sur Este", "Sur Oeste", "Terraza", "Interior", "VIP", "Fumadores", "NoFumadores", "Familiar" };

    public ConsultarDisponibilidadValidator()
    {
        // ===== VALIDACIONES DE FECHA Y HORA =====
        RuleFor(v => v.FechaHora)
            .NotEmpty()
            .WithMessage("La fecha y hora son requeridas.")
            .WithErrorCode("FECHA_HORA_REQUERIDA");

        RuleFor(v => v.FechaHora)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("La fecha y hora no puede ser en el pasado.")
            .WithErrorCode("FECHA_HORA_PASADO");

        RuleFor(v => v.FechaHora)
            .LessThan(DateTime.UtcNow.Date.AddDays(181))
            .WithMessage("No se pueden hacer reservaciones con más de 180 días de anticipación.")
            .WithErrorCode("FECHA_HORA_MUY_FUTURA");

        // ===== VALIDACIONES DE NÚMERO DE PERSONAS =====
        RuleFor(v => v.NumeroPersonas)
            .GreaterThan(0)
            .WithMessage("El número de personas debe ser mayor a 0.")
            .WithErrorCode("NUMERO_PERSONAS_INVALIDO");

        RuleFor(v => v.NumeroPersonas)
            .LessThanOrEqualTo(100)
            .WithMessage("El número máximo de personas por reservación es 100.")
            .WithErrorCode("NUMERO_PERSONAS_EXCESIVO");

        // ===== VALIDACIONES DE DURACIÓN =====
        RuleFor(v => v.DuracionEstimadaMinutos)
            .GreaterThanOrEqualTo(30)
            .WithMessage("La duración mínima de una reservación es 30 minutos.")
            .WithErrorCode("DURACION_MUY_CORTA");

        RuleFor(v => v.DuracionEstimadaMinutos)
            .LessThanOrEqualTo(360)
            .WithMessage("La duración máxima de una reservación es 6 horas.")
            .WithErrorCode("DURACION_MUY_LARGA");

        // ===== VALIDACIONES DE ZONA PREFERIDA =====
        RuleFor(v => v.ZonaPreferida)
            .Must(zona => string.IsNullOrWhiteSpace(zona) || _zonasValidas.Contains(zona, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"La zona debe ser una de: {string.Join(", ", _zonasValidas)}.")
            .WithErrorCode("ZONA_PREFERIDA_INVALIDA")
            .When(v => !string.IsNullOrWhiteSpace(v.ZonaPreferida));

        RuleFor(v => v.ZonaPreferida)
            .MaximumLength(100)
            .WithMessage("La zona preferida no puede exceder 100 caracteres.")
            .WithErrorCode("ZONA_PREFERIDA_LONGITUD")
            .When(v => !string.IsNullOrWhiteSpace(v.ZonaPreferida));

        // ===== VALIDACIONES DE MESA PREFERIDA =====
        RuleFor(v => v.MesaPreferida)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de mesa no puede ser vacío.")
            .WithErrorCode("MESA_PREFERIDA_INVALIDA")
            .When(v => v.MesaPreferida.HasValue);

        // ===== VALIDACIONES DE RANGO ALTERNATIVAS =====
        RuleFor(v => v.RangoAlternativasMinutos)
            .GreaterThan(0)
            .WithMessage("El rango de alternativas debe ser mayor a 0.")
            .WithErrorCode("RANGO_ALTERNATIVAS_INVALIDO")
            .When(v => v.MostrarAlternativas);

        RuleFor(v => v.RangoAlternativasMinutos)
            .LessThanOrEqualTo(240)
            .WithMessage("El rango de alternativas no puede exceder 4 horas.")
            .WithErrorCode("RANGO_ALTERNATIVAS_MUY_LARGO")
            .When(v => v.MostrarAlternativas);

        // ===== VALIDACIONES DE MARGEN TOLERANCIA =====
        RuleFor(v => v.MargenToleranciaPersonas)
            .GreaterThan(0)
            .WithMessage("El margen de tolerancia debe ser mayor a 0.")
            .WithErrorCode("MARGEN_TOLERANCIA_INVALIDO");

        RuleFor(v => v.MargenToleranciaPersonas)
            .LessThanOrEqualTo(100)
            .WithMessage("El margen de tolerancia no puede exceder 100 personas.")
            .WithErrorCode("MARGEN_TOLERANCIA_EXCESIVO");

        // ===== VALIDACIONES PARA EVENTOS ESPECIALES =====
        RuleFor(v => v.DuracionEstimadaMinutos)
            .GreaterThanOrEqualTo(120)
            .WithMessage("Los eventos especiales deben tener una duración mínima de 2 horas.")
            .WithErrorCode("EVENTO_DURACION_INSUFICIENTE")
            .When(v => v.EsEventoEspecial);

        RuleFor(v => v.NumeroPersonas)
            .GreaterThanOrEqualTo(6)
            .WithMessage("Los eventos especiales deben ser para al menos 6 personas.")
            .WithErrorCode("EVENTO_PERSONAS_INSUFICIENTES")
            .When(v => v.EsEventoEspecial);

        RuleFor(v => v.FechaHora)
            .Must(BeValidEventTime)
            .WithMessage("Los eventos especiales deben programarse con al menos 48 horas de anticipación.")
            .WithErrorCode("EVENTO_ANTICIPACION_INSUFICIENTE")
            .When(v => v.EsEventoEspecial);

        // ===== VALIDACIONES DE LÓGICA DE NEGOCIO =====
        RuleFor(v => v)
            .Must(BeValidCapacityRange)
            .WithMessage("El rango de capacidad solicitado no es válido.")
            .WithErrorCode("CAPACIDAD_RANGO_INVALIDO")
            .WithName("CapacityRange");

        RuleFor(v => v)
            .Must(BeValidTimeRange)
            .WithMessage("El rango de tiempo solicitado no es válido.")
            .WithErrorCode("TIEMPO_RANGO_INVALIDO")
            .WithName("TimeRange")
            .When(v => v.MostrarAlternativas);
    }

    private static bool BeValidEventTime(DateTime fechaHora)
    {
        // Eventos especiales requieren al menos 48 horas de anticipación
        return fechaHora >= DateTime.UtcNow.AddHours(48);
    }

    private static bool BeValidCapacityRange(ConsultarDisponibilidadQuery query)
    {
        // Validar que el margen de tolerancia no haga que el rango sea demasiado amplio
        var capacidadMinima = query.NumeroPersonas - query.MargenToleranciaPersonas;
        var capacidadMaxima = query.PermitirCapacidadMayor ? 100 : query.NumeroPersonas + query.MargenToleranciaPersonas;

        return capacidadMinima > 0 && capacidadMaxima <= 100 && capacidadMinima <= capacidadMaxima;
    }

    private static bool BeValidTimeRange(ConsultarDisponibilidadQuery query)
    {
        // Solo validar si se muestran alternativas
        if (!query.MostrarAlternativas) return true;

        // Validar que el rango de tiempo no cause conflictos graves
        var horaInicio = query.FechaHora.AddMinutes(-query.RangoAlternativasMinutos);
        var horaFin = query.FechaHora.AddMinutes(query.RangoAlternativasMinutos);

        // Verificar que no sea un rango imposible (más de 24 horas)
        return (horaFin - horaInicio).TotalHours <= 24;
    }
} 