using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Commands.ActualizarMovimiento;

public class ActualizarMovimientoCommandHandler : IRequestHandler<ActualizarMovimientoCommand, Result<MovimientoInventarioDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ActualizarMovimientoCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<MovimientoInventarioDto>> Handle(ActualizarMovimientoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[ActualizarMovimiento] Buscando ingrediente con movimiento ID: {request.Id}");
            var ingrediente = await _context.Ingredientes
                .Include(i => i.Movimientos)
                .FirstOrDefaultAsync(i => i.Movimientos.Any(m => m.Id == request.Id), cancellationToken);
            Console.WriteLine($"[ActualizarMovimiento] Ingrediente encontrado: {(ingrediente != null ? ingrediente.Id.ToString() : "null")}");

            if (ingrediente == null)
            {
                Console.WriteLine("[ActualizarMovimiento] Movimiento no encontrado en ningún ingrediente.");
                return Result.Failure<MovimientoInventarioDto>("Movimiento no encontrado");
            }

            var movimiento = ingrediente.Movimientos.FirstOrDefault(m => m.Id == request.Id);
            Console.WriteLine($"[ActualizarMovimiento] Movimiento encontrado: {(movimiento != null ? movimiento.Id.ToString() : "null")}");
            if (movimiento == null)
            {
                Console.WriteLine("[ActualizarMovimiento] Movimiento no encontrado en la colección de movimientos.");
                return Result.Failure<MovimientoInventarioDto>("Movimiento no encontrado");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId, cancellationToken);
            Console.WriteLine($"[ActualizarMovimiento] Usuario encontrado: {(usuario != null ? usuario.Id.ToString() : "null")}");
            if (usuario == null)
            {
                Console.WriteLine("[ActualizarMovimiento] Usuario no encontrado.");
                return Result.Failure<MovimientoInventarioDto>("Usuario no encontrado");
            }

            if (request.Cantidad.HasValue && request.Cantidad.Value > 0)
            {
                // Calcular la diferencia para ajustar el stock
                var diferenciaCantidad = request.Cantidad.Value - movimiento.Cantidad;
                
                // Actualizar la cantidad del movimiento
                movimiento.ActualizarCantidad(request.Cantidad.Value);
                
                // Ajustar el stock del ingrediente usando métodos públicos
                if (diferenciaCantidad > 0)
                {
                    // Incremento: usar el método público para incrementar stock
                    ingrediente.IncrementarStock(diferenciaCantidad, "Ajuste por actualización de movimiento");
                }
                else if (diferenciaCantidad < 0)
                {
                    // Decremento: usar el método público para decrementar stock
                    var cantidadADecrementar = Math.Abs(diferenciaCantidad);
                    if (ingrediente.Stock < cantidadADecrementar)
                    {
                        return Result.Failure<MovimientoInventarioDto>("No hay suficiente stock para realizar el ajuste");
                    }
                    ingrediente.DecrementarStock(cantidadADecrementar, "Ajuste por actualización de movimiento");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Motivo))
            {
                movimiento.ActualizarMotivo(request.Motivo);
            }

            Console.WriteLine("[ActualizarMovimiento] Guardando cambios...");
            await _context.SaveChangesAsync(cancellationToken);
            Console.WriteLine("[ActualizarMovimiento] Cambios guardados correctamente.");

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
                UsuarioId = request.UsuarioId,
                NombreUsuario = usuario.NombreCompleto ?? "Usuario",
                NumeroDocumento = null,
                ProveedorId = null, // No incluimos proveedor para evitar Include
                NombreProveedor = "",
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
            return Result.Failure<MovimientoInventarioDto>($"Error al actualizar movimiento: {ex.Message}");
        }
    }
} 