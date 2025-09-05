using FluentValidation;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarCantidadItem;

public class ActualizarCantidadItemCommandValidator : AbstractValidator<ActualizarCantidadItemCommand>
{
    public ActualizarCantidadItemCommandValidator(IComandaRepository comandaRepository)
    {
        RuleFor(x => x.ComandaId)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("El ID del item es obligatorio");

        RuleFor(x => x.NuevaCantidad)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad debe ser 0 (para eliminar) o mayor");

        // Validación de existencia y estado editable podría hacerse en el handler para mensajes específicos
    }
}



