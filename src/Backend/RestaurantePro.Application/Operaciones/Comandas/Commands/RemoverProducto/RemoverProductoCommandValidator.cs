using FluentValidation;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.RemoverProducto;

public class RemoverProductoCommandValidator : AbstractValidator<RemoverProductoCommand>
{
    public RemoverProductoCommandValidator(IComandaRepository comandaRepository)
    {
        RuleFor(x => x.ComandaId)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio")
            .MustAsync(async (comandaId, ct) =>
            {
                var comanda = await comandaRepository.ObtenerPorIdAsync(comandaId, true, ct);
                if (comanda == null) return false;
                
                // Validar que esté en estado editable
                return comanda.Estado == EstadoComanda.Creada || 
                       comanda.Estado == EstadoComanda.EnProceso;
            }).WithMessage("La comanda no existe o no permite remover productos");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("El ID del item es obligatorio");

        RuleFor(x => x.Cantidad)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad a remover debe ser mayor o igual a cero");

        RuleFor(x => x.Observaciones)
            .MaximumLength(200).WithMessage("Las observaciones no pueden exceder los 200 caracteres");

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var comanda = await comandaRepository.ObtenerPorIdAsync(command.ComandaId, true, ct);
                if (comanda == null) return false;

                // Validar que el item existe en la comanda
                var item = comanda.Items.FirstOrDefault(i => i.Id == command.ItemId);
                if (item == null) return false;

                // Si cantidad es 0, se remueve todo (válido)
                if (command.Cantidad == 0) return true;

                // Validar que la cantidad a remover no exceda la disponible
                return command.Cantidad <= item.Cantidad;
            }).WithMessage("El item no existe en la comanda o la cantidad a remover excede la disponible");
    }
} 