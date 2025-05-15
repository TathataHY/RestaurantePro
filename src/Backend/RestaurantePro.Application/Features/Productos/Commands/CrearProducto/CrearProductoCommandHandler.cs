using AutoMapper;
using MediatR;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Productos.Commands.CrearProducto
{
    public class CrearProductoCommandHandler : IRequestHandler<CrearProductoCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CrearProductoCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> Handle(CrearProductoCommand request, CancellationToken cancellationToken)
        {
            var producto = _mapper.Map<Producto>(request);
            
            // Asignar fechas
            producto.FechaCreacion = DateTime.Now;
            
            // Agregar el producto
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync(cancellationToken);
            
            // Agregar los ingredientes del producto
            if (request.Ingredientes != null && request.Ingredientes.Any())
            {
                foreach (var ingredienteInfo in request.Ingredientes)
                {
                    var ingredienteProducto = new IngredienteProducto
                    {
                        ProductoId = producto.Id,
                        IngredienteId = ingredienteInfo.IngredienteId,
                        Cantidad = ingredienteInfo.Cantidad,
                        Opcional = ingredienteInfo.Opcional
                    };
                    
                    _context.IngredientesProductos.Add(ingredienteProducto);
                }
                
                await _context.SaveChangesAsync(cancellationToken);
            }
            
            return producto.Id;
        }
    }
} 