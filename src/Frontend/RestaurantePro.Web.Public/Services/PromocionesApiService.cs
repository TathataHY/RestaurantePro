using System.Net.Http.Json;
using RestaurantePro.Web.Public.Models;

namespace RestaurantePro.Web.Public.Services;

public class PromocionesApiService
{
    private readonly HttpClient _http;

    public PromocionesApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PromocionDto>> ObtenerAsync(bool soloVigentes = true, string ordenarPor = "FechaCreacion", string direccion = "desc")
    {
        var url = $"api/public/promociones?soloVigentes={soloVigentes}&ordenarPor={Uri.EscapeDataString(ordenarPor)}&direccion={Uri.EscapeDataString(direccion)}";
        try
        {
            var resp = await _http.GetFromJsonAsync<ApiResponse<List<PromocionDto>>>(url);
            return resp?.Data ?? new List<PromocionDto>();
        }
        catch
        {
            return new List<PromocionDto>();
        }
    }
}

public class PromocionDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal ValorDescuento { get; set; }
    public decimal MontoMinimo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool EstaVigente { get; set; }
}


