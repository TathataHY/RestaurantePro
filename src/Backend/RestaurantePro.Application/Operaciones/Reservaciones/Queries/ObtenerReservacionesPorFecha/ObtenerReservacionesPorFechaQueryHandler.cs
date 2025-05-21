using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPorFecha
{
    /// <summary>
    /// Handler para la consulta ObtenerReservacionesPorFechaQuery
    /// </summary>
    public class ObtenerReservacionesPorFechaQueryHandler : IRequestHandler<ObtenerReservacionesPorFechaQuery, Result<List<ReservacionDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IReservacionRepository _reservacionRepository;
        private readonly IMesaRepository _mesaRepository;
        private readonly IClienteRepository _clienteRepository;

        public ObtenerReservacionesPorFechaQueryHandler(
            IMapper mapper,
            IReservacionRepository reservacionRepository,
            IMesaRepository mesaRepository,
            IClienteRepository clienteRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _reservacionRepository = reservacionRepository ?? throw new ArgumentNullException(nameof(reservacionRepository));
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
        }

        public async Task<Result<List<ReservacionDto>>> Handle(ObtenerReservacionesPorFechaQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener reservaciones para la fecha especificada
                var reservaciones = await _reservacionRepository.ObtenerPorFechaAsync(request.Fecha, cancellationToken);
                
                if (!reservaciones.Any())
                {
                    return Result<List<ReservacionDto>>.Success(new List<ReservacionDto>());
                }
                
                // Mapear a DTOs
                var reservacionesDto = _mapper.Map<List<ReservacionDto>>(reservaciones);
                
                // Obtener información adicional para cada reservación
                var mesaIds = reservaciones.Select(r => r.MesaId).Distinct().ToList();
                var clienteIds = reservaciones.Select(r => r.ClienteId).Distinct().ToList();
                
                // Obtener mesas
                var mesas = new Dictionary<Guid, int>();
                foreach (var mesaId in mesaIds)
                {
                    try
                    {
                        var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                        mesas.Add(mesaId, mesa.Numero);
                    }
                    catch (KeyNotFoundException)
                    {
                        // Ignorar si la mesa no existe
                    }
                }
                
                // Obtener clientes
                var clientes = new Dictionary<Guid, string>();
                foreach (var clienteId in clienteIds)
                {
                    var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
                    if (cliente != null)
                    {
                        clientes.Add(clienteId, cliente.Nombre.NombreCompleto);
                    }
                }
                
                // Asignar información adicional a los DTOs
                foreach (var reservacionDto in reservacionesDto)
                {
                    if (mesas.TryGetValue(reservacionDto.MesaId, out var numeroMesa))
                    {
                        reservacionDto.NumeroMesa = numeroMesa;
                    }
                    
                    if (clientes.TryGetValue(reservacionDto.ClienteId, out var nombreCliente))
                    {
                        reservacionDto.NombreCliente = nombreCliente;
                    }
                }
                
                return Result<List<ReservacionDto>>.Success(reservacionesDto);
            }
            catch (Exception ex)
            {
                return Result<List<ReservacionDto>>.Failure($"Error al obtener las reservaciones: {ex.Message}");
            }
        }
    }
} 