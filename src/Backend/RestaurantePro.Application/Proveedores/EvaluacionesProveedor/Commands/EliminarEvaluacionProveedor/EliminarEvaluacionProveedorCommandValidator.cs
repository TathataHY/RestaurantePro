using FluentValidation;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.EliminarEvaluacionProveedor;

public class EliminarEvaluacionProveedorCommandValidator : AbstractValidator<EliminarEvaluacionProveedorCommand>
{
    public EliminarEvaluacionProveedorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la evaluación es obligatorio");
    }
} 