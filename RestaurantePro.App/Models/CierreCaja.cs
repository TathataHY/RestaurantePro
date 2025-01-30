namespace RestaurantePro.App.Models;
public class CierreCaja
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalComandas { get; set; }
    public string UsuarioId { get; set; }
    public List<Comanda> Comandas { get; set; }
    public string Observaciones { get; set; }
} 