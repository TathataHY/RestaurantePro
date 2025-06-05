namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionPorId;

/// <summary>
/// Validator para ObtenerReservacionPorIdQuery
/// </summary>
public class ObtenerReservacionPorIdValidator : AbstractValidator<ObtenerReservacionPorIdQuery>
{
    public ObtenerReservacionPorIdValidator()
    {
        // Hacemos que solo valide Id si el objeto no es nulo para evitar excepciones
        When(x => x != null, () => {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("El ID de la reservación es requerido para realizar la consulta")
                .WithErrorCode("RESERVACION_ID_REQUERIDO");
        });
    }
} 