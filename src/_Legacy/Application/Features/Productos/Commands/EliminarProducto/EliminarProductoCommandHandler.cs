using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Productos.Commands.EliminarProducto
{
    public class EliminarProductoCommandHandler : IRequestHandler<EliminarProductoCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public EliminarProductoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(EliminarProductoCommand request, CancellationToken cancellationToken)
        {
            var producto = await _context.Productos
                .Include(p => p.Ingredientes)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (producto == null)
                return false;

            // Eliminar primero los ingredientes del producto
            foreach (var ingrediente in producto.Ingredientes)
            {
                _context.IngredientesProductos.Remove(ingrediente);
            }

            // Eliminar el producto
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
} 