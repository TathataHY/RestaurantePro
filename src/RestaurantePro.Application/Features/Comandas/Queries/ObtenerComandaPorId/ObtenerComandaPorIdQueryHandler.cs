using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Comandas.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Comandas.Queries.ObtenerComandaPorId
{
    public class ObtenerComandaPorIdQueryHandler : IRequestHandler<ObtenerComandaPorIdQuery, ComandaDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerComandaPorIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ComandaDto> Handle(ObtenerComandaPorIdQuery request, CancellationToken cancellationToken)
        {
            var comanda = await _context.Comandas
                .Include(c => c.Mesa)
                .Include(c => c.Usuario)
                .Include(c => c.ComandaDetalles)
                    .ThenInclude(cd => cd.Producto)
                .Include(c => c.ComandaDetalles)
                    .ThenInclude(cd => cd.Personalizaciones)
                        .ThenInclude(p => p.Ingrediente)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (comanda == null)
                return null;

            return _mapper.Map<ComandaDto>(comanda);
        }
    }
} 