using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesaPorId;

/// <summary>
/// Handler para obtener una mesa específica por ID
/// </summary>
public class ObtenerMesaPorIdQueryHandler : IRequestHandler<ObtenerMesaPorIdQuery, Result<MesaDto>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerMesaPorIdQueryHandler> _logger;

    public ObtenerMesaPorIdQueryHandler(
        IMesaRepository mesaRepository,
        IMapper mapper,
        ILogger<ObtenerMesaPorIdQueryHandler> logger)
    {
        _mesaRepository = mesaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<MesaDto>> Handle(ObtenerMesaPorIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obteniendo mesa con ID: {MesaId}", request.Id);

        try
        {
            var mesa = await _mesaRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            
            if (mesa == null)
            {
                _logger.LogWarning("Mesa con ID {MesaId} no encontrada", request.Id);
                return Result.Failure<MesaDto>(new List<string> { "La mesa especificada no existe" });
            }

            var mesaDto = _mapper.Map<MesaDto>(mesa);
            
            _logger.LogInformation("Mesa obtenida exitosamente: {MesaId}", request.Id);
            return Result.Success(mesaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener mesa con ID {MesaId}", request.Id);
            return Result.Failure<MesaDto>(new List<string> { "Error interno al obtener la mesa" });
        }
    }
} 