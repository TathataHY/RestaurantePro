using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public interface IPreparacionesService
{
    Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesAsync(bool soloDisponibles = true);
    Task<ApiResponse<PreparacionDto>> ObtenerPreparacionAsync(Guid id);
    Task<ApiResponse<List<PreparacionDto>>> BuscarPreparacionesAsync(string terminoBusqueda);
    Task<ApiResponse<PreparacionDto>> CrearPreparacionAsync(PreparacionDto preparacion);
    Task<ApiResponse<PreparacionDto>> ActualizarPreparacionAsync(Guid id, PreparacionDto preparacion);
    Task<ApiResponse<bool>> EliminarPreparacionAsync(Guid id);
    Task<ApiResponse<EstadisticasPreparacionesDto>> ObtenerEstadisticasAsync();
    Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesPorCategoriaAsync(string categoria);
    Task<ApiResponse<PreparacionDto>> CambiarDisponibilidadAsync(Guid id, bool disponible);
    
    // Métodos adicionales para ViewModels
    Task<ApiResponse<PreparacionDto>> IniciarPreparacionAsync(Guid id, IniciarPreparacionDto dto);
    Task<ApiResponse<PreparacionDto>> CompletarPreparacionAsync(Guid id);
    Task<ApiResponse<PreparacionDto>> CancelarPreparacionAsync(Guid id, CancelarPreparacionDto dto);
    Task<ApiResponse<List<PreparacionDto>>> ObtenerColaPreparacionesAsync();
    Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesPorEstadoAsync(string estado);
} 