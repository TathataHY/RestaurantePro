using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarIngrediente;

public class ActualizarIngredienteCommandHandler : IRequestHandler<ActualizarIngredienteCommand, Result<IngredienteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ActualizarIngredienteCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IngredienteDto>> Handle(ActualizarIngredienteCommand request, CancellationToken cancellationToken)
    {
        // Cargar el ingrediente con la colección de movimientos para que EF Core pueda trackear los cambios
        var ingrediente = await _context.Ingredientes
            .Include(i => i.Movimientos)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (ingrediente == null)
        {
            return Result.Failure<IngredienteDto>("Ingrediente no encontrado");
        }

        try
        {
            // Usar los métodos de dominio para actualizar las propiedades
            if (!string.IsNullOrWhiteSpace(request.Nombre) && request.Nombre != ingrediente.Nombre)
                ingrediente.ActualizarNombre(request.Nombre);
                
            if (!string.IsNullOrWhiteSpace(request.Descripcion) && request.Descripcion != ingrediente.Descripcion)
                ingrediente.ActualizarDescripcion(request.Descripcion);
                
            if (request.StockMinimo != ingrediente.StockMinimo)
                ingrediente.ActualizarStockMinimo(request.StockMinimo);
                
            if (request.CostoPromedio != ingrediente.CostoPromedio)
                ingrediente.ActualizarCostoPromedio(request.CostoPromedio);

            // No usar Update() explícitamente, confiar en el tracking automático de EF Core
            await _context.SaveChangesAsync(cancellationToken);
            
            var ingredienteDto = _mapper.Map<IngredienteDto>(ingrediente);
            return Result.Success(ingredienteDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<IngredienteDto>($"Error al actualizar ingrediente: {ex.Message}");
        }
    }
} 