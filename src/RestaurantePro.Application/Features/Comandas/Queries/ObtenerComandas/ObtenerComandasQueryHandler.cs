using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Comandas.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Comandas.Queries.ObtenerComandas
{
    public class ObtenerComandasQueryHandler : IRequestHandler<ObtenerComandasQuery, List<ComandaDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerComandasQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ComandaDto>> Handle(ObtenerComandasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Comandas
                .Include(c => c.Mesa)
                .Include(c => c.Usuario)
                .Include(c => c.ComandaDetalles)
                    .ThenInclude(cd => cd.Producto)
                .Include(c => c.ComandaDetalles)
                    .ThenInclude(cd => cd.Personalizaciones)
                        .ThenInclude(p => p.Ingrediente)
                .AsQueryable();

            // Aplicar filtros
            if (request.MesaId.HasValue)
                query = query.Where(c => c.MesaId == request.MesaId.Value);

            if (request.Estado.HasValue)
                query = query.Where(c => c.Estado == request.Estado.Value);

            if (request.FechaDesde.HasValue)
                query = query.Where(c => c.FechaCreacion >= request.FechaDesde.Value);

            if (request.FechaHasta.HasValue)
                query = query.Where(c => c.FechaCreacion <= request.FechaHasta.Value);

            if (!string.IsNullOrEmpty(request.UsuarioId))
                query = query.Where(c => c.UsuarioId == request.UsuarioId);

            // Ordenar por fecha de creación (descendente)
            query = query.OrderByDescending(c => c.FechaCreacion);

            var comandas = await query.ToListAsync(cancellationToken);
            
            return _mapper.Map<List<ComandaDto>>(comandas);
        }
    }
} 