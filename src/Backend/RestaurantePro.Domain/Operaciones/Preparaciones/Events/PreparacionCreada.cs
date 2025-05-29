namespace RestaurantePro.Domain.Operaciones.Preparaciones.Events;

/// <summary>
/// Evento que se dispara cuando se crea una nueva preparación diaria
/// </summary>
public class PreparacionCreada : DomainEvent
{
    /// <summary>
    /// ID de la preparación creada
    /// </summary>
    public Guid PreparacionId { get; }
    
    /// <summary>
    /// ID del producto preparado
    /// </summary>
    public Guid ProductoId { get; }
    
    /// <summary>
    /// Cantidad preparada
    /// </summary>
    public int CantidadPreparada { get; }
    
    /// <summary>
    /// ID del chef que realizó la preparación
    /// </summary>
    public Guid ChefId { get; }

    /// <summary>
    /// Constructor del evento
    /// </summary>
    public PreparacionCreada(Guid preparacionId, Guid productoId, int cantidadPreparada, Guid chefId)
    {
        PreparacionId = preparacionId;
        ProductoId = productoId;
        CantidadPreparada = cantidadPreparada;
        ChefId = chefId;
    }
} 