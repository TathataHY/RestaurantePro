namespace RestaurantePro.Application.Core.Productos.Queries.CalcularCostoReceta;

/// <summary>
/// Query para calcular el costo total de ingredientes de una receta
/// </summary>
public class CalcularCostoRecetaQuery : IRequest<Result<decimal>>
{
    /// <summary>
    /// ID de la receta
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Cantidad de porciones para calcular el costo
    /// </summary>
    public int CantidadPorciones { get; set; } = 1;

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public CalcularCostoRecetaQuery() { }

    /// <summary>
    /// Constructor con ID de receta
    /// </summary>
    public CalcularCostoRecetaQuery(Guid id, int cantidadPorciones = 1)
    {
        Id = id;
        CantidadPorciones = cantidadPorciones;
    }
} 