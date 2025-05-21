using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion
{
    /// <summary>
    /// Handler para el comando CrearReservacionCommand
    /// </summary>
    public class CrearReservacionCommandHandler : IRequestHandler<CrearReservacionCommand, Result<ReservacionDto>>
    {
        private readonly IMapper _mapper;
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public CrearReservacionCommandHandler(
            IMapper mapper,
            IReservacionRepository reservacionRepository,
            IMesaRepository mesaRepository,
            IClienteRepository clienteRepository,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<Result<ReservacionDto>> Handle(CrearReservacionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si el cliente existe
                var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
                if (cliente == null)
                {
                    return Result<ReservacionDto>.Failure("El cliente especificado no existe.");
                }

                // Si no se especificó una mesa, buscar una disponible según la cantidad de comensales
                Guid mesaId;
                if (!request.MesaId.HasValue)
                {
                    var mesasDisponibles = await _reservacionRepository.ObtenerMesasDisponiblesAsync(
                        request.FechaReservacion,
                        request.FechaReservacion.TimeOfDay,
                        request.CantidadComensales,
                        request.DuracionMinutos,
                        cancellationToken);
                    
                    if (!mesasDisponibles.Any())
                    {
                        return Result<ReservacionDto>.Failure("No hay mesas disponibles para la fecha, hora y cantidad de comensales solicitados.");
                    }
                    
                    // Seleccionar la mesa con capacidad más cercana a la cantidad de comensales
                    var mesasConCapacidad = new System.Collections.Generic.List<(Guid MesaId, int Capacidad)>();
                    foreach (var id in mesasDisponibles)
                    {
                        var mesa = await _mesaRepository.ObtenerPorIdAsync(id);
                        mesasConCapacidad.Add((id, mesa.Capacidad));
                    }
                    
                    mesaId = mesasConCapacidad
                        .OrderBy(m => m.Capacidad - request.CantidadComensales)
                        .First().MesaId;
                }
                else
                {
                    mesaId = request.MesaId.Value;
                    
                    // Verificar si la mesa existe
                    try
                    {
                        var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                        
                        // Verificar si la mesa tiene capacidad suficiente
                        if (mesa.Capacidad < request.CantidadComensales)
                        {
                            return Result<ReservacionDto>.Failure("La mesa seleccionada no tiene capacidad suficiente para la cantidad de comensales.");
                        }
                        
                        // Verificar si la mesa está disponible
                        var disponible = await _reservacionRepository.VerificarDisponibilidadMesaAsync(
                            mesaId,
                            request.FechaReservacion,
                            request.FechaReservacion.TimeOfDay,
                            request.DuracionMinutos,
                            cancellationToken);
                        
                        if (!disponible)
                        {
                            return Result<ReservacionDto>.Failure("La mesa seleccionada no está disponible en el horario solicitado.");
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        return Result<ReservacionDto>.Failure("La mesa especificada no existe.");
                    }
                }
                
                // Crear la reservación
                var reservacion = Reservacion.Crear(
                    mesaId,
                    request.ClienteId,
                    request.FechaReservacion,
                    TimeSpan.FromMinutes(request.DuracionMinutos),
                    request.CantidadComensales,
                    request.Telefono,
                    request.Email,
                    request.Observaciones,
                    request.OcasionEspecial,
                    request.SolicitudesEspeciales);
                
                // Guardar la reservación
                await _reservacionRepository.AgregarAsync(reservacion);
                await _reservacionRepository.GuardarCambiosAsync(cancellationToken);
                
                // Mapear a DTO
                var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);
                
                // Obtener y asignar información adicional
                try
                {
                    var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                    reservacionDto.NumeroMesa = mesa.Numero;
                    
                    reservacionDto.NombreCliente = $"{cliente.Nombre.NombreCompleto}";
                }
                catch (Exception)
                {
                    // Ignorar errores al obtener información adicional
                }
                
                return Result<ReservacionDto>.Success(reservacionDto);
            }
            catch (Exception ex)
            {
                return Result<ReservacionDto>.Failure($"Error al crear la reservación: {ex.Message}");
            }
        }
    }
} 