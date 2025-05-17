using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Productos.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Productos.Queries.ObtenerProductoPorId
{
    public class ObtenerProductoPorIdQueryHandler : IRequestHandler<ObtenerProductoPorIdQuery, ProductoDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerProductoPorIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductoDto> Handle(ObtenerProductoPorIdQuery request, CancellationToken cancellationToken)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Ingredientes)
                .ThenInclude(i => i.Ingrediente)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (producto == null)
                return null;

            return _mapper.Map<ProductoDto>(producto);
        }
    }
} 