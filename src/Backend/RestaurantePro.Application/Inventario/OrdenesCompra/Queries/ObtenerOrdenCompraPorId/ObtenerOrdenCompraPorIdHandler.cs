using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Queries.ObtenerOrdenCompraPorId;

/// <summary>
/// Handler para obtener una orden de compra específica por ID
/// </summary>
public class ObtenerOrdenCompraPorIdHandler : IRequestHandler<ObtenerOrdenCompraPorIdQuery, Result<OrdenCompraDto>>
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerOrdenCompraPorIdHandler> _logger;

    public ObtenerOrdenCompraPorIdHandler(
        IOrdenCompraRepository ordenCompraRepository,
        IMapper mapper,
        ILogger<ObtenerOrdenCompraPorIdHandler> logger)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrdenCompraDto>> Handle(
        ObtenerOrdenCompraPorIdQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Obteniendo orden de compra por ID: {OrdenCompraId}", request.Id);

        try
        {
            // Buscar la orden de compra por ID - FORZAR LECTURA DESDE BD SIN CACHE
            _logger.LogInformation("🔍 Usando método sin rastreo para evitar cache de EF Core");
            
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdSinRastreoAsync(request.Id, cancellationToken);
            
            if (ordenCompra == null)
            {
                _logger.LogWarning("⚠️ Orden de compra no encontrada: {OrdenCompraId}", request.Id);
                return Result.Failure<OrdenCompraDto>($"Orden de compra no encontrada: {request.Id}");
            }

            _logger.LogInformation("✅ Orden obtenida desde BD - Estado actual: {Estado}", ordenCompra.Estado);

            // DIAGNÓSTICO: Verificar el estado antes del mapping
            _logger.LogInformation("🔍 [DIAGNÓSTICO] Estado de la entidad antes del mapping: {Estado} (Valor: {Valor})", 
                ordenCompra.Estado, (int)ordenCompra.Estado);

            // Mapear a DTO
            var ordenCompraDto = _mapper.Map<OrdenCompraDto>(ordenCompra);

            // DIAGNÓSTICO: Verificar el estado después del mapping
            _logger.LogInformation("🔍 [DIAGNÓSTICO] Estado del DTO después del mapping: {Estado} (Valor: {Valor})", 
                ordenCompraDto.Estado, (int)ordenCompraDto.Estado);

            _logger.LogInformation("✅ Orden de compra obtenida exitosamente: {OrdenCompraId}", request.Id);

            return Result.Success(ordenCompraDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener orden de compra por ID: {OrdenCompraId}", request.Id);
            return Result.Failure<OrdenCompraDto>($"Error al obtener la orden de compra: {ex.Message}");
        }
    }
} 