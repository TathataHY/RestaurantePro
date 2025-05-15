using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Productos.Commands.CambiarDisponibilidadProducto
{
    public class CambiarDisponibilidadProductoCommandHandler : IRequestHandler<CambiarDisponibilidadProductoCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public CambiarDisponibilidadProductoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(CambiarDisponibilidadProductoCommand request, CancellationToken cancellationToken)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (producto == null)
                return false;

            producto.Disponible = request.Disponible;
            producto.UltimaModificacion = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
} 