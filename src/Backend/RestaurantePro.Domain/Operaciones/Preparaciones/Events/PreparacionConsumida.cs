namespace RestaurantePro.Domain.Operaciones.Preparaciones.Events;

/// <summary>
/// Evento que se dispara cuando se consume una cantidad de una preparación
/// </summary>
public class PreparacionConsumida : DomainEvent
{
    /// <summary>
    /// ID de la preparación
    /// </summary>
    public Guid PreparacionId { get; }
    
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; }
    
    /// <summary>
    /// Cantidad consumida
    /// </summary>
    public int CantidadConsumida { get; }
    
    /// <summary>
    /// Cantidad restante después del consumo
    /// </summary>
    public int CantidadRestante { get; }

    /// <summary>
    /// Constructor del evento
    /// </summary>
    public PreparacionConsumida(Guid preparacionId, Guid productoId, int cantidadConsumida, int cantidadRestante)
    {
        PreparacionId = preparacionId;
        ProductoId = productoId;
        CantidadConsumida = cantidadConsumida;
        CantidadRestante = cantidadRestante;
    }
} 