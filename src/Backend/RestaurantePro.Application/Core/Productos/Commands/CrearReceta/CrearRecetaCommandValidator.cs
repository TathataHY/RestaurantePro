using FluentValidation;

namespace RestaurantePro.Application.Core.Productos.Commands.CrearReceta;

/// <summary>
/// Validador para el comando CrearRecetaCommand
/// </summary>
public class CrearRecetaCommandValidator : AbstractValidator<CrearRecetaCommand>
{
    public CrearRecetaCommandValidator()
    {
        RuleFor(x => x.ProductoId)
            .NotEmpty()
            .WithMessage("El ID del producto es obligatorio");

        RuleFor(x => x.Preparacion)
            .NotEmpty()
            .WithMessage("Las indicaciones de preparación son obligatorias")
            .MaximumLength(2000)
            .WithMessage("Las indicaciones de preparación no pueden exceder 2000 caracteres");

        RuleFor(x => x.TiempoPreparacionMinutos)
            .GreaterThan(0)
            .WithMessage("El tiempo de preparación debe ser mayor a 0 minutos")
            .LessThanOrEqualTo(480) // 8 horas máximo
            .WithMessage("El tiempo de preparación no puede exceder 8 horas");

        RuleFor(x => x.Ingredientes)
            .NotNull()
            .WithMessage("La lista de ingredientes es obligatoria")
            .Must(ingredientes => ingredientes != null && ingredientes.Any())
            .WithMessage("La receta debe tener al menos un ingrediente");

        RuleForEach(x => x.Ingredientes)
            .SetValidator(new AgregarIngredienteDtoValidator());
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