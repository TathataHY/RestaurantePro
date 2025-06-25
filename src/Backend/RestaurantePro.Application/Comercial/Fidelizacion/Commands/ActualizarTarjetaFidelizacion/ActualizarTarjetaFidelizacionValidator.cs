using FluentValidation;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActualizarTarjetaFidelizacion;

/// <summary>
/// Validador para el comando de actualizar tarjeta de fidelización
/// </summary>
public class ActualizarTarjetaFidelizacionValidator : AbstractValidator<ActualizarTarjetaFidelizacionCommand>
{
    public ActualizarTarjetaFidelizacionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID de la tarjeta es requerido");

        RuleFor(x => x.Nivel)
            .IsInEnum()
            .WithMessage("El nivel de fidelización debe ser un valor válido");

        RuleFor(x => x.MultiplicadorPuntos)
            .GreaterThan(0)
            .WithMessage("El multiplicador de puntos debe ser mayor a 0")
            .LessThanOrEqualTo(10)
            .WithMessage("El multiplicador de puntos no puede ser mayor a 10");

        RuleFor(x => x.LimiteMensual)
            .GreaterThan(0)
            .When(x => x.LimiteMensual.HasValue)
            .WithMessage("El límite mensual debe ser mayor a 0")
            .LessThanOrEqualTo(100000)
            .When(x => x.LimiteMensual.HasValue)
            .WithMessage("El límite mensual no puede ser mayor a 100,000");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones))
            .WithMessage("Las observaciones no pueden exceder 500 caracteres");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");
    }
} 