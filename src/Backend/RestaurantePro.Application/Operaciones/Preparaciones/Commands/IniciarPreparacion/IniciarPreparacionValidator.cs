namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.IniciarPreparacion;

/// <summary>
/// Validador para IniciarPreparacionCommand
/// </summary>
public class IniciarPreparacionValidator : AbstractValidator<IniciarPreparacionCommand>
{
    public IniciarPreparacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la preparación es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la preparación no puede ser un GUID vacío");

        RuleFor(x => x.ChefId)
            .NotEmpty().WithMessage("El ID del chef es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del chef no puede ser un GUID vacío");
    }
} 