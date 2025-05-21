using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion
{
    /// <summary>
    /// Handler para el comando ConfirmarReservacionCommand
    /// </summary>
    public class ConfirmarReservacionCommandHandler : IRequestHandler<ConfirmarReservacionCommand, Result<ReservacionDto>>
    {
        private readonly IMapper _mapper;
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IDateTime _dateTime;

        public ConfirmarReservacionCommandHandler(
            IMapper mapper,
            IReservacionRepository reservacionRepository,
            IMesaRepository mesaRepository,
            IDateTime dateTime)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        }

        public async Task<Result<ReservacionDto>> Handle(ConfirmarReservacionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener la reservación
                var reservacion = await _reservacionRepository.ObtenerPorIdAsync(request.ReservacionId, cancellationToken);
                
                if (reservacion == null)
                {
                    return Result<ReservacionDto>.Failure($"No se encontró la reservación con ID {request.ReservacionId}");
                }
                
                // Validar el estado actual de la reservación
                if (reservacion.Estado != EstadoReservacion.Pendiente)
                {
                    return Result<ReservacionDto>.Failure($"Solo se pueden confirmar reservaciones en estado Pendiente. Estado actual: {reservacion.Estado}");
                }
                
                // Confirmar la reservación
                reservacion.Confirmar(request.CodigoConfirmacion);
                
                // Actualizar observaciones si se proporcionaron
                if (!string.IsNullOrEmpty(request.Observaciones))
                {
                    reservacion.ActualizarObservaciones(request.Observaciones);
                }
                
                // Guardar cambios
                await _reservacionRepository.ActualizarAsync(reservacion);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                // Mapear a DTO
                var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);
                
                // Obtener y asignar información adicional (número de mesa)
                try
                {
                    var mesa = await _mesaRepository.ObtenerPorIdAsync(reservacion.MesaId);
                    reservacionDto.NumeroMesa = mesa.Numero;
                }
                catch (KeyNotFoundException)
                {
                    // No hacer nada si no se encuentra la mesa
                }
                
                return Result<ReservacionDto>.Success(reservacionDto);
            }
            catch (InvalidOperationException ex)
            {
                return Result<ReservacionDto>.Failure($"Error de operación: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<ReservacionDto>.Failure($"Error al confirmar la reservación: {ex.Message}");
            }
        }
    }
} 