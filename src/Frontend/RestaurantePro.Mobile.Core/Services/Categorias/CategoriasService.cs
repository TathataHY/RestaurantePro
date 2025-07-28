using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Categorias;

/// <summary>
/// Implementación del servicio de categorías de productos
/// </summary>
public class CategoriasService : ICategoriasService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    public CategoriasService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    /// <summary>
    /// Obtener todas las categorías de productos
    /// </summary>
    public async Task<ApiResponse<List<CategoriaProductoDto>>> ObtenerCategoriasAsync(bool soloActivas = true)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var endpoint = soloActivas ? "api/categorias/activas" : "api/categorias";
            return await _apiService.GetAsync<List<CategoriaProductoDto>>(endpoint, token);
        }
        catch (Exception)
        {
            return ApiResponse<List<CategoriaProductoDto>>.ErrorResponse("Error al obtener categorías", "Error al obtener categorías");
        }
    }

    /// <summary>
    /// Obtener una categoría específica por ID
    /// </summary>
    public async Task<ApiResponse<CategoriaProductoDto>> ObtenerCategoriaAsync(Guid id)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<CategoriaProductoDto>($"api/categorias/{id}", token);
        }
        catch (Exception)
        {
            return ApiResponse<CategoriaProductoDto>.ErrorResponse("Error al obtener categoría", "Error al obtener categoría");
        }
    }

    /// <summary>
    /// Obtener productos por categoría
    /// </summary>
    public async Task<ApiResponse<List<ProductoDto>>> ObtenerProductosPorCategoriaAsync(Guid categoriaId, bool soloActivos = true)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var endpoint = soloActivos 
                ? $"api/categorias/{categoriaId}/productos/activos" 
                : $"api/categorias/{categoriaId}/productos";
            return await _apiService.GetAsync<List<ProductoDto>>(endpoint, token);
        }
        catch (Exception)
        {
            return ApiResponse<List<ProductoDto>>.ErrorResponse("Error al obtener productos de categoría", "Error al obtener productos de categoría");
        }
    }

    /// <summary>
    /// Obtener categorías activas con productos
    /// </summary>
    public async Task<ApiResponse<List<CategoriaProductoDto>>> ObtenerCategoriasActivasAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<List<CategoriaProductoDto>>("api/categorias/activas", token);
        }
        catch (Exception)
        {
            return ApiResponse<List<CategoriaProductoDto>>.ErrorResponse("Error al obtener categorías activas", "Error al obtener categorías activas");
        }
    }

    /// <summary>
    /// Buscar categorías por nombre
    /// </summary>
    public async Task<ApiResponse<List<CategoriaProductoDto>>> BuscarCategoriasAsync(string nombre)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var endpoint = $"api/categorias/buscar?nombre={Uri.EscapeDataString(nombre)}";
            return await _apiService.GetAsync<List<CategoriaProductoDto>>(endpoint, token);
        }
        catch (Exception)
        {
            return ApiResponse<List<CategoriaProductoDto>>.ErrorResponse("Error al buscar categorías", "Error al buscar categorías");
        }
    }
} 