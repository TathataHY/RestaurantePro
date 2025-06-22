using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarComanda;

public class ActualizarComandaCommandValidator : AbstractValidator<ActualizarComandaCommand>
{
    public ActualizarComandaCommandValidator(
        IComandaRepository comandaRepository,
        IApplicationDbContext dbContext)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio")
            .MustAsync(async (id, ct) =>
            {
                return await comandaRepository.ObtenerPorIdAsync(id, false, ct) != null;
            }).WithMessage("La comanda especificada no existe");

        RuleFor(x => x.MesaId)
            .MustAsync(async (mesaId, ct) =>
            {
                if (!mesaId.HasValue || mesaId.Value == Guid.Empty) return true;
                return await dbContext.Mesas.FindAsync(new object[] { mesaId.Value }, ct) != null;
            }).WithMessage("La mesa especificada no existe");

        RuleFor(x => x.ClienteId)
            .MustAsync(async (clienteId, ct) =>
            {
                if (!clienteId.HasValue) return true;
                return await dbContext.Clientes.FindAsync(new object[] { clienteId.Value }, ct) != null;
            }).WithMessage("El cliente especificado no existe");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder los 500 caracteres");
    }
} 