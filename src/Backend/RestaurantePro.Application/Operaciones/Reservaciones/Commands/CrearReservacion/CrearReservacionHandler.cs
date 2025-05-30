using MediatR;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;

/// <summary>
/// Handler para crear reservaciones
/// </summary>
public class CrearReservacionHandler : IRequestHandler<CrearReservacionCommand, Result<ReservacionDto>>
{
    public CrearReservacionHandler()
    {
        // Constructor temporal
    }

    public async Task<Result<ReservacionDto>> Handle(CrearReservacionCommand request, CancellationToken cancellationToken)
    {
        // TODO: Implementar lógica completa cuando estén disponibles las dependencias
        await Task.Delay(1, cancellationToken);
        throw new NotImplementedException("Handler de crear reservación pendiente de implementación");
    }
} 