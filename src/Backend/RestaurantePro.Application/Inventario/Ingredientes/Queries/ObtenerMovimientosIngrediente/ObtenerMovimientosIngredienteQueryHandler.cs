using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;

namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerMovimientosIngrediente;

public class ObtenerMovimientosIngredienteQueryHandler : IRequestHandler<ObtenerMovimientosIngredienteQuery, Result<List<MovimientoInventarioDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ObtenerMovimientosIngredienteQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<MovimientoInventarioDto>>> Handle(ObtenerMovimientosIngredienteQuery request, CancellationToken cancellationToken)
    {
        // Obtener el ingrediente con sus movimientos incluidos
        var ingrediente = await _context.Ingredientes
            .Include(i => i.Movimientos)
            .FirstOrDefaultAsync(i => i.Id == request.IngredienteId, cancellationToken);

        if (ingrediente == null)
        {
            return Result.Failure<List<MovimientoInventarioDto>>("Ingrediente no encontrado");
        }

        // Filtrar los movimientos según los criterios
        var movimientos = ingrediente.Movimientos.AsQueryable();

        // Aplicar filtros de fecha si se proporcionan
        if (request.FechaDesde.HasValue)
        {
            movimientos = movimientos.Where(m => m.Fecha >= request.FechaDesde.Value);
        }

        if (request.FechaHasta.HasValue)
        {
            movimientos = movimientos.Where(m => m.Fecha <= request.FechaHasta.Value);
        }

        // Ordenar por fecha descendente
        movimientos = movimientos.OrderByDescending(m => m.Fecha);

        // Aplicar límite
        if (request.Limite.HasValue)
        {
            movimientos = movimientos.Take(request.Limite.Value);
        }

        var movimientosList = movimientos.ToList();
        var movimientosDto = _mapper.Map<List<MovimientoInventarioDto>>(movimientosList);

        return Result.Success(movimientosDto);
    }
} 