using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Clientes.Dtos;
using RestaurantePro.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Clientes.Queries.ObtenerClientes
{
    public class ObtenerClientesQueryHandler : IRequestHandler<ObtenerClientesQuery, List<ClienteDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerClientesQueryHandler(
            IApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ClienteDto>> Handle(ObtenerClientesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Clientes
                .Include(c => c.TarjetasFidelizacion)
                .ThenInclude(t => t.Nivel)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(request.Busqueda))
            {
                var busqueda = request.Busqueda.ToLower();
                query = query.Where(c => 
                    c.Nombre.ToLower().Contains(busqueda) || 
                    c.Apellido.ToLower().Contains(busqueda) || 
                    c.Email.ToLower().Contains(busqueda) || 
                    c.Telefono.Contains(busqueda));
            }

            if (request.Activos.HasValue)
            {
                query = query.Where(c => c.Activo == request.Activos.Value);
            }

            if (request.NivelFidelizacionId.HasValue)
            {
                query = query.Where(c => c.TarjetasFidelizacion
                    .Any(t => t.NivelId == request.NivelFidelizacionId.Value));
            }

            if (request.PuntosMinimos.HasValue)
            {
                query = query.Where(c => c.TarjetasFidelizacion
                    .Any(t => t.PuntosDisponibles >= request.PuntosMinimos.Value));
            }

            if (request.ConTarjeta.HasValue)
            {
                if (request.ConTarjeta.Value)
                {
                    query = query.Where(c => c.TarjetasFidelizacion.Any());
                }
                else
                {
                    query = query.Where(c => !c.TarjetasFidelizacion.Any());
                }
            }

            // Aplicar paginación
            var totalItems = await query.CountAsync(cancellationToken);
            var clientes = await query
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Skip((request.Pagina - 1) * request.TamañoPagina)
                .Take(request.TamañoPagina)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs
            return _mapper.Map<List<ClienteDto>>(clientes);
        }
    }
} 