using FluentValidation;
using RestaurantePro.Application.Common.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.ActualizarPreparacion;

/// <summary>
/// Validador para ActualizarPreparacionCommand
/// </summary>
public class ActualizarPreparacionValidator : AbstractValidator<ActualizarPreparacionCommand>
{
    public ActualizarPreparacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la preparación es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la preparación no puede ser un GUID vacío");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        RuleFor(x => x.Prioridad)
            .InclusiveBetween(1, 4).WithMessage("La prioridad debe ser un valor válido (1=Baja, 2=Media, 3=Alta, 4=Crítica)")
            .When(x => x.Prioridad.HasValue);

        RuleFor(x => x.TiempoEstimado)
            .GreaterThan(TimeSpan.Zero).WithMessage("El tiempo estimado debe ser mayor a 0")
            .LessThanOrEqualTo(TimeSpan.FromHours(2)).WithMessage("El tiempo estimado no puede exceder 2 horas")
            .When(x => x.TiempoEstimado.HasValue);
    }
} 