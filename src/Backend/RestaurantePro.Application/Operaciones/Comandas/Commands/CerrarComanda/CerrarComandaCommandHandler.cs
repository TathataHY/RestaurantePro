using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CerrarComanda;

public class CerrarComandaCommandHandler : IRequestHandler<CerrarComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CerrarComandaCommandHandler> _logger;

    public CerrarComandaCommandHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<CerrarComandaCommandHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(CerrarComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🏁 Cerrando comanda {ComandaId} - Método de pago: {MetodoPago}", 
            request.ComandaId, request.MetodoPago);

        // 1. Buscar la comanda
        var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, true, cancellationToken);
        if (comanda == null)
            return Result.Failure<ComandaDto>($"No se encontró la comanda con ID {request.ComandaId}");

        // 2. Validar estado para cerrar
        if (comanda.Estado == EstadoComanda.Cancelada)
            return Result.Failure<ComandaDto>("No se puede cerrar una comanda cancelada");
        if (comanda.Estado == EstadoComanda.Finalizada)
            return Result.Failure<ComandaDto>("La comanda ya está cerrada");

        // 3. Validar que tenga productos
        if (!comanda.Items.Any())
            return Result.Failure<ComandaDto>("No se puede cerrar una comanda sin productos");

        // 4. Finalizar la comanda
        try
        {
            _logger.LogInformation("🏁 Iniciando transiciones de estado para comanda {ComandaId}. Estado actual: {EstadoActual}", 
                request.ComandaId, comanda.Estado);

            // Realizar todas las transiciones necesarias hasta llegar a Finalizada
            while (comanda.Estado != EstadoComanda.Finalizada)
            {
                _logger.LogInformation("🔄 Estado actual de comanda {ComandaId}: {EstadoActual}", 
                    request.ComandaId, comanda.Estado);
                
                switch (comanda.Estado)
                {
                    case EstadoComanda.Creada:
                        _logger.LogInformation("📝 Transicionando comanda {ComandaId} de Creada a EnProceso", request.ComandaId);
                comanda.MarcarEnPreparacion();
                        break;
                    case EstadoComanda.EnProceso:
                        _logger.LogInformation("📝 Transicionando comanda {ComandaId} de EnProceso a Lista", request.ComandaId);
                comanda.MarcarLista();
                        break;
                    case EstadoComanda.Lista:
                        _logger.LogInformation("📝 Transicionando comanda {ComandaId} de Lista a Entregada", request.ComandaId);
                comanda.MarcarEntregada();
                        break;
                    case EstadoComanda.Entregada:
                        _logger.LogInformation("📝 Transicionando comanda {ComandaId} de Entregada a Finalizada", request.ComandaId);
                        if (!comanda.MarcarPagada())
                        {
                            _logger.LogError("❌ Error al marcar comanda {ComandaId} como pagada", request.ComandaId);
                            return Result.Failure<ComandaDto>("No se pudo cambiar el estado de la comanda a finalizada");
            }
                        break;
                    case EstadoComanda.Finalizada:
                        _logger.LogInformation("✅ Comanda {ComandaId} ya está finalizada", request.ComandaId);
                        break;
                    default:
                        _logger.LogError("❌ Estado no válido para comanda {ComandaId}: {EstadoActual}", 
                            request.ComandaId, comanda.Estado);
                return Result.Failure<ComandaDto>($"No se pudo cerrar la comanda desde el estado {comanda.Estado}");
            }

                _logger.LogInformation("✅ Estado de comanda {ComandaId} después de transición: {EstadoActual}", 
                    request.ComandaId, comanda.Estado);
            }

            _logger.LogInformation("🎉 Comanda {ComandaId} finalizada exitosamente. Estado final: {EstadoFinal}", 
                request.ComandaId, comanda.Estado);

            // Agregar observaciones de cierre si se proporcionan
            if (!string.IsNullOrWhiteSpace(request.Observaciones))
            {
                comanda.ActualizarObservaciones(request.Observaciones);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar la comanda");
            return Result.Failure<ComandaDto>($"Error al cerrar la comanda: {ex.Message}");
        }

        // 5. Guardar cambios
        _logger.LogInformation("💾 Guardando cambios para comanda {ComandaId}. Estado antes de guardar: {EstadoActual}", 
            request.ComandaId, comanda.Estado);
        
        await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);
        
        _logger.LogInformation("✅ Cambios guardados para comanda {ComandaId}. Estado después de guardar: {EstadoActual}", 
            request.ComandaId, comanda.Estado);

        // 6. Mapear a DTO y retornar
        var dto = _mapper.Map<ComandaDto>(comanda);
        _logger.LogInformation("🎯 Comanda {ComandaId} mapeada a DTO. Estado en DTO: {EstadoDTO}", 
            request.ComandaId, dto.Estado);
        
        return Result.Success(dto);
    }
} 