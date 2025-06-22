namespace RestaurantePro.Application.Core.Productos.Commands.CrearReceta;

public class CrearRecetaCommand
{
    public Guid ProductoId { get; set; }
    public string Preparacion { get; set; } = string.Empty;
    public int TiempoPreparacionMinutos { get; set; }
    public List<AgregarIngredienteDto> Ingredientes { get; set; } = new();
} 