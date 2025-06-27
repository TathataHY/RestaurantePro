using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientosInventario;

public class ObtenerMovimientosInventarioQueryHandler : IRequestHandler<ObtenerMovimientosInventarioQuery, Result<PaginatedList<MovimientoInventarioDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ObtenerMovimientosInventarioQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<MovimientoInventarioDto>>> Handle(ObtenerMovimientosInventarioQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Como MovimientoInventario es una owned entity, necesitamos consultar a través de Ingredientes
            var query = _context.Ingredientes
                .Include(i => i.Movimientos)
                .Include(i => i.ProveedorPrincipal)
                .AsQueryable();

            // Aplicar filtros
            if (request.FechaInicio.HasValue)
            {
                query = query.Where(i => i.Movimientos.Any(m => m.Fecha >= request.FechaInicio.Value));
            }

            if (request.FechaFin.HasValue)
            {
                query = query.Where(i => i.Movimientos.Any(m => m.Fecha <= request.FechaFin.Value));
            }

            if (request.TipoMovimiento.HasValue)
            {
                query = query.Where(i => i.Movimientos.Any(m => m.TipoMovimiento == request.TipoMovimiento.Value));
            }

            if (request.IngredienteId.HasValue)
            {
                query = query.Where(i => i.Id == request.IngredienteId.Value);
            }

            if (request.UsuarioId.HasValue)
            {
                // Nota: Los movimientos no tienen UsuarioId en la entidad actual
                // Este filtro se aplicaría si se agrega esa propiedad
            }

            if (!string.IsNullOrWhiteSpace(request.Buscar))
            {
                query = query.Where(i => i.Movimientos.Any(m => m.Motivo.Contains(request.Buscar)));
            }

            // Obtener ingredientes con sus movimientos
            var ingredientes = await query.ToListAsync(cancellationToken);

            // Extraer y aplanar todos los movimientos
            var movimientos = ingredientes
                .SelectMany(i => i.Movimientos.Select(m => new
                {
                    Movimiento = m,
                    Ingrediente = i
                }))
                .ToList();

            // Aplicar filtros adicionales a los movimientos
            if (request.FechaInicio.HasValue)
            {
                movimientos = movimientos.Where(x => x.Movimiento.Fecha >= request.FechaInicio.Value).ToList();
            }

            if (request.FechaFin.HasValue)
            {
                movimientos = movimientos.Where(x => x.Movimiento.Fecha <= request.FechaFin.Value).ToList();
            }

            if (request.TipoMovimiento.HasValue)
            {
                movimientos = movimientos.Where(x => x.Movimiento.TipoMovimiento == request.TipoMovimiento.Value).ToList();
            }

            // Ordenar
            var ordenarPor = request.OrdenarPor?.ToLower() ?? "fecha";
            var direccion = request.DireccionOrdenamiento?.ToLower() ?? "desc";

            movimientos = ordenarPor switch
            {
                "fecha" => direccion == "asc" 
                    ? movimientos.OrderBy(x => x.Movimiento.Fecha).ToList()
                    : movimientos.OrderByDescending(x => x.Movimiento.Fecha).ToList(),
                "cantidad" => direccion == "asc"
                    ? movimientos.OrderBy(x => x.Movimiento.Cantidad).ToList()
                    : movimientos.OrderByDescending(x => x.Movimiento.Cantidad).ToList(),
                "tipo" => direccion == "asc"
                    ? movimientos.OrderBy(x => x.Movimiento.TipoMovimiento).ToList()
                    : movimientos.OrderByDescending(x => x.Movimiento.TipoMovimiento).ToList(),
                "ingrediente" => direccion == "asc"
                    ? movimientos.OrderBy(x => x.Ingrediente.Nombre).ToList()
                    : movimientos.OrderByDescending(x => x.Ingrediente.Nombre).ToList(),
                _ => movimientos.OrderByDescending(x => x.Movimiento.Fecha).ToList()
            };

            // Crear DTOs
            var dtos = movimientos.Select(x => new MovimientoInventarioDto
            {
                Id = x.Movimiento.Id,
                IngredienteId = x.Ingrediente.Id,
                NombreIngrediente = x.Ingrediente.Nombre,
                Tipo = x.Movimiento.TipoMovimiento,
                Cantidad = x.Movimiento.Cantidad,
                CostoUnitario = 0, // No disponible en la entidad actual
                StockAnterior = 0, // No disponible en la entidad actual
                StockResultante = x.Movimiento.CantidadFinal ?? 0,
                Motivo = x.Movimiento.Motivo,
                Observaciones = null, // No disponible en la entidad actual
                UsuarioId = Guid.Empty, // No disponible en la entidad actual
                NombreUsuario = "Sistema", // Placeholder
                NumeroDocumento = null,
                ProveedorId = x.Ingrediente.ProveedorPrincipal?.Id,
                NombreProveedor = x.Ingrediente.ProveedorPrincipal?.Nombre ?? "",
                FechaCreacion = x.Movimiento.Fecha,
                FechaModificacion = x.Movimiento.FechaActualizacion,
                CreadoPor = x.Movimiento.CreatedBy ?? "Sistema",
                ModificadoPor = x.Movimiento.LastModifiedBy ?? "Sistema",
                Activo = !x.Movimiento.EstaEliminado,
                ColorTipo = x.Movimiento.TipoMovimiento == TipoMovimientoInventario.Ingreso ? "success" : "danger",
                IconoTipo = x.Movimiento.TipoMovimiento == TipoMovimientoInventario.Ingreso ? "arrow-up" : "arrow-down",
                FechaTexto = x.Movimiento.Fecha.ToString("dd/MM/yyyy HH:mm"),
                CantidadTexto = $"{x.Movimiento.Cantidad} {x.Ingrediente.UnidadMedida}"
            }).ToList();

            // Paginar
            var totalCount = dtos.Count;
            var items = dtos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var paginatedList = new PaginatedList<MovimientoInventarioDto>(items, totalCount, request.PageNumber, request.PageSize);

            return Result.Success(paginatedList);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedList<MovimientoInventarioDto>>($"Error al obtener movimientos: {ex.Message}");
        }
    }
} 