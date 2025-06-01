namespace RestaurantePro.Application.Core.Productos.Queries.VerificarDisponibilidadProducto;

/// <summary>
/// Query para verificar disponibilidad de un producto
/// </summary>
public class VerificarDisponibilidadProductoQuery : IRequest<RestaurantePro.Domain.Core.SharedKernel.Results.Result<DisponibilidadProductoDto>>
{
    /// <summary>
    /// ID del producto a verificar
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad solicitada del producto
    /// </summary>
    public int CantidadSolicitada { get; set; } = 1;

    /// <summary>
    /// Constructor para crear query de verificación
    /// </summary>
    public VerificarDisponibilidadProductoQuery(Guid productoId, int cantidadSolicitada = 1)
    {
        ProductoId = productoId;
        CantidadSolicitada = cantidadSolicitada;
    }

    /// <summary>
    /// Constructor sin parámetros para serialización
    /// </summary>
    public VerificarDisponibilidadProductoQuery() { }
} 