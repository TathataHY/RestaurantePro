using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using RestaurantePro.App.Models;

namespace RestaurantePro.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    //private const string BaseUrl = "https://localhost:7209/api";
    private const string BaseUrl = "https://192.168.18.107:45455/api";
    private string _authToken;

    public ApiService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        
        _httpClient = new HttpClient(handler);
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    // Autenticación
    public async Task<(Usuario Usuario, string Token)> LoginAsync(string username, string password)
    {
        var loginData = new { NombreUsuario = username, Contraseña = password };
        var response = await PostAsync<LoginResponse>("/usuario/login", loginData);
        SetAuthToken(response.Token);
        return (response.Usuario, response.Token);
    }

    // Métodos genéricos
    private async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    // Implementar métodos específicos para cada entidad
    public async Task<List<Usuario>> GetUsuariosAsync()
    {
        return await GetAsync<List<Usuario>>("/usuario");
    }

    public async Task<List<Mesa>> GetMesasAsync()
    {
        return await GetAsync<List<Mesa>>("/mesa");
    }

    public async Task<List<Plato>> GetPlatosAsync()
    {
        return await GetAsync<List<Plato>>("/plato");
    }

    // Métodos para Usuario
    public async Task<Usuario> GetUsuarioByIdAsync(int id)
    {
        return await GetAsync<Usuario>($"/usuario/{id}");
    }

    public async Task<Usuario> SaveUsuarioAsync(Usuario usuario)
    {
        if (usuario.Id != 0)
        {
            await PutAsync($"/usuario/{usuario.Id}", usuario);
            return usuario;
        }
        return await PostAsync<Usuario>("/usuario", usuario);
    }

    public async Task DeleteUsuarioAsync(int id)
    {
        await DeleteAsync($"/usuario/{id}");
    }

    // Métodos para Mesa
    public async Task<Mesa> GetMesaByIdAsync(int id)
    {
        return await GetAsync<Mesa>($"/mesa/{id}");
    }

    public async Task<List<Mesa>> GetMesasDisponiblesAsync()
    {
        return await GetAsync<List<Mesa>>("/mesa/disponibles");
    }

    public async Task<Mesa> SaveMesaAsync(Mesa mesa)
    {
        if (mesa.Id != 0)
        {
            await PutAsync($"/mesa/{mesa.Id}", mesa);
            return mesa;
        }
        return await PostAsync<Mesa>("/mesa", mesa);
    }

    public async Task DeleteMesaAsync(int id)
    {
        await DeleteAsync($"/mesa/{id}");
    }

    // Métodos para Comanda
    public async Task<List<Comanda>> GetComandasAsync()
    {
        return await GetAsync<List<Comanda>>("/comanda");
    }

    public async Task<Comanda> GetComandaByIdAsync(int id)
    {
        return await GetAsync<Comanda>($"/comanda/{id}");
    }

    public async Task<Comanda> SaveComandaAsync(Comanda comanda)
    {
        if (comanda.Id != 0)
        {
            await PutAsync($"/comanda/{comanda.Id}", comanda);
            return comanda;
        }
        return await PostAsync<Comanda>("/comanda", comanda);
    }

    public async Task UpdateComandaEstadoAsync(int comandaId, EstadoComanda estado)
    {
        await PutAsync($"/comanda/{comandaId}/estado", new { Estado = estado });
    }

    public async Task DeleteComandaAsync(int id)
    {
        await DeleteAsync($"/comanda/{id}");
    }

    public async Task<List<Plato>> GetPlatosDisponiblesAsync()
    {
        return await GetAsync<List<Plato>>("/plato/disponibles");
    }

    public async Task<Plato> SavePlatoAsync(Plato plato)
    {
        if (plato.Id != 0)
        {
            await PutAsync($"/plato/{plato.Id}", plato);
            return plato;
        }
        return await PostAsync<Plato>("/plato", plato);
    }

    public async Task DeletePlatoAsync(int id)
    {
        await DeleteAsync($"/plato/{id}");
    }

    public async Task<List<ComandaDetalle>> GetComandaDetallesAsync(int comandaId)
    {
        return await GetAsync<List<ComandaDetalle>>($"/comanda/{comandaId}/detalles");
    }

    public async Task<ComandaDetalle> SaveComandaDetalleAsync(ComandaDetalle detalle)
    {
        if (detalle.Id != 0)
        {
            await PutAsync($"/comanda/detalle/{detalle.Id}", detalle);
            return detalle;
        }
        return await PostAsync<ComandaDetalle>($"/comanda/{detalle.ComandaId}/detalle", detalle);
    }

    // Método genérico PUT
    private async Task PutAsync<T>(string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
    }

    // Método genérico DELETE
    private async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Venta>> GetVentasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var response = await _httpClient.GetAsync($"api/ventas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Venta>>();
    }

    public async Task<Plato> AddPlatoAsync(Plato plato)
    {
        var response = await _httpClient.PostAsJsonAsync("api/platos", plato);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Plato>();
    }

    public async Task<Plato> UpdatePlatoAsync(Plato plato)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/platos/{plato.Id}", plato);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Plato>();
    }

    public async Task<Plato> GetPlatoByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/platos/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Plato>();
    }
}

public class LoginResponse
{
    public string Token { get; set; }
    public Usuario Usuario { get; set; }
} 