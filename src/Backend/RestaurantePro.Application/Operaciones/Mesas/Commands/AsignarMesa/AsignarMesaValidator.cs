using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarMesa;

/// <summary>
/// Validator para AsignarMesaCommand
/// </summary>
public class AsignarMesaValidator : AbstractValidator<AsignarMesaCommand>
{
    public AsignarMesaValidator()
    {
        RuleFor(x => x.MesaId)
            .NotEmpty()
            .WithMessage("El ID de la mesa es obligatorio");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .WithMessage("Las observaciones no pueden exceder los 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Observaciones));

        RuleFor(x => x.MeseroId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del mesero no puede ser un GUID vacío")
            .When(x => x.MeseroId.HasValue);
    }
} 