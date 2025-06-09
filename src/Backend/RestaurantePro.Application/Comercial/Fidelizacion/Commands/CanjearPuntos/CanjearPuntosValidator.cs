namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntos;

/// <summary>
/// 🔍 Validator para CanjearPuntosCommand con validaciones de negocio
/// </summary>
public class CanjearPuntosValidator : AbstractValidator<CanjearPuntosCommand>
{
    public CanjearPuntosValidator()
    {
        // 🎯 Validar ClienteId
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("🚫 El ID del cliente es obligatorio");

        // 🎯 Validar puntos a utilizar
        RuleFor(x => x.PuntosAUtilizar)
            .GreaterThan(0)
            .WithMessage("🚫 La cantidad de puntos debe ser mayor a 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("🚫 No se pueden canjear más de 10,000 puntos en una sola operación");

        // 🎯 Validar ComandaId si se proporciona
        RuleFor(command => command)
            .Must(x => !x.ComandaId.HasValue || x.ComandaId.Value != Guid.Empty)
            .When(x => x.ComandaId.HasValue)
            .WithName("ComandaId")
            .WithMessage("🚫 El ID de la comanda es obligatorio cuando se especifica");

        // 🎯 Validar motivo si se proporciona
        RuleFor(x => x.Motivo)
            .MaximumLength(500)
            .WithMessage("🚫 El motivo no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Motivo));

        // 🎯 Validar múltiplos de puntos (generalmente se canjean en múltiplos de 10 o 100)
        RuleFor(x => x.PuntosAUtilizar)
            .Must(puntos => puntos % 10 == 0)
            .WithMessage("🚫 Los puntos deben canjearse en múltiplos de 10")
            .When(x => x.PuntosAUtilizar > 0);
    }
} 