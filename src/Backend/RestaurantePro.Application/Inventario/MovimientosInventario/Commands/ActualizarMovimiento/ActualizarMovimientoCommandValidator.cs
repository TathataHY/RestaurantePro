using FluentValidation;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Commands.ActualizarMovimiento;

public class ActualizarMovimientoCommandValidator : AbstractValidator<ActualizarMovimientoCommand>
{
    public ActualizarMovimientoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del movimiento es requerido");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0)
            .When(x => x.Cantidad.HasValue)
            .WithMessage("La cantidad debe ser mayor a 0");

        RuleFor(x => x.CostoUnitario)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CostoUnitario.HasValue)
            .WithMessage("El costo unitario debe ser mayor o igual a 0");

        RuleFor(x => x.Motivo)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Motivo))
            .WithMessage("El motivo no puede exceder 500 caracteres");

        RuleFor(x => x.Observaciones)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones))
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres");
    }
} 