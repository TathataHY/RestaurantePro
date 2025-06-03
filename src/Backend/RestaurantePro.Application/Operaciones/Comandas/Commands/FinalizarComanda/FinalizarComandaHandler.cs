using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.FinalizarComanda;

/// <summary>
/// 🍽️ Handler para finalizar comandas
/// </summary>
public class FinalizarComandaHandler : IRequestHandler<FinalizarComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FinalizarComandaHandler> _logger;

    public FinalizarComandaHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<FinalizarComandaHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(
        FinalizarComandaCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🍽️ Iniciando finalización de comanda {ComandaId} por usuario {UsuarioId}", 
            request.ComandaId, request.UsuarioId);

        try
        {
            // Obtener la comanda
            var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, cancellationToken);
            if (comanda == null)
            {
                _logger.LogWarning("❌ Comanda {ComandaId} no encontrada", request.ComandaId);
                return Result.Failure<ComandaDto>("Comanda no encontrada");
            }

            // Validar estado
            if (comanda.Estado == EstadoComanda.Finalizada)
            {
                _logger.LogWarning("⚠️ Comanda {ComandaId} ya está finalizada", request.ComandaId);
                return Result.Failure<ComandaDto>("La comanda ya está finalizada");
            }

            if (comanda.Estado == EstadoComanda.Cancelada)
            {
                _logger.LogWarning("⚠️ Comanda {ComandaId} está cancelada", request.ComandaId);
                return Result.Failure<ComandaDto>("No se puede finalizar una comanda cancelada");
            }

            // Validar que tenga items
            if (comanda.Items == null || !comanda.Items.Any())
            {
                _logger.LogWarning("⚠️ Comanda {ComandaId} no tiene items", request.ComandaId);
                return Result.Failure<ComandaDto>("No se puede finalizar una comanda sin items");
            }

            // 4. Finalizar la comanda usando el método correcto
            try
            {
                comanda.ActualizarEstado(EstadoComanda.Finalizada);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError("❌ Error al finalizar comanda {ComandaId}: {Error}", 
                    request.ComandaId, ex.Message);
                return Result.Failure<ComandaDto>($"No se puede cambiar el estado");
            }

            // 5. Agregar observaciones si las hay - usando la propiedad directamente
            if (!string.IsNullOrWhiteSpace(request.ObservacionesFinalizacion))
            {
                // TODO: Implementar método AgregarObservacion en la entidad Comanda
                // Por ahora comentamos esta funcionalidad
                // comanda.AgregarObservacion($"[FINALIZACIÓN] {request.ObservacionesFinalizacion}");
                _logger.LogInformation("📝 Observaciones de finalización: {Observaciones}", request.ObservacionesFinalizacion);
            }

            // 6. Guardar cambios
            await _comandaRepository.ActualizarAsync(comanda, cancellationToken);

            // 7. Mapear a DTO
            var comandaDto = _mapper.Map<ComandaDto>(comanda);

            _logger.LogInformation("✅ Comanda {ComandaId} finalizada exitosamente. Items: {TotalItems}, Total: {Total:C}", 
                request.ComandaId, comanda.Items.Count, comanda.Total?.Total ?? 0);

            // 8. Log adicional si hay notificación de mesero
            if (request.NotificarMesero)
            {
                _logger.LogInformation("📱 Se enviará notificación al mesero para comanda {ComandaId}", 
                    request.ComandaId);
            }

            return Result.Success(comandaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al finalizar comanda {ComandaId}", request.ComandaId);
            return Result.Failure<ComandaDto>($"Error interno al finalizar comanda: {ex.Message}");
        }
    }
} 