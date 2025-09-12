using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IComandasApiService
{
    Task<PaginatedList<ComandaDto>?> ObtenerComandasAsync(ComandaFiltrosDto filtros);
    Task<ComandaDto?> ObtenerComandaAsync(Guid id);
    Task<ApiResponse<ComandaDto>?> CrearComandaAsync(CrearComandaRequest request);
    Task<ApiResponse<ComandaDto>?> ActualizarComandaAsync(ActualizarComandaRequest request);
    Task<ApiResponse<bool>?> EliminarComandaAsync(Guid id);
    Task<ApiResponse<bool>?> CambiarEstadoAsync(CambiarEstadoComandaRequest request);
    Task<ApiResponse<bool>?> CambiarPrioridadAsync(CambiarPrioridadComandaRequest request);
    Task<ApiResponse<bool>?> AsignarCocineroAsync(AsignarCocineroRequest request);
    Task<ApiResponse<bool>?> ReasignarMesaAsync(ReasignarMesaRequest request);
    Task<ApiResponse<ComandaDto>?> DividirComandaAsync(DividirComandaRequest request);
    Task<ApiResponse<ComandaDto>?> FusionarComandasAsync(FusionarComandasRequest request);
    Task<ComandaEstadisticasDto?> ObtenerEstadisticasAsync();
    Task<List<ComandaEstadoDto>?> ObtenerHistorialEstadosAsync(Guid comandaId);
    Task<List<ComandaDto>?> ObtenerComandasActivasAsync();
    Task<List<ComandaDto>?> ObtenerComandasUrgentesAsync();
    Task<List<ComandaDto>?> ObtenerComandasLentasAsync();
    Task<List<string>?> ObtenerEstadosAsync();
    Task<List<string>?> ObtenerPrioridadesAsync();
    Task<List<string>?> ObtenerTiposComandaAsync();
    Task<ApiResponse<string>?> ObtenerSiguienteNumeroComandaAsync();
    Task<ApiResponse<bool>?> ValidarNumeroComandaAsync(string numeroComanda, Guid? comandaIdExcluir = null);
    Task<ApiResponse<byte[]>?> ExportarComandasAsync(ComandaFiltrosDto filtros, string formato = "Excel");
    Task<List<ComandaDto>?> ObtenerComandasPorMesaAsync(Guid mesaId);
    Task<List<ComandaDto>?> ObtenerComandasPorMeseroAsync(Guid meseroId);
    Task<List<ComandaDto>?> ObtenerComandasPorClienteAsync(Guid clienteId);
}
