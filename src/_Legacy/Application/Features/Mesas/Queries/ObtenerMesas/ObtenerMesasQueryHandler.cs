using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Mesas.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Mesas.Queries.ObtenerMesas
{
    public class ObtenerMesasQueryHandler : IRequestHandler<ObtenerMesasQuery, List<MesaDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerMesasQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<MesaDto>> Handle(ObtenerMesasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Mesas.AsQueryable();

            // Filtrar por estado si se proporciona
            if (request.Estado.HasValue)
            {
                query = query.Where(m => m.Estado == request.Estado.Value);
            }

            // Filtrar por capacidad mínima si se proporciona
            if (request.CapacidadMinima.HasValue)
            {
                query = query.Where(m => m.Capacidad >= request.CapacidadMinima.Value);
            }

            // Filtrar por activas si se proporciona
            if (request.SoloActivas.HasValue && request.SoloActivas.Value)
            {
                query = query.Where(m => m.Activa);
            }

            // Ordenar por número
            query = query.OrderBy(m => m.Numero);

            var mesas = await query.ToListAsync(cancellationToken);
            return _mapper.Map<List<MesaDto>>(mesas);
        }
    }
} 