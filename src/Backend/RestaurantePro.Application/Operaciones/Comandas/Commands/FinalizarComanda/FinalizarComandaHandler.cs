using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.FinalizarComanda;

/// <summary>
/// 🍽️ Handler para finalizar comandas
/// </summary>
public class FinalizarComandaHandler : IRequestHandler<FinalizarComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FinalizarComandaHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public FinalizarComandaHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<FinalizarComandaHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
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
            _logger.LogInformation("🔍 Estado actual de comanda {ComandaId}: {Estado}", request.ComandaId, comanda.Estado);
            
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
                // Usar el método de dominio normal para cambiar el estado
                comanda.ActualizarEstado(EstadoComanda.Finalizada);
                _logger.LogInformation("Estado actualizado a Finalizada usando método de dominio");
                
                // Guardar cambios
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Cambios guardados exitosamente");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError("❌ Error al finalizar comanda {ComandaId}: {Error}", 
                    request.ComandaId, ex.Message);
                return Result.Failure<ComandaDto>($"No se puede cambiar el estado");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError("❌ Error de concurrencia al finalizar comanda {ComandaId}: {Error}", 
                    request.ComandaId, ex.Message);
                return Result.Failure<ComandaDto>($"Error de concurrencia al finalizar comanda");
            }

            // 5. Agregar observaciones si las hay - usando la propiedad directamente
            if (!string.IsNullOrWhiteSpace(request.ObservacionesFinalizacion))
            {
                // TODO: Implementar método AgregarObservacion en la entidad Comanda
                // Por ahora comentamos esta funcionalidad
                // comanda.AgregarObservacion($"[FINALIZACIÓN] {request.ObservacionesFinalizacion}");
                _logger.LogInformation("📝 Observaciones de finalización: {Observaciones}", request.ObservacionesFinalizacion);
            }

            // 6. Mapear a DTO (ya no necesitamos guardar de nuevo)
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