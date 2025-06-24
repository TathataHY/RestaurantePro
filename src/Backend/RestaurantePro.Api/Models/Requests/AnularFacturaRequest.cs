namespace RestaurantePro.Api.Models.Requests;

/// <summary>
/// DTO para solicitar la anulación de una factura
/// </summary>
public class AnularFacturaRequest
{
    /// <summary>
    /// Motivo de la anulación (mínimo 10 caracteres)
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del motivo (opcional)
    /// </summary>
    public string? DescripcionDetallada { get; set; }

    /// <summary>
    /// Tipo de anulación: Normal, Emergencia, Administrativa, Devolución, SolicitudCliente, Programada
    /// </summary>
    public string TipoAnulacion { get; set; } = "Normal";

    /// <summary>
    /// Observaciones adicionales (opcional)
    /// </summary>
    public string? ObservacionesAdicionales { get; set; }

    /// <summary>
    /// Indica si se debe generar nota de crédito automáticamente
    /// </summary>
    public bool GenerarNotaCredito { get; set; } = false;

    /// <summary>
    /// Indica si se debe notificar al cliente sobre la anulación
    /// </summary>
    public bool NotificarCliente { get; set; } = true;

    /// <summary>
    /// Indica si se debe procesar devolución automática del pago
    /// </summary>
    public bool ProcesarDevolucionPago { get; set; } = false;

    /// <summary>
    /// Método de devolución: Efectivo, Tarjeta, Transferencia, SaldoFavor
    /// </summary>
    public string? MetodoDevolucion { get; set; }

    /// <summary>
    /// Indica si se debe revertir movimientos de inventario
    /// </summary>
    public bool RevertirInventario { get; set; } = true;

    /// <summary>
    /// Indica si se debe cancelar puntos de fidelización otorgados
    /// </summary>
    public bool CancelarPuntosFidelizacion { get; set; } = true;
} 