using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Inventory;

public interface IPreparacionesService
{
    Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesAsync(bool soloDisponibles = true, CancellationToken cancellationToken = default);
    Task<ApiResponse<PreparacionDto>> ObtenerPreparacionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PreparacionDto>>> BuscarPreparacionesAsync(string terminoBusqueda, CancellationToken cancellationToken = default);
    Task<ApiResponse<PreparacionDto>> CrearPreparacionAsync(PreparacionDto preparacion, CancellationToken cancellationToken = default);
    Task<ApiResponse<PreparacionDto>> ActualizarPreparacionAsync(Guid id, PreparacionDto preparacion, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> EliminarPreparacionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<EstadisticasPreparacionesDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default);
    Task<ApiResponse<PreparacionDto>> CambiarDisponibilidadAsync(Guid id, bool disponible, CancellationToken cancellationToken = default);
    
    // Métodos adicionales para ViewModels
    Task<ApiResponse<PreparacionDto>> IniciarPreparacionAsync(Guid id, IniciarPreparacionDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PreparacionDto>> CompletarPreparacionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PreparacionDto>> CancelarPreparacionAsync(Guid id, CancelarPreparacionDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PreparacionDto>>> ObtenerColaPreparacionesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PreparacionDto>>> ObtenerPreparacionesPorEstadoAsync(string estado, CancellationToken cancellationToken = default);
} 