using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IReportesInventarioApiService
{
    Task<List<ReporteInventarioDto>> ObtenerReportesInventarioAsync();
    Task<ReporteInventarioDto?> ObtenerReporteInventarioPorIdAsync(Guid id);
    Task<byte[]?> GenerarReporteInventarioAsync(Guid reporteId, Dictionary<string, object> parametros);
    Task<object?> ObtenerAnalisisStockAsync();
    Task<object?> ObtenerMovimientosInventarioAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<object?> ObtenerProductosProximosVencerAsync(int dias);
    Task<object?> ObtenerRotacionInventarioAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<object?> ObtenerAnalisisCostosAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<object?> ObtenerPrediccionDemandaAsync(int meses);
}
