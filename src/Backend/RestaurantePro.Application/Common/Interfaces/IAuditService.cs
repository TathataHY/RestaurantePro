namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio de auditoría
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra una operación de auditoría
    /// </summary>
    /// <param name="evento">Evento de auditoría</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>ID del registro de auditoría</returns>
    Task<Guid> RegistrarEventoAsync(EventoAuditoria evento, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene registros de auditoría
    /// </summary>
    /// <param name="filtros">Filtros de búsqueda</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de registros</returns>
    Task<List<RegistroAuditoria>> ObtenerRegistrosAsync(FiltrosAuditoria filtros, CancellationToken cancellationToken = default);
}

/// <summary>
/// Evento de auditoría
/// </summary>
public class EventoAuditoria
{
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public Guid? EntidadId { get; set; }
    public Guid UsuarioId { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public string? Observaciones { get; set; }
    public Dictionary<string, object>? DatosAdicionales { get; set; }
}

/// <summary>
/// Registro de auditoría
/// </summary>
public class RegistroAuditoria
{
    public Guid Id { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public Guid? EntidadId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime Fecha { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public string? Observaciones { get; set; }
    public string? DatosAdicionales { get; set; }
}

/// <summary>
/// Filtros para búsqueda de auditoría
/// </summary>
public class FiltrosAuditoria
{
    public Guid? UsuarioId { get; set; }
    public string? Entidad { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public List<string>? Acciones { get; set; }
} 