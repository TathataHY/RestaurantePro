using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class MesasApiService : IMesasApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public MesasApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
    {
        _httpFactory = httpFactory;
        _tokenStore = tokenStore;
    }

    private HttpClient CreateClient()
    {
        var http = _httpFactory.CreateClient("Api");
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            if (!http.DefaultRequestHeaders.Contains("X-Bearer-Token"))
            {
                http.DefaultRequestHeaders.Add("X-Bearer-Token", _tokenStore.Token);
            }
        }
        return http;
    }

    public async Task<PaginatedList<MesaDto>?> ObtenerAsync(string? estado = null, string? ubicacion = null, int? capacidadMinima = null, int pageNumber = 1, int pageSize = 10)
    {
        var http = CreateClient();
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(estado)) query.Add($"estado={Uri.EscapeDataString(estado)}");
        if (!string.IsNullOrWhiteSpace(ubicacion)) query.Add($"ubicacion={Uri.EscapeDataString(ubicacion)}");
        if (capacidadMinima.HasValue) query.Add($"capacidadMinima={capacidadMinima.Value}");
        query.Add($"pageNumber={pageNumber}");
        query.Add($"pageSize={pageSize}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

        try
        {
            var resp = await http.GetFromJsonAsync<ApiResponse<PaginatedList<MesaDto>>>(
                $"api/operaciones/mesas{qs}");
            return resp?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<MesaDto?> ObtenerPorIdAsync(Guid id)
    {
        var http = CreateClient();
        try
        {
            var resp = await http.GetFromJsonAsync<ApiResponse<MesaDto>>($"api/operaciones/mesas/{id}");
            return resp?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<MesaDto?> CrearAsync(CrearMesaRequest dto)
    {
        var http = CreateClient();
        var res = await http.PostAsJsonAsync("api/operaciones/mesas", dto);
        if (!res.IsSuccessStatusCode)
        {
            var err = await TryReadErrorAsync(res);
            throw new InvalidOperationException(err ?? "No se pudo crear la mesa.");
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<MesaDto>>();
        return resp?.Data;
    }

    public async Task<MesaDto?> ActualizarAsync(Guid id, ActualizarMesaRequest dto)
    {
        var http = CreateClient();
        var res = await http.PutAsJsonAsync($"api/operaciones/mesas/{id}", dto);
        if (!res.IsSuccessStatusCode)
        {
            var err = await TryReadErrorAsync(res);
            throw new InvalidOperationException(err ?? "No se pudo actualizar la mesa.");
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<MesaDto>>();
        return resp?.Data;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var http = CreateClient();
        var res = await http.DeleteAsync($"api/operaciones/mesas/{id}");
        return res.IsSuccessStatusCode;
    }

    /// <summary>
    /// Obtiene todas las mesas
    /// </summary>
    public async Task<List<MesaDto>> ObtenerMesasAsync()
    {
        var result = await ObtenerAsync();
        return result?.Items ?? new List<MesaDto>();
    }

    /// <summary>
    /// Obtiene una mesa por ID
    /// </summary>
    public async Task<MesaDto?> ObtenerMesaPorIdAsync(Guid id)
    {
        return await ObtenerPorIdAsync(id);
    }

    /// <summary>
    /// Crea una nueva mesa
    /// </summary>
    public async Task<MesaDto?> CrearMesaAsync(CrearMesaRequest request)
    {
        return await CrearAsync(request);
    }

    /// <summary>
    /// Actualiza una mesa existente
    /// </summary>
    public async Task<MesaDto?> ActualizarMesaAsync(Guid id, ActualizarMesaRequest request)
    {
        return await ActualizarAsync(id, request);
    }

    /// <summary>
    /// Elimina una mesa
    /// </summary>
    public async Task<bool> EliminarMesaAsync(Guid id)
    {
        return await EliminarAsync(id);
    }

    /// <summary>
    /// Cambia el estado de una mesa
    /// </summary>
    public async Task<bool> CambiarEstadoMesaAsync(Guid id, string estado)
    {
        var http = CreateClient();
        var res = await http.PostAsync($"api/operaciones/mesas/{id}/cambiar-estado?estado={Uri.EscapeDataString(estado)}", null);
        return res.IsSuccessStatusCode;
    }

    private class ApiResponse<T>
    {
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public List<string>? Errors { get; set; }
    }

    private static async Task<string?> TryReadErrorAsync(HttpResponseMessage res)
    {
        try
        {
            var err = await res.Content.ReadFromJsonAsync<ApiResponse<object>>();
            if (err?.Errors != null && err.Errors.Count > 0)
            {
                return string.Join("; ", err.Errors);
            }
            if (!string.IsNullOrWhiteSpace(err?.Message)) return err!.Message;
        }
        catch
        {
            // Ignorar errores de parseo y devolver null
        }
        return null;
    }
}


