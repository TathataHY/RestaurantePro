namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CancelarPreparacion;

/// <summary>
/// Validador para CancelarPreparacionCommand
/// </summary>
public class CancelarPreparacionValidator : AbstractValidator<CancelarPreparacionCommand>
{
    public CancelarPreparacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la preparación es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la preparación no puede ser un GUID vacío");

        RuleFor(x => x.MotivoCancelacion)
            .NotEmpty().WithMessage("El motivo de cancelación es obligatorio")
            .MaximumLength(200).WithMessage("El motivo de cancelación no puede exceder 200 caracteres");
    }
} 