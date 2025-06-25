using MediatR;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActivarTarjetaFidelizacion;

public class ActivarTarjetaFidelizacionCommand : IRequest<Result<TarjetaFidelizacionDto>>
{
    public Guid Id { get; set; }
} 