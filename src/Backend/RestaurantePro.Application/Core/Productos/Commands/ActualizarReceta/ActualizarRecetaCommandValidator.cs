using FluentValidation;

namespace RestaurantePro.Application.Core.Productos.Commands.ActualizarReceta;

/// <summary>
/// Validador para el comando ActualizarRecetaCommand
/// </summary>
public class ActualizarRecetaCommandValidator : AbstractValidator<ActualizarRecetaCommand>
{
    public ActualizarRecetaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID de la receta es obligatorio");

        RuleFor(x => x.Preparacion)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.Preparacion))
            .WithMessage("Las indicaciones de preparación no pueden estar vacías")
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Preparacion))
            .WithMessage("Las indicaciones de preparación no pueden exceder 2000 caracteres");

        RuleFor(x => x.TiempoPreparacionMinutos)
            .GreaterThan(0)
            .When(x => x.TiempoPreparacionMinutos > 0)
            .WithMessage("El tiempo de preparación debe ser mayor a 0 minutos")
            .LessThanOrEqualTo(480) // 8 horas máximo
            .When(x => x.TiempoPreparacionMinutos > 0)
            .WithMessage("El tiempo de preparación no puede exceder 8 horas");

        RuleFor(x => x.Ingredientes)
            .Must(ingredientes => ingredientes == null || !ingredientes.Any() || ingredientes.All(i => i != null))
            .When(x => x.Ingredientes != null)
            .WithMessage("Todos los ingredientes deben ser válidos");

        RuleForEach(x => x.Ingredientes)
            .SetValidator(new AgregarIngredienteDtoValidator())
            .When(x => x.Ingredientes != null && x.Ingredientes.Any());
    }
}

/// <summary>
/// Validador para AgregarIngredienteDto
/// </summary>
public class AgregarIngredienteDtoValidator : AbstractValidator<AgregarIngredienteDto>
{
    public AgregarIngredienteDtoValidator()
    {
        RuleFor(x => x.IngredienteId)
            .NotEmpty()
            .WithMessage("El ID del ingrediente es obligatorio");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .WithMessage("La cantidad debe ser mayor a 0");
    }
} 