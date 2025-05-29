using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO completo para la entidad Comanda
/// Incluye toda la información de la comanda con propiedades calculadas
/// </summary>
public class ComandaDto : BaseDto
{
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
    /// Estado de la comanda como enum string para facilitar el frontend
    /// </summary>
    public string EstadoTexto => Estado.ToString();

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
    public List<ItemComandaDto> Items { get; set; } = new();

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
} 