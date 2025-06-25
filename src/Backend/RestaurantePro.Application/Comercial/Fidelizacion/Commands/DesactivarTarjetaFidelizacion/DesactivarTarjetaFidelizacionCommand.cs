using MediatR;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.DesactivarTarjetaFidelizacion;

public class DesactivarTarjetaFidelizacionCommand : IRequest<Result<TarjetaFidelizacionDto>>
{
    public Guid Id { get; set; }
} 