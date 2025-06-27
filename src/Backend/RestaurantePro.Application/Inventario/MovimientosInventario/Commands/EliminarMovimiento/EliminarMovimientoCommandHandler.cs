using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Commands.EliminarMovimiento;

public class EliminarMovimientoCommandHandler : IRequestHandler<EliminarMovimientoCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public EliminarMovimientoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(EliminarMovimientoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[EliminarMovimiento] Buscando ingrediente con movimiento ID: {request.Id}");
            var ingrediente = await _context.Ingredientes
                .Include(i => i.Movimientos)
                .FirstOrDefaultAsync(i => i.Movimientos.Any(m => m.Id == request.Id), cancellationToken);
            Console.WriteLine($"[EliminarMovimiento] Ingrediente encontrado: {(ingrediente != null ? ingrediente.Id.ToString() : "null")}");

            if (ingrediente == null)
            {
                Console.WriteLine("[EliminarMovimiento] Movimiento no encontrado en ningún ingrediente.");
                return Result.Failure<bool>("Movimiento no encontrado");
            }

            var movimiento = ingrediente.Movimientos.FirstOrDefault(m => m.Id == request.Id);
            Console.WriteLine($"[EliminarMovimiento] Movimiento encontrado: {(movimiento != null ? movimiento.Id.ToString() : "null")}");
            if (movimiento == null)
            {
                Console.WriteLine("[EliminarMovimiento] Movimiento no encontrado en la colección de movimientos.");
                return Result.Failure<bool>("Movimiento no encontrado");
            }

            // Verificar si el movimiento ya fue aplicado
            if (movimiento.EstaAplicado)
            {
                Console.WriteLine("[EliminarMovimiento] Movimiento ya aplicado, no se puede eliminar.");
                return Result.Failure<bool>("No se puede eliminar un movimiento que ya fue aplicado al stock");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == request.UsuarioId, cancellationToken);
            Console.WriteLine($"[EliminarMovimiento] Usuario encontrado: {(usuario != null ? usuario.Id.ToString() : "null")}");
            if (usuario == null)
            {
                Console.WriteLine("[EliminarMovimiento] Usuario no encontrado.");
                return Result.Failure<bool>("Usuario no encontrado");
            }

            // Usar soft delete directo en lugar de MarcarComoEliminado
            movimiento.MarkAsDeleted();

            Console.WriteLine("[EliminarMovimiento] Guardando cambios...");
            await _context.SaveChangesAsync(cancellationToken);
            Console.WriteLine("[EliminarMovimiento] Cambios guardados correctamente.");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Error al eliminar movimiento: {ex.Message}");
        }
    }
} 