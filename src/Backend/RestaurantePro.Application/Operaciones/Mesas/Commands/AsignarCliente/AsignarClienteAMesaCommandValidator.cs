using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarCliente;

public class AsignarClienteAMesaCommandValidator : AbstractValidator<AsignarClienteAMesaCommand>
{
    public AsignarClienteAMesaCommandValidator()
    {
        RuleFor(x => x.MesaId)
            .NotEmpty().WithMessage("El ID de la mesa es obligatorio");
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("El ID del cliente es obligatorio");
        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));
    }
} 