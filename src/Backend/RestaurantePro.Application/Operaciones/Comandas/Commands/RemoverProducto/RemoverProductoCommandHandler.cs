using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.RemoverProducto;

public class RemoverProductoCommandHandler : IRequestHandler<RemoverProductoCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RemoverProductoCommandHandler> _logger;

    public RemoverProductoCommandHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<RemoverProductoCommandHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(RemoverProductoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("➖ Removiendo item {ItemId} de comanda {ComandaId} - Cantidad: {Cantidad}", 
            request.ItemId, request.ComandaId, request.Cantidad);

        // 1. Buscar la comanda
        var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, true, cancellationToken);
        if (comanda == null)
            return Result.Failure<ComandaDto>($"No se encontró la comanda con ID {request.ComandaId}");

        // 2. Validar estado editable
        if (comanda.Estado != EstadoComanda.Creada && comanda.Estado != EstadoComanda.EnProceso)
            return Result.Failure<ComandaDto>($"No se puede remover productos de una comanda en estado '{comanda.Estado}'");

        // 3. Buscar el item
        var item = comanda.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item == null)
            return Result.Failure<ComandaDto>($"No se encontró el item con ID {request.ItemId} en la comanda");

        // 4. Validar cantidad a remover
        if (request.Cantidad <= 0)
            return Result.Failure<ComandaDto>("La cantidad a remover debe ser mayor a cero");
        if (request.Cantidad > item.Cantidad)
            return Result.Failure<ComandaDto>($"No se puede remover más cantidad ({request.Cantidad}) de la disponible ({item.Cantidad})");

        // 5. Remover el producto
        try
        {
            if (request.Cantidad == item.Cantidad)
            {
                // Remover todo el item
                comanda.RemoverProducto(request.ItemId);
            }
            else
            {
                // Remover cantidad parcial - primero remover el item completo y luego agregar la cantidad restante
                comanda.RemoverProducto(request.ItemId);
                comanda.AgregarProducto(item.ProductoId, item.Cantidad - request.Cantidad, item.PrecioUnitario, item.Observaciones);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover producto de la comanda");
            return Result.Failure<ComandaDto>($"Error al remover producto: {ex.Message}");
        }

        // 6. Guardar cambios
        await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);

        // 7. Mapear a DTO y retornar
        var dto = _mapper.Map<ComandaDto>(comanda);
        return Result.Success(dto);
    }
} 