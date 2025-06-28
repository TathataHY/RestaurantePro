using RestaurantePro.Application.Core.Productos.DTOs;

namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerRecetaPorId;

/// <summary>
/// Query para obtener una receta específica por su ID
/// </summary>
public class ObtenerRecetaPorIdQuery : IRequest<Result<RecetaDto>>
{
    /// <summary>
    /// ID de la receta a obtener
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Incluir información de ingredientes
    /// </summary>
    public bool IncluirIngredientes { get; set; } = true;

    /// <summary>
    /// Incluir información del producto
    /// </summary>
    public bool IncluirProducto { get; set; } = true;

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public ObtenerRecetaPorIdQuery() { }

    /// <summary>
    /// Constructor con ID
    /// </summary>
    public ObtenerRecetaPorIdQuery(Guid id)
    {
        Id = id;
    }
} 