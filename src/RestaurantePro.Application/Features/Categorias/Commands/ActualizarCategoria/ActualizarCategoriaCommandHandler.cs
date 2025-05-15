using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Categorias.Commands.ActualizarCategoria
{
    public class ActualizarCategoriaCommandHandler : IRequestHandler<ActualizarCategoriaCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarCategoriaCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ActualizarCategoriaCommand request, CancellationToken cancellationToken)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (categoria == null)
                return false;

            categoria.Nombre = request.Nombre;
            categoria.Descripcion = request.Descripcion;
            categoria.ImagenUrl = request.ImagenUrl;
            categoria.Orden = request.Orden;
            categoria.UltimaModificacion = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
} 