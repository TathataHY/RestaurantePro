namespace RestaurantePro.Application.Comercial.Promociones.DTOs;

/// <summary>
/// DTO de request para aplicar una promoción
/// </summary>
public class AplicarPromocionRequest
{
    /// <summary>
    /// ID de la promoción a aplicar
    /// </summary>
    public Guid PromocionId { get; set; }

    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// ID de la comanda (opcional)
    /// </summary>
    public Guid? ComandaId { get; set; }

    /// <summary>
    /// ID de la factura (opcional)
    /// </summary>
    public Guid? FacturaId { get; set; }

    /// <summary>
    /// Monto original de la compra
    /// </summary>
    public decimal MontoOriginal { get; set; }

    /// <summary>
    /// IDs de los productos específicos (opcional)
    /// </summary>
    public List<Guid>? ProductosIds { get; set; }
} 