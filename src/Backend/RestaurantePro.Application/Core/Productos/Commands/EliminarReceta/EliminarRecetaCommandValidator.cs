using FluentValidation;

namespace RestaurantePro.Application.Core.Productos.Commands.EliminarReceta;

/// <summary>
/// Validador para el comando EliminarRecetaCommand
/// </summary>
public class EliminarRecetaCommandValidator : AbstractValidator<EliminarRecetaCommand>
{
    public EliminarRecetaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID de la receta es obligatorio");
    }
} 