using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public interface IRecetasApiService
{
    Task<List<RecetaDto>> ObtenerRecetasAsync();
    Task<RecetaDto?> ObtenerRecetaPorIdAsync(Guid id);
    Task<RecetaDto?> CrearRecetaAsync(CrearRecetaRequest request);
    Task<RecetaDto?> ActualizarRecetaAsync(Guid id, ActualizarRecetaRequest request);
    Task<bool> EliminarRecetaAsync(Guid id);
    Task<bool> CambiarEstadoRecetaAsync(Guid id, bool activa);
    Task<PaginatedList<RecetaDto>?> ObtenerRecetasPaginadasAsync(int pagina, int tamanoPagina, bool? soloActivas = null, Guid? productoId = null, string? filtroTexto = null, string? ordenarPor = null, string? direccion = null);
    Task<decimal> CalcularCostoRecetaAsync(Guid recetaId);
}
