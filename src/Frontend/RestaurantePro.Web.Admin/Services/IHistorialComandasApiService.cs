using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// 📋 Servicio para gestionar el historial de comandas
/// </summary>
public interface IHistorialComandasApiService
{
    /// <summary>
    /// 🔍 Obtener historial de comandas con filtros avanzados
    /// </summary>
    Task<PaginatedList<ComandaSummaryDto>?> ObtenerHistorialComandasAsync(
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        Guid? mesaId = null,
        Guid? meseroId = null,
        Guid? clienteId = null,
        string? estado = null,
        decimal? montoMinimo = null,
        decimal? montoMaximo = null,
        string? terminoBusqueda = null,
        bool incluirCanceladas = false,
        bool soloFinalizadas = true,
        string ordenarPor = "FechaMasReciente");

    /// <summary>
    /// 🔍 Obtener historial de comandas por mesa específica
    /// </summary>
    Task<PaginatedList<ComandaSummaryDto>?> ObtenerHistorialPorMesaAsync(
        Guid mesaId,
        DateTime? fechaDesde = null,
        int pageNumber = 1);

    /// <summary>
    /// 🔍 Obtener historial básico (últimos 30 días)
    /// </summary>
    Task<PaginatedList<ComandaSummaryDto>?> ObtenerHistorialBasicoAsync(
        int pageNumber = 1,
        int pageSize = 20);
}
