namespace RestaurantePro.Application.Operaciones.Comandas.Commands.FinalizarComanda;

/// <summary>
/// 🔍 Validador para FinalizarComandaCommand
/// </summary>
public class FinalizarComandaValidator : AbstractValidator<FinalizarComandaCommand>
{
    public FinalizarComandaValidator()
    {
        // Validación ComandaId
        RuleFor(x => x.ComandaId)
            .NotEmpty()
            .WithMessage("🍽️ ComandaId es requerido para finalizar")
            .WithErrorCode("FINALIZAR_COMANDA_ID_REQUERIDO");

        // Validación UsuarioId
        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("👤 UsuarioId es requerido para finalizar")
            .WithErrorCode("FINALIZAR_USUARIO_ID_REQUERIDO");

        // Validación ObservacionesFinalizacion (longitud)
        RuleFor(x => x.ObservacionesFinalizacion)
            .MaximumLength(500)
            .WithMessage("📝 Las observaciones no pueden exceder 500 caracteres")
            .WithErrorCode("FINALIZAR_OBSERVACIONES_LONGITUD");

        // Validación fecha (no puede ser futura)
        RuleFor(x => x.FechaFinalizacion)
            .Must(fecha => !fecha.HasValue || fecha.Value <= DateTime.UtcNow.AddMinutes(5))
            .WithMessage("🕐 La fecha de finalización no puede ser en el futuro")
            .WithErrorCode("FINALIZAR_FECHA_FUTURA");

        // Validación fecha (no puede ser muy antigua - más de 24 horas)
        RuleFor(x => x.FechaFinalizacion)
            .Must(fecha => !fecha.HasValue || fecha.Value >= DateTime.UtcNow.AddHours(-24))
            .WithMessage("📅 La fecha de finalización no puede ser mayor a 24 horas en el pasado")
            .WithErrorCode("FINALIZAR_FECHA_ANTIGUA");
    }
} 