namespace RestaurantePro.Domain.Operaciones.Preparaciones.Events;

/// <summary>
/// Evento que se dispara cuando una preparación se agota completamente
/// </summary>
public class PreparacionAgotada : DomainEvent
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
    /// Constructor del evento
    /// </summary>
    public PreparacionAgotada(Guid preparacionId, Guid productoId)
    {
        PreparacionId = preparacionId;
        ProductoId = productoId;
    }
} 