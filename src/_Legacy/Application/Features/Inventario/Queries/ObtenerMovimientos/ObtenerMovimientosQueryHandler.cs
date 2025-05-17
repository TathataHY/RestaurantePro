using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Mappings;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Features.Inventario.Dtos;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Inventario.Queries.ObtenerMovimientos
{
    public class ObtenerMovimientosQueryHandler : IRequestHandler<ObtenerMovimientosQuery, PaginatedList<MovimientoDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public ObtenerMovimientosQueryHandler(
            IApplicationDbContext context, 
            IMapper mapper,
            IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<PaginatedList<MovimientoDto>> Handle(ObtenerMovimientosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.MovimientosInventario
                .Include(m => m.Ingrediente)
                .AsQueryable();

            // Aplicar filtros
            if (request.IngredienteId.HasValue)
            {
                query = query.Where(m => m.IngredienteId == request.IngredienteId.Value);
            }

            if (request.TipoMovimiento.HasValue)
            {
                query = query.Where(m => m.TipoMovimiento == request.TipoMovimiento.Value);
            }

            if (request.FechaDesde.HasValue)
            {
                query = query.Where(m => m.Fecha >= request.FechaDesde.Value);
            }

            if (request.FechaHasta.HasValue)
            {
                // Incluir todo el día hasta las 23:59:59
                DateTime fechaHastaFinal = request.FechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(m => m.Fecha <= fechaHastaFinal);
            }

            if (!string.IsNullOrWhiteSpace(request.Referencia))
            {
                query = query.Where(m => m.Referencia.Contains(request.Referencia));
            }

            if (!string.IsNullOrWhiteSpace(request.UsuarioId))
            {
                query = query.Where(m => m.UsuarioId == request.UsuarioId);
            }

            // Aplicar ordenamiento
            query = ApplyOrdering(query, request.OrderBy, request.OrderDirection);

            // Proyectar y paginar resultados
            var paginatedList = await query
                .ProjectTo<MovimientoDto>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber, request.PageSize);

            // Complementar con nombres de usuarios
            foreach (var movimiento in paginatedList.Items)
            {
                if (!string.IsNullOrEmpty(movimiento.Usuario))
                {
                    var usuario = await _userService.GetUserNameAsync(movimiento.Usuario);
                    if (!string.IsNullOrEmpty(usuario))
                    {
                        movimiento.Usuario = usuario;
                    }
                }
            }

            return paginatedList;
        }

        private IQueryable<Domain.Entities.MovimientoInventario> ApplyOrdering(
            IQueryable<Domain.Entities.MovimientoInventario> query,
            string orderBy,
            string orderDirection)
        {
            var isAscending = orderDirection.ToLower() == "asc";

            return orderBy.ToLower() switch
            {
                "fecha" => isAscending
                    ? query.OrderBy(m => m.Fecha)
                    : query.OrderByDescending(m => m.Fecha),

                "ingrediente" => isAscending
                    ? query.OrderBy(m => m.Ingrediente.Nombre)
                    : query.OrderByDescending(m => m.Ingrediente.Nombre),

                "tipomovimiento" => isAscending
                    ? query.OrderBy(m => m.TipoMovimiento)
                    : query.OrderByDescending(m => m.TipoMovimiento),

                "cantidad" => isAscending
                    ? query.OrderBy(m => m.Cantidad)
                    : query.OrderByDescending(m => m.Cantidad),

                "usuario" => isAscending
                    ? query.OrderBy(m => m.UsuarioId)
                    : query.OrderByDescending(m => m.UsuarioId),

                _ => isAscending
                    ? query.OrderBy(m => m.Fecha)
                    : query.OrderByDescending(m => m.Fecha)
            };
        }
    }
} 