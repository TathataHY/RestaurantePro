using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientosPorTipo;

public class ObtenerMovimientosPorTipoQueryHandler : IRequestHandler<ObtenerMovimientosPorTipoQuery, Result<PaginatedList<MovimientoInventarioDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ObtenerMovimientosPorTipoQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<MovimientoInventarioDto>>> Handle(ObtenerMovimientosPorTipoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Como MovimientoInventario es una owned entity, necesitamos consultar a través de Ingredientes
            var query = _context.Ingredientes
                .Include(i => i.Movimientos)
                .Include(i => i.ProveedorPrincipal)
                .Where(i => i.Movimientos.Any(m => m.TipoMovimiento == request.TipoMovimiento))
                .AsQueryable();

            // Aplicar filtros adicionales
            if (request.FechaInicio.HasValue)
            {
                query = query.Where(i => i.Movimientos.Any(m => m.Fecha >= request.FechaInicio.Value));
            }

            if (request.FechaFin.HasValue)
            {
                query = query.Where(i => i.Movimientos.Any(m => m.Fecha <= request.FechaFin.Value));
            }

            if (request.IngredienteId.HasValue)
            {
                query = query.Where(i => i.Id == request.IngredienteId.Value);
            }

            // Obtener ingredientes con sus movimientos
            var ingredientes = await query.ToListAsync(cancellationToken);

            // Extraer y aplanar todos los movimientos del tipo específico
            var movimientos = ingredientes
                .SelectMany(i => i.Movimientos
                    .Where(m => m.TipoMovimiento == request.TipoMovimiento)
                    .Select(m => new
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

            // Ordenar por fecha descendente
            movimientos = movimientos.OrderByDescending(x => x.Movimiento.Fecha).ToList();

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
            return Result.Failure<PaginatedList<MovimientoInventarioDto>>($"Error al obtener movimientos por tipo: {ex.Message}");
        }
    }
} 