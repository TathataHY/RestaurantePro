using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId
{
    /// <summary>
    /// Manejador para la consulta ObtenerComandaPorIdQuery
    /// </summary>
    public class ObtenerComandaPorIdQueryHandler : IRequestHandler<ObtenerComandaPorIdQuery, Result<ComandaDto>>
    {
        private readonly IMapper _mapper;
        private readonly IComandaRepository _comandaRepository;
        private readonly IMesaRepository _mesaRepository;

        public ObtenerComandaPorIdQueryHandler(
            IMapper mapper,
            IComandaRepository comandaRepository,
            IMesaRepository mesaRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
        }

        public async Task<Result<ComandaDto>> Handle(ObtenerComandaPorIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener la comanda del repositorio
                var comanda = await _comandaRepository.ObtenerPorIdAsync(request.Id, request.IncluirItems, cancellationToken);
                
                if (comanda == null)
                {
                    return Result<ComandaDto>.Failure($"No se encontró la comanda con ID {request.Id}");
                }
                
                // Mapear a DTO
                var comandaDto = _mapper.Map<ComandaDto>(comanda);
                
                // Obtener y asignar información adicional (número de mesa)
                try
                {
                    var mesa = await _mesaRepository.ObtenerPorIdAsync(comanda.MesaId);
                    comandaDto.NumeroMesa = mesa.Numero;
                }
                catch (KeyNotFoundException)
                {
                    // No hacer nada si no se encuentra la mesa
                }
                
                return Result<ComandaDto>.Success(comandaDto);
            }
            catch (Exception ex)
            {
                return Result<ComandaDto>.Failure($"Error al obtener la comanda: {ex.Message}");
            }
        }
    }
} 