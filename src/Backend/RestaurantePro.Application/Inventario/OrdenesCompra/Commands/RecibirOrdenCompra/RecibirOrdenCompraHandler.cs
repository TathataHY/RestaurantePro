using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RecibirOrdenCompra;

/// <summary>
/// Handler para recibir una orden de compra
/// </summary>
public class RecibirOrdenCompraHandler : IRequestHandler<RecibirOrdenCompraCommand, Result<OrdenCompraDto>>
{
    private readonly IOrdenCompraRepository _ordenCompraRepository;
    private readonly IIngredienteRepository _ingredienteRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RecibirOrdenCompraHandler> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IApplicationDbContext _dbContext;

    public RecibirOrdenCompraHandler(
        IOrdenCompraRepository ordenCompraRepository,
        IIngredienteRepository ingredienteRepository,
        IMapper mapper,
        ILogger<RecibirOrdenCompraHandler> logger,
        IDateTimeService dateTimeService,
        IApplicationDbContext dbContext)
    {
        _ordenCompraRepository = ordenCompraRepository;
        _ingredienteRepository = ingredienteRepository;
        _mapper = mapper;
        _logger = logger;
        _dateTimeService = dateTimeService;
        _dbContext = dbContext;
    }

    public async Task<Result<OrdenCompraDto>> Handle(
        RecibirOrdenCompraCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📦 Iniciando recepción de orden de compra {OrdenCompraId}", request.Id);
        _logger.LogInformation("🔧 Tipo de repositorio inyectado: {TipoRepositorio}", _ordenCompraRepository.GetType().Name);

        try
        {
            // Buscar la orden de compra
            var ordenCompra = await _ordenCompraRepository.ObtenerPorIdConItemsAsync(request.Id, cancellationToken);
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

            // Log: Estado después de llamar a Recibir
            _logger.LogInformation("🔍 Estado de la orden tras Recibir(): {Estado}", ordenCompra.Estado);

            // ACTUALIZAR STOCK DE INGREDIENTES
            _logger.LogInformation("📈 Actualizando stock de ingredientes para orden {OrdenCompraId}", request.Id);
            
            foreach (var item in ordenCompra.Items)
            {
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(item.IngredienteId, cancellationToken);
                if (ingrediente != null)
                {
                    _logger.LogInformation("📦 Incrementando stock del ingrediente {IngredienteId} en {Cantidad} unidades", 
                        item.IngredienteId, item.Cantidad);
                    
                    ingrediente.IncrementarStock(
                        item.Cantidad,
                        $"Recepción de orden de compra #{request.Id}");
                    
                    await _ingredienteRepository.ActualizarAsync(ingrediente, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("⚠️ Ingrediente no encontrado: {IngredienteId}", item.IngredienteId);
                }
            }

            // Actualizar la orden de compra en el repositorio
            _logger.LogInformation("🔍 Estado de la orden tras Recibir(): {Estado}", ordenCompra.Estado);
            
            // SOLUCIÓN DEFINITIVA: Forzar la persistencia del estado usando el contexto directamente
            _logger.LogInformation("🔧 [Handler] Forzando persistencia directa del estado usando IApplicationDbContext");
            
            // Castear a DbContext para acceder a ChangeTracker y Entry
            if (_dbContext is Microsoft.EntityFrameworkCore.DbContext dbContext)
            {
                // Forzar detección de cambios
                dbContext.ChangeTracker.DetectChanges();
                
                // Obtener entry de la entidad
                var entry = dbContext.Entry(ordenCompra);
                _logger.LogInformation("🔧 [Handler] Estado de rastreo: {Estado}", entry.State);
                
                // Marcar explícitamente la propiedad Estado como modificada
                entry.Property(nameof(OrdenCompra.Estado)).IsModified = true;
                _logger.LogInformation("🔧 [Handler] Propiedad Estado marcada como modificada");
                
                // Guardar cambios directamente
                var changesSaved = await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("🔧 [Handler] Cambios guardados directamente: {Cambios}", changesSaved);
            }
            else
            {
                _logger.LogWarning("⚠️ No se pudo castear IApplicationDbContext a DbContext. Usando método normal del repositorio.");
                await _ordenCompraRepository.ActualizarAsync(ordenCompra, cancellationToken);
            }
            
            _logger.LogInformation("🔍 Estado de la orden tras ActualizarAsync(): {Estado}", ordenCompra.Estado);

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