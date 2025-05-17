using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Categorias.Commands.EliminarCategoria
{
    public class EliminarCategoriaCommandHandler : IRequestHandler<EliminarCategoriaCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public EliminarCategoriaCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(EliminarCategoriaCommand request, CancellationToken cancellationToken)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (categoria == null)
                return false;

            // Verificar si la categoría tiene productos, en ese caso no se permite eliminar
            if (categoria.Productos.Any())
                return false;

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
} 