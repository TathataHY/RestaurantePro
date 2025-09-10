using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

/// <summary>
/// Servicio para gestionar facturas del restaurante
/// </summary>
public class FacturasApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public FacturasApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
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

    /// <summary>
    /// Obtiene la lista paginada de facturas con filtros
    /// </summary>
    public async Task<PaginatedList<FacturaDto>?> ObtenerFacturasAsync(FacturaFiltrosDto filtros)
    {
        try
        {
            var http = CreateClient();
            var queryParams = new List<string>
            {
                $"pageNumber={filtros.PageNumber}",
                $"pageSize={filtros.PageSize}",
                $"ordenarPor={filtros.OrdenarPor}",
                $"direccionOrden={filtros.DireccionOrden}"
            };

            if (!string.IsNullOrEmpty(filtros.Busqueda))
                queryParams.Add($"busqueda={Uri.EscapeDataString(filtros.Busqueda)}");
            
            if (!string.IsNullOrEmpty(filtros.Estado))
                queryParams.Add($"estado={Uri.EscapeDataString(filtros.Estado)}");
            
            if (!string.IsNullOrEmpty(filtros.TipoPago))
                queryParams.Add($"tipoPago={Uri.EscapeDataString(filtros.TipoPago)}");
            
            if (!string.IsNullOrEmpty(filtros.MetodoPago))
                queryParams.Add($"metodoPago={Uri.EscapeDataString(filtros.MetodoPago)}");
            
            if (filtros.ClienteId.HasValue)
                queryParams.Add($"clienteId={filtros.ClienteId.Value}");
            
            if (filtros.MesaId.HasValue)
                queryParams.Add($"mesaId={filtros.MesaId.Value}");
            
            if (filtros.MeseroId.HasValue)
                queryParams.Add($"meseroId={filtros.MeseroId.Value}");
            
            if (filtros.FechaInicio.HasValue)
                queryParams.Add($"fechaInicio={filtros.FechaInicio.Value:yyyy-MM-dd}");
            
            if (filtros.FechaFin.HasValue)
                queryParams.Add($"fechaFin={filtros.FechaFin.Value:yyyy-MM-dd}");
            
            if (filtros.MontoMinimo.HasValue)
                queryParams.Add($"montoMinimo={filtros.MontoMinimo.Value}");
            
            if (filtros.MontoMaximo.HasValue)
                queryParams.Add($"montoMaximo={filtros.MontoMaximo.Value}");
            
            if (filtros.EsFacturaElectronica.HasValue)
                queryParams.Add($"esFacturaElectronica={filtros.EsFacturaElectronica.Value}");

            var queryString = string.Join("&", queryParams);
            var response = await http.GetFromJsonAsync<ApiResponse<PaginatedList<FacturaDto>>>($"api/comercial/facturas?{queryString}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener facturas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene una factura específica por ID
    /// </summary>
    public async Task<FacturaDto?> ObtenerFacturaAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<FacturaDto>>($"api/comercial/facturas/{id}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener factura: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Crea una nueva factura
    /// </summary>
    public async Task<ApiResponse<FacturaDto>?> CrearFacturaAsync(CrearFacturaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync("api/comercial/facturas", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear factura: {ex.Message}");
            return new ApiResponse<FacturaDto>
            {
                Success = false,
                Message = $"Error al crear factura: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Actualiza una factura existente
    /// </summary>
    public async Task<ApiResponse<FacturaDto>?> ActualizarFacturaAsync(ActualizarFacturaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PutAsJsonAsync($"api/comercial/facturas/{request.Id}", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar factura: {ex.Message}");
            return new ApiResponse<FacturaDto>
            {
                Success = false,
                Message = $"Error al actualizar factura: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Elimina una factura (soft delete)
    /// </summary>
    public async Task<ApiResponse<bool>?> EliminarFacturaAsync(Guid id)
    {
        try
        {
            var http = CreateClient();
            var response = await http.DeleteAsync($"api/comercial/facturas/{id}");
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar factura: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al eliminar factura: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Cancela una factura
    /// </summary>
    public async Task<ApiResponse<bool>?> CancelarFacturaAsync(CancelarFacturaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/comercial/facturas/{request.FacturaId}/cancelar", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cancelar factura: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al cancelar factura: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Registra un pago para una factura
    /// </summary>
    public async Task<ApiResponse<FacturaPagoDto>?> RegistrarPagoAsync(RegistrarPagoRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/comercial/facturas/{request.FacturaId}/pagar", request);
            return await response.Content.ReadFromJsonAsync<ApiResponse<FacturaPagoDto>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al registrar pago: {ex.Message}");
            return new ApiResponse<FacturaPagoDto>
            {
                Success = false,
                Message = $"Error al registrar pago: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene las estadísticas de facturas
    /// </summary>
    public async Task<FacturaEstadisticasDto?> ObtenerEstadisticasAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<FacturaEstadisticasDto>>("api/comercial/facturas/estadisticas");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estadísticas de facturas: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los pagos de una factura
    /// </summary>
    public async Task<List<FacturaPagoDto>?> ObtenerPagosAsync(Guid facturaId)
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<FacturaPagoDto>>>($"api/comercial/facturas/{facturaId}/pagos");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener pagos: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Reimprime una factura
    /// </summary>
    public async Task<ApiResponse<byte[]>?> ReimprimirFacturaAsync(ReimprimirFacturaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/comercial/facturas/{request.FacturaId}/reimprimir", request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = content,
                    Message = "Factura reimpresa correctamente"
                };
            }
            else
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = $"Error al reimprimir factura: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al reimprimir factura: {ex.Message}");
            return new ApiResponse<byte[]>
            {
                Success = false,
                Message = $"Error al reimprimir factura: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Genera factura electrónica
    /// </summary>
    public async Task<ApiResponse<byte[]>?> GenerarFacturaElectronicaAsync(GenerarFacturaElectronicaRequest request)
    {
        try
        {
            var http = CreateClient();
            var response = await http.PostAsJsonAsync($"api/comercial/facturas/{request.FacturaId}/factura-electronica", request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = content,
                    Message = "Factura electrónica generada correctamente"
                };
            }
            else
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = $"Error al generar factura electrónica: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar factura electrónica: {ex.Message}");
            return new ApiResponse<byte[]>
            {
                Success = false,
                Message = $"Error al generar factura electrónica: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene los métodos de pago disponibles
    /// </summary>
    public async Task<List<string>?> ObtenerMetodosPagoAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/comercial/facturas/metodos-pago");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener métodos de pago: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los estados de factura disponibles
    /// </summary>
    public async Task<List<string>?> ObtenerEstadosAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/comercial/facturas/estados");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener estados: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Obtiene los tipos de pago disponibles
    /// </summary>
    public async Task<List<string>?> ObtenerTiposPagoAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/comercial/facturas/tipos-pago");
            return response?.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener tipos de pago: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Exporta la lista de facturas a Excel
    /// </summary>
    public async Task<ApiResponse<byte[]>?> ExportarFacturasAsync(FacturaFiltrosDto filtros, string formato = "Excel")
    {
        try
        {
            var http = CreateClient();
            var request = new { Filtros = filtros, Formato = formato };
            var response = await http.PostAsJsonAsync("api/comercial/facturas/exportar", request);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = content,
                    Message = "Facturas exportadas correctamente"
                };
            }
            else
            {
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = $"Error al exportar facturas: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar facturas: {ex.Message}");
            return new ApiResponse<byte[]>
            {
                Success = false,
                Message = $"Error al exportar facturas: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Obtiene el siguiente número de factura
    /// </summary>
    public async Task<ApiResponse<string>?> ObtenerSiguienteNumeroFacturaAsync()
    {
        try
        {
            var http = CreateClient();
            var response = await http.GetFromJsonAsync<ApiResponse<string>>("api/comercial/facturas/siguiente-numero");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener siguiente número de factura: {ex.Message}");
            return new ApiResponse<string>
            {
                Success = false,
                Message = $"Error al obtener siguiente número de factura: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Valida si un número de factura ya existe
    /// </summary>
    public async Task<ApiResponse<bool>?> ValidarNumeroFacturaAsync(string numeroFactura, Guid? facturaIdExcluir = null)
    {
        try
        {
            var http = CreateClient();
            var queryParams = $"numeroFactura={Uri.EscapeDataString(numeroFactura)}";
            if (facturaIdExcluir.HasValue)
                queryParams += $"&excluirId={facturaIdExcluir.Value}";
            
            var response = await http.GetFromJsonAsync<ApiResponse<bool>>($"api/comercial/facturas/validar-numero?{queryParams}");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al validar número de factura: {ex.Message}");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error al validar número de factura: {ex.Message}"
            };
        }
    }
}
