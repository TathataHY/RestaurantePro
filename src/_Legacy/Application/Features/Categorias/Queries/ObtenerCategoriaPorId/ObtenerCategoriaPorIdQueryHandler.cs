using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Categorias.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Categorias.Queries.ObtenerCategoriaPorId
{
    public class ObtenerCategoriaPorIdQueryHandler : IRequestHandler<ObtenerCategoriaPorIdQuery, CategoriaDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerCategoriaPorIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CategoriaDto> Handle(ObtenerCategoriaPorIdQuery request, CancellationToken cancellationToken)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (categoria == null)
                return null;

            var categoriaDto = _mapper.Map<CategoriaDto>(categoria);
            categoriaDto.CantidadProductos = categoria.Productos.Count;
            
            return categoriaDto;
        }
    }
} 