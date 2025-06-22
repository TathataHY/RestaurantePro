using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarProducto;

public class AgregarProductoCommandHandler : IRequestHandler<AgregarProductoCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AgregarProductoCommandHandler> _logger;

    public AgregarProductoCommandHandler(
        IComandaRepository comandaRepository,
        IProductoRepository productoRepository,
        IMapper mapper,
        ILogger<AgregarProductoCommandHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _productoRepository = productoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ComandaDto>> Handle(AgregarProductoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("➕ Agregando producto {ProductoId} x{Cantidad} a comanda {ComandaId}", 
            request.ProductoId, request.Cantidad, request.ComandaId);

        // 1. Buscar la comanda
        var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId, true, cancellationToken);
        if (comanda == null)
            return Result.Failure<ComandaDto>($"No se encontró la comanda con ID {request.ComandaId}");

        // 2. Validar estado editable
        if (comanda.Estado != EstadoComanda.Creada && comanda.Estado != EstadoComanda.EnProceso)
            return Result.Failure<ComandaDto>($"No se puede agregar productos a una comanda en estado '{comanda.Estado}'");

        // 3. Buscar y validar el producto
        var producto = await _productoRepository.ObtenerPorIdAsync(request.ProductoId, cancellationToken);
        if (producto == null)
            return Result.Failure<ComandaDto>($"No se encontró el producto con ID {request.ProductoId}");
        if (!producto.Activo)
            return Result.Failure<ComandaDto>($"El producto '{producto.Nombre}' no está activo");

        // 4. Agregar producto a la comanda
        try
        {
            comanda.AgregarItem(
                producto.Id,
                producto.Nombre,
                request.Cantidad,
                producto.Precio,
                request.Observaciones
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar producto a la comanda");
            return Result.Failure<ComandaDto>($"Error al agregar producto '{producto.Nombre}': {ex.Message}");
        }

        // 5. Guardar cambios
        await _comandaRepository.ActualizarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);

        // 6. Mapear a DTO y retornar
        var dto = _mapper.Map<ComandaDto>(comanda);
        return Result.Success(dto);
    }
} 