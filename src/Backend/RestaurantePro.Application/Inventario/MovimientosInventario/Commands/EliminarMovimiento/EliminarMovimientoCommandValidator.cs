using FluentValidation;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Commands.EliminarMovimiento;

public class EliminarMovimientoCommandValidator : AbstractValidator<EliminarMovimientoCommand>
{
    public EliminarMovimientoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del movimiento es requerido");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        RuleFor(x => x.MotivoEliminacion)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.MotivoEliminacion))
            .WithMessage("El motivo de eliminación no puede exceder 500 caracteres");
    }
} 