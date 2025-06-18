using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Monitoring.HealthChecks;

/// <summary>
/// Health check para verificar la conexión a la base de datos
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly RestauranteProDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(RestauranteProDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar que se pueda establecer conexión con la base de datos
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                _logger.LogError("No se puede conectar a la base de datos");
                return HealthCheckResult.Unhealthy("No se puede establecer conexión con la base de datos");
            }

            // Realizar una consulta sencilla para verificar que la base funciona correctamente
            var commandTimeout = _dbContext.Database.GetCommandTimeout();
            if (commandTimeout.HasValue)
            {
                _logger.LogInformation("Tiempo de espera actual de comandos: {CommandTimeout} segundos", commandTimeout.Value);
            }

            // Verificamos la consistencia de la base ejecutando una consulta simple y universal
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            
            _logger.LogInformation("Conexión a la base de datos establecida correctamente");
            
            // Verificar otros detalles relevantes del estado de la base de datos
            var migrationsApplied = await _dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
            var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            
            // Crear un diccionario para los datos de salud
            var data = new Dictionary<string, object>
            {
                ["ProviderName"] = _dbContext.Database.ProviderName,
                ["MigrationsApplied"] = migrationsApplied,
                ["PendingMigrations"] = pendingMigrations
            };
            
            return HealthCheckResult.Healthy("Conexión a la base de datos establecida correctamente", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar la salud de la base de datos: {ErrorMessage}", ex.Message);
            return HealthCheckResult.Unhealthy("No se pudo establecer conexión con la base de datos", ex);
        }
    }
} 