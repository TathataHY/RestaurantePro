using FluentValidation;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.AgregarPuntos;

/// <summary>
/// Validador para el comando de agregar puntos a una tarjeta de fidelización
/// </summary>
public class AgregarPuntosValidator : AbstractValidator<AgregarPuntosCommand>
{
    public AgregarPuntosValidator()
    {
        RuleFor(x => x.TarjetaId)
            .NotEmpty()
            .WithMessage("El ID de la tarjeta es requerido");

        RuleFor(x => x.Puntos)
            .GreaterThan(0)
            .WithMessage("Los puntos a agregar deben ser mayores a cero");

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .WithMessage("La descripción es requerida")
            .MaximumLength(200)
            .WithMessage("La descripción no puede exceder 200 caracteres");

        RuleFor(x => x.MontoTransaccion)
            .GreaterThan(0)
            .When(x => x.MontoTransaccion.HasValue)
            .WithMessage("El monto de la transacción debe ser mayor a cero cuando se especifica");

        RuleFor(x => x.Referencia)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Referencia))
            .WithMessage("La referencia no puede exceder 50 caracteres");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");
    }
} 