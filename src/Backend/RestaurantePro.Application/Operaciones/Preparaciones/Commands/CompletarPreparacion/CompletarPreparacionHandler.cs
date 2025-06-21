using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CompletarPreparacion;

/// <summary>
/// Handler para completar una preparación
/// </summary>
public class CompletarPreparacionHandler : IRequestHandler<CompletarPreparacionCommand, Result<PreparacionDto>>
{
    private readonly IPreparacionRepository _preparacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CompletarPreparacionHandler> _logger;

    public CompletarPreparacionHandler(
        IPreparacionRepository preparacionRepository,
        IMapper mapper,
        ILogger<CompletarPreparacionHandler> logger)
    {
        _preparacionRepository = preparacionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PreparacionDto>> Handle(
        CompletarPreparacionCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("✅ Iniciando completado de preparación {PreparacionId}", request.Id);

        try
        {
            // Buscar la preparación existente
            var preparacion = await _preparacionRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (preparacion == null)
            {
                _logger.LogWarning("⚠️ Preparación no encontrada: {PreparacionId}", request.Id);
                return Result.Failure<PreparacionDto>("Preparación no encontrada");
            }

            // Validar que la preparación esté en estado disponible
            if (preparacion.Estado != EstadoPreparacion.Disponible)
            {
                _logger.LogWarning("⚠️ No se puede completar una preparación en estado {Estado}: {PreparacionId}", 
                    preparacion.Estado, request.Id);
                return Result.Failure<PreparacionDto>($"No se puede completar una preparación en estado {preparacion.Estado}");
            }

            // Marcar como agotada (equivalente a completar en el contexto de preparaciones)
            preparacion.ConsumirCantidad(preparacion.CantidadDisponible);

            // Guardar cambios
            await _preparacionRepository.ActualizarAsync(preparacion, cancellationToken);

            _logger.LogInformation("✅ Preparación completada exitosamente: {PreparacionId}", request.Id);

            var preparacionDto = _mapper.Map<PreparacionDto>(preparacion);
            return Result.Success(preparacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al completar preparación {PreparacionId}", request.Id);
            return Result.Failure<PreparacionDto>($"Error al completar la preparación: {ex.Message}");
        }
    }
} 