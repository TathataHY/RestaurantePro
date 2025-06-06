using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;

public class CancelarReservacionValidator : AbstractValidator<CancelarReservacionCommand>
{
    private readonly IApplicationDbContext _context;
    private const int HORAS_MINIMAS_ANTICIPACION = 2;

    public CancelarReservacionValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.ReservacionId)
            .NotEmpty().WithMessage("El ID de la reservación es obligatorio");

        RuleFor(x => x.Motivo)
            .IsInEnum().WithMessage("El motivo de cancelación no es válido")
            .When(x => x.ReservacionId != Guid.Empty);

        RuleFor(x => x.MotivoDetalle)
            .NotEmpty().WithMessage("El detalle del motivo de cancelación es obligatorio")
            .When(x => x.ReservacionId != Guid.Empty);

        RuleFor(x => x)
            .MustAsync(ReservacionExiste)
            .WithMessage("La reservación especificada no existe")
            .When(x => x.ReservacionId != Guid.Empty);

        RuleFor(x => x)
            .MustAsync(ReservacionPuedeSerCancelada)
            .WithMessage("La reservación no puede ser cancelada en su estado actual")
            .When(x => x.ReservacionId != Guid.Empty);

        RuleFor(x => x)
            .MustAsync(CumpleAnticipacionMinima)
            .WithMessage($"La cancelación debe realizarse con al menos {HORAS_MINIMAS_ANTICIPACION} horas de anticipación")
            .When(x => x.ReservacionId != Guid.Empty);
    }

    private async Task<bool> ReservacionExiste(CancelarReservacionCommand command, CancellationToken cancellationToken)
    {
        return await _context.Reservaciones
            .AnyAsync(r => r.Id == command.ReservacionId, cancellationToken);
    }

    private async Task<bool> ReservacionPuedeSerCancelada(CancelarReservacionCommand command, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == command.ReservacionId, cancellationToken);

        if (reservacion == null)
            return false;

        // Solo se pueden cancelar reservaciones en estado Pendiente o Confirmada
        return reservacion.Estado == EstadoReservacion.Pendiente || 
               reservacion.Estado == EstadoReservacion.Confirmada;
    }

    private async Task<bool> CumpleAnticipacionMinima(CancelarReservacionCommand command, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .FirstOrDefaultAsync(r => r.Id == command.ReservacionId, cancellationToken);

        if (reservacion == null)
            return false;

        // Calcular el tiempo de anticipación
        var tiempoHastaReservacion = reservacion.Fecha.Add(reservacion.Hora) - DateTime.Now;
        
        // La reservación debe cancelarse con al menos 2 horas de anticipación
        return tiempoHastaReservacion.TotalHours >= HORAS_MINIMAS_ANTICIPACION;
    }
} 