using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.EnviarOrdenCompra;

/// <summary>
/// Handler para enviar una orden de compra al proveedor
/// </summary>
public class EnviarOrdenCompraHandler : IRequestHandler<EnviarOrdenCompraCommand, Result<OrdenCompraDto>>
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<EnviarOrdenCompraHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public EnviarOrdenCompraHandler(
        IOrdenCompraRepository ordenCompraRepository,
        IMapper mapper,
        ILogger<EnviarOrdenCompraHandler> logger,
        IDateTimeService dateTimeService)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _mapper = mapper;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<OrdenCompraDto>> Handle(
        EnviarOrdenCompraCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📤 Iniciando envío de orden de compra {OrdenCompraId}", request.Id);

        try
        {
            // Buscar la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdConItemsAsync(request.Id, cancellationToken);
            if (ordenCompra == null)
            {
                _logger.LogWarning("⚠️ Orden de compra no encontrada: {OrdenCompraId}", request.Id);
                return Result.Failure<OrdenCompraDto>("Orden de compra no encontrada");
            }

            // Validar que la orden esté en estado pendiente o confirmada
            if (ordenCompra.Estado != EstadoOrdenCompra.Pendiente && ordenCompra.Estado != EstadoOrdenCompra.Confirmada)
            {
                _logger.LogWarning("⚠️ No se puede enviar una orden en estado {Estado}", ordenCompra.Estado);
                return Result.Failure<OrdenCompraDto>($"No se puede enviar una orden en estado {ordenCompra.Estado}");
            }

            // Enviar la orden usando el método de la entidad con la fecha correcta
            ordenCompra.Enviar(_dateTimeService.Now);

            // Guardar cambios
            await _ordenCompraRepository.ActualizarAsync(ordenCompra, cancellationToken);

            _logger.LogInformation("✅ Orden de compra enviada exitosamente: {OrdenCompraId}", request.Id);

            var ordenCompraDto = _mapper.Map<OrdenCompraDto>(ordenCompra);
            return Result.Success(ordenCompraDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar orden de compra {OrdenCompraId}", request.Id);
            return Result.Failure<OrdenCompraDto>($"Error al enviar la orden de compra: {ex.Message}");
        }
    }
} 