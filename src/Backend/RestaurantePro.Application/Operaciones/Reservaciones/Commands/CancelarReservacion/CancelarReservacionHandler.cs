using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;


namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;

/// <summary>
/// Handler para cancelar una reservación
/// </summary>
public class CancelarReservacionHandler : IRequestHandler<CancelarReservacionCommand, Result<ReservacionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CancelarReservacionHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public CancelarReservacionHandler(
        IApplicationDbContext context,
        ILogger<CancelarReservacionHandler> logger,
        IEmailService emailService,
        INotificationService notificationService,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<Result<ReservacionDto>> Handle(CancelarReservacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar si se solicitó cancelación
            cancellationToken.ThrowIfCancellationRequested();
            
            _logger.LogInformation("Iniciando cancelación de reservación {ReservacionId}", request.ReservacionId);

            // Buscar la reservación
            var reservacion = await _context.Reservaciones
                .Include(r => r.Cliente)
                .Include(r => r.Mesa)
                .FirstOrDefaultAsync(r => r.Id == request.ReservacionId, cancellationToken);

            if (reservacion == null)
            {
                _logger.LogWarning("Reservación {ReservacionId} no encontrada", request.ReservacionId);
                return Result.Failure($"No se encontró la reservación con ID {request.ReservacionId}").ToGeneric<ReservacionDto>();
            }

            // Intentar cancelar la reservación
            try
            {
                // La validación de estado ya se hizo en el validator
                reservacion.Cancelar(request.MotivoDetalle ?? request.Motivo.ToString());
                
                // Registrar la cancelación
                _logger.LogInformation("Reservación {ReservacionId} cancelada por {UsuarioId}. Motivo: {Motivo}", 
                    reservacion.Id, request.UsuarioId, request.MotivoDetalle);
                
                // Guardar cambios
                await _context.SaveChangesAsync(cancellationToken);
                
                // Notificar al cliente si es necesario
                if (request.NotificarCliente && reservacion.Cliente != null)
                {
                    await NotificarCliente(reservacion, request);
                }
                
                // Crear DTO para retornar
                var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);
                return Result<ReservacionDto>.Success(reservacionDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error al cancelar reservación {ReservacionId}: {Error}", request.ReservacionId, ex.Message);
                return Result.Failure(ex.Message).ToGeneric<ReservacionDto>();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error inesperado al cancelar reservación {ReservacionId}: {Error}", request.ReservacionId, ex.Message);
            return Result.Failure($"Error al cancelar la reservación: {ex.Message}").ToGeneric<ReservacionDto>();
        }
    }

    private static bool PuedeSerCancelada(Reservacion reservacion)
    {
        return reservacion.Estado == EstadoReservacion.Confirmada ||
               reservacion.Estado == EstadoReservacion.Pendiente;
    }

    private static bool CumplePoliticaCancelacion(Reservacion reservacion)
    {
        // Usar la propiedad calculada FechaReservacion que combina Fecha + Hora
        var tiempoAnticipacion = reservacion.FechaReservacion - DateTime.Now;
        return tiempoAnticipacion.TotalHours >= 2;
    }

    private async Task NotificarCliente(Reservacion reservacion, CancelarReservacionCommand request)
    {
        try
        {
            if (reservacion.Cliente == null || string.IsNullOrEmpty(reservacion.Email))
            {
                _logger.LogWarning("No se puede notificar al cliente de la reservación {ReservacionId} - datos de contacto no disponibles", 
                    reservacion.Id);
                return;
            }

            // Primero intentamos enviar un email
            if (!string.IsNullOrEmpty(reservacion.Email))
            {
                var emailSubject = "Cancelación de su reserva";
                var emailBody = $"Estimado/a {reservacion.Cliente.Nombre},<br><br>" +
                           $"Le informamos que su reserva para el {reservacion.Fecha:dd/MM/yyyy} a las {reservacion.Hora:hh\\:mm} " +
                           $"ha sido cancelada.<br><br>" +
                           $"Motivo: {request.MotivoDetalle}<br><br>" +
                           $"Lamentamos cualquier inconveniente que esto pueda causarle.<br><br>" +
                           $"Atentamente,<br>El equipo del restaurante";

                await _emailService.SendEmailAsync(reservacion.Email, emailSubject, emailBody);
                _logger.LogInformation("Email de cancelación enviado a {Email} para reservación {ReservacionId}", 
                    reservacion.Email, reservacion.Id);
            }

            // Luego intentamos enviar una notificación (SMS o push)
            if (!string.IsNullOrEmpty(reservacion.Telefono))
            {
                var mensaje = $"Su reserva para el {reservacion.Fecha:dd/MM/yyyy} a las {reservacion.Hora:hh\\:mm} ha sido cancelada. " +
                              $"Motivo: {request.Motivo}";
                
                await _notificationService.EnviarNotificacionAsync(
                    reservacion.ClienteId,
                    "Reservación Cancelada",
                    mensaje,
                    "Reservacion");
                    
                _logger.LogInformation("Notificación de cancelación enviada a {Telefono} para reservación {ReservacionId}", 
                    reservacion.Telefono, reservacion.Id);
            }
        }
        catch (Exception ex)
        {
            // No queremos que un error de notificación interrumpa el proceso principal
            _logger.LogError(ex, "Error al enviar notificación de cancelación para reservación {ReservacionId}", 
                reservacion.Id);
        }
    }

    private async Task RegistrarAuditoriaCancelacion(Reservacion reservacion, EstadoReservacion estadoAnterior, string canceladoPor)
    {
        try
        {
            _logger.LogInformation("Auditoría: Reservación {ReservacionId} cancelada. Estado anterior: {EstadoAnterior}, Cancelado por: {CanceladoPor}", 
                reservacion.Id, estadoAnterior, canceladoPor);
                
            // TODO: Implementar cuando AuditoriaReservacion esté disponible en el dominio
            /*
            var auditoria = new AuditoriaReservacion
            {
                ReservacionId = reservacion.Id,
                Accion = "Cancelación",
                EstadoAnterior = estadoAnterior.ToString(),
                EstadoNuevo = EstadoReservacion.Cancelada.ToString(),
                Detalles = $"Motivo: {reservacion.MotivoCancelacion}",
                RealizadoPor = canceladoPor,
                FechaAccion = DateTime.UtcNow
            };

            _context.AuditoriasReservaciones.Add(auditoria);
            await _context.SaveChangesAsync();
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar auditoría de cancelación {ReservacionId}", 
                reservacion.Id);
        }
    }
} 

