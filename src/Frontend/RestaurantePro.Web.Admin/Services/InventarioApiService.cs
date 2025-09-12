using System.Text;
using System.Text.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// Servicio para la gestión de inventario
/// </summary>
public class InventarioApiService : IInventarioApiService
{
    private readonly HttpClient _httpClient;
    private readonly TokenStore _tokenStore;

    public InventarioApiService(IHttpClientFactory httpClientFactory, TokenStore tokenStore)
    {
        _httpClient = httpClientFactory.CreateClient("Api");
        _tokenStore = tokenStore;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClient;
        var token = _tokenStore.Token;
        
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        
        return client;
    }

    #region Ingredientes

    /// <summary>
    /// Obtiene ingredientes paginados
    /// </summary>
    public async Task<PaginatedList<IngredienteDto>?> ObtenerIngredientesPaginadosAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        InventarioFiltrosDto? filtros = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (filtros != null)
            {
                if (!string.IsNullOrEmpty(filtros.Nombre))
                    queryParams.Add($"nombre={Uri.EscapeDataString(filtros.Nombre)}");
                if (!string.IsNullOrEmpty(filtros.Categoria))
                    queryParams.Add($"categoria={Uri.EscapeDataString(filtros.Categoria)}");
                if (!string.IsNullOrEmpty(filtros.Proveedor))
                    queryParams.Add($"proveedor={Uri.EscapeDataString(filtros.Proveedor)}");
                if (filtros.EstaActivo)
                    queryParams.Add($"estaActivo={filtros.EstaActivo}");
                if (filtros.StockBajo)
                    queryParams.Add($"stockBajo={filtros.StockBajo}");
                if (filtros.VencimientoProximo)
                    queryParams.Add($"vencimientoProximo={filtros.VencimientoProximo}");
                if (filtros.FechaVencimientoDesde.HasValue)
                    queryParams.Add($"fechaVencimientoDesde={filtros.FechaVencimientoDesde.Value:yyyy-MM-dd}");
                if (filtros.FechaVencimientoHasta.HasValue)
                    queryParams.Add($"fechaVencimientoHasta={filtros.FechaVencimientoHasta.Value:yyyy-MM-dd}");
                if (filtros.StockMinimoDesde.HasValue)
                    queryParams.Add($"stockMinimoDesde={filtros.StockMinimoDesde.Value}");
                if (filtros.StockMinimoHasta.HasValue)
                    queryParams.Add($"stockMinimoHasta={filtros.StockMinimoHasta.Value}");
                if (filtros.CostoDesde.HasValue)
                    queryParams.Add($"costoDesde={filtros.CostoDesde.Value}");
                if (filtros.CostoHasta.HasValue)
                    queryParams.Add($"costoHasta={filtros.CostoHasta.Value}");
            }

            var queryString = string.Join("&", queryParams);
            var response = await CreateClient().GetAsync($"api/inventario/ingredientes?{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PaginatedList<IngredienteDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener ingredientes: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene un ingrediente por ID
    /// </summary>
    public async Task<IngredienteDto?> ObtenerIngredienteAsync(Guid id)
    {
        try
        {
            var response = await CreateClient().GetAsync($"api/inventario/ingredientes/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IngredienteDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener ingrediente: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea un nuevo ingrediente
    /// </summary>
    public async Task<IngredienteDto?> CrearIngredienteAsync(IngredienteDto ingrediente)
    {
        try
        {
            var json = JsonSerializer.Serialize(ingrediente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await CreateClient().PostAsync("api/inventario/ingredientes", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IngredienteDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear ingrediente: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Actualiza un ingrediente existente
    /// </summary>
    public async Task<IngredienteDto?> ActualizarIngredienteAsync(Guid id, IngredienteDto ingrediente)
    {
        try
        {
            var json = JsonSerializer.Serialize(ingrediente);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await CreateClient().PutAsync($"api/inventario/ingredientes/{id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IngredienteDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar ingrediente: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Elimina un ingrediente
    /// </summary>
    public async Task<bool> EliminarIngredienteAsync(Guid id)
    {
        try
        {
            var response = await CreateClient().DeleteAsync($"api/inventario/ingredientes/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar ingrediente: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene estadísticas de inventario
    /// </summary>
    public async Task<InventarioEstadisticasDto?> ObtenerEstadisticasAsync()
    {
        try
        {
            var response = await CreateClient().GetAsync("api/inventario/estadisticas");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<InventarioEstadisticasDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene alertas de inventario
    /// </summary>
    public async Task<List<AlertaInventarioDto>?> ObtenerAlertasAsync()
    {
        try
        {
            var response = await CreateClient().GetAsync("api/inventario/alertas");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<AlertaInventarioDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener alertas: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Movimientos de Inventario

    /// <summary>
    /// Obtiene movimientos de inventario paginados
    /// </summary>
    public async Task<PaginatedList<MovimientoInventarioDto>?> ObtenerMovimientosPaginadosAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        MovimientoInventarioFiltrosDto? filtros = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (filtros != null)
            {
                if (filtros.IngredienteId.HasValue)
                    queryParams.Add($"ingredienteId={filtros.IngredienteId.Value}");
                if (filtros.TipoMovimiento.HasValue)
                    queryParams.Add($"tipoMovimiento={(int)filtros.TipoMovimiento.Value}");
                if (filtros.FechaDesde.HasValue)
                    queryParams.Add($"fechaDesde={filtros.FechaDesde.Value:yyyy-MM-dd}");
                if (filtros.FechaHasta.HasValue)
                    queryParams.Add($"fechaHasta={filtros.FechaHasta.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(filtros.UsuarioResponsable))
                    queryParams.Add($"usuarioResponsable={Uri.EscapeDataString(filtros.UsuarioResponsable)}");
                if (filtros.OrdenCompraId.HasValue)
                    queryParams.Add($"ordenCompraId={filtros.OrdenCompraId.Value}");
                if (filtros.ComandaId.HasValue)
                    queryParams.Add($"comandaId={filtros.ComandaId.Value}");
            }

            var queryString = string.Join("&", queryParams);
            var response = await CreateClient().GetAsync($"api/inventario/movimientos?{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PaginatedList<MovimientoInventarioDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener movimientos: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea un nuevo movimiento de inventario
    /// </summary>
    public async Task<MovimientoInventarioDto?> CrearMovimientoAsync(MovimientoInventarioDto movimiento)
    {
        try
        {
            var json = JsonSerializer.Serialize(movimiento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await CreateClient().PostAsync("api/inventario/movimientos", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MovimientoInventarioDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear movimiento: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Órdenes de Compra

    /// <summary>
    /// Obtiene órdenes de compra paginadas
    /// </summary>
    public async Task<PaginatedList<OrdenCompraDto>?> ObtenerOrdenesCompraPaginadasAsync(
        int pageNumber = 1, 
        int pageSize = 20, 
        OrdenCompraFiltrosDto? filtros = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (filtros != null)
            {
                if (!string.IsNullOrEmpty(filtros.NumeroOrden))
                    queryParams.Add($"numeroOrden={Uri.EscapeDataString(filtros.NumeroOrden)}");
                if (filtros.ProveedorId.HasValue)
                    queryParams.Add($"proveedorId={filtros.ProveedorId.Value}");
                if (filtros.Estado.HasValue)
                    queryParams.Add($"estado={(int)filtros.Estado.Value}");
                if (filtros.FechaDesde.HasValue)
                    queryParams.Add($"fechaDesde={filtros.FechaDesde.Value:yyyy-MM-dd}");
                if (filtros.FechaHasta.HasValue)
                    queryParams.Add($"fechaHasta={filtros.FechaHasta.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(filtros.UsuarioResponsable))
                    queryParams.Add($"usuarioResponsable={Uri.EscapeDataString(filtros.UsuarioResponsable)}");
                if (filtros.TotalDesde.HasValue)
                    queryParams.Add($"totalDesde={filtros.TotalDesde.Value}");
                if (filtros.TotalHasta.HasValue)
                    queryParams.Add($"totalHasta={filtros.TotalHasta.Value}");
            }

            var queryString = string.Join("&", queryParams);
            var response = await CreateClient().GetAsync($"api/inventario/ordenes-compra?{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PaginatedList<OrdenCompraDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener órdenes de compra: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene una orden de compra por ID
    /// </summary>
    public async Task<OrdenCompraDto?> ObtenerOrdenCompraAsync(Guid id)
    {
        try
        {
            var response = await CreateClient().GetAsync($"api/inventario/ordenes-compra/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrdenCompraDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener orden de compra: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea una nueva orden de compra
    /// </summary>
    public async Task<OrdenCompraDto?> CrearOrdenCompraAsync(OrdenCompraDto ordenCompra)
    {
        try
        {
            var json = JsonSerializer.Serialize(ordenCompra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await CreateClient().PostAsync("api/inventario/ordenes-compra", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrdenCompraDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear orden de compra: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Actualiza una orden de compra existente
    /// </summary>
    public async Task<OrdenCompraDto?> ActualizarOrdenCompraAsync(Guid id, OrdenCompraDto ordenCompra)
    {
        try
        {
            var json = JsonSerializer.Serialize(ordenCompra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await CreateClient().PutAsync($"api/inventario/ordenes-compra/{id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrdenCompraDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar orden de compra: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Elimina una orden de compra
    /// </summary>
    public async Task<bool> EliminarOrdenCompraAsync(Guid id)
    {
        try
        {
            var response = await CreateClient().DeleteAsync($"api/inventario/ordenes-compra/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar orden de compra: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Obtiene estadísticas de órdenes de compra
    /// </summary>
    public async Task<OrdenCompraEstadisticasDto?> ObtenerEstadisticasOrdenesCompraAsync()
    {
        try
        {
            var response = await CreateClient().GetAsync("api/inventario/ordenes-compra/estadisticas");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrdenCompraEstadisticasDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de órdenes de compra: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Utilidades

    /// <summary>
    /// Obtiene categorías de ingredientes
    /// </summary>
    public async Task<List<string>?> ObtenerCategoriasAsync()
    {
        try
        {
            var response = await CreateClient().GetAsync("api/inventario/categorias");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener categorías: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene proveedores para inventario
    /// </summary>
    public async Task<List<ProveedorDto>?> ObtenerProveedoresAsync()
    {
        try
        {
            var response = await CreateClient().GetAsync("api/inventario/proveedores");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ProveedorDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener proveedores: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Exporta inventario a Excel
    /// </summary>
    public async Task<byte[]?> ExportarInventarioAsync(InventarioFiltrosDto? filtros = null)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (filtros != null)
            {
                if (!string.IsNullOrEmpty(filtros.Nombre))
                    queryParams.Add($"nombre={Uri.EscapeDataString(filtros.Nombre)}");
                if (!string.IsNullOrEmpty(filtros.Categoria))
                    queryParams.Add($"categoria={Uri.EscapeDataString(filtros.Categoria)}");
                if (!string.IsNullOrEmpty(filtros.Proveedor))
                    queryParams.Add($"proveedor={Uri.EscapeDataString(filtros.Proveedor)}");
                if (filtros.EstaActivo)
                    queryParams.Add($"estaActivo={filtros.EstaActivo}");
                if (filtros.StockBajo)
                    queryParams.Add($"stockBajo={filtros.StockBajo}");
                if (filtros.VencimientoProximo)
                    queryParams.Add($"vencimientoProximo={filtros.VencimientoProximo}");
            }

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await CreateClient().GetAsync($"api/inventario/exportar{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar inventario: {ex.Message}");
            return null;
        }
    }

    #endregion
}
