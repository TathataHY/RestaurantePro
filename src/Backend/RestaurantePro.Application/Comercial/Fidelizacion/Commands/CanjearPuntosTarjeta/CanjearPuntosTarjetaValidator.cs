using FluentValidation;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntosTarjeta;

/// <summary>
/// Validador para el comando de canjear puntos de una tarjeta de fidelización
/// </summary>
public class CanjearPuntosTarjetaValidator : AbstractValidator<CanjearPuntosTarjetaCommand>
{
    public CanjearPuntosTarjetaValidator()
    {
        RuleFor(x => x.TarjetaId)
            .NotEmpty()
            .WithMessage("El ID de la tarjeta es requerido");

        RuleFor(x => x.PuntosACanjear)
            .GreaterThan(0)
            .WithMessage("Los puntos a canjear deben ser mayores a cero");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("La descripción es requerida")
            .MaximumLength(200)
            .WithMessage("La descripción no puede exceder 200 caracteres");

        RuleFor(x => x.Referencia)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Referencia))
            .WithMessage("La referencia no puede exceder 50 caracteres");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");
    }
} 