using FluentValidation;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AplicarDescuento;

public class AplicarDescuentoCommandValidator : AbstractValidator<AplicarDescuentoCommand>
{
    public AplicarDescuentoCommandValidator(IComandaRepository comandaRepository)
    {
        RuleFor(x => x.ComandaId)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio")
            .MustAsync(async (comandaId, ct) =>
            {
                var comanda = await comandaRepository.ObtenerPorIdAsync(comandaId, false, ct);
                if (comanda == null) return false;
                
                // Validar que esté en estado válido para aplicar descuentos
                return comanda.Estado != EstadoComanda.Cancelada && 
                       comanda.Estado != EstadoComanda.Finalizada;
            }).WithMessage("La comanda no existe o no permite aplicar descuentos");

        RuleFor(x => x.PorcentajeDescuento)
            .GreaterThan(0).WithMessage("El porcentaje de descuento debe ser mayor a 0")
            .LessThanOrEqualTo(100).WithMessage("El porcentaje de descuento no puede exceder 100%");

        RuleFor(x => x.Motivo)
            .MaximumLength(200).WithMessage("El motivo del descuento no puede exceder los 200 caracteres");

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var comanda = await comandaRepository.ObtenerPorIdAsync(command.ComandaId, false, ct);
                if (comanda == null) return false;

                // Validar que no tenga descuento previo
                return !comanda.DescuentoFidelizacion.HasValue || 
                       comanda.DescuentoFidelizacion.Value == 0;
            }).WithMessage("La comanda ya tiene un descuento aplicado");
    }
} 