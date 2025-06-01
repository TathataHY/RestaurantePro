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

        // Validación alternativa por código
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

        // Validación de que se proporcione al menos un identificador
        RuleFor(v => v)
            .Must(v => v.ReservacionId != Guid.Empty || !string.IsNullOrEmpty(v.CodigoReservacion))
            .WithMessage("Debe proporcionar el ID de reservación o el código de reservación.")
            .WithName("Identificacion");

        // Validación de método de confirmación
        RuleFor(v => v.MetodoConfirmacion)
            .NotEmpty()
            .WithMessage("El método de confirmación es requerido.")
            .MaximumLength(50)
            .WithMessage("El método de confirmación no puede exceder 50 caracteres.")
            .Must(BeValidMetodoConfirmacion)
            .WithMessage("El método de confirmación debe ser válido: Manual, Telefono, Email, SMS, App.");
    }

    private void ConfigurarValidacionesCondicionales()
    {
        // Validaciones cuando se proporciona el confirmado por
        RuleFor(v => v.ConfirmadoPor)
            .NotEmpty()
            .WithMessage("La persona que confirma es requerida para confirmaciones manuales.")
            .MaximumLength(100)
            .WithMessage("El nombre de quien confirma no puede exceder 100 caracteres.")
            .When(v => v.MetodoConfirmacion == "Manual");

        // Validaciones de notas de confirmación
        RuleFor(v => v.NotasConfirmacion)
            .MaximumLength(500)
            .WithMessage("Las notas de confirmación no pueden exceder 500 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.NotasConfirmacion));

        // Validaciones de datos adicionales
        RuleFor(v => v.DatosAdicionales)
            .Must(DatosAdicionalesValidos)
            .WithMessage("Los datos adicionales contienen información inválida.")
            .When(v => v.DatosAdicionales != null && v.DatosAdicionales.Any());
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

    private async Task<bool> CodigoReservacionExiste(string codigoReservacion, CancellationToken cancellationToken)
    {
        return await _context.Reservaciones
            .AnyAsync(r => r.CodigoReservacion == codigoReservacion, cancellationToken);
    }

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
        return datosAdicionales.All(kvp => 
            !string.IsNullOrEmpty(kvp.Key) && 
            kvp.Key.Length <= 50);
    }

    private async Task<bool> ReservacionEsConfirmable(Guid reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == reservacionId, cancellationToken);

        if (reservacion == null)
            return false;

        // Solo se pueden confirmar reservaciones pendientes
        return reservacion.Estado == EstadoReservacion.Pendiente;
    }

    private async Task<bool> NoExcedeTiempoLimiteConfirmacion(Guid reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == reservacionId, cancellationToken);

        if (reservacion == null)
            return false;

        // Permitir confirmación hasta 2 horas antes de la reservación
        var tiempoLimite = reservacion.FechaHora.AddHours(-2);
        return DateTime.UtcNow <= tiempoLimite;
    }

    private async Task<bool> MesaSigueDisponible(Guid reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .Include(r => r.Mesa)
            .FirstOrDefaultAsync(r => r.Id == reservacionId, cancellationToken);

        if (reservacion == null)
            return false;

        // Verificar que no haya conflictos con otras reservaciones confirmadas
        var conflictos = await _context.Reservaciones
            .Where(r => r.Id != reservacionId &&
                       r.MesaId == reservacion.MesaId &&
                       r.Estado == EstadoReservacion.Confirmada &&
                       r.FechaHora.Date == reservacion.FechaHora.Date)
            .AnyAsync(cancellationToken);

        return !conflictos;
    }

    #endregion
} 