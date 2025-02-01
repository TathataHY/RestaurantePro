namespace RestaurantePro.Api.Models;

public class Comanda
{
    public Comanda()
    {
        Detalles = new HashSet<ComandaDetalle>();
    }

    public int Id { get; set; }
    public int NumeroMesa { get; set; }
    public DateTime FechaHora { get; set; }
    public int MesaId { get; set; }
    public string MeseroId { get; set; }
    public decimal Total { get; set; }
    public EstadoComanda Estado { get; set; }
    public string Observaciones { get; set; }

    public Mesa Mesa { get; set; }
    public Usuario Mesero { get; set; }
    public ICollection<ComandaDetalle> Detalles { get; set; }
}

public enum EstadoComanda
{
    Pendiente,
    EnPreparacion,
    Lista,
    Entregada,
    Cancelada
}
