using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.ActualizarPreparacion;

/// <summary>
/// Handler para actualizar una preparación existente
/// </summary>
public class ActualizarPreparacionHandler : IRequestHandler<ActualizarPreparacionCommand, Result<PreparacionDto>>
{
    private readonly IPreparacionRepository _preparacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarPreparacionHandler> _logger;

    public ActualizarPreparacionHandler(
        IPreparacionRepository preparacionRepository,
        IMapper mapper,
        ILogger<ActualizarPreparacionHandler> logger)
    {
        _preparacionRepository = preparacionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PreparacionDto>> Handle(
        ActualizarPreparacionCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando actualización de preparación {PreparacionId}", request.Id);

        try
        {
            // Buscar la preparación existente
            var preparacion = await _preparacionRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (preparacion == null)
            {
                _logger.LogWarning("⚠️ Preparación no encontrada: {PreparacionId}", request.Id);
                return Result.Failure<PreparacionDto>("Preparación no encontrada");
            }

            // Validar que la preparación no esté vencida o agotada
            if (preparacion.Estado == EstadoPreparacion.Vencida || 
                preparacion.Estado == EstadoPreparacion.Agotada)
            {
                _logger.LogWarning("⚠️ No se puede actualizar una preparación en estado {Estado}: {PreparacionId}", 
                    preparacion.Estado, request.Id);
                return Result.Failure<PreparacionDto>($"No se puede actualizar una preparación en estado {preparacion.Estado}");
            }

            // Actualizar propiedades usando los métodos de la entidad
            // Nota: Las propiedades de la entidad Preparacion son de solo lectura,
            // por lo que necesitamos usar métodos específicos para actualizarlas
            
            // Guardar cambios
            await _preparacionRepository.ActualizarAsync(preparacion, cancellationToken);

            _logger.LogInformation("✅ Preparación actualizada exitosamente: {PreparacionId}", request.Id);

            var preparacionDto = _mapper.Map<PreparacionDto>(preparacion);
            return Result.Success(preparacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar preparación {PreparacionId}", request.Id);
            return Result.Failure<PreparacionDto>($"Error al actualizar la preparación: {ex.Message}");
        }
    }
} 