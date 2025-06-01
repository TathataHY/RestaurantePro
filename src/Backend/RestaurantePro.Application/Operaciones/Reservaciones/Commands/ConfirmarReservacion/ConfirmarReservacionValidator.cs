namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;

/// <summary>
/// Validador para ConfirmarReservacionCommand
/// Valida reglas de negocio para la confirmación de reservaciones pendientes
/// </summary>
public class ConfirmarReservacionValidator : AbstractValidator<ConfirmarReservacionCommand>
{
    private readonly IApplicationDbContext _context;

    public ConfirmarReservacionValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesCondicionales();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        // Validación de identificación de reservación
        RuleFor(v => v.ReservacionId)
            .NotEmpty()
            .WithMessage("El ID de la reservación es requerido.")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de la reservación no puede ser un GUID vacío.")
            .MustAsync(ReservacionExiste)
            .WithMessage("La reservación especificada no existe.")
            .When(v => v.ReservacionId != Guid.Empty);

        // TODO: Validación alternativa por código cuando la propiedad esté disponible
        /*
        RuleFor(v => v.CodigoReservacion)
            .NotEmpty()
            .WithMessage("El código de reservación es requerido cuando no se proporciona ID.")
            .Length(6, 20)
            .WithMessage("El código de reservación debe tener entre 6 y 20 caracteres.")
            .Matches(@"^[A-Z0-9\-]+$")
            .WithMessage("El código de reservación solo puede contener letras mayúsculas, números y guiones.")
            .MustAsync(CodigoReservacionExiste)
            .WithMessage("El código de reservación especificado no existe.")
            .When(v => v.ReservacionId == Guid.Empty);
        */

        // Validación de método de confirmación
        RuleFor(v => v.MetodoConfirmacion)
            .NotEmpty()
            .WithMessage("El método de confirmación es requerido.")
            .Must(BeValidMetodoConfirmacion)
            .WithMessage("El método de confirmación no es válido. Valores permitidos: Manual, Telefono, Email, SMS, App.");

        // Validación de quien confirma
        RuleFor(v => v.ConfirmadoPor)
            .MaximumLength(100)
            .WithMessage("El nombre de quien confirma no puede exceder 100 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.ConfirmadoPor));

        // Validación de notas de confirmación
        RuleFor(v => v.NotasConfirmacion)
            .MaximumLength(500)
            .WithMessage("Las notas de confirmación no pueden exceder 500 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.NotasConfirmacion));
    }

    private void ConfigurarValidacionesCondicionales()
    {
        // Validación de datos adicionales
        RuleFor(v => v.DatosAdicionales)
            .Must(DatosAdicionalesValidos!)
            .WithMessage("Los datos adicionales contienen información inválida o exceden el límite permitido.")
            .When(v => v.DatosAdicionales != null);
    }

    private void ConfigurarValidacionesNegocio()
    {
        // Validación de estado de reservación
        RuleFor(v => v.ReservacionId)
            .MustAsync(ReservacionEsConfirmable)
            .WithMessage("La reservación no puede ser confirmada en su estado actual.")
            .When(v => v.ReservacionId != Guid.Empty);

        // Validación de tiempo límite para confirmación
        RuleFor(v => v.ReservacionId)
            .MustAsync(NoExcedeTiempoLimiteConfirmacion)
            .WithMessage("Ha excedido el tiempo límite para confirmar la reservación.")
            .When(v => v.ReservacionId != Guid.Empty);

        // Validación de disponibilidad de mesa
        RuleFor(v => v.ReservacionId)
            .MustAsync(MesaSigueDisponible)
            .WithMessage("La mesa ya no está disponible para la fecha y hora de la reservación.")
            .When(v => v.ReservacionId != Guid.Empty);
    }

    #region Métodos de validación privados

    private async Task<bool> ReservacionExiste(Guid reservacionId, CancellationToken cancellationToken)
    {
        return await _context.Reservaciones
            .AnyAsync(r => r.Id == reservacionId, cancellationToken);
    }

    // TODO: Implementar cuando CodigoReservacion esté disponible en la entidad
    /*
    private async Task<bool> CodigoReservacionExiste(string codigoReservacion, CancellationToken cancellationToken)
    {
        return await _context.Reservaciones
            .AnyAsync(r => r.CodigoReservacion == codigoReservacion, cancellationToken);
    }
    */

    private static bool BeValidMetodoConfirmacion(string metodo)
    {
        var metodosValidos = new[] { "Manual", "Telefono", "Email", "SMS", "App" };
        return metodosValidos.Contains(metodo, StringComparer.OrdinalIgnoreCase);
    }

    private static bool DatosAdicionalesValidos(Dictionary<string, object> datosAdicionales)
    {
        // Validar que no haya demasiados datos adicionales
        if (datosAdicionales.Count > 10)
            return false;

        // Validar que las claves no sean muy largas
        if (datosAdicionales.Keys.Any(k => k.Length > 50))
            return false;

        return true;
    }

    private async Task<bool> ReservacionEsConfirmable(Guid reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == reservacionId, cancellationToken);

        if (reservacion == null)
            return false;

        // Solo se pueden confirmar reservaciones en estado Pendiente
        return reservacion.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Pendiente;
    }

    private async Task<bool> NoExcedeTiempoLimiteConfirmacion(Guid reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == reservacionId, cancellationToken);

        if (reservacion == null)
            return false;

        // Permitir confirmación hasta 2 horas antes de la reservación
        var fechaHoraReservacion = reservacion.Fecha.Add(reservacion.Hora);
        var tiempoLimite = fechaHoraReservacion.AddHours(-2);
        
        return DateTime.UtcNow <= tiempoLimite;
    }

    private async Task<bool> MesaSigueDisponible(Guid reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == reservacionId, cancellationToken);

        if (reservacion == null)
            return false;

        // Verificar que no haya conflictos con otras reservaciones confirmadas
        var fechaHoraReservacion = reservacion.Fecha.Add(reservacion.Hora);
        var conflictos = await _context.Reservaciones
            .Where(r => r.Id != reservacionId &&
                       r.MesaId == reservacion.MesaId &&
                       r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Confirmada &&
                       r.Fecha.Date == reservacion.Fecha.Date)
            .AnyAsync(cancellationToken);

        return !conflictos;
    }

    #endregion
} 