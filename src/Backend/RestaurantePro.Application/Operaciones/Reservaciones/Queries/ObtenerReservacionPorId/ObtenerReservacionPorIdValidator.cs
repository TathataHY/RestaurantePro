using FluentValidation;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionPorId;

/// <summary>
/// Validator para ObtenerReservacionPorIdQuery
/// </summary>
public class ObtenerReservacionPorIdValidator : AbstractValidator<ObtenerReservacionPorIdQuery>
{
    public ObtenerReservacionPorIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID de la reservación es requerido")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la reservación no puede ser un GUID vacío");
    }
} 