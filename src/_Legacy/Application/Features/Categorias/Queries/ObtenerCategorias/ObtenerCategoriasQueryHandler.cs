using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Categorias.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Categorias.Queries.ObtenerCategorias
{
    public class ObtenerCategoriasQueryHandler : IRequestHandler<ObtenerCategoriasQuery, List<CategoriaDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerCategoriasQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CategoriaDto>> Handle(ObtenerCategoriasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Categorias
                .Include(c => c.Productos)
                .AsQueryable();

            // Si se indica, filtrar solo categorías activas
            if (request.SoloActivas.HasValue && request.SoloActivas.Value)
            {
                query = query.Where(c => c.Productos.Any(p => p.Disponible));
            }

            // Ordenar por orden (ascendente)
            query = query.OrderBy(c => c.Orden);

            var categorias = await query.ToListAsync(cancellationToken);
            
            var categoriasDto = _mapper.Map<List<CategoriaDto>>(categorias);
            
            // Agregar la cantidad de productos por categoría
            for (int i = 0; i < categorias.Count; i++)
            {
                categoriasDto[i].CantidadProductos = categorias[i].Productos.Count;
            }
            
            return categoriasDto;
        }
    }
} 