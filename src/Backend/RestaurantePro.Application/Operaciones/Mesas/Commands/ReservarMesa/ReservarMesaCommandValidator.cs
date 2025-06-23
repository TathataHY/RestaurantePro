using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.ReservarMesa;

public class ReservarMesaCommandValidator : AbstractValidator<ReservarMesaCommand>
{
    public ReservarMesaCommandValidator()
    {
        RuleFor(x => x.MesaId)
            .NotEmpty().WithMessage("El ID de la mesa es obligatorio");
            
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("El ID del cliente es obligatorio");
            
        RuleFor(x => x.FechaReserva)
            .NotEmpty().WithMessage("La fecha de reserva es obligatoria")
            .GreaterThan(DateTime.Today).WithMessage("La fecha de reserva debe ser futura");
            
        RuleFor(x => x.HoraReserva)
            .NotEmpty().WithMessage("La hora de reserva es obligatoria")
            .Must(hora => hora >= TimeSpan.FromHours(6) && hora <= TimeSpan.FromHours(23))
            .WithMessage("La hora de reserva debe estar entre 6:00 AM y 11:00 PM");
            
        RuleFor(x => x.NumeroPersonas)
            .GreaterThan(0).WithMessage("El número de personas debe ser mayor a 0")
            .LessThanOrEqualTo(20).WithMessage("El número de personas no puede exceder 20");
            
        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));
    }
} 