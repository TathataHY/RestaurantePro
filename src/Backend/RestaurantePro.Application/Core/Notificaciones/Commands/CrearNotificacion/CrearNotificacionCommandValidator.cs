namespace RestaurantePro.Application.Core.Notificaciones.Commands.CrearNotificacion;

/// <summary>
/// Validador para el comando de crear notificación
/// </summary>
public class CrearNotificacionCommandValidator : AbstractValidator<CrearNotificacionCommand>
{
    public CrearNotificacionCommandValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty()
                .WithMessage("El título es obligatorio")
            .MaximumLength(100)
                .WithMessage("El título no puede exceder los 100 caracteres")
            .MinimumLength(3)
                .WithMessage("El título debe tener al menos 3 caracteres");

        RuleFor(x => x.Mensaje)
            .NotEmpty()
                .WithMessage("El mensaje es obligatorio")
            .MaximumLength(500)
                .WithMessage("El mensaje no puede exceder los 500 caracteres")
            .MinimumLength(10)
                .WithMessage("El mensaje debe tener al menos 10 caracteres");

        RuleFor(x => x.Tipo)
            .NotEmpty()
                .WithMessage("El tipo es obligatorio")
            .MaximumLength(50)
                .WithMessage("El tipo no puede exceder los 50 caracteres")
            .Must(BeValidTipo)
                .WithMessage("El tipo debe ser uno de los valores válidos: Informativa, Advertencia, Error, Exito");

        RuleFor(x => x.DestinatarioId)
            .NotEmpty()
                .WithMessage("El ID del destinatario es obligatorio")
            .NotEqual(Guid.Empty)
                .WithMessage("El ID del destinatario no puede estar vacío");

        RuleFor(x => x.EntidadRelacionadaId)
            .NotEqual(Guid.Empty)
                .When(x => x.EntidadRelacionadaId.HasValue)
                .WithMessage("El ID de la entidad relacionada no puede estar vacío si se proporciona");
    }

    private static bool BeValidTipo(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return false;

        var tiposValidos = new[] { "Informativa", "Advertencia", "Error", "Exito" };
        return tiposValidos.Contains(tipo, StringComparer.OrdinalIgnoreCase);
    }
} 