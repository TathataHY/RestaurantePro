using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Pagos.Commands.CancelarPago
{
    public class CancelarPagoCommandHandler : IRequestHandler<CancelarPagoCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CancelarPagoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(CancelarPagoCommand request, CancellationToken cancellationToken)
        {
            // Verificar que el pago exista
            var pago = await _context.Pagos
                .Include(p => p.Comanda)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (pago == null)
                throw new Exception($"No existe un pago con el ID {request.Id}");

            // Verificar que el pago esté en estado completado
            if (pago.Estado != EstadoPago.Completado)
                throw new Exception($"Solo se pueden cancelar pagos en estado Completado, estado actual: {pago.Estado}");

            // Obtener el ID del usuario actual
            var usuarioId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(usuarioId))
                throw new Exception("No se ha podido identificar al usuario");

            // Actualizar el estado del pago
            pago.Estado = EstadoPago.Cancelado;
            pago.Notas = (string.IsNullOrEmpty(pago.Notas) ? "" : pago.Notas + " | ") + 
                          $"Cancelado por {usuarioId} el {DateTime.Now}. Motivo: {request.MotivoCancelacion}";
            pago.UltimaModificacion = DateTime.Now;

            // Si la comanda está pagada, cambiarla a entregada
            if (pago.Comanda.Estado == EstadoComanda.Pagada)
            {
                pago.Comanda.Estado = EstadoComanda.Entregada;
                pago.Comanda.UltimaModificacion = DateTime.Now;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
} 