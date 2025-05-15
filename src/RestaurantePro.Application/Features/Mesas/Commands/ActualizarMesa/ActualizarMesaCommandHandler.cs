using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Mesas.Commands.ActualizarMesa
{
    public class ActualizarMesaCommandHandler : IRequestHandler<ActualizarMesaCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarMesaCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ActualizarMesaCommand request, CancellationToken cancellationToken)
        {
            var mesa = await _context.Mesas
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (mesa == null)
                return false;

            // Verificar que el número de mesa no exista si se está cambiando
            if (mesa.Numero != request.Numero)
            {
                var existeNumero = await _context.Mesas
                    .AnyAsync(m => m.Numero == request.Numero && m.Id != request.Id, cancellationToken);

                if (existeNumero)
                    throw new Exception($"Ya existe una mesa con el número {request.Numero}");
            }

            mesa.Numero = request.Numero;
            mesa.Capacidad = request.Capacidad;
            mesa.Ubicacion = request.Ubicacion;
            mesa.Activa = request.Activa;
            mesa.Estado = request.Estado;
            mesa.UltimaModificacion = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
} 