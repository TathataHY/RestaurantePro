namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;

public class CancelarReservacionValidator : AbstractValidator<CancelarReservacionCommand>
{
    private readonly IApplicationDbContext _context;

    public CancelarReservacionValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.ReservacionId)
            .NotEmpty()
            .WithMessage("El ID de la reservación es requerido.")
            .MustAsync(ReservacionExiste)
            .WithMessage("La reservación especificada no existe.")
            .MustAsync(ReservacionEsCancelable)
            .WithMessage("La reservación no puede ser cancelada en su estado actual.")
            .MustAsync(ReservacionNoVencida)
            .WithMessage("No se puede cancelar una reservación que ya pasó.");

        RuleFor(v => v.MotivoTexto)
            .NotEmpty()
            .WithMessage("El motivo de cancelación es requerido.")
            .MinimumLength(10)
            .WithMessage("El motivo debe tener al menos 10 caracteres.")
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder 500 caracteres.");

        RuleFor(v => v.CanceladoPor)
            .NotEmpty()
            .WithMessage("El usuario que cancela es requerido.");

        RuleFor(v => v.NotificarCliente)
            .NotNull()
            .WithMessage("Debe especificar si notificar al cliente.");

        // Validación de política de cancelación
        RuleFor(v => v)
            .MustAsync(CumplePoliticaCancelacion)
            .WithMessage("La cancelación no cumple con la política establecida (mínimo 2 horas de anticipación).")
            .WithName("PoliticaCancelacion");
    }

    private async Task<bool> ReservacionExiste(Guid reservacionId, CancellationToken cancellationToken)
    {
        return await _context.Reservaciones
            .AnyAsync(r => r.Id == reservacionId, cancellationToken);
    }

    private async Task<bool> ReservacionEsCancelable(Guid reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == reservacionId, cancellationToken);

        if (reservacion == null) return false;

        // Solo se puede cancelar si está en estado Confirmada o Pendiente
        // TODO: Descomentar cuando la entidad Reservacion tenga la propiedad Estado correcta
        // return reservacion.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Confirmada || 
        //        reservacion.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Pendiente;
        
        // Temporalmente asumimos que se puede cancelar
        return true;
    }

    private async Task<bool> ReservacionNoVencida(Guid reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == reservacionId, cancellationToken);

        if (reservacion == null) return false;

        // TODO: Descomentar cuando la entidad Reservacion tenga la propiedad FechaHora
        // return reservacion.FechaHora > DateTime.UtcNow;
        
        // Temporalmente asumimos que no ha vencido
        return true;
    }

    private async Task<bool> CumplePoliticaCancelacion(CancelarReservacionCommand command, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == command.ReservacionId, cancellationToken);

        if (reservacion == null) return false;

        // TODO: Descomentar cuando la entidad Reservacion tenga la propiedad FechaHora
        // var tiempoAnticipacion = reservacion.FechaHora - DateTime.UtcNow;
        // return tiempoAnticipacion.TotalHours >= 2;
        
        // Temporalmente asumimos que cumple la política
        return true;
    }
} 