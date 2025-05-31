namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ConsultarDisponibilidad;

public class ConsultarDisponibilidadValidator : AbstractValidator<ConsultarDisponibilidadQuery>
{
    private readonly string[] _zonasValidas = { "Terraza", "Interior", "VIP", "Fumadores", "NoFumadores", "Familiar" };

    public ConsultarDisponibilidadValidator()
    {
        // Validaciones de fecha y hora
        RuleFor(v => v.FechaHora)
            .NotEmpty()
            .WithMessage("La fecha y hora son requeridas.")
            .GreaterThan(DateTime.Now.AddMinutes(15))
            .WithMessage("La reservación debe ser al menos 15 minutos en el futuro.")
            .LessThan(DateTime.Now.AddDays(90))
            .WithMessage("No se pueden consultar reservaciones con más de 90 días de anticipación.");

        // Validaciones de horario de atención
        RuleFor(v => v.FechaHora)
            .Must(BeWithinBusinessHours)
            .WithMessage("La consulta debe estar dentro del horario de atención (11:00 AM - 11:00 PM).")
            .Must(NotBeOnRestDay)
            .WithMessage("No se pueden hacer reservaciones en días de descanso.");

        // Validaciones de número de personas
        RuleFor(v => v.NumeroPersonas)
            .GreaterThan(0)
            .WithMessage("El número de personas debe ser mayor a 0.")
            .LessThanOrEqualTo(20)
            .WithMessage("No se pueden consultar reservaciones para más de 20 personas.")
            .Must(BeReasonableGroupSize)
            .WithMessage("Para grupos grandes (más de 12 personas), debe contactar directamente al restaurante.");

        // Validaciones de duración
        RuleFor(v => v.DuracionEstimadaMinutos)
            .GreaterThanOrEqualTo(30)
            .WithMessage("La duración mínima debe ser de 30 minutos.")
            .LessThanOrEqualTo(480)
            .WithMessage("La duración máxima no puede exceder 8 horas.")
            .Must(BeValidDuration)
            .WithMessage("La duración debe ser en incrementos de 15 minutos.");

        // Validaciones de zona
        RuleFor(v => v.ZonaPreferida)
            .Must(zona => string.IsNullOrEmpty(zona) || _zonasValidas.Contains(zona, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"La zona debe ser una de: {string.Join(", ", _zonasValidas)}.")
            .When(v => !string.IsNullOrEmpty(v.ZonaPreferida));

        // Validaciones de mesa específica
        RuleFor(v => v.MesaPreferida)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de mesa no puede ser vacío.")
            .When(v => v.MesaPreferida.HasValue);

        // Validaciones de alternativas
        RuleFor(v => v.RangoAlternativasMinutos)
            .GreaterThanOrEqualTo(15)
            .WithMessage("El rango de alternativas debe ser al menos 15 minutos.")
            .LessThanOrEqualTo(240)
            .WithMessage("El rango de alternativas no puede exceder 4 horas.")
            .When(v => v.MostrarAlternativas);

        // Validaciones de tolerancia
        RuleFor(v => v.MargenToleranciaPersonas)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El margen de tolerancia no puede ser negativo.")
            .LessThanOrEqualTo(5)
            .WithMessage("El margen de tolerancia no puede exceder 5 personas.");

        // Validaciones para eventos especiales
        RuleFor(v => v.DuracionEstimadaMinutos)
            .GreaterThanOrEqualTo(120)
            .WithMessage("Los eventos especiales deben tener una duración mínima de 2 horas.")
            .When(v => v.EsEventoEspecial);

        RuleFor(v => v.NumeroPersonas)
            .GreaterThanOrEqualTo(6)
            .WithMessage("Los eventos especiales deben ser para al menos 6 personas.")
            .When(v => v.EsEventoEspecial);

        RuleFor(v => v.FechaHora)
            .Must(BeValidEventTime)
            .WithMessage("Los eventos especiales deben programarse con al menos 48 horas de anticipación.")
            .When(v => v.EsEventoEspecial);

        // Validaciones de lógica de negocio
        RuleFor(v => v)
            .Must(BeValidCapacityRange)
            .WithMessage("El rango de capacidad solicitado no es válido.")
            .WithName("CapacityRange");

        RuleFor(v => v)
            .Must(BeValidTimeRange)
            .WithMessage("El rango de tiempo solicitado no es válido.")
            .WithName("TimeRange");
    }

    private static bool BeWithinBusinessHours(DateTime fechaHora)
    {
        var hora = fechaHora.TimeOfDay;
        return hora >= TimeSpan.FromHours(11) && hora <= TimeSpan.FromHours(23);
    }

    private static bool NotBeOnRestDay(DateTime fechaHora)
    {
        // En este ejemplo, el restaurante cierra los lunes
        return fechaHora.DayOfWeek != DayOfWeek.Monday;
    }

    private static bool BeReasonableGroupSize(int numeroPersonas)
    {
        // Para grupos muy grandes, se requiere contacto directo
        return numeroPersonas <= 12;
    }

    private static bool BeValidDuration(int duracionMinutos)
    {
        // La duración debe ser en incrementos de 15 minutos
        return duracionMinutos % 15 == 0;
    }

    private static bool BeValidEventTime(DateTime fechaHora)
    {
        // Eventos especiales requieren al menos 48 horas de anticipación
        return fechaHora >= DateTime.Now.AddHours(48);
    }

    private static bool BeValidCapacityRange(ConsultarDisponibilidadQuery query)
    {
        // Validar que el margen de tolerancia no haga que el rango sea demasiado amplio
        var capacidadMinima = query.NumeroPersonas - query.MargenToleranciaPersonas;
        var capacidadMaxima = query.PermitirCapacidadMayor ? 20 : query.NumeroPersonas + query.MargenToleranciaPersonas;

        return capacidadMinima > 0 && capacidadMaxima <= 20 && capacidadMinima <= capacidadMaxima;
    }

    private static bool BeValidTimeRange(ConsultarDisponibilidadQuery query)
    {
        if (!query.MostrarAlternativas) return true;

        // Validar que el rango de tiempo no cause conflictos
        var horaInicio = query.FechaHora.AddMinutes(-query.RangoAlternativasMinutos);
        var horaFin = query.FechaHora.AddMinutes(query.RangoAlternativasMinutos);

        // Verificar que el rango esté dentro del horario de atención
        var horaInicioValida = horaInicio.TimeOfDay >= TimeSpan.FromHours(11);
        var horaFinValida = horaFin.TimeOfDay <= TimeSpan.FromHours(23);

        return horaInicioValida && horaFinValida;
    }
} 