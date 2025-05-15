using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Productos.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Productos.Queries.ObtenerProductos
{
    public class ObtenerProductosQueryHandler : IRequestHandler<ObtenerProductosQuery, List<ProductoDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerProductosQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ProductoDto>> Handle(ObtenerProductosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Ingredientes)
                .ThenInclude(i => i.Ingrediente)
                .AsQueryable();

            // Filtrar por categoría si se proporciona
            if (request.CategoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == request.CategoriaId.Value);
            }

            // Filtrar por disponibilidad si se proporciona
            if (request.Disponible.HasValue)
            {
                query = query.Where(p => p.Disponible == request.Disponible.Value);
            }

            // Ordenar por nombre
            query = query.OrderBy(p => p.Nombre);

            var productos = await query.ToListAsync(cancellationToken);
            return _mapper.Map<List<ProductoDto>>(productos);
        }
    }
} 