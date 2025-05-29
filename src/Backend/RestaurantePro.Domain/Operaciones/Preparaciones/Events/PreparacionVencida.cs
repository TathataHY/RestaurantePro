namespace RestaurantePro.Domain.Operaciones.Preparaciones.Events;

/// <summary>
/// Evento que se dispara cuando una preparación vence
/// </summary>
public class PreparacionVencida : DomainEvent
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
    /// Cantidad desperdiciada
    /// </summary>
    public int CantidadDesperdiciada { get; }

    /// <summary>
    /// Constructor del evento
    /// </summary>
    public PreparacionVencida(Guid preparacionId, Guid productoId, int cantidadDesperdiciada)
    {
        PreparacionId = preparacionId;
        ProductoId = productoId;
        CantidadDesperdiciada = cantidadDesperdiciada;
    }
}

/// <summary>
/// Evento que se dispara cuando una preparación está por vencer
/// </summary>
public class PreparacionPorVencer : DomainEvent
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
    /// Fecha de vencimiento
    /// </summary>
    public DateTime? FechaVencimiento { get; }

    /// <summary>
    /// Constructor del evento
    /// </summary>
    public PreparacionPorVencer(Guid preparacionId, Guid productoId, DateTime? fechaVencimiento)
    {
        PreparacionId = preparacionId;
        ProductoId = productoId;
        FechaVencimiento = fechaVencimiento;
    }
}

/// <summary>
/// Evento que se dispara cuando se agrega cantidad adicional a una preparación
/// </summary>
public class CantidadAgregadaAPreparacion : DomainEvent
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
    /// Cantidad agregada
    /// </summary>
    public int CantidadAgregada { get; }
    
    /// <summary>
    /// Cantidad anterior
    /// </summary>
    public int CantidadAnterior { get; }
    
    /// <summary>
    /// Cantidad total después de agregar
    /// </summary>
    public int CantidadTotal { get; }

    /// <summary>
    /// Constructor del evento
    /// </summary>
    public CantidadAgregadaAPreparacion(Guid preparacionId, Guid productoId, int cantidadAgregada, int cantidadAnterior, int cantidadTotal)
    {
        PreparacionId = preparacionId;
        ProductoId = productoId;
        CantidadAgregada = cantidadAgregada;
        CantidadAnterior = cantidadAnterior;
        CantidadTotal = cantidadTotal;
    }
} 