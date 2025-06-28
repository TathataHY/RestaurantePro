namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;

/// <summary>
/// Validador para CrearReservacionCommand
/// Valida reglas de negocio para la creación de reservaciones en el restaurante
/// </summary>
public class CrearReservacionValidator : AbstractValidator<CrearReservacionCommand>
{
    public CrearReservacionValidator()
    {
        // Fecha debe ser futura o hoy
        RuleFor(x => x.FechaHoraReservacion)
            .Must(BeFutureDate)
            .WithMessage("La fecha de reservación debe ser futura");

        // Fecha no puede ser más de 90 días en el futuro (solo si es futura)
        RuleFor(x => x.FechaHoraReservacion)
            .Must(BeWithin90Days)
            .WithMessage("No se pueden hacer reservaciones con más de 90 días de anticipación")
            .When(x => BeFutureDate(x.FechaHoraReservacion));

        // Validar horario comercial para todas las fechas que no son hoy (incluyendo fechas pasadas)
        RuleFor(x => x.FechaHoraReservacion)
            .Must(BeWithinBusinessHours)
            .WithMessage("La hora de reservación debe estar entre las 12:00 PM y 10:00 PM")
            .When(x => !IsToday(x.FechaHoraReservacion));

        // Validar horario comercial para hoy
        RuleFor(x => x.FechaHoraReservacion)
            .Must(BeWithinBusinessHours)
            .WithMessage("La hora de reservación debe estar entre las 12:00 PM y 10:00 PM")
            .When(x => BeFutureDate(x.FechaHoraReservacion) && !IsDateWithoutTime(x.FechaHoraReservacion) && IsToday(x.FechaHoraReservacion));

        // Validar anticipación mínima (solo si es hoy)
        RuleFor(x => x.FechaHoraReservacion)
            .Must(BeAtLeastOneHourInAdvance)
            .WithMessage("Las reservaciones deben hacerse con al menos 1 hora de anticipación")
            .When(x => BeFutureDate(x.FechaHoraReservacion) && IsToday(x.FechaHoraReservacion));

        // Número de personas válido
        RuleFor(x => x.NumeroPersonas)
            .GreaterThan(0)
            .WithMessage("El número de personas debe ser mayor a 0");

        RuleFor(x => x.NumeroPersonas)
            .LessThanOrEqualTo(20)
            .WithMessage("El número máximo de personas por reservación es 20");

        // Nombre del cliente obligatorio
        RuleFor(x => x.NombreCliente)
            .NotEmpty()
            .WithMessage("El nombre del cliente es obligatorio")
            .MinimumLength(2)
            .WithMessage("El nombre del cliente debe tener al menos 2 caracteres")
            .MaximumLength(100)
            .WithMessage("El nombre del cliente no puede exceder 100 caracteres");

        // Teléfono obligatorio con formato válido
        RuleFor(x => x.TelefonoContacto)
            .NotEmpty()
            .WithMessage("El teléfono de contacto es obligatorio")
            .Must(BeValidPhoneNumber)
            .WithMessage("El teléfono debe tener un formato válido");

        // Observaciones opcionales con longitud máxima
        RuleFor(x => x.Observaciones)
            .MaximumLength(1000)
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        // Email opcional con formato válido
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido")
            .When(x => !string.IsNullOrEmpty(x.Email));

        // Canal es obligatorio
        RuleFor(x => x.Canal)
            .NotEmpty()
            .WithMessage("El canal de reservación es obligatorio");
    }

    private static bool BeFutureDate(DateTime fechaHora)
    {
        // Consideramos válida cualquier fecha en el futuro o de hoy
        return fechaHora.Date >= DateTime.Now.Date;
    }

    private static bool IsToday(DateTime fechaHora)
    {
        return fechaHora.Date == DateTime.Now.Date;
    }

    private static bool IsDateWithoutTime(DateTime fechaHora)
    {
        // Si la hora es exactamente medianoche, consideramos que es una fecha sin hora específica
        return fechaHora.Hour == 0 && fechaHora.Minute == 0 && fechaHora.Second == 0;
    }

    private static bool BeWithin90Days(DateTime fechaHora)
    {
        // La fecha no debe ser más de 90 días en el futuro
        return (fechaHora.Date - DateTime.Now.Date).TotalDays <= 90;
    }

    private static bool BeWithinBusinessHours(DateTime fechaHora)
    {
        // Horario comercial: entre 12:00 PM (hora 12) y 10:00 PM (hora 22)
        int hora = fechaHora.Hour;
        return hora >= 12 && hora <= 22;
    }

    private static bool BeAtLeastOneHourInAdvance(DateTime fechaHora)
    {
        // Para reservaciones en el mismo día, debe haber al menos 1 hora de anticipación
        return fechaHora >= DateTime.Now.AddMinutes(60);
    }

    private static bool BeValidPhoneNumber(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            return false;

        // Remover espacios, guiones y paréntesis para validación
        var numeroLimpio = telefono.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");

        // Debe tener entre 7 y 15 dígitos
        if (numeroLimpio.Length < 7 || numeroLimpio.Length > 15)
            return false;

        // Debe contener solo dígitos
        return numeroLimpio.All(char.IsDigit);
    }
}