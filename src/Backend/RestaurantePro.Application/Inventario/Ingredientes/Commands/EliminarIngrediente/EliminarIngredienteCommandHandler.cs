using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.EliminarIngrediente;

public class EliminarIngredienteCommandHandler : IRequestHandler<EliminarIngredienteCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public EliminarIngredienteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(EliminarIngredienteCommand request, CancellationToken cancellationToken)
    {
        // Cargar el ingrediente con la colección de movimientos para que EF Core pueda trackear los cambios
        var ingrediente = await _context.Ingredientes
            .Include(i => i.Movimientos)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (ingrediente == null)
        {
            return Result.Failure<bool>("Ingrediente no encontrado");
        }

        try
        {
            // Usar el método de dominio para desactivar el ingrediente
            ingrediente.Desactivar();
            
            // No usar Update() explícitamente, confiar en el tracking automático de EF Core
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Error al eliminar ingrediente: {ex.Message}");
        }
    }
} 