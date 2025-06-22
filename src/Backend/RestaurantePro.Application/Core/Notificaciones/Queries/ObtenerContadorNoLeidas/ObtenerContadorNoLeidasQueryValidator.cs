namespace RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerContadorNoLeidas;

/// <summary>
/// Validador para la query de obtener contador de notificaciones no leídas
/// </summary>
public class ObtenerContadorNoLeidasQueryValidator : AbstractValidator<ObtenerContadorNoLeidasQuery>
{
    public ObtenerContadorNoLeidasQueryValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
                .WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty)
                .WithMessage("El ID del usuario no puede estar vacío");
    }
} 