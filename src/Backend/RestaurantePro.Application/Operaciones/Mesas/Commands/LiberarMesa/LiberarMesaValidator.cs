namespace RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;

/// <summary>
/// Validator para LiberarMesaCommand
/// </summary>
public class LiberarMesaValidator : AbstractValidator<LiberarMesaCommand>
{
    public LiberarMesaValidator()
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