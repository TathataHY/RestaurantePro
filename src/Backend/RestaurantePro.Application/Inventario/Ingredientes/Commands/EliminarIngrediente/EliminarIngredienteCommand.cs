using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.EliminarIngrediente;

public record EliminarIngredienteCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
} 