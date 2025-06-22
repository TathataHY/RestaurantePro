using FluentValidation;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CerrarComanda;

public class CerrarComandaCommandValidator : AbstractValidator<CerrarComandaCommand>
{
    private static readonly string[] MetodosPagoValidos = { "efectivo", "tarjeta", "transferencia", "pago_movil" };

    public CerrarComandaCommandValidator(IComandaRepository comandaRepository)
    {
        RuleFor(x => x.ComandaId)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio")
            .MustAsync(async (comandaId, ct) =>
            {
                var comanda = await comandaRepository.ObtenerPorIdAsync(comandaId, true, ct);
                if (comanda == null) return false;
                
                // Validar que esté en estado válido para cerrar
                return comanda.Estado != EstadoComanda.Cancelada && 
                       comanda.Estado != EstadoComanda.Finalizada;
            }).WithMessage("La comanda no existe o no permite ser cerrada");

        RuleFor(x => x.MetodoPago)
            .NotEmpty().WithMessage("El método de pago es obligatorio")
            .Must(metodo => MetodosPagoValidos.Contains(metodo.ToLower()))
                .WithMessage($"El método de pago debe ser uno de: {string.Join(", ", MetodosPagoValidos)}");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres");

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var comanda = await comandaRepository.ObtenerPorIdAsync(command.ComandaId, true, ct);
                if (comanda == null) return false;

                // Validar que tenga productos
                return comanda.Items.Any();
            }).WithMessage("No se puede cerrar una comanda sin productos");
    }
} 