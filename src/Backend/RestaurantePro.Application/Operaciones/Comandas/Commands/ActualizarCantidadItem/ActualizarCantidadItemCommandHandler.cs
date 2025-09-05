using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarCantidadItem;

public class ActualizarCantidadItemCommandHandler : IRequestHandler<ActualizarCantidadItemCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarCantidadItemCommandHandler> _logger;

    public ActualizarCantidadItemCommandHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<ActualizarCantidadItemCommandHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(ActualizarCantidadItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("✏️ Actualizando cantidad del item {ItemId} en comanda {ComandaId} a {Cantidad}",
            request.ItemId, request.ComandaId, request.NuevaCantidad);

        // 1. Buscar la comanda
        var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, true, cancellationToken);
        if (comanda == null)
            return Result.Failure<ComandaDto>($"No se encontró la comanda con ID {request.ComandaId}");

        // 2. Validar estado editable
        if (comanda.Estado != EstadoComanda.Creada && comanda.Estado != EstadoComanda.EnProceso)
            return Result.Failure<ComandaDto>($"No se puede actualizar un item en estado '{comanda.Estado}'");

        // 3. Buscar el item
        var item = comanda.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item == null)
            return Result.Failure<ComandaDto>($"No se encontró el item con ID {request.ItemId} en la comanda");

        // 4. Actualizar cantidad (o eliminar si llega 0)
        try
        {
            if (request.NuevaCantidad <= 0)
            {
                comanda.RemoverProducto(request.ItemId);
            }
            else
            {
                item.ActualizarCantidad(request.NuevaCantidad);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cantidad del item {ItemId}", request.ItemId);
            return Result.Failure<ComandaDto>($"Error al actualizar cantidad: {ex.Message}");
        }

        // 5. Guardar cambios
        await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);

        // 6. Mapear a DTO
        var dto = _mapper.Map<ComandaDto>(comanda);
        return Result.Success(dto);
    }
}



