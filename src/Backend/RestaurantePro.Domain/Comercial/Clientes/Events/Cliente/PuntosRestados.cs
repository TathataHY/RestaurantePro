namespace RestaurantePro.Domain.Comercial.Clientes.Events.Cliente;

/// <summary>
/// Evento que se dispara cuando se restan puntos a un cliente
/// </summary>
public class PuntosRestados : DomainEvent
{
    /// <summary>
    /// ID del cliente al que se restaron puntos
    /// </summary>
    public Guid ClienteId { get; }
    
    /// <summary>
    /// Cantidad de puntos restados
    /// </summary>
    public int Cantidad { get; }
    
    /// <summary>
    /// Total de puntos que quedaron después de la resta
    /// </summary>
    public int PuntosTotales { get; }
    
    /// <summary>
    /// Motivo por el cual se restaron los puntos
    /// </summary>
    public string Motivo { get; }

    /// <summary>
    /// Constructor para crear el evento de puntos restados
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="cantidad">Cantidad de puntos restados</param>
    /// <param name="puntosTotales">Total de puntos actuales</param>
    /// <param name="motivo">Motivo de la resta</param>
    public PuntosRestados(Guid clienteId, int cantidad, int puntosTotales, string motivo)
    {
        ClienteId = clienteId;
        Cantidad = cantidad;
        PuntosTotales = puntosTotales;
        Motivo = motivo;
    }
} 