using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Comandas.Commands.ActualizarEstadoComanda
{
    public class ActualizarEstadoComandaCommandHandler : IRequestHandler<ActualizarEstadoComandaCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarEstadoComandaCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ActualizarEstadoComandaCommand request, CancellationToken cancellationToken)
        {
            var comanda = await _context.Comandas
                .Include(c => c.Mesa)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (comanda == null)
                return false;

            // Verificar que la transición de estado sea válida
            if (!EsTransicionEstadoValida(comanda.Estado, request.NuevoEstado))
                throw new Exception($"No es posible cambiar el estado de {comanda.Estado} a {request.NuevoEstado}");

            // Actualizar el estado de la comanda
            comanda.Estado = request.NuevoEstado;
            comanda.UltimaModificacion = DateTime.Now;

            // Si la comanda se marca como completada o pagada, actualizar la fecha de completado
            if (request.NuevoEstado == EstadoComanda.Pagada)
            {
                comanda.FechaCompletado = DateTime.Now;

                // Si la comanda se marca como pagada, liberar la mesa
                comanda.Mesa.Estado = EstadoMesa.Libre;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private bool EsTransicionEstadoValida(EstadoComanda estadoActual, EstadoComanda nuevoEstado)
        {
            // Reglas de transición de estados:
            switch (estadoActual)
            {
                case EstadoComanda.Pendiente:
                    return nuevoEstado == EstadoComanda.EnPreparacion || nuevoEstado == EstadoComanda.Cancelada;

                case EstadoComanda.EnPreparacion:
                    return nuevoEstado == EstadoComanda.Lista || nuevoEstado == EstadoComanda.Cancelada;

                case EstadoComanda.Lista:
                    return nuevoEstado == EstadoComanda.Entregada || nuevoEstado == EstadoComanda.Cancelada;

                case EstadoComanda.Entregada:
                    return nuevoEstado == EstadoComanda.Pagada;

                case EstadoComanda.Pagada:
                    return false; // No se puede cambiar de Pagada a otro estado

                case EstadoComanda.Cancelada:
                    return false; // No se puede cambiar de Cancelada a otro estado

                default:
                    return false;
            }
        }
    }
} 