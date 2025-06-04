namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda;

/// <summary>
/// Evento de dominio que se dispara cuando una comanda es transferida de una mesa a otra
/// </summary>
public class ComandaTransferida : DomainEvent
{
    /// <summary>
    /// ID de la comanda transferida
    /// </summary>
    public Guid ComandaId { get; }
    
    /// <summary>
    /// ID de la mesa de origen
    /// </summary>
    public Guid MesaOrigenId { get; }
    
    /// <summary>
    /// ID de la mesa de destino
    /// </summary>
    public Guid MesaDestinoId { get; }
    
    /// <summary>
    /// Constructor para el evento de comanda transferida
    /// </summary>
    /// <param name="comandaId">ID de la comanda transferida</param>
    /// <param name="mesaOrigenId">ID de la mesa de origen</param>
    /// <param name="mesaDestinoId">ID de la mesa de destino</param>
    public ComandaTransferida(Guid comandaId, Guid mesaOrigenId, Guid mesaDestinoId)
    {
        ComandaId = comandaId;
        MesaOrigenId = mesaOrigenId;
        MesaDestinoId = mesaDestinoId;
    }
} 