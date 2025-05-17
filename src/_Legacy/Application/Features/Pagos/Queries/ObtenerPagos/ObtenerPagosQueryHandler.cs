using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Pagos.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Pagos.Queries.ObtenerPagos
{
    public class ObtenerPagosQueryHandler : IRequestHandler<ObtenerPagosQuery, List<PagoDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerPagosQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<PagoDto>> Handle(ObtenerPagosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Pagos
                .Include(p => p.Comanda)
                .Include(p => p.Usuario)
                .AsQueryable();

            // Aplicar filtros
            if (request.ComandaId.HasValue)
                query = query.Where(p => p.ComandaId == request.ComandaId.Value);

            if (request.Estado.HasValue)
                query = query.Where(p => p.Estado == request.Estado.Value);

            if (request.MetodoPago.HasValue)
                query = query.Where(p => p.MetodoPago == request.MetodoPago.Value);

            if (request.FechaDesde.HasValue)
                query = query.Where(p => p.FechaCreacion >= request.FechaDesde.Value);

            if (request.FechaHasta.HasValue)
                query = query.Where(p => p.FechaCreacion <= request.FechaHasta.Value);

            if (!string.IsNullOrEmpty(request.UsuarioId))
                query = query.Where(p => p.UsuarioId == request.UsuarioId);

            // Ordenar por fecha de creación (descendente)
            query = query.OrderByDescending(p => p.FechaCreacion);

            var pagos = await query.ToListAsync(cancellationToken);
            
            return _mapper.Map<List<PagoDto>>(pagos);
        }
    }
} 