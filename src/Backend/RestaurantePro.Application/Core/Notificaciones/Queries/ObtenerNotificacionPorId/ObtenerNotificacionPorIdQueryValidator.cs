namespace RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerNotificacionPorId;

/// <summary>
/// Validador para la query de obtener notificación por ID
/// </summary>
public class ObtenerNotificacionPorIdQueryValidator : AbstractValidator<ObtenerNotificacionPorIdQuery>
{
    public ObtenerNotificacionPorIdQueryValidator()
    {
        RuleFor(x => x.NotificacionId)
            .NotEmpty()
                .WithMessage("El ID de la notificación es obligatorio")
            .NotEqual(Guid.Empty)
                .WithMessage("El ID de la notificación no puede estar vacío");
    }
} 