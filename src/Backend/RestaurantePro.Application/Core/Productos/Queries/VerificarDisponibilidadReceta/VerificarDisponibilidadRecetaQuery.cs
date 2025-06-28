using RestaurantePro.Application.Core.Productos.DTOs;

namespace RestaurantePro.Application.Core.Productos.Queries.VerificarDisponibilidadReceta;

/// <summary>
/// Query para verificar disponibilidad de ingredientes para una receta
/// </summary>
public class VerificarDisponibilidadRecetaQuery : IRequest<Result<DisponibilidadRecetaDto>>
{
    /// <summary>
    /// ID de la receta
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Cantidad de porciones a verificar
    /// </summary>
    public int Cantidad { get; set; } = 1;

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public VerificarDisponibilidadRecetaQuery() { }

    /// <summary>
    /// Constructor con ID de receta y cantidad
    /// </summary>
    public VerificarDisponibilidadRecetaQuery(Guid id, int cantidad = 1)
    {
        Id = id;
        Cantidad = cantidad;
    }
} 