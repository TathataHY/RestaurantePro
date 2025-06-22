using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.EliminarComanda;

/// <summary>
/// Validator para EliminarComandaCommand
/// </summary>
public class EliminarComandaCommandValidator : AbstractValidator<EliminarComandaCommand>
{
    public EliminarComandaCommandValidator(IApplicationDbContext context)
    {
        // 🎯 Validar ComandaId
        RuleFor(x => x.ComandaId)
            .NotEmpty()
            .WithMessage("🚫 El ID de la comanda es obligatorio");

        // 🎯 Validar que la comanda existe
        RuleFor(x => x.ComandaId)
            .MustAsync(async (comandaId, cancellation) =>
            {
                var comanda = await context.Comandas.FindAsync(new object[] { comandaId }, cancellation);
                return comanda != null;
            })
            .WithMessage("🚫 La comanda especificada no existe");

        // 🎯 Validar que la comanda no está eliminada
        RuleFor(x => x.ComandaId)
            .MustAsync(async (comandaId, cancellation) =>
            {
                var comanda = await context.Comandas.FindAsync(new object[] { comandaId }, cancellation);
                return comanda != null && !comanda.EstaEliminada;
            })
            .WithMessage("🚫 La comanda ya ha sido eliminada");

        // 🎯 Validar estado de la comanda
        RuleFor(x => x.ComandaId)
            .MustAsync(async (comandaId, cancellation) =>
            {
                var comanda = await context.Comandas.FindAsync(new object[] { comandaId }, cancellation);
                if (comanda == null) return false;

                // Estados que no permiten eliminación
                var estadosNoEliminables = new[] 
                { 
                    EstadoComanda.Facturada, 
                    EstadoComanda.Cerrada 
                };

                return !estadosNoEliminables.Contains(comanda.Estado);
            })
            .WithMessage("🚫 No se puede eliminar una comanda que ya ha sido facturada o cerrada");

        // 🎯 Validar que no hay items en preparación
        RuleFor(x => x.ComandaId)
            .MustAsync(async (comandaId, cancellation) =>
            {
                var comanda = await context.Comandas
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Id == comandaId, cancellation);

                if (comanda == null) return false;

                // Verificar que no hay items en preparación
                return !comanda.Items.Any(i => i.Estado == Domain.Operaciones.Comandas.Enums.EstadoItem.EnPreparacion);
            })
            .WithMessage("🚫 No se puede eliminar una comanda con items en preparación");
    }
} 