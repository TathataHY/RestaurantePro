namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

using RestaurantePro.Domain.Comercial.Clientes.Enums;

/// <summary>
/// Representa un movimiento de puntos para reportes
/// </summary>
public class MovimientoPuntosDto
{
    /// <summary>
    /// ID del movimiento
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Fecha del movimiento
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Tipo de movimiento
    /// </summary>
    public TipoMovimientoPuntos Tipo { get; set; }

    /// <summary>
    /// Descripción del movimiento
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Puntos del movimiento (positivo para acumulación, negativo para canje)
    /// </summary>
    public int Puntos { get; set; }

    /// <summary>
    /// Saldo después del movimiento
    /// </summary>
    public int SaldoResultante { get; set; }

    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// ID de la tarjeta de fidelización
    /// </summary>
    public Guid TarjetaFidelizacionId { get; set; }

    /// <summary>
    /// Número de la tarjeta
    /// </summary>
    public string NumeroTarjeta { get; set; } = string.Empty;

    /// <summary>
    /// Monto asociado (si aplica)
    /// </summary>
    public decimal? MontoAsociado { get; set; }

    /// <summary>
    /// Referencia externa (factura, promoción, etc.)
    /// </summary>
    public string? ReferenciaExterna { get; set; }

    /// <summary>
    /// Estado del movimiento
    /// </summary>
    public EstadoTransaccionPuntos Estado { get; set; }

    /// <summary>
    /// Usuario que procesó el movimiento
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Nombre del usuario
    /// </summary>
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }

    // Propiedades calculadas para UI
    /// <summary>
    /// Texto descriptivo del tipo de movimiento
    /// </summary>
    public string TipoTexto => Tipo switch
    {
        TipoMovimientoPuntos.AcumulacionCompra => "Acumulación por Compra",
        TipoMovimientoPuntos.AcumulacionPromocion => "Acumulación por Promoción",
        TipoMovimientoPuntos.AcumulacionManual => "Acumulación Manual",
        TipoMovimientoPuntos.Canje => "Canje de Puntos",
        TipoMovimientoPuntos.AjustePositivo => "Ajuste Positivo",
        TipoMovimientoPuntos.AjusteNegativo => "Ajuste Negativo",
        TipoMovimientoPuntos.Vencimiento => "Vencimiento",
        TipoMovimientoPuntos.Transferencia => "Transferencia",
        TipoMovimientoPuntos.BonoInicial => "Bono de Bienvenida",
        TipoMovimientoPuntos.BonoCumpleanos => "Bono de Cumpleaños",
        TipoMovimientoPuntos.BonoRegistro => "Bono de Registro",
        _ => "Desconocido"
    };

    /// <summary>
    /// Color para mostrar en UI según el tipo
    /// </summary>
    public string Color => Puntos >= 0 ? "success" : "warning";

    /// <summary>
    /// Icono para mostrar en UI según el tipo
    /// </summary>
    public string Icono => Tipo switch
    {
        TipoMovimientoPuntos.AcumulacionCompra => "fas fa-plus-circle",
        TipoMovimientoPuntos.AcumulacionPromocion => "fas fa-gift",
        TipoMovimientoPuntos.AcumulacionManual => "fas fa-edit",
        TipoMovimientoPuntos.Canje => "fas fa-exchange-alt",
        TipoMovimientoPuntos.AjustePositivo => "fas fa-arrow-up",
        TipoMovimientoPuntos.AjusteNegativo => "fas fa-arrow-down",
        TipoMovimientoPuntos.Vencimiento => "fas fa-clock",
        TipoMovimientoPuntos.Transferencia => "fas fa-transfer",
        TipoMovimientoPuntos.BonoInicial => "fas fa-star",
        TipoMovimientoPuntos.BonoCumpleanos => "fas fa-birthday-cake",
        TipoMovimientoPuntos.BonoRegistro => "fas fa-user-plus",
        _ => "fas fa-question-circle"
    };

    /// <summary>
    /// Puntos formateados para mostrar con signo
    /// </summary>
    public string PuntosFormateados => Puntos >= 0 ? $"+{Puntos:N0}" : Puntos.ToString("N0");
} 