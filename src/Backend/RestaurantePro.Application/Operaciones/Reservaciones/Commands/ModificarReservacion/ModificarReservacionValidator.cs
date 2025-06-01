namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ModificarReservacion;

/// <summary>
/// Validator para ModificarReservacionCommand
/// </summary>
public class ModificarReservacionValidator : AbstractValidator<ModificarReservacionCommand>
{
    public ModificarReservacionValidator()
    {
        RuleFor(x => x.ReservacionId)
            .NotEmpty()
            .WithMessage("El ID de la reservación es requerido");

        RuleFor(x => x.NuevaFechaReservacion)
            .NotEmpty()
            .WithMessage("La nueva fecha de reservación es requerida")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("La nueva fecha no puede ser anterior a hoy")
            .LessThanOrEqualTo(DateTime.Today.AddDays(90))
            .WithMessage("No se pueden hacer reservaciones con más de 90 días de anticipación");

        RuleFor(x => x.NuevaHoraReservacion)
            .NotEmpty()
            .WithMessage("La nueva hora de reservación es requerida")
            .Must(BeValidBusinessHour)
            .WithMessage("La hora debe estar entre las 08:00 y las 23:00");

        RuleFor(x => x.NuevoNumeroPersonas)
            .GreaterThan(0)
            .WithMessage("El número de personas debe ser mayor a 0")
            .LessThanOrEqualTo(20)
            .WithMessage("No se pueden hacer reservaciones para más de 20 personas");

        RuleFor(x => x.MotivoModificacion)
            .NotEmpty()
            .WithMessage("El motivo de la modificación es requerido")
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder 500 caracteres");

        RuleFor(x => x.ObservacionesModificacion)
            .MaximumLength(1000)
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ObservacionesModificacion));

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        // Validación de fecha y hora combinadas
        RuleFor(x => x)
            .Must(x => CombineDateAndTime(x.NuevaFechaReservacion, x.NuevaHoraReservacion) > DateTime.Now.AddHours(2))
            .WithMessage("La nueva fecha y hora debe ser al menos 2 horas en el futuro")
            .When(x => x.NuevaFechaReservacion >= DateTime.Today);
    }

    private static bool BeValidBusinessHour(TimeSpan hora)
    {
        return hora >= TimeSpan.FromHours(8) && hora <= TimeSpan.FromHours(23);
    }

    private static DateTime CombineDateAndTime(DateTime fecha, TimeSpan hora)
    {
        return fecha.Date.Add(hora);
    }
} 