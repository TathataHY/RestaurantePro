using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.ObtenerMovimientoPorId;

public class ObtenerMovimientoPorIdQueryHandler : IRequestHandler<ObtenerMovimientoPorIdQuery, Result<MovimientoInventarioDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ObtenerMovimientoPorIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<MovimientoInventarioDto>> Handle(ObtenerMovimientoPorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Como MovimientoInventario es una owned entity, necesitamos buscar a través de Ingredientes
            var ingrediente = await _context.Ingredientes
                .Include(i => i.Movimientos)
                .Include(i => i.ProveedorPrincipal)
                .FirstOrDefaultAsync(i => i.Movimientos.Any(m => m.Id == request.Id), cancellationToken);

            if (ingrediente == null)
            {
                return Result.Failure<MovimientoInventarioDto>("Movimiento no encontrado");
            }

            var movimiento = ingrediente.Movimientos.FirstOrDefault(m => m.Id == request.Id);
            if (movimiento == null)
            {
                return Result.Failure<MovimientoInventarioDto>("Movimiento no encontrado");
            }

            // Crear el DTO
            var dto = new MovimientoInventarioDto
            {
                Id = movimiento.Id,
                IngredienteId = ingrediente.Id,
                NombreIngrediente = ingrediente.Nombre,
                Tipo = movimiento.TipoMovimiento,
                Cantidad = movimiento.Cantidad,
                CostoUnitario = 0, // No disponible en la entidad actual
                StockAnterior = 0, // No disponible en la entidad actual
                StockResultante = movimiento.CantidadFinal ?? 0,
                Motivo = movimiento.Motivo,
                Observaciones = null, // No disponible en la entidad actual
                UsuarioId = Guid.Empty, // No disponible en la entidad actual
                NombreUsuario = "Sistema", // Placeholder
                NumeroDocumento = null,
                ProveedorId = ingrediente.ProveedorPrincipal?.Id,
                NombreProveedor = ingrediente.ProveedorPrincipal?.Nombre ?? "",
                FechaCreacion = movimiento.Fecha,
                FechaModificacion = movimiento.FechaActualizacion,
                CreadoPor = movimiento.CreatedBy ?? "Sistema",
                ModificadoPor = movimiento.LastModifiedBy ?? "Sistema",
                Activo = !movimiento.EstaEliminado,
                ColorTipo = movimiento.TipoMovimiento == Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso ? "success" : "danger",
                IconoTipo = movimiento.TipoMovimiento == Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso ? "arrow-up" : "arrow-down",
                FechaTexto = movimiento.Fecha.ToString("dd/MM/yyyy HH:mm"),
                CantidadTexto = $"{movimiento.Cantidad} {ingrediente.UnidadMedida}"
            };

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            return Result.Failure<MovimientoInventarioDto>($"Error al obtener movimiento: {ex.Message}");
        }
    }
} 