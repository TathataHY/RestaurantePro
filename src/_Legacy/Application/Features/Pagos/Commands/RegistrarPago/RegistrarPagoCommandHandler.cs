using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Pagos.Commands.RegistrarPago
{
    public class RegistrarPagoCommandHandler : IRequestHandler<RegistrarPagoCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public RegistrarPagoCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(RegistrarPagoCommand request, CancellationToken cancellationToken)
        {
            // Verificar que la comanda exista
            var comanda = await _context.Comandas
                .Include(c => c.Mesa)
                .FirstOrDefaultAsync(c => c.Id == request.ComandaId, cancellationToken);

            if (comanda == null)
                throw new Exception($"No existe una comanda con el ID {request.ComandaId}");

            // Verificar que la comanda esté en estado entregada
            if (comanda.Estado != EstadoComanda.Entregada)
                throw new Exception($"La comanda debe estar en estado Entregada para poder registrar un pago, estado actual: {comanda.Estado}");

            // Verificar que el monto del pago no sea mayor al total de la comanda
            if (request.Monto > comanda.Total)
                throw new Exception($"El monto del pago (${request.Monto}) no puede ser mayor al total de la comanda (${comanda.Total})");

            // Obtener el ID del usuario actual
            var usuarioId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(usuarioId))
                throw new Exception("No se ha podido identificar al usuario");

            // Crear el pago
            var pago = new Pago
            {
                ComandaId = request.ComandaId,
                UsuarioId = usuarioId,
                MetodoPago = request.MetodoPago,
                Referencia = request.Referencia,
                Monto = request.Monto,
                Estado = EstadoPago.Completado,
                Notas = request.Notas,
                FechaCreacion = DateTime.Now
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync(cancellationToken);

            // Verificar si el pago cubre el total de la comanda
            decimal totalPagado = request.Monto;
            var pagosPrevios = await _context.Pagos
                .Where(p => p.ComandaId == request.ComandaId && p.Estado == EstadoPago.Completado && p.Id != pago.Id)
                .ToListAsync(cancellationToken);

            if (pagosPrevios.Any())
                totalPagado += pagosPrevios.Sum(p => p.Monto);

            // Si el total pagado es igual o mayor al total de la comanda, marcar la comanda como pagada
            if (totalPagado >= comanda.Total)
            {
                comanda.Estado = EstadoComanda.Pagada;
                comanda.FechaCompletado = DateTime.Now;
                comanda.UltimaModificacion = DateTime.Now;

                // Liberar la mesa
                comanda.Mesa.Estado = EstadoMesa.Libre;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return pago.Id;
        }
    }
} 