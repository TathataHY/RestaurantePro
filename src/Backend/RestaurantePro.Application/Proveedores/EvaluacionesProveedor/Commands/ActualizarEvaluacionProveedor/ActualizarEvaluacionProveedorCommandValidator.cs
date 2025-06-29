using FluentValidation;

namespace RestaurantePro.Application.Proveedores.EvaluacionesProveedor.Commands.ActualizarEvaluacionProveedor;

public class ActualizarEvaluacionProveedorCommandValidator : AbstractValidator<ActualizarEvaluacionProveedorCommand>
{
    public ActualizarEvaluacionProveedorCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la evaluación es obligatorio");
        RuleFor(x => x.CalificacionGeneral)
            .InclusiveBetween(1, 5).WithMessage("La calificación general debe estar entre 1 y 5");
        RuleFor(x => x.CalificacionCalidad)
            .InclusiveBetween(1, 5).WithMessage("La calificación de calidad debe estar entre 1 y 5");
        RuleFor(x => x.CalificacionPuntualidad)
            .InclusiveBetween(1, 5).WithMessage("La calificación de puntualidad debe estar entre 1 y 5");
        RuleFor(x => x.CalificacionComunicacion)
            .InclusiveBetween(1, 5).WithMessage("La calificación de comunicación debe estar entre 1 y 5");
        RuleFor(x => x.CalificacionPrecios)
            .InclusiveBetween(1, 5).WithMessage("La calificación de precios debe estar entre 1 y 5");
        RuleFor(x => x.Comentarios)
            .MaximumLength(1000).WithMessage("Los comentarios no pueden exceder los 1000 caracteres");
    }
} 