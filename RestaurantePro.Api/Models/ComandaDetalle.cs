namespace RestaurantePro.Api.Models;

public class ComandaDetalle
{
    public int Id { get; set; }
    public int ComandaId { get; set; }
    public int PlatoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string Observaciones { get; set; }
    public string PlatoNombre { get; set; }

    public Comanda Comanda { get; set; }
    public Plato Plato { get; set; }
} 