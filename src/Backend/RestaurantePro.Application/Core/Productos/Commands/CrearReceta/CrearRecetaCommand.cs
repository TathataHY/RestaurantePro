namespace RestaurantePro.Application.Core.Productos.Commands.CrearReceta;
using MediatR;

public class CrearRecetaCommand : IRequest<Result<RecetaDto>>
{
    public Guid ProductoId { get; set; }
    public string Preparacion { get; set; } = string.Empty;
    public int TiempoPreparacionMinutos { get; set; }
    public List<AgregarIngredienteDto> Ingredientes { get; set; } = new();
} 