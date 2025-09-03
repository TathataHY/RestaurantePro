namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;

public class CrearComandaRequest
{
    public string MesaId { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public List<ProductoComandaRequest> Productos { get; set; } = new();
}

public class ProductoComandaRequest
{
    public string ProductoId { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
}
