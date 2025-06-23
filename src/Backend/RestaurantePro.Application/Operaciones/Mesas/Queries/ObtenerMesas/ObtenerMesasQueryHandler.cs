using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesas;

/// <summary>
/// Handler para obtener todas las mesas con filtros opcionales
/// </summary>
public class ObtenerMesasQueryHandler : IRequestHandler<ObtenerMesasQuery, Result<List<MesaDto>>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerMesasQueryHandler> _logger;

    public ObtenerMesasQueryHandler(
        IMesaRepository mesaRepository,
        IMapper mapper,
        ILogger<ObtenerMesasQueryHandler> logger)
    {
        _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<MesaDto>>> Handle(ObtenerMesasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo mesas con filtros: Estado={Estado}, Ubicacion={Ubicacion}, CapacidadMinima={CapacidadMinima}",
                request.Estado, request.Ubicacion, request.CapacidadMinima);

            // Obtener todas las mesas
            var mesas = await _mesaRepository.ObtenerTodasAsync();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(request.Estado))
            {
                if (Enum.TryParse<EstadoMesa>(request.Estado, true, out var estadoEnum))
                {
                    mesas = mesas.Where(m => m.Estado == estadoEnum).ToList();
                }
                else
                {
                    _logger.LogWarning($"Filtro de estado inválido: {request.Estado}");
                    mesas = new List<Mesa>(); // Si el filtro es inválido, no retorna nada
                }
            }
            if (!string.IsNullOrWhiteSpace(request.Ubicacion))
            {
                mesas = mesas.Where(m => m.Ubicacion.Equals(request.Ubicacion, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (request.CapacidadMinima.HasValue)
            {
                mesas = mesas.Where(m => m.Capacidad >= request.CapacidadMinima.Value).ToList();
            }

            // Mapear a DTOs
            var mesasDto = _mapper.Map<List<MesaDto>>(mesas);

            _logger.LogInformation("✅ Se obtuvieron {Count} mesas exitosamente", mesasDto.Count);

            return Result.Success(mesasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener mesas");
            return Result.Failure<List<MesaDto>>($"Error interno: {ex.Message}");
        }
    }
} 