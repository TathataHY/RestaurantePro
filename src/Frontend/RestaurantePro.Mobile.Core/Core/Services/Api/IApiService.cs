using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Api;

/// <summary>
/// Servicio para comunicación con la API - V1 Fundamental
/// </summary>
public interface IApiService
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, string? token = null);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data, string? token = null);
    Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data, string? token = null);
    Task<ApiResponse<bool>> DeleteAsync(string endpoint, string? token = null);
} 