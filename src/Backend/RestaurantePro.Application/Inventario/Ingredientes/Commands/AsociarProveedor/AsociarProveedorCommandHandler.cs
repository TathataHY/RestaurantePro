using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.AsociarProveedor;

public class AsociarProveedorCommandHandler : IRequestHandler<AsociarProveedorCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public AsociarProveedorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(AsociarProveedorCommand request, CancellationToken cancellationToken)
    {
        // Cargar el ingrediente con la colección de movimientos para que EF Core pueda trackear los cambios
        var ingrediente = await _context.Ingredientes
            .Include(i => i.Movimientos)
            .FirstOrDefaultAsync(i => i.Id == request.IngredienteId, cancellationToken);
        if (ingrediente == null)
        {
            return Result.Failure<bool>("Ingrediente no encontrado");
        }

        // Verificar que el proveedor existe
        var proveedor = await _context.Proveedores
            .FirstOrDefaultAsync(p => p.Id == request.ProveedorId, cancellationToken);
        if (proveedor == null)
        {
            return Result.Failure<bool>("Proveedor no encontrado");
        }

        try
        {
            // Usar el método de dominio para asociar el proveedor
            ingrediente.AsociarProveedorPrincipal(request.ProveedorId);
            
            // No usar Update() explícitamente, confiar en el tracking automático de EF Core
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success<bool>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Error al asociar proveedor: {ex.Message}");
        }
    }
} 