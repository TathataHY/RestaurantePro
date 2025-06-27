using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarMovimiento;

public class RegistrarMovimientoCommandHandler : IRequestHandler<RegistrarMovimientoCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public RegistrarMovimientoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(RegistrarMovimientoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ingrediente = await _context.Ingredientes
                .Include(i => i.Movimientos)
                .FirstOrDefaultAsync(i => i.Id == request.IngredienteId, cancellationToken);

            if (ingrediente == null)
            {
                return Result.Failure<Guid>("Ingrediente no encontrado");
            }

            // Aplicar el movimiento según el tipo
            MovimientoInventario movimiento;
            if (request.TipoMovimiento == TipoMovimientoInventario.Ingreso)
            {
                movimiento = ingrediente.IncrementarStock(request.Cantidad, request.Motivo, request.Fecha);
            }
            else if (request.TipoMovimiento == TipoMovimientoInventario.Egreso)
            {
                movimiento = ingrediente.DecrementarStock(request.Cantidad, request.Motivo, request.Fecha);
            }
            else
            {
                return Result.Failure<Guid>("Tipo de movimiento no válido");
            }

            // Agregar explícitamente el movimiento al contexto para que EF Core lo rastree
            _context.MovimientosInventario.Add(movimiento);

            // Forzar la detección de cambios en EF Core
            if (_context is DbContext dbContext)
            {
                dbContext.ChangeTracker.DetectChanges();
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(movimiento.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>($"Error al registrar movimiento: {ex.Message}");
        }
    }
} 