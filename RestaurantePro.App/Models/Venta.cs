namespace RestaurantePro.App.Models;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int ComandaId { get; set; }
    public decimal Monto { get; set; }
    public string FormaPago { get; set; }
    public string NumeroTransaccion { get; set; }
    public virtual Comanda Comanda { get; set; }
} 