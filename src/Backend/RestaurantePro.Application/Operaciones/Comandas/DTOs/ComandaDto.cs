namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO completo para la entidad Comanda
/// Incluye toda la información de la comanda con propiedades calculadas
/// </summary>
public class ComandaDto : BaseDto
{
    /// <summary>
    /// Tipo de comanda (Mesa, Delivery, TakeAway)
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// ID de la mesa asociada a la comanda
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// Número de la mesa (si está disponible)
    /// </summary>
    public int NumeroMesa { get; set; }

    /// <summary>
    /// ID del mesero responsable de la comanda
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Nombre del mesero (si está disponible)
    /// </summary>
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>
    /// ID del cliente asociado (opcional)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente (si está disponible)
    /// </summary>
    public string? NombreCliente { get; set; }

    /// <summary>
    /// Estado actual de la comanda
    /// </summary>
    public EstadoComanda Estado { get; set; }

    /// <summary>
    /// Estado de la comanda en texto amigable para el usuario
    /// </summary>
    public string EstadoTexto { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de apertura de la comanda
    /// </summary>
    public DateTime FechaApertura { get; set; }

    /// <summary>
    /// Fecha de cierre de la comanda
    /// </summary>
    public DateTime? FechaCierre { get; set; }

    /// <summary>
    /// Observaciones de la comanda
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Lista de items de la comanda
    /// </summary>
    public List<ComandaProductoDto> Items { get; set; } = new();

    /// <summary>
    /// Subtotal de la comanda (sin impuestos ni descuentos)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Impuestos aplicados
    /// </summary>
    public decimal Impuestos { get; set; }

    /// <summary>
    /// Descuentos aplicados
    /// </summary>
    public decimal Descuentos { get; set; }

    /// <summary>
    /// Total final de la comanda
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Cantidad total de items en la comanda
    /// </summary>
    public int CantidadItems => Items.Count;

    /// <summary>
    /// Indica si la comanda tiene descuentos
    /// </summary>
    public bool TieneDescuentos => Descuentos > 0;

    /// <summary>
    /// Tiempo transcurrido desde la apertura de la comanda
    /// </summary>
    public TimeSpan? TiempoAbierta => FechaCierre.HasValue 
        ? FechaCierre.Value - FechaApertura 
        : DateTime.Now - FechaApertura;

    // 🔥 PROPIEDADES BÁSICAS AGREGADAS para mejor UX
    /// <summary>
    /// Estados calculados para facilitar el frontend
    /// </summary>
    public bool EstaCreada => Estado == EstadoComanda.Creada;
    public bool EstaEnProceso => Estado == EstadoComanda.EnProceso;
    public bool EstaLista => Estado == EstadoComanda.Lista;
    public bool EstaEntregada => Estado == EstadoComanda.Entregada;
    public bool EstaFinalizada => Estado == EstadoComanda.Finalizada;
    public bool EstaCancelada => Estado == EstadoComanda.Cancelada;
    
    /// <summary>
    /// Tiempo de preparación estimado en minutos
    /// </summary>
    public int TiempoPreparacionMinutos => Items.Count > 0 ? Items.Count * 5 : 15; // 5 min por item

    /// <summary>
    /// Tiempo formateado para mostrar en UI
    /// </summary>
    public string TiempoAbiertaTexto => TiempoAbierta?.ToString(@"hh\:mm") ?? "00:00";

    /// <summary>
    /// Número de comanda formateado
    /// </summary>
    public string NumeroComanda { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la persona que recibe la entrega (solo para Delivery)
    /// </summary>
    public string? NombreEntrega { get; set; }

    /// <summary>
    /// Dirección de entrega (solo para Delivery)
    /// </summary>
    public string? DireccionEntrega { get; set; }

    /// <summary>
    /// Teléfono de contacto para la entrega (solo para Delivery)
    /// </summary>
    public string? TelefonoEntrega { get; set; }

    /// <summary>
    /// Mesa formateada para mostrar
    /// </summary>
    public string MesaTexto => $"Mesa {NumeroMesa}";
} 