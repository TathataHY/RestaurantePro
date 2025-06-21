using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RecibirOrdenCompra;

/// <summary>
/// Handler para recibir una orden de compra
/// </summary>
public class RecibirOrdenCompraHandler : IRequestHandler<RecibirOrdenCompraCommand, Result<OrdenCompraDto>>
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RecibirOrdenCompraHandler> _logger;
    private readonly IDateTimeService _dateTimeService;

    public RecibirOrdenCompraHandler(
        IOrdenCompraRepository ordenCompraRepository,
        IMapper mapper,
        ILogger<RecibirOrdenCompraHandler> logger,
        IDateTimeService dateTimeService)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _mapper = mapper;
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<OrdenCompraDto>> Handle(
        RecibirOrdenCompraCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📦 Iniciando recepción de orden de compra {OrdenCompraId}", request.Id);

        try
        {
            // Buscar la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (ordenCompra == null)
            {
                _logger.LogWarning("⚠️ Orden de compra no encontrada: {OrdenCompraId}", request.Id);
                return Result.Failure<OrdenCompraDto>("Orden de compra no encontrada");
            }

            // Validar que la orden esté en estado enviada
            if (ordenCompra.Estado != EstadoOrdenCompra.Enviada)
            {
                _logger.LogWarning("⚠️ Solo se pueden recibir órdenes en estado Enviada. Estado actual: {Estado}", 
                    ordenCompra.Estado);
                return Result.Failure<OrdenCompraDto>($"Solo se pueden recibir órdenes en estado Enviada. Estado actual: {ordenCompra.Estado}");
            }

            // Recibir la orden usando el método de la entidad
            ordenCompra.Recibir(_dateTimeService.Now, request.NotasRecepcion);

            // Guardar cambios
            await _ordenCompraRepository.ActualizarAsync(ordenCompra, cancellationToken);

            _logger.LogInformation("✅ Orden de compra recibida exitosamente: {OrdenCompraId}", request.Id);

            var ordenCompraDto = _mapper.Map<OrdenCompraDto>(ordenCompra);
            return Result.Success(ordenCompraDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al recibir orden de compra {OrdenCompraId}", request.Id);
            return Result.Failure<OrdenCompraDto>($"Error al recibir la orden de compra: {ex.Message}");
        }
    }
} 