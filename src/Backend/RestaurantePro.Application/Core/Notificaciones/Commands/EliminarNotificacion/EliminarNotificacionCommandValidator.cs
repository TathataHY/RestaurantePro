namespace RestaurantePro.Application.Core.Notificaciones.Commands.EliminarNotificacion;

/// <summary>
/// Validador para el comando de eliminar notificación
/// </summary>
public class EliminarNotificacionCommandValidator : AbstractValidator<EliminarNotificacionCommand>
{
    public EliminarNotificacionCommandValidator()
    {
        RuleFor(x => x.NotificacionId)
            .NotEmpty()
                .WithMessage("El ID de la notificación es obligatorio")
            .NotEqual(Guid.Empty)
                .WithMessage("El ID de la notificación no puede estar vacío");
    }
} 