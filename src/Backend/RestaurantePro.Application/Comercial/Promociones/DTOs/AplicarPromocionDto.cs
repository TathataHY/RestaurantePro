namespace RestaurantePro.Application.Comercial.Promociones.DTOs;

/// <summary>
/// DTO de respuesta para la aplicación de promoción
/// </summary>
public class AplicarPromocionDto
{
    public Guid PromocionId { get; set; }
    public string CodigoPromocion { get; set; } = string.Empty;
    public string NombrePromocion { get; set; } = string.Empty;
    public TipoAplicacionPromocion TipoAplicacion { get; set; }
    public Guid? FacturaId { get; set; }
    public Guid? ComandaId { get; set; }
    public decimal MontoDescuento { get; set; }
    public decimal PorcentajeDescuento { get; set; }
    public DateTime FechaAplicacion { get; set; }
    public Guid? AutorizadoPor { get; set; }
    public bool AplicacionExitosa { get; set; }
    public List<Guid> ProductosAfectados { get; set; } = new();
    public string? MensajeResultado { get; set; }
}

/// <summary>
/// Tipos de aplicación de promoción
/// </summary>
public enum TipoAplicacionPromocion
{
    /// <summary>
    /// Aplicar a toda la factura
    /// </summary>
    FacturaCompleta = 1,

    /// <summary>
    /// Aplicar a productos específicos
    /// </summary>
    ProductosEspecificos = 2,

    /// <summary>
    /// Aplicar por categoría de productos
    /// </summary>
    PorCategoria = 3,

    /// <summary>
    /// Aplicar por cantidad mínima
    /// </summary>
    PorCantidadMinima = 4,

    /// <summary>
    /// Aplicar por monto mínimo
    /// </summary>
    PorMontoMinimo = 5
} 