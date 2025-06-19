using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Inventario;

/// <summary>
/// Trabajo para verificar fechas de expiración de ingredientes y productos
/// </summary>
public class ExpirationCheckJob : BackgroundJobBase
{
    private readonly IProductoRepository _productoRepository;
    private readonly IIngredienteRepository _ingredienteRepository;

    /// <inheritdoc/>
    public override string JobName => "ExpirationCheck";

    /// <inheritdoc/>
    public override string Description => "Verifica ingredientes y productos próximos a expirar";

    public ExpirationCheckJob(
        ILogger<ExpirationCheckJob> logger,
        IProductoRepository productoRepository,
        IIngredienteRepository ingredienteRepository) : base(logger)
    {
        _productoRepository = productoRepository;
        _ingredienteRepository = ingredienteRepository;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteInternalAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ejecutando trabajo de verificación de fechas de expiración");
        
        var expirationThreshold = DateTime.UtcNow.AddDays(7);
        var itemsFound = false;

        var productosAExpirar = await _productoRepository.BuscarAsync(p => p.FechaExpiracion <= expirationThreshold, cancellationToken);
        foreach (var producto in productosAExpirar)
        {
            _logger.LogWarning("El producto {ProductoNombre} está próximo a expirar en {FechaExpiracion}", producto.Nombre, producto.FechaExpiracion);
            itemsFound = true;
        }

        var ingredientesAExpirar = await _ingredienteRepository.BuscarAsync(i => i.FechaExpiracion <= expirationThreshold, cancellationToken);
        foreach (var ingrediente in ingredientesAExpirar)
        {
            _logger.LogWarning("El ingrediente {IngredienteNombre} está próximo a expirar en {FechaExpiracion}", ingrediente.Nombre, ingrediente.FechaExpiracion);
            itemsFound = true;
        }

        if (!itemsFound)
        {
            _logger.LogInformation("No se encontraron productos o ingredientes próximos a expirar");
        }
    }
} 