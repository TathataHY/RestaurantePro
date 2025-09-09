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
    public async Task<ApiResponse<List<CategoriaProductoDto>>> ObtenerCategoriasAsync(bool soloActivas = true, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<CategoriaProductoDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<CategoriaProductoDto>>($"api/core/categorias?soloActivas={soloActivas}", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<CategoriaProductoDto>>.Failure($"Error al obtener categorías: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener una categoría específica por ID
    /// </summary>
    public async Task<ApiResponse<CategoriaProductoDto>> ObtenerCategoriaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<CategoriaProductoDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<CategoriaProductoDto>($"api/core/categorias/{id}", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoriaProductoDto>.Failure($"Error al obtener categoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener productos por categoría
    /// </summary>
    public async Task<ApiResponse<List<ProductoDto>>> ObtenerProductosPorCategoriaAsync(Guid categoriaId, bool soloActivos = true, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<ProductoDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<ProductoDto>>($"api/core/productos/categoria/{categoriaId}?soloActivos={soloActivos}", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ProductoDto>>.Failure($"Error al obtener productos de categoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtener categorías activas con productos
    /// </summary>
    public async Task<ApiResponse<List<CategoriaProductoDto>>> ObtenerCategoriasActivasAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<CategoriaProductoDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<CategoriaProductoDto>>("api/core/categorias?soloActivas=true&ocultarVacias=true", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<CategoriaProductoDto>>.Failure($"Error al obtener categorías activas: {ex.Message}");
        }
    }

    /// <summary>
    /// Buscar categorías por nombre
    /// </summary>
    public async Task<ApiResponse<List<CategoriaProductoDto>>> BuscarCategoriasAsync(string nombre, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<CategoriaProductoDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<CategoriaProductoDto>>($"api/core/categorias?nombre={Uri.EscapeDataString(nombre)}", token, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<CategoriaProductoDto>>.Failure($"Error al buscar categorías: {ex.Message}");
        }
    }
} 