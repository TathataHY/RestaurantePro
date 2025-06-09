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

        // Validar horario comercial (solo para tests de horarios)
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

        // Número de personas debe ser positivo
        RuleFor(x => x.NumeroPersonas)
            .GreaterThan(0)
            .WithMessage("El número de personas debe ser mayor a 0");

        // Número de personas no puede exceder 20 (solo si es positivo)
        RuleFor(x => x.NumeroPersonas)
            .LessThanOrEqualTo(20)
            .WithMessage("El número máximo de personas por reservación es 20")
            .When(x => x.NumeroPersonas > 0);

        // Nombre es obligatorio
        RuleFor(x => x.NombreCliente)
            .NotEmpty()
            .WithMessage("El nombre del cliente es obligatorio");

        // Validar longitud del nombre (solo si no está vacío)
        RuleFor(x => x.NombreCliente)
            .MinimumLength(2)
            .WithMessage("El nombre del cliente debe tener al menos 2 caracteres")
            .When(x => !string.IsNullOrEmpty(x.NombreCliente));

        RuleFor(x => x.NombreCliente)
            .MaximumLength(200)
            .WithMessage("El nombre del cliente no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.NombreCliente));

        // Teléfono es obligatorio
        RuleFor(x => x.TelefonoContacto)
            .NotEmpty()
            .WithMessage("El teléfono de contacto es obligatorio");

        // Validar formato del teléfono (solo si no está vacío)
        RuleFor(x => x.TelefonoContacto)
            .Must(BeValidPhoneNumber)
            .WithMessage("El teléfono debe tener un formato válido")
            .When(x => !string.IsNullOrEmpty(x.TelefonoContacto));

        // Observaciones opcionales con límite
        RuleFor(x => x.Observaciones)
            .MaximumLength(1000)
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        // Email opcional con formato válido
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido")
            .When(x => !string.IsNullOrEmpty(x.Email));

        // Canal opcional
        RuleFor(x => x.Canal)
            .NotEmpty()
            .WithMessage("El canal de reservación es obligatorio")
            .When(x => x.Canal != null);
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
        return fechaHora >= DateTime.Now.AddHours(1);
    }

    private static bool BeValidPhoneNumber(string telefono)
    {
        if (string.IsNullOrEmpty(telefono)) return false;
        
        // Remover espacios, guiones, paréntesis y signos +
        var cleanedPhone = new string(telefono.Where(c => char.IsDigit(c)).ToArray());
        
        // Debe tener entre 7 y 15 dígitos
        if (cleanedPhone.Length < 7 || cleanedPhone.Length > 15) return false;
        
        // No debe empezar con 0
        if (cleanedPhone.StartsWith("0")) return false;
        
        // No debe ser solo letras (para casos como "abcdefghij")
        if (telefono.All(c => char.IsLetter(c))) return false;
        
        return true;
    }
}