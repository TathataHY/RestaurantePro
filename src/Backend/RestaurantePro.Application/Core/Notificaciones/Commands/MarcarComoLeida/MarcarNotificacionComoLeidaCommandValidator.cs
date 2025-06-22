namespace RestaurantePro.Application.Core.Notificaciones.Commands.MarcarComoLeida;

/// <summary>
/// Validador para el comando de marcar notificación como leída
/// </summary>
public class MarcarNotificacionComoLeidaCommandValidator : AbstractValidator<MarcarNotificacionComoLeidaCommand>
{
    public MarcarNotificacionComoLeidaCommandValidator()
    {
        RuleFor(x => x.NotificacionId)
            .NotEmpty()
                .WithMessage("El ID de la notificación es obligatorio")
            .NotEqual(Guid.Empty)
                .WithMessage("El ID de la notificación no puede estar vacío");
    }
} 