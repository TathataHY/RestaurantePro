namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ModificarReservacion;

/// <summary>
/// Validator para ModificarReservacionCommand
/// </summary>
public class ModificarReservacionValidator : AbstractValidator<ModificarReservacionCommand>
{
    public ModificarReservacionValidator()
    {
        RuleFor(x => x.ReservacionId)
            .NotEmpty()
            .WithMessage("El ID de la reservación es requerido")
            .WithErrorCode("RESERVACION_ID_REQUERIDO");

        RuleFor(x => x.NuevaFechaReservacion)
            .NotEmpty()
            .WithMessage("La nueva fecha de reservación es requerida")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("La fecha de reservación no puede ser en el pasado")
            .WithErrorCode("FECHA_RESERVACION_PASADO")
            .Must(FechaEnRangoPermitido)
            .WithMessage("La fecha de reservación no puede ser más de 90 días en el futuro")
            .WithErrorCode("FECHA_RESERVACION_MUY_FUTURA");

        RuleFor(x => x.NuevaHoraReservacion)
            .NotEmpty()
            .WithMessage("La nueva hora de reservación es requerida")
            .Must(BeValidBusinessHour)
            .WithMessage("La hora de reservación debe estar entre las 10:00 y las 22:30")
            .WithErrorCode("HORA_RESERVACION_FUERA_HORARIO");

        RuleFor(x => x.NuevoNumeroPersonas)
            .GreaterThan(0)
            .WithMessage("El número de personas debe ser mayor a 0")
            .WithErrorCode("NUMERO_PERSONAS_INVALIDO")
            .LessThanOrEqualTo(20)
            .WithMessage("El número de personas no puede exceder 20")
            .WithErrorCode("NUMERO_PERSONAS_EXCESIVO");

        // Validaciones opcionales de Mesa y Cliente
        RuleFor(x => x.NuevaMesaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la mesa es requerido")
            .WithErrorCode("MESA_ID_REQUERIDO")
            .When(x => x.NuevaMesaId.HasValue);

        RuleFor(x => x.NuevoClienteId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del cliente es requerido")
            .WithErrorCode("CLIENTE_ID_REQUERIDO")
            .When(x => x.NuevoClienteId.HasValue);

        RuleFor(x => x.MotivoModificacion)
            .NotEmpty()
            .WithMessage("El motivo de modificación es requerido")
            .WithErrorCode("MOTIVO_MODIFICACION_REQUERIDO")
            .MaximumLength(500)
            .WithMessage("El motivo de modificación no puede exceder 500 caracteres")
            .WithErrorCode("MOTIVO_MODIFICACION_LONGITUD");

        RuleFor(x => x.ObservacionesModificacion)
            .MaximumLength(1000)
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres")
            .WithErrorCode("OBSERVACIONES_LONGITUD")
            .When(x => !string.IsNullOrEmpty(x.ObservacionesModificacion));

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido")
            .WithErrorCode("USUARIO_ID_REQUERIDO");

        // Validación de fecha y hora combinadas
        RuleFor(x => x)
            .Must(x => CombineDateAndTime(x.NuevaFechaReservacion, x.NuevaHoraReservacion) > DateTime.Now.AddHours(2))
            .WithMessage("La nueva fecha y hora debe ser al menos 2 horas en el futuro")
            .When(x => x.NuevaFechaReservacion >= DateTime.Today);
    }

    private static bool FechaEnRangoPermitido(DateTime fecha)
    {
        // Permitir hasta 90 días desde hoy (inclusive)
        // Usar Date para normalizar y evitar problemas con horas
        var fechaBase = DateTime.Now.Date;
        var fechaLimite = fechaBase.AddDays(90);
        return fecha.Date <= fechaLimite;
    }

    private static bool BeValidBusinessHour(TimeSpan hora)
    {
        // Cambiar a 10:00 - 22:30 como esperan las pruebas
        return hora >= TimeSpan.FromHours(10) && hora <= new TimeSpan(22, 30, 0);
    }

    private static DateTime CombineDateAndTime(DateTime fecha, TimeSpan hora)
    {
        return fecha.Date.Add(hora);
    }
} 