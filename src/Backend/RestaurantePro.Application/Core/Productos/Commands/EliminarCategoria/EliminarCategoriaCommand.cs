using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Core.Productos.Commands.EliminarCategoria;

/// <summary>
/// Command para eliminar una categoría de productos
/// Vertical Slice: EliminarCategoria
/// </summary>
public class EliminarCategoriaCommand : IRequest<Result>
{
    /// <summary>
    /// Identificador único de la categoría a eliminar
    /// </summary>
    public Guid Id { get; set; }
}
