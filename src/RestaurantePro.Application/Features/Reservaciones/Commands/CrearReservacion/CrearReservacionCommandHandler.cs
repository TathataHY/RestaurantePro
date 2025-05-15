using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.CrearReservacion
{
    public class CrearReservacionCommandHandler : IRequestHandler<CrearReservacionCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CrearReservacionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(CrearReservacionCommand request, CancellationToken cancellationToken)
        {
            // Verificar que la mesa exista
            var mesa = await _context.Mesas
                .FirstOrDefaultAsync(m => m.Id == request.MesaId, cancellationToken);

            if (mesa == null)
                throw new Exception($"No existe una mesa con el ID {request.MesaId}");

            // Verificar que la mesa esté activa
            if (!mesa.Activa)
                throw new Exception($"La mesa {mesa.Numero} no está activa");

            // Verificar que la mesa tenga capacidad suficiente
            if (mesa.Capacidad < request.CantidadPersonas)
                throw new Exception($"La mesa {mesa.Numero} tiene capacidad para {mesa.Capacidad} personas, pero se solicitan {request.CantidadPersonas}");

            // Calcular la fecha y hora de inicio y fin de la reservación
            DateTime fechaHoraInicio = request.FechaReservacion.Date.Add(request.HoraInicio);
            DateTime fechaHoraFin = fechaHoraInicio.AddMinutes(request.DuracionMinutos);

            // Verificar que no haya otra reservación para la misma mesa en el mismo horario
            bool existeConflicto = await _context.Reservaciones
                .Where(r => r.MesaId == request.MesaId &&
                            r.Estado != EstadoReservacion.Cancelada &&
                            r.Estado != EstadoReservacion.Completada &&
                            r.Estado != EstadoReservacion.NoShow)
                .AnyAsync(r => (fechaHoraInicio >= r.FechaReservacion && fechaHoraInicio < r.FechaReservacion.AddMinutes(r.DuracionEstimada)) ||
                               (fechaHoraFin > r.FechaReservacion && fechaHoraFin <= r.FechaReservacion.AddMinutes(r.DuracionEstimada)) ||
                               (fechaHoraInicio <= r.FechaReservacion && fechaHoraFin >= r.FechaReservacion.AddMinutes(r.DuracionEstimada)),
                          cancellationToken);

            if (existeConflicto)
                throw new Exception($"Ya existe una reservación para la mesa {mesa.Numero} en el horario solicitado");

            // Obtener el ID del usuario actual (si está autenticado)
            var usuarioId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : null;

            // Crear la reservación
            var reservacion = new Reservacion
            {
                MesaId = request.MesaId,
                NombreCliente = request.NombreCliente,
                Telefono = request.TelefonoCliente,
                Email = request.EmailCliente,
                NumeroPersonas = request.CantidadPersonas,
                FechaReservacion = fechaHoraInicio,
                DuracionEstimada = request.DuracionMinutos,
                Estado = EstadoReservacion.Pendiente,
                Notas = request.Notas,
                UsuarioId = usuarioId,
                FechaCreacion = DateTime.Now
            };

            _context.Reservaciones.Add(reservacion);
            await _context.SaveChangesAsync(cancellationToken);

            return reservacion.Id;
        }
    }
} 