namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ActualizarReservacion;

/// <summary>
/// Validador para ActualizarReservacionCommand
/// </summary>
public class ActualizarReservacionValidator : AbstractValidator<ActualizarReservacionCommand>
{
    public ActualizarReservacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la reservación es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la reservación no puede ser un GUID vacío");

        RuleFor(x => x.FechaReservacion)
            .GreaterThan(DateTime.Today).WithMessage("La fecha de reservación debe ser posterior a hoy")
            .When(x => x.FechaReservacion != default(DateTime));

        RuleFor(x => x.HoraReservacion)
            .Must(hora => hora >= TimeSpan.FromHours(6) && hora <= TimeSpan.FromHours(23))
            .WithMessage("La hora de reservación debe estar entre 6:00 AM y 11:00 PM")
            .When(x => x.HoraReservacion != default(TimeSpan));

        RuleFor(x => x.NumeroPersonas)
            .GreaterThan(0).WithMessage("El número de personas debe ser mayor a 0")
            .LessThanOrEqualTo(20).WithMessage("El número de personas no puede exceder 20")
            .When(x => x.NumeroPersonas > 0);

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        RuleFor(x => x.MesaId)
            .NotEqual(Guid.Empty).WithMessage("El ID de la mesa no puede ser un GUID vacío")
            .When(x => x.MesaId.HasValue);
    }
} 