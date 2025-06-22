using FluentValidation;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CambiarEstadoComanda;

public class CambiarEstadoComandaCommandValidator : AbstractValidator<CambiarEstadoComandaCommand>
{
    private static readonly string[] EstadosValidos = { "enproceso", "lista", "entregada", "finalizada" };

    public CambiarEstadoComandaCommandValidator(IComandaRepository comandaRepository)
    {
        RuleFor(x => x.ComandaId)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio")
            .MustAsync(async (comandaId, ct) =>
            {
                var comanda = await comandaRepository.ObtenerPorIdAsync(comandaId, false, ct);
                if (comanda == null) return false;
                
                // Validar que no esté cancelada o finalizada
                return comanda.Estado != EstadoComanda.Cancelada && 
                       comanda.Estado != EstadoComanda.Finalizada;
            }).WithMessage("La comanda no existe o no permite cambios de estado");

        RuleFor(x => x.NuevoEstado)
            .NotEmpty().WithMessage("El nuevo estado es obligatorio")
            .Must(estado => EstadosValidos.Contains(estado.ToLower()))
                .WithMessage($"El estado debe ser uno de: {string.Join(", ", EstadosValidos)}");

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var comanda = await comandaRepository.ObtenerPorIdAsync(command.ComandaId, false, ct);
                if (comanda == null) return false;

                // Validar transiciones permitidas
                return EsTransicionValida(comanda.Estado, command.NuevoEstado.ToLower());
            }).WithMessage("La transición de estado no está permitida");
    }

    private static bool EsTransicionValida(EstadoComanda estadoActual, string nuevoEstado)
    {
        return (estadoActual, nuevoEstado) switch
        {
            (EstadoComanda.Creada, "enproceso") => true,
            (EstadoComanda.Creada, "finalizada") => true,
            (EstadoComanda.EnProceso, "lista") => true,
            (EstadoComanda.EnProceso, "finalizada") => true,
            (EstadoComanda.Lista, "entregada") => true,
            (EstadoComanda.Lista, "finalizada") => true,
            (EstadoComanda.Entregada, "finalizada") => true,
            _ => false
        };
    }
} 