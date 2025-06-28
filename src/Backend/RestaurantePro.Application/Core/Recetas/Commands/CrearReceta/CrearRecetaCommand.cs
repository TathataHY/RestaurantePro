namespace RestaurantePro.Application.Core.Recetas.Commands.CrearReceta;
using MediatR;
using RestaurantePro.Application.Core.Recetas.DTOs;

public class CrearRecetaCommand : IRequest<Result<RecetaDto>>
{
    public Guid ProductoId { get; set; }
    public string Preparacion { get; set; } = string.Empty;
    public int TiempoPreparacionMinutos { get; set; }
    public List<AgregarIngredienteDto> Ingredientes { get; set; } = new();
} 
