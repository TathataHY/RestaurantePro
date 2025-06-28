namespace RestaurantePro.Application.Core.Recetas.Commands.ActualizarReceta;
using MediatR;
using RestaurantePro.Application.Core.Recetas.DTOs;

public class ActualizarRecetaCommand : IRequest<Result<RecetaDto>>
{
    public Guid Id { get; set; }
    public string Preparacion { get; set; } = string.Empty;
    public int TiempoPreparacionMinutos { get; set; }
    public List<AgregarIngredienteDto> Ingredientes { get; set; } = new();
} 
