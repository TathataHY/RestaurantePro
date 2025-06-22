namespace RestaurantePro.Application.Core.Notificaciones.Commands.MarcarTodasComoLeidas;

/// <summary>
/// Validador para el comando de marcar todas las notificaciones como leídas
/// </summary>
public class MarcarTodasNotificacionesComoLeidasCommandValidator : AbstractValidator<MarcarTodasNotificacionesComoLeidasCommand>
{
    public MarcarTodasNotificacionesComoLeidasCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
                .WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty)
                .WithMessage("El ID del usuario no puede estar vacío");
    }
} 