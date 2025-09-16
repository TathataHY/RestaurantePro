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
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesas;

/// <summary>
/// Handler para obtener mesas con filtros opcionales y paginación
/// </summary>
public class ObtenerMesasQueryHandler : IRequestHandler<ObtenerMesasQuery, Result<PaginatedList<MesaDto>>>
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

    public async Task<Result<PaginatedList<MesaDto>>> Handle(ObtenerMesasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo mesas con filtros: Estado={Estado}, Ubicacion={Ubicacion}, CapacidadMinima={CapacidadMinima}, Page={PageNumber}, Size={PageSize}",
                request.Estado, request.Ubicacion, request.CapacidadMinima, request.PageNumber, request.PageSize);

            // Obtener todas las mesas
            List<Mesa> mesas = (await _mesaRepository.ObtenerTodasAsync()).ToList();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(request.Estado) && !request.Estado.Equals("Todas", StringComparison.OrdinalIgnoreCase))
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

            // Ordenar por número de mesa para mostrar secuencialmente
            mesas = mesas.OrderBy(m => m.Numero).ToList();

            // Aplicar paginación
            var totalCount = mesas.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            
            var mesasPaginadas = mesas
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Mapear a DTOs
            var mesasDto = _mapper.Map<List<MesaDto>>(mesasPaginadas);

            // Crear resultado paginado
            var resultado = new PaginatedList<MesaDto>(
                mesasDto,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            _logger.LogInformation("✅ Se obtuvieron {Count} mesas exitosamente. Página {Page}/{TotalPages}",
                mesasDto.Count, request.PageNumber, totalPages);

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener mesas");
            return Result.Failure<PaginatedList<MesaDto>>($"Error interno: {ex.Message}");
        }
    }

} 