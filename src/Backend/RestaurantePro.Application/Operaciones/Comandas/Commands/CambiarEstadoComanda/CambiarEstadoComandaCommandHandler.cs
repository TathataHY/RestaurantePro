using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Invalidation;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CambiarEstadoComanda;

public class CambiarEstadoComandaCommandHandler : IRequestHandler<CambiarEstadoComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CambiarEstadoComandaCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CambiarEstadoComandaCommandHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<CambiarEstadoComandaCommandHandler> logger,
        ICacheService cacheService)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<ComandaDto>> Handle(CambiarEstadoComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Cambiando estado de comanda {ComandaId} a {NuevoEstado}", 
            request.ComandaId, request.NuevoEstado);

        // 1. Buscar la comanda con Items (requerido para validar invariantes/subtotales)
        var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, incluirItems: true, cancellationToken);
        if (comanda == null)
            return Result.Failure<ComandaDto>($"No se encontró la comanda con ID {request.ComandaId}");

        // 2. Validar que no esté cancelada o finalizada
        if (comanda.Estado == EstadoComanda.Cancelada)
            return Result.Failure<ComandaDto>("No se puede cambiar el estado de una comanda cancelada");
        if (comanda.Estado == EstadoComanda.Finalizada)
            return Result.Failure<ComandaDto>("No se puede cambiar el estado de una comanda finalizada");

        // 3. Cambiar estado según el nuevo estado solicitado
        bool cambioExitoso = false;
        try
        {
            switch (request.NuevoEstado.ToLower())
            {
                case "enproceso":
                    cambioExitoso = comanda.MarcarEnPreparacion();
                    break;
                case "lista":
                    cambioExitoso = comanda.MarcarLista();
                    break;
                case "entregada":
                    cambioExitoso = comanda.MarcarEntregada();
                    break;
                case "finalizada":
                    cambioExitoso = comanda.MarcarPagada();
                    break;
                default:
                    return Result.Failure<ComandaDto>($"Estado '{request.NuevoEstado}' no válido");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de comanda");
            return Result.Failure<ComandaDto>($"Error al cambiar estado: {ex.Message}");
        }

        if (!cambioExitoso)
            return Result.Failure<ComandaDto>($"No se puede cambiar de '{comanda.Estado}' a '{request.NuevoEstado}'");

        // 4. Guardar cambios
        await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);

        // 5. Invalidar caché relevante en background para no bloquear el comando
        _ = Task.Run(() =>
        {
            try
            {
                _cacheService.InvalidatePattern("ObtenerComandasPaginadasQuery_");
                _cacheService.InvalidatePattern("ObtenerComandasPorMesaQuery_");
                _cacheService.InvalidateForEntity("ObtenerComandaPorIdQuery_", comanda.Id);
            }
            catch
            {
                // Best-effort: no bloquear por fallos de invalidación
            }
        }, cancellationToken);

        // 6. Mapear a DTO y retornar
        var dto = _mapper.Map<ComandaDto>(comanda);
        return Result.Success(dto);
    }
} 