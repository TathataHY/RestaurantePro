namespace RestaurantePro.Mobile.Core.Features.Operations.Comandas.Models;

public class CrearComandaRequest
{
    public string MeseroId { get; set; } = string.Empty;
    public string? MesaId { get; set; }
    public string? ClienteId { get; set; }
    public string? Observaciones { get; set; }
    public List<ProductoComandaRequest> ProductosIniciales { get; set; } = new();
    public List<ProductoComandaRequest> Items { get; set; } = new();

    /// <summary>
    /// Tipo de comanda (Mesa, Delivery, TakeAway). Valor por defecto: Mesa
    /// </summary>
    public string Tipo { get; set; } = "Mesa";

    /// <summary>
    /// Nombre de la persona que recibe la entrega (requerido para Delivery)
    /// </summary>
    public string? NombreEntrega { get; set; }

    /// <summary>
    /// Dirección de entrega (requerido para Delivery)
    /// </summary>
    public string? DireccionEntrega { get; set; }

    /// <summary>
    /// Teléfono de contacto para la entrega (requerido para Delivery)
    /// </summary>
    public string? TelefonoEntrega { get; set; }
}

public class ProductoComandaRequest
{
    public string ProductoId { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
}
