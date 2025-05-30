namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;

/// <summary>
/// Validador para CrearReservacionCommand
/// Valida reglas de negocio para la creación de reservaciones en el restaurante
/// </summary>
public class CrearReservacionValidator : AbstractValidator<CrearReservacionCommand>
{
    public CrearReservacionValidator()
    {
        // Validaciones básicas requeridas
        RuleFor(x => x.NombreCliente)
            .NotEmpty()
            .WithMessage("El nombre del cliente es obligatorio")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres")
            .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-\.]+$")
            .WithMessage("El nombre solo puede contener letras, espacios, guiones y puntos");

        RuleFor(x => x.Telefono)
            .NotEmpty()
            .WithMessage("El teléfono es obligatorio")
            .Matches(@"^[\d\-\+\(\)\s]{7,20}$")
            .WithMessage("El teléfono debe tener un formato válido (7-20 dígitos)");

        RuleFor(x => x.FechaHoraReservacion)
            .NotEmpty()
            .WithMessage("La fecha y hora de reservación es obligatoria")
            .GreaterThan(DateTime.Now.AddMinutes(30))
            .WithMessage("La reservación debe ser al menos 30 minutos en el futuro")
            .LessThan(DateTime.Now.AddDays(90))
            .WithMessage("No se pueden hacer reservaciones con más de 90 días de anticipación");

        RuleFor(x => x.NumeroPersonas)
            .GreaterThan(0)
            .WithMessage("El número de personas debe ser mayor a 0")
            .LessThanOrEqualTo(20)
            .WithMessage("No se pueden hacer reservaciones para más de 20 personas por mesa");
            // .When(x => !x.EsRecurrente); // TODO: Implementar cuando esté la propiedad EsRecurrente

        RuleFor(x => x.Canal)
            .NotEmpty()
            .WithMessage("El canal de reservación es obligatorio")
            .MaximumLength(30)
            .WithMessage("El canal no puede exceder 30 caracteres")
            .Must(BeValidCanal)
            .WithMessage("Canal no válido. Valores permitidos: Web, Telefono, App, Presencial, WhatsApp");

        // Validaciones opcionales pero con reglas específicas
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("El email debe tener un formato válido")
            .MaximumLength(150)
            .WithMessage("El email no puede exceder 150 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Email));

        // TODO: Implementar cuando estén las propiedades en CrearReservacionCommand
        /*
        RuleFor(x => x.Comentarios)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Comentarios));
        */

        RuleFor(x => x.NotasInternas)
            .MaximumLength(500)
            .WithMessage("Las notas internas no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.NotasInternas));

        // Validaciones de horario de negocio
        RuleFor(x => x.FechaHoraReservacion)
            .Must(BeWithinBusinessHours)
            .WithMessage("La reservación debe estar dentro del horario de atención (11:00 AM - 11:00 PM)")
            .Must(NotBeOnRestDay)
            .WithMessage("No se pueden hacer reservaciones en días de descanso (verificar calendario)");

        // Validación para grupos grandes
        RuleFor(x => x.NumeroPersonas)
            .LessThanOrEqualTo(8)
            .WithMessage("Para grupos de más de 8 personas, debe contactar directamente al restaurante")
            .When(x => x.Canal.Equals("Web", StringComparison.OrdinalIgnoreCase) || 
                      x.Canal.Equals("App", StringComparison.OrdinalIgnoreCase));

        // TODO: Implementar cuando estén las propiedades en CrearReservacionCommand
        /*
        RuleFor(x => x.DuracionEstimadaMinutos)
            .GreaterThan(30)
            .WithMessage("La duración debe ser de al menos 30 minutos")
            .LessThanOrEqualTo(480)
            .WithMessage("La duración no puede exceder 8 horas");
        */
    }

    private static bool BeValidCanal(string canal)
    {
        var canalesValidos = new[] { "Web", "Telefono", "App", "Presencial", "WhatsApp", "Delivery" };
        return canalesValidos.Contains(canal, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeWithinBusinessHours(DateTime fechaHora)
    {
        // Validar horario de atención (11:00 AM - 11:00 PM)
        var hora = fechaHora.TimeOfDay;
        return hora >= TimeSpan.FromHours(11) && hora <= TimeSpan.FromHours(23);
    }

    private static bool NotBeOnRestDay(DateTime fechaHora)
    {
        // Por ahora solo validamos que no sea muy pasado, 
        // en producción aquí se consultaría el calendario de días de descanso
        return fechaHora.DayOfWeek != DayOfWeek.Monday; // Ejemplo: los lunes cerrado
    }
} 