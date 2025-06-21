namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ReprogramarReservacion;

/// <summary>
/// Validador para ReprogramarReservacionCommand
/// </summary>
public class ReprogramarReservacionValidator : AbstractValidator<ReprogramarReservacionCommand>
{
    public ReprogramarReservacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la reservación es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la reservación no puede ser un GUID vacío");

        RuleFor(x => x.NuevaFechaReservacion)
            .NotEmpty().WithMessage("La nueva fecha de reservación es obligatoria")
            .GreaterThan(DateTime.Today).WithMessage("La nueva fecha de reservación debe ser posterior a hoy");

        RuleFor(x => x.NuevaHoraReservacion)
            .NotEmpty().WithMessage("La nueva hora de reservación es obligatoria")
            .Must(hora => hora >= TimeSpan.FromHours(6) && hora <= TimeSpan.FromHours(23))
            .WithMessage("La nueva hora de reservación debe estar entre 6:00 AM y 11:00 PM");

        RuleFor(x => x.MotivoReprogramacion)
            .MaximumLength(200).WithMessage("El motivo de reprogramación no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.MotivoReprogramacion));
    }
} 