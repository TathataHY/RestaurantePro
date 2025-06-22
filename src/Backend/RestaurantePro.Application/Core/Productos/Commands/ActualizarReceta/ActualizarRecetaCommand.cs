namespace RestaurantePro.Application.Core.Productos.Commands.ActualizarReceta;

public class ActualizarRecetaCommand
{
    public Guid Id { get; set; }
    public string Preparacion { get; set; } = string.Empty;
    public int TiempoPreparacionMinutos { get; set; }
    public List<AgregarIngredienteDto> Ingredientes { get; set; } = new();
} 