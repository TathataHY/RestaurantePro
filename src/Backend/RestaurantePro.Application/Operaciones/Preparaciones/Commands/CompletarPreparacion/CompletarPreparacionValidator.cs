namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CompletarPreparacion;

/// <summary>
/// Validador para CompletarPreparacionCommand
/// </summary>
public class CompletarPreparacionValidator : AbstractValidator<CompletarPreparacionCommand>
{
    public CompletarPreparacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la preparación es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la preparación no puede ser un GUID vacío");

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("Las observaciones de completado no pueden exceder 300 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        RuleFor(x => x.TiempoReal)
            .GreaterThan(TimeSpan.Zero).WithMessage("El tiempo real debe ser mayor a 0")
            .LessThanOrEqualTo(TimeSpan.FromHours(4)).WithMessage("El tiempo real no puede exceder 4 horas")
            .When(x => x.TiempoReal.HasValue);
    }
} 