using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.IniciarPreparacion;

/// <summary>
/// Handler para iniciar una preparación
/// </summary>
public class IniciarPreparacionHandler : IRequestHandler<IniciarPreparacionCommand, Result<PreparacionDto>>
{
    private readonly IPreparacionRepository _preparacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<IniciarPreparacionHandler> _logger;

    public IniciarPreparacionHandler(
        IPreparacionRepository preparacionRepository,
        IMapper mapper,
        ILogger<IniciarPreparacionHandler> logger)
    {
        _preparacionRepository = preparacionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PreparacionDto>> Handle(
        IniciarPreparacionCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 Iniciando preparación {PreparacionId}", request.Id);

        try
        {
            // Buscar la preparación existente
            var preparacion = await _preparacionRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (preparacion == null)
            {
                _logger.LogWarning("⚠️ Preparación no encontrada: {PreparacionId}", request.Id);
                return Result.Failure<PreparacionDto>("Preparación no encontrada");
            }

            // Validar que la preparación esté en estado preparando
            if (preparacion.Estado != EstadoPreparacion.Preparando)
            {
                _logger.LogWarning("⚠️ No se puede iniciar una preparación en estado {Estado}: {PreparacionId}", 
                    preparacion.Estado, request.Id);
                return Result.Failure<PreparacionDto>($"No se puede iniciar una preparación en estado {preparacion.Estado}");
            }

            // Marcar como disponible (equivalente a iniciar en el contexto de preparaciones)
            preparacion.MarcarComoDisponible();

            // Guardar cambios
            await _preparacionRepository.ActualizarAsync(preparacion, cancellationToken);

            _logger.LogInformation("✅ Preparación iniciada exitosamente: {PreparacionId}", request.Id);

            var preparacionDto = _mapper.Map<PreparacionDto>(preparacion);
            return Result.Success(preparacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al iniciar preparación {PreparacionId}", request.Id);
            return Result.Failure<PreparacionDto>($"Error al iniciar la preparación: {ex.Message}");
        }
    }
} 