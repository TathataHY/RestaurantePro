using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarCantidadItem;

public class ActualizarCantidadItemCommand : IRequest<Result<ComandaDto>>
{
    public Guid ComandaId { get; set; }
    public Guid ItemId { get; set; }
    public int NuevaCantidad { get; set; }
}



