using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CancelarPreparacion;

/// <summary>
/// Handler para cancelar una preparación
/// </summary>
public class CancelarPreparacionHandler : IRequestHandler<CancelarPreparacionCommand, Result<PreparacionDto>>
{
    private readonly IPreparacionRepository _preparacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelarPreparacionHandler> _logger;

    public CancelarPreparacionHandler(
        IPreparacionRepository preparacionRepository,
        IMapper mapper,
        ILogger<CancelarPreparacionHandler> logger)
    {
        _preparacionRepository = preparacionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PreparacionDto>> Handle(
        CancelarPreparacionCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("❌ Iniciando cancelación de preparación {PreparacionId}", request.Id);

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
                _logger.LogWarning("⚠️ No se puede cancelar una preparación en estado {Estado}: {PreparacionId}", 
                    preparacion.Estado, request.Id);
                return Result.Failure<PreparacionDto>($"No se puede cancelar una preparación en estado {preparacion.Estado}");
            }

            // Marcar como vencida (equivalente a cancelar en el contexto de preparaciones)
            preparacion.MarcarComoVencida();

            // Guardar cambios
            await _preparacionRepository.ActualizarAsync(preparacion, cancellationToken);

            _logger.LogInformation("✅ Preparación cancelada exitosamente: {PreparacionId}", request.Id);

            var preparacionDto = _mapper.Map<PreparacionDto>(preparacion);
            return Result.Success(preparacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al cancelar preparación {PreparacionId}", request.Id);
            return Result.Failure<PreparacionDto>($"Error al cancelar la preparación: {ex.Message}");
        }
    }
} 