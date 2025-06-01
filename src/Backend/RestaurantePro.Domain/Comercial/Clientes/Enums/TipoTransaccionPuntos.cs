namespace RestaurantePro.Domain.Comercial.Clientes.Enums;

/// <summary>
/// Tipos de transacción de puntos
/// </summary>
public enum TipoTransaccionPuntos
{
    /// <summary>
    /// Acumulación por compra
    /// </summary>
    AcumulacionCompra = 1,

    /// <summary>
    /// Acumulación por promoción
    /// </summary>
    AcumulacionPromocion = 2,

    /// <summary>
    /// Acumulación manual
    /// </summary>
    AcumulacionManual = 3,

    /// <summary>
    /// Canje de puntos
    /// </summary>
    Canje = 4,

    /// <summary>
    /// Ajuste positivo
    /// </summary>
    AjustePositivo = 5,

    /// <summary>
    /// Ajuste negativo
    /// </summary>
    AjusteNegativo = 6,

    /// <summary>
    /// Vencimiento de puntos
    /// </summary>
    Vencimiento = 7,

    /// <summary>
    /// Transferencia entre tarjetas
    /// </summary>
    Transferencia = 8
} 