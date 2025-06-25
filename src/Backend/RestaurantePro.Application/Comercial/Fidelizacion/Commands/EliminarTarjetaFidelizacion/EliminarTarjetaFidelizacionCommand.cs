using MediatR;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.EliminarTarjetaFidelizacion;

public class EliminarTarjetaFidelizacionCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
} 