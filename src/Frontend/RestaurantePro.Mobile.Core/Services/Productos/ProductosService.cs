using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Common;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Productos;

/// <summary>
/// Servicio para gestión de productos - Operaciones de consulta
/// </summary>
public class ProductosService : IProductosService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;
    private const string BaseEndpoint = "api/core/productos";

    public ProductosService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    /// <summary>
    /// Obtener todos los productos disponibles con paginación
    /// </summary>
    public async Task<ApiResponse<List<ProductoDto>>> ObtenerProductosPaginadosAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        string? filtro = null, 
        bool soloActivos = true)
    {
        try
        {
            var queryParams = $"?pageNumber={pageNumber}&pageSize={pageSize}&soloActivos={soloActivos}";
            
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                queryParams += $"&filtro={Uri.EscapeDataString(filtro)}";
            }

            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<PaginatedList<ProductoDto>>($"{BaseEndpoint}{queryParams}", token);
            
            // Convertir PaginatedList a List para mantener compatibilidad
            if (response.Success && response.Data != null)
            {
                return ApiResponse<List<ProductoDto>>.SuccessResponse(response.Data.Items, response.Message);
            }
            
            return ApiResponse<List<ProductoDto>>.ErrorResponse(response.Errors, response.Message, response.StatusCode);
        }
        catch (Exception)
        {
            return ApiResponse<List<ProductoDto>>.ErrorResponse("Error al obtener productos paginados", "Error al obtener productos paginados");
        }
    }

    /// <summary>
    /// Obtener producto por ID específico
    /// </summary>
    public async Task<ApiResponse<ProductoDto>> ObtenerProductoPorIdAsync(Guid productoId)
    {
        try
        {
            if (productoId == Guid.Empty)
            {
                return ApiResponse<ProductoDto>.ErrorResponse("El ID del producto es requerido", "El ID del producto es requerido");
            }

            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<ProductoDto>($"{BaseEndpoint}/{productoId}", token);
        }
        catch (Exception)
        {
            return ApiResponse<ProductoDto>.ErrorResponse("Error al obtener producto por ID", "Error al obtener producto por ID");
        }
    }

    /// <summary>
    /// Obtener productos por categoría específica
    /// </summary>
    public async Task<ApiResponse<List<ProductoDto>>> ObtenerProductosPorCategoriaAsync(
        Guid categoriaId, 
        bool soloActivos = true)
    {
        try
        {
            if (categoriaId == Guid.Empty)
            {
                return ApiResponse<List<ProductoDto>>.ErrorResponse("El ID de la categoría es requerido", "El ID de la categoría es requerido");
            }

            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<List<ProductoDto>>($"{BaseEndpoint}/categoria/{categoriaId}?soloActivos={soloActivos}", token);
        }
        catch (Exception)
        {
            return ApiResponse<List<ProductoDto>>.ErrorResponse("Error al obtener productos por categoría", "Error al obtener productos por categoría");
        }
    }

    /// <summary>
    /// Verificar disponibilidad de un producto específico
    /// </summary>
    public async Task<ApiResponse<DisponibilidadProductoDto>> VerificarDisponibilidadProductoAsync(Guid productoId)
    {
        try
        {
            if (productoId == Guid.Empty)
            {
                return ApiResponse<DisponibilidadProductoDto>.ErrorResponse("El ID del producto es requerido", "El ID del producto es requerido");
            }

            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<DisponibilidadProductoDto>($"{BaseEndpoint}/{productoId}/disponibilidad", token);
        }
        catch (Exception)
        {
            return ApiResponse<DisponibilidadProductoDto>.ErrorResponse("Error al verificar disponibilidad del producto", "Error al verificar disponibilidad del producto");
        }
    }

    /// <summary>
    /// Buscar productos por texto (nombre, descripción, categoría)
    /// </summary>
    public async Task<ApiResponse<List<ProductoDto>>> BuscarProductosAsync(
        string textoBusqueda, 
        bool soloActivos = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(textoBusqueda))
            {
                return ApiResponse<List<ProductoDto>>.ErrorResponse("El texto de búsqueda es requerido", "El texto de búsqueda es requerido");
            }

            var encodedText = Uri.EscapeDataString(textoBusqueda);
            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<List<ProductoDto>>($"{BaseEndpoint}/buscar?texto={encodedText}&soloActivos={soloActivos}", token);
        }
        catch (Exception)
        {
            return ApiResponse<List<ProductoDto>>.ErrorResponse("Error al buscar productos", "Error al buscar productos");
        }
    }

    /// <summary>
    /// Obtener productos más populares (para recomendaciones)
    /// </summary>
    public async Task<ApiResponse<List<ProductoDto>>> ObtenerProductosPopularesAsync(int limite = 10)
    {
        try
        {
            if (limite <= 0)
            {
                return ApiResponse<List<ProductoDto>>.ErrorResponse("El límite debe ser mayor a 0", "El límite debe ser mayor a 0");
            }

            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<List<ProductoDto>>($"{BaseEndpoint}/populares?limite={limite}", token);
        }
        catch (Exception)
        {
            return ApiResponse<List<ProductoDto>>.ErrorResponse("Error al obtener productos populares", "Error al obtener productos populares");
        }
    }

    /// <summary>
    /// Obtener productos disponibles para agregar a comandas (filtrados)
    /// </summary>
    public async Task<ApiResponse<List<ProductoDto>>> ObtenerProductosDisponiblesParaComandasAsync(Guid? categoriaId = null)
    {
        try
        {
            var endpoint = $"{BaseEndpoint}/disponibles-comandas";
            
            if (categoriaId.HasValue && categoriaId.Value != Guid.Empty)
            {
                endpoint += $"?categoriaId={categoriaId.Value}";
            }

            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<List<ProductoDto>>(endpoint, token);
        }
        catch (Exception)
        {
            return ApiResponse<List<ProductoDto>>.ErrorResponse("Error al obtener productos disponibles para comandas", "Error al obtener productos disponibles para comandas");
        }
    }

    /// <summary>
    /// Obtener categorías de productos disponibles
    /// </summary>
    public async Task<ApiResponse<List<CategoriaProductoDto>>> ObtenerCategoriasAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            return await _apiService.GetAsync<List<CategoriaProductoDto>>("api/core/categorias", token);
        }
        catch (Exception)
        {
            return ApiResponse<List<CategoriaProductoDto>>.ErrorResponse("Error al obtener categorías", "Error al obtener categorías");
        }
    }

    /// <summary>
    /// Crear un nuevo producto
    /// </summary>
    public async Task<ApiResponse<ProductoDto>> CrearProductoAsync(CrearProductoRequest request)
    {
        try
        {
            if (request is null)
            {
                return ApiResponse<ProductoDto>.ErrorResponse("Request inválido", "Request inválido");
            }

            var token = await _authService.GetTokenAsync();
            return await _apiService.PostAsync<ProductoDto>(BaseEndpoint, request, token);
        }
        catch (Exception)
        {
            return ApiResponse<ProductoDto>.ErrorResponse("Error al crear producto", "Error al crear producto");
        }
    }

    /// <summary>
    /// Actualizar un producto existente
    /// </summary>
    public async Task<ApiResponse<ProductoDto>> ActualizarProductoAsync(ActualizarProductoRequest request)
    {
        try
        {
            if (request is null || request.Id == Guid.Empty)
            {
                return ApiResponse<ProductoDto>.ErrorResponse("Request inválido", "Request inválido");
            }

            var token = await _authService.GetTokenAsync();
            return await _apiService.PutAsync<ProductoDto>($"{BaseEndpoint}/{request.Id}", request, token);
        }
        catch (Exception)
        {
            return ApiResponse<ProductoDto>.ErrorResponse("Error al actualizar producto", "Error al actualizar producto");
        }
    }

    /// <summary>
    /// Eliminar un producto
    /// </summary>
    public async Task<ApiResponse<bool>> EliminarProductoAsync(Guid productoId)
    {
        try
        {
            if (productoId == Guid.Empty)
            {
                return ApiResponse<bool>.ErrorResponse("El ID del producto es requerido", "El ID del producto es requerido");
            }

            var token = await _authService.GetTokenAsync();
            return await _apiService.DeleteAsync($"{BaseEndpoint}/{productoId}");
        }
        catch (Exception)
        {
            return ApiResponse<bool>.ErrorResponse("Error al eliminar producto", "Error al eliminar producto");
        }
    }
} 