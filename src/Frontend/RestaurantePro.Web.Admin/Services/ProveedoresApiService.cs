using System.Text;
using System.Text.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// Servicio para interactuar con la API de proveedores
/// </summary>
public class ProveedoresApiService : IProveedoresApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;

    public ProveedoresApiService(IHttpClientFactory httpClientFactory, TokenStore tokenStore)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("Api");
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
        return client;
    }

    #region Proveedores

    /// <summary>
    /// Obtiene proveedores paginados con filtros
    /// </summary>
    public async Task<PaginatedList<ProveedorDto>?> GetProveedoresPaginados(int pageNumber = 1, int pageSize = 20, ProveedorFiltrosDto? filtros = null)
    {
        try
        {
            var client = CreateClient();
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (filtros != null)
            {
                if (!string.IsNullOrEmpty(filtros.Nombre))
                    queryParams.Add($"nombre={Uri.EscapeDataString(filtros.Nombre)}");
                if (!string.IsNullOrEmpty(filtros.Ruc))
                    queryParams.Add($"ruc={Uri.EscapeDataString(filtros.Ruc)}");
                if (!string.IsNullOrEmpty(filtros.Ciudad))
                    queryParams.Add($"ciudad={Uri.EscapeDataString(filtros.Ciudad)}");
                if (!string.IsNullOrEmpty(filtros.Pais))
                    queryParams.Add($"pais={Uri.EscapeDataString(filtros.Pais)}");
                if (filtros.EstaActivo)
                    queryParams.Add($"estaActivo={filtros.EstaActivo}");
                if (filtros.FechaCreacionDesde.HasValue)
                    queryParams.Add($"fechaCreacionDesde={filtros.FechaCreacionDesde.Value:yyyy-MM-dd}");
                if (filtros.FechaCreacionHasta.HasValue)
                    queryParams.Add($"fechaCreacionHasta={filtros.FechaCreacionHasta.Value:yyyy-MM-dd}");
                if (filtros.MontoMinimoCompras.HasValue)
                    queryParams.Add($"montoMinimoCompras={filtros.MontoMinimoCompras.Value}");
                if (filtros.MontoMaximoCompras.HasValue)
                    queryParams.Add($"montoMaximoCompras={filtros.MontoMaximoCompras.Value}");
                if (filtros.TieneContactos)
                    queryParams.Add($"tieneContactos={filtros.TieneContactos}");
                if (filtros.TieneOrdenesCompra)
                    queryParams.Add($"tieneOrdenesCompra={filtros.TieneOrdenesCompra}");
            }

            var url = $"api/proveedores?{string.Join("&", queryParams)}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PaginatedList<ProveedorDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener proveedores paginados: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene un proveedor por ID
    /// </summary>
    public async Task<ProveedorDto?> GetProveedorById(Guid id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"api/proveedores/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProveedorDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener proveedor por ID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea un nuevo proveedor
    /// </summary>
    public async Task<ProveedorDto?> CreateProveedor(CrearProveedorRequest request)
    {
        try
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/proveedores", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProveedorDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear proveedor: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Actualiza un proveedor existente
    /// </summary>
    public async Task<ProveedorDto?> UpdateProveedor(Guid id, CrearProveedorRequest request)
    {
        try
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/proveedores/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProveedorDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar proveedor: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Elimina un proveedor
    /// </summary>
    public async Task<bool> DeleteProveedor(Guid id)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"api/proveedores/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar proveedor: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Contactos de Proveedores

    /// <summary>
    /// Obtiene contactos de un proveedor
    /// </summary>
    public async Task<List<ContactoProveedorDto>?> GetContactosProveedor(Guid proveedorId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync($"api/proveedores/{proveedorId}/contactos");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ContactoProveedorDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener contactos del proveedor: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea un nuevo contacto de proveedor
    /// </summary>
    public async Task<ContactoProveedorDto?> CreateContactoProveedor(CrearContactoProveedorRequest request)
    {
        try
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"api/proveedores/{request.ProveedorId}/contactos", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ContactoProveedorDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear contacto de proveedor: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Actualiza un contacto de proveedor
    /// </summary>
    public async Task<ContactoProveedorDto?> UpdateContactoProveedor(Guid proveedorId, Guid contactoId, CrearContactoProveedorRequest request)
    {
        try
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/proveedores/{proveedorId}/contactos/{contactoId}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ContactoProveedorDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar contacto de proveedor: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Elimina un contacto de proveedor
    /// </summary>
    public async Task<bool> DeleteContactoProveedor(Guid proveedorId, Guid contactoId)
    {
        try
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"api/proveedores/{proveedorId}/contactos/{contactoId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar contacto de proveedor: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Estadísticas y Reportes

    /// <summary>
    /// Obtiene estadísticas de proveedores
    /// </summary>
    public async Task<ProveedorEstadisticasDto?> GetProveedorEstadisticas()
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync("api/proveedores/estadisticas");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProveedorEstadisticasDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de proveedores: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Exporta proveedores a Excel
    /// </summary>
    public async Task<byte[]?> ExportarProveedoresExcel(ExportarProveedoresRequest request)
    {
        try
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/proveedores/exportar/excel", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar proveedores a Excel: {ex.Message}");
            return null;
        }
    }

    #endregion

    /// <summary>
    /// Obtiene un proveedor por ID
    /// </summary>
    public async Task<ProveedorDto?> ObtenerProveedorPorIdAsync(Guid id)
    {
        return await GetProveedorById(id);
    }

    /// <summary>
    /// Crea un nuevo proveedor
    /// </summary>
    public async Task<ProveedorDto?> CrearProveedorAsync(CrearProveedorRequest request)
    {
        return await CreateProveedor(request);
    }

    /// <summary>
    /// Actualiza un proveedor existente
    /// </summary>
    public async Task<ProveedorDto?> ActualizarProveedorAsync(Guid id, ActualizarProveedorRequest request)
    {
        try
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/proveedores/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProveedorDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar proveedor: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Elimina un proveedor
    /// </summary>
    public async Task<bool> EliminarProveedorAsync(Guid id)
    {
        return await DeleteProveedor(id);
    }

    /// <summary>
    /// Cambia el estado de un proveedor
    /// </summary>
    public async Task<bool> CambiarEstadoProveedorAsync(Guid id, bool activo)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsync($"api/proveedores/{id}/cambiar-estado?activo={activo}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar estado del proveedor: {ex.Message}");
            return false;
        }
    }
}
