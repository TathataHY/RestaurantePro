using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Infrastructure.Services;

/// <summary>
/// Implementación stub del servicio de auditoría para desarrollo
/// </summary>
public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    public async Task<Guid> RegistrarEventoAsync(EventoAuditoria evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📋 Registro de auditoría: {Accion} en {Entidad} por usuario {UsuarioId}", 
            evento.Accion, evento.Entidad, evento.UsuarioId);

        // En una implementación real, esto se guardaría en base de datos
        await Task.Delay(1, cancellationToken); // Simular operación async
        
        return Guid.NewGuid();
    }

    public async Task<List<RegistroAuditoria>> ObtenerRegistrosAsync(FiltrosAuditoria filtros, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📊 Consulta de registros de auditoría");

        // En una implementación real, esto consultaría la base de datos
        await Task.Delay(1, cancellationToken); // Simular operación async
        
        return new List<RegistroAuditoria>();
    }
} 