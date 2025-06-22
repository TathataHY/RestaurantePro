using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarMovimiento;

public class RegistrarMovimientoCommandHandler : IRequestHandler<RegistrarMovimientoCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public RegistrarMovimientoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(RegistrarMovimientoCommand request, CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                // Cargar el ingrediente con la colección de movimientos para que EF Core pueda trackear los cambios
                var ingrediente = await _context.Ingredientes
                    .Include(i => i.Movimientos)
                    .FirstOrDefaultAsync(i => i.Id == request.IngredienteId, cancellationToken);

                if (ingrediente == null)
                {
                    return Result.Failure<bool>("Ingrediente no encontrado");
                }

                // Usar los métodos de dominio según el tipo de movimiento
                if (request.TipoMovimiento == TipoMovimientoInventario.Ingreso)
                {
                    ingrediente.IncrementarStock(request.Cantidad, request.Motivo);
                }
                else if (request.TipoMovimiento == TipoMovimientoInventario.Egreso)
                {
                    ingrediente.DecrementarStock(request.Cantidad, request.Motivo);
                }
                else
                {
                    return Result.Failure<bool>("Tipo de movimiento no válido");
                }

                // Guardar cambios con manejo de concurrencia
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success(true);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                retryCount++;
                
                if (retryCount >= maxRetries)
                {
                    return Result.Failure<bool>($"Error de concurrencia al registrar movimiento después de {maxRetries} intentos: {ex.Message}");
                }
                
                // Esperar un poco antes de reintentar
                await Task.Delay(100 * retryCount, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Error al registrar movimiento: {ex.Message}");
            }
        }

        return Result.Failure<bool>("Error inesperado al registrar movimiento");
    }
} 