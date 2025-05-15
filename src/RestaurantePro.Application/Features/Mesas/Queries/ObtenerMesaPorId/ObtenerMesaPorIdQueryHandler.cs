using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Mesas.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Mesas.Queries.ObtenerMesaPorId
{
    public class ObtenerMesaPorIdQueryHandler : IRequestHandler<ObtenerMesaPorIdQuery, MesaDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerMesaPorIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MesaDto> Handle(ObtenerMesaPorIdQuery request, CancellationToken cancellationToken)
        {
            var mesa = await _context.Mesas
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (mesa == null)
                return null;

            return _mapper.Map<MesaDto>(mesa);
        }
    }
} 