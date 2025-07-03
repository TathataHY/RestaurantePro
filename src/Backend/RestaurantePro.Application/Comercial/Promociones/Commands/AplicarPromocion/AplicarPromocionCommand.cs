namespace RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;

/// <summary>
/// Command para aplicar una promoción a una factura o comanda
/// Permite aplicar descuentos promocionales con validaciones de negocio
/// </summary>
public class AplicarPromocionCommand : IRequest<Result<AplicarPromocionDto>>
{
    /// <summary>
    /// ID de la promoción a aplicar
    /// </summary>
    public Guid PromocionId { get; set; }

    /// <summary>
    /// Código de promoción (alternativo al ID)
    /// </summary>
    public string? CodigoPromocion { get; set; }

    /// <summary>
    /// ID de la factura donde aplicar la promoción
    /// </summary>
    public Guid? FacturaId { get; set; }

    /// <summary>
    /// ID de la comanda donde aplicar la promoción
    /// </summary>
    public Guid? ComandaId { get; set; }

    /// <summary>
    /// IDs de productos específicos para la promoción
    /// </summary>
    public List<Guid>? ProductosIds { get; set; }

    /// <summary>
    /// ID del cliente que aplica la promoción
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Monto original de la compra para calcular el descuento
    /// </summary>
    public decimal MontoOriginal { get; set; }

    /// <summary>
    /// Tipo de aplicación de la promoción
    /// </summary>
    public TipoAplicacionPromocion TipoAplicacion { get; set; }

    /// <summary>
    /// Usuario que autoriza la aplicación
    /// </summary>
    public Guid? AutorizadoPor { get; set; }

    /// <summary>
    /// Notas adicionales sobre la aplicación
    /// </summary>
    public string? NotasAplicacion { get; set; }

    /// <summary>
    /// Indica si se debe validar restricciones de cliente
    /// </summary>
    public bool ValidarRestriccionesCliente { get; set; } = true;

    /// <summary>
    /// Indica si se debe validar límites de uso
    /// </summary>
    public bool ValidarLimitesUso { get; set; } = true;

    /// <summary>
    /// Datos adicionales de la aplicación
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    public AplicarPromocionCommand(Guid promocionId, TipoAplicacionPromocion tipoAplicacion)
    {
        PromocionId = promocionId;
        TipoAplicacion = tipoAplicacion;
    }

    public AplicarPromocionCommand(string codigoPromocion, TipoAplicacionPromocion tipoAplicacion)
    {
        CodigoPromocion = codigoPromocion;
        TipoAplicacion = tipoAplicacion;
    }

    public AplicarPromocionCommand() { }
}
