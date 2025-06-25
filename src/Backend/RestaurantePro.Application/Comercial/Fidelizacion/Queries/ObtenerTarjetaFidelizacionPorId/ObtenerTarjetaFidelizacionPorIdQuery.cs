namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerTarjetaFidelizacionPorId;

/// <summary>
/// Query para obtener una tarjeta de fidelización específica por ID
/// </summary>
public class ObtenerTarjetaFidelizacionPorIdQuery : IRequest<Result<TarjetaFidelizacionDto>>
{
    /// <summary>
    /// ID de la tarjeta de fidelización
    /// </summary>
    public Guid Id { get; set; }
} 