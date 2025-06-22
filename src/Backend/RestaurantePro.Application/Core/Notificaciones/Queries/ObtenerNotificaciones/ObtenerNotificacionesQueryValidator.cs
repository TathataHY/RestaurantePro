namespace RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerNotificaciones;

/// <summary>
/// Validador para la query de obtener notificaciones
/// </summary>
public class ObtenerNotificacionesQueryValidator : AbstractValidator<ObtenerNotificacionesQuery>
{
    public ObtenerNotificacionesQueryValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
                .WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty)
                .WithMessage("El ID del usuario no puede estar vacío");
    }
} 