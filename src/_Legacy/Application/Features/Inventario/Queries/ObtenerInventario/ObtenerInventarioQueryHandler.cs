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

namespace RestaurantePro.Application.Features.Inventario.Queries.ObtenerInventario
{
    public class ObtenerInventarioQueryHandler : IRequestHandler<ObtenerInventarioQuery, PaginatedList<InventarioDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ObtenerInventarioQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<InventarioDto>> Handle(ObtenerInventarioQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Inventarios
                .Include(i => i.Ingrediente)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(request.Filtro))
            {
                var filtro = request.Filtro.ToLower();
                query = query.Where(i => i.Ingrediente.Nombre.ToLower().Contains(filtro) || 
                                        i.Ingrediente.Categoria.ToLower().Contains(filtro));
            }

            if (!string.IsNullOrWhiteSpace(request.Categoria))
            {
                var categoria = request.Categoria.ToLower();
                query = query.Where(i => i.Ingrediente.Categoria.ToLower() == categoria);
            }

            if (request.SoloConAlertas)
            {
                query = query.Where(i => i.CantidadDisponible <= i.CantidadMinima);
            }

            // Aplicar ordenamiento
            query = ApplyOrdering(query, request.OrderBy, request.OrderDirection);

            // Proyectar y paginar resultados
            var paginatedList = await query
                .ProjectTo<InventarioDto>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber, request.PageSize);

            // Calcular propiedades adicionales
            foreach (var item in paginatedList.Items)
            {
                item.ValorTotal = item.CantidadDisponible * item.CostoUnitario;
                item.RequiereReposicion = item.CantidadDisponible <= item.CantidadMinima;
                
                if (item.CantidadOptima > 0)
                {
                    item.PorcentajeDisponible = Math.Round((item.CantidadDisponible / item.CantidadOptima) * 100, 2);
                }
                else
                {
                    item.PorcentajeDisponible = 0;
                }
            }

            return paginatedList;
        }

        private IQueryable<Domain.Entities.Inventario> ApplyOrdering(
            IQueryable<Domain.Entities.Inventario> query, 
            string orderBy, 
            string orderDirection)
        {
            var isAscending = orderDirection.ToLower() == "asc";

            return orderBy.ToLower() switch
            {
                "nombreingrediente" => isAscending 
                    ? query.OrderBy(i => i.Ingrediente.Nombre) 
                    : query.OrderByDescending(i => i.Ingrediente.Nombre),
                
                "categoria" => isAscending 
                    ? query.OrderBy(i => i.Ingrediente.Categoria) 
                    : query.OrderByDescending(i => i.Ingrediente.Categoria),
                
                "cantidaddisponible" => isAscending 
                    ? query.OrderBy(i => i.CantidadDisponible) 
                    : query.OrderByDescending(i => i.CantidadDisponible),
                
                "costounitario" => isAscending 
                    ? query.OrderBy(i => i.CostoUnitario) 
                    : query.OrderByDescending(i => i.CostoUnitario),
                
                "ultimaactualizacion" => isAscending 
                    ? query.OrderBy(i => i.UltimaActualizacion) 
                    : query.OrderByDescending(i => i.UltimaActualizacion),
                
                _ => isAscending 
                    ? query.OrderBy(i => i.Ingrediente.Nombre) 
                    : query.OrderByDescending(i => i.Ingrediente.Nombre)
            };
        }
    }
} 