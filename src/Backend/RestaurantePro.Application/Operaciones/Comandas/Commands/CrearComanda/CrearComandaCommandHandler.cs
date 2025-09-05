using MediatR;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Invalidation;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;

public class CrearComandaCommandHandler : IRequestHandler<CrearComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearComandaCommandHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IMesaRepository _mesaRepository;

    public CrearComandaCommandHandler(
        IComandaRepository comandaRepository,
        IProductoRepository productoRepository,
        IMapper mapper,
        ILogger<CrearComandaCommandHandler> logger,
        ICacheService cacheService,
        IMesaRepository mesaRepository)
    {
        _comandaRepository = comandaRepository;
        _productoRepository = productoRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
        _mesaRepository = mesaRepository;
    }

    public async Task<Result<ComandaDto>> Handle(CrearComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🍽️ Iniciando creación de comanda - Mesero: {MeseroId}, Mesa: {MesaId}", request.MeseroId, request.MesaId);

        // Validación básica (más validaciones profundas en el Validator)
        if (request.MeseroId == Guid.Empty)
            return Result.Failure<ComandaDto>("El mesero es obligatorio");
        if (request.MesaId == null || request.MesaId == Guid.Empty)
            return Result.Failure<ComandaDto>("La mesa es obligatoria");
        if (request.ProductosIniciales == null || !request.ProductosIniciales.Any())
            return Result.Failure<ComandaDto>("Debe agregar al menos un producto a la comanda");

        // 1. Crear la comanda usando el factory del dominio
        var comanda = Comanda.Crear(
            request.MeseroId,
            request.ClienteId,
            request.MesaId,
            request.Observaciones
        );

        // 2. Agregar productos iniciales
        foreach (var prod in request.ProductosIniciales)
        {
            // Validar producto
            var producto = await _productoRepository.ObtenerPorIdAsync(prod.ProductoId, cancellationToken);
            if (producto == null)
                return Result.Failure<ComandaDto>($"El producto con ID {prod.ProductoId} no existe");
            if (!producto.EstaActivo)
                return Result.Failure<ComandaDto>($"El producto '{producto.Nombre}' no está activo");
            if (prod.Cantidad <= 0)
                return Result.Failure<ComandaDto>($"La cantidad para el producto '{producto.Nombre}' debe ser mayor a cero");

            // Agregar al dominio (usando método del agregado)
            try
            {
                comanda.AgregarItem(
                    producto.Id,
                    producto.Nombre,
                    prod.Cantidad,
                    producto.Precio.Valor,
                    prod.Observaciones
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar producto a la comanda");
                return Result.Failure<ComandaDto>($"Error al agregar producto '{producto.Nombre}': {ex.Message}");
            }
        }

        // 3. Guardar en BD
        await _comandaRepository.AgregarAsync(comanda, cancellationToken);
        await _comandaRepository.GuardarCambiosAsync(cancellationToken);

        // 3.1. Si hay mesa asociada, marcarla como Ocupada
        if (request.MesaId.HasValue && request.MesaId.Value != Guid.Empty)
        {
            try
            {
                var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId.Value, cancellationToken);
                if (mesa != null)
                {
                    mesa.MarcarComoOcupada();
                    await _mesaRepository.ActualizarAsync(mesa, cancellationToken);
                    await _mesaRepository.GuardarCambiosAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo marcar la mesa {MesaId} como Ocupada al crear la comanda {ComandaId}", request.MesaId, comanda.Id);
                // Continuar sin bloquear la creación de la comanda
            }
        }

        // 4. Invalidar caché para que el listado refleje la nueva comanda y el estado de mesas
        _cacheService.InvalidatePattern("ObtenerComandasPaginadasQuery_");
        _cacheService.InvalidatePattern("ObtenerComandasPorMesaQuery_");
        _cacheService.InvalidateForEntity("ObtenerComandaPorIdQuery_", comanda.Id);
        _cacheService.InvalidatePattern("ObtenerMesasDisponiblesQuery_");
        _cacheService.InvalidatePattern("ObtenerEstadoMesasQuery_");

        // 5. Mapear a DTO y retornar
        var dto = _mapper.Map<ComandaDto>(comanda);
        return Result.Success(dto);
    }
} 