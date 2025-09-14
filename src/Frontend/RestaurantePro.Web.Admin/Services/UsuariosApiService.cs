using System.Net.Http.Json;
using RestaurantePro.Web.Admin.Models;

namespace RestaurantePro.Web.Admin.Services;

public class UsuariosApiService : IUsuariosApiService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly TokenStore _tokenStore;

    public UsuariosApiService(IHttpClientFactory httpFactory, TokenStore tokenStore)
    {
        _httpFactory = httpFactory;
        _tokenStore = tokenStore;
    }

    public async Task<List<UsuarioDto>> ObtenerUsuariosAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? filtro = null,
        bool? soloActivos = null,
        string? rol = null,
        string orderBy = "NombreCompleto",
        string orderDirection = "asc")
    {
        var http = _httpFactory.CreateClient("Api");
        // Refuerzo: adjuntar explícitamente el token
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            if (!http.DefaultRequestHeaders.Contains("X-Bearer-Token"))
            {
                http.DefaultRequestHeaders.Add("X-Bearer-Token", _tokenStore.Token);
            }
        }
        var url = $"api/core/usuarios?pageNumber={pageNumber}&pageSize={pageSize}&orderBy={orderBy}&orderDirection={orderDirection}";
        
        // Solo agregar soloActivos si tiene un valor específico
        if (soloActivos.HasValue)
        {
            url += $"&soloActivos={soloActivos.Value}";
        }
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            url += $"&filtro={Uri.EscapeDataString(filtro)}";
        }
        if (!string.IsNullOrWhiteSpace(rol))
        {
            url += $"&rol={Uri.EscapeDataString(rol)}";
        }
        var res = await http.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            return new List<UsuarioDto>();
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<List<UsuarioDto>>>();
        return resp?.Data ?? new List<UsuarioDto>();
    }

    public async Task<UsuarioDto?> ObtenerPorIdAsync(Guid id)
    {
        Console.WriteLine($"🔍 [TELEFONO DEBUG FRONTEND] Solicitando usuario con ID: {id}");
        
        var http = _httpFactory.CreateClient("Api");
        if (!string.IsNullOrWhiteSpace(_tokenStore.Token))
        {
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenStore.Token);
            if (!http.DefaultRequestHeaders.Contains("X-Bearer-Token"))
            {
                http.DefaultRequestHeaders.Add("X-Bearer-Token", _tokenStore.Token);
            }
        }
        var resp = await http.GetFromJsonAsync<ApiResponse<UsuarioDto>>($"api/core/usuarios/{id}");
        
        if (resp?.Data != null)
        {
            Console.WriteLine($"🔍 [TELEFONO DEBUG FRONTEND] Usuario recibido - ID: {resp.Data.Id}, Email: {resp.Data.Email}, Telefono: '{resp.Data.Telefono ?? "NULL"}'");
        }
        else
        {
            Console.WriteLine($"🔍 [TELEFONO DEBUG FRONTEND] No se recibió usuario para ID: {id}");
        }
        
        return resp?.Data;
    }

    public async Task<(UsuarioDto? Data, string? ErrorMessage)> CrearAsync(CrearUsuarioRequest request)
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
        var res = await http.PostAsJsonAsync("api/core/usuarios", request);
        if (!res.IsSuccessStatusCode)
        {
            // Intentar leer el mensaje de error del backend
            try
            {
                var errorContent = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"🔍 [ERROR DEBUG] Respuesta del servidor: {errorContent}");
                
                var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<object>>(errorContent);
                
                // Procesar errores para mostrar solo los más importantes
                var errorMessage = errorResponse?.Message ?? "Error desconocido";
                if (errorResponse?.Errors?.Any() == true)
                {
                    errorMessage = ProcesarErroresDeValidacion(errorResponse.Errors);
                }
                
                return (null, errorMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔍 [ERROR DEBUG] Error al parsear respuesta: {ex.Message}");
                return (null, $"Error del servidor: {res.StatusCode}");
            }
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<UsuarioDto>>();
        return (resp?.Data, null);
    }

    public async Task<(UsuarioDto? Data, string? ErrorMessage)> ActualizarAsync(Guid id, ActualizarUsuarioRequest request)
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
        var res = await http.PutAsJsonAsync($"api/core/usuarios/{id}", request);
        if (!res.IsSuccessStatusCode)
        {
            // Intentar leer el mensaje de error del backend
            try
            {
                var errorContent = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"🔍 [ERROR DEBUG] Respuesta del servidor: {errorContent}");
                
                var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<object>>(errorContent);
                
                // Procesar errores para mostrar solo los más importantes
                var errorMessage = errorResponse?.Message ?? "Error desconocido";
                if (errorResponse?.Errors?.Any() == true)
                {
                    errorMessage = ProcesarErroresDeValidacion(errorResponse.Errors);
                }
                
                return (null, errorMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔍 [ERROR DEBUG] Error al parsear respuesta: {ex.Message}");
                return (null, $"Error del servidor: {res.StatusCode}");
            }
        }
        var resp = await res.Content.ReadFromJsonAsync<ApiResponse<UsuarioDto>>();
        return (resp?.Data, null);
    }

    public async Task<bool> EliminarAsync(Guid id)
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
        var res = await http.DeleteAsync($"api/core/usuarios/{id}");
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> CambiarEstadoAsync(Guid id, bool activo)
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
        var res = await http.PostAsync($"api/core/usuarios/{id}/cambiar-estado?activo={activo}", null);
        return res.IsSuccessStatusCode;
    }

    public async Task<List<string>> ObtenerRolesDisponiblesAsync()
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
        var resp = await http.GetFromJsonAsync<ApiResponse<List<string>>>("api/core/usuarios/roles");
        return resp?.Data ?? new List<string>();
    }

    /// <summary>
    /// Procesa los errores de validación para mostrar solo los más importantes de forma legible
    /// </summary>
    private string ProcesarErroresDeValidacion(List<string> errores)
    {
        if (errores == null || !errores.Any())
            return "Error de validación";

        var erroresImportantes = new List<string>();
        var erroresSecundarios = new List<string>();

        foreach (var error in errores)
        {
            // Separar errores múltiples en el mismo string (separados por punto y coma)
            var erroresIndividuales = error.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                         .Select(e => e.Trim())
                                         .Where(e => !string.IsNullOrEmpty(e));

            foreach (var errorIndividual in erroresIndividuales)
            {
                // Clasificar errores por importancia
                if (EsErrorImportante(errorIndividual))
                {
                    erroresImportantes.Add(errorIndividual);
                }
                else
                {
                    erroresSecundarios.Add(errorIndividual);
                }
            }
        }

        // Construir mensaje final
        var mensajeFinal = "❌ Error de validación:";
        
        if (erroresImportantes.Any())
        {
            // Mostrar máximo 3 errores importantes
            var erroresAMostrar = erroresImportantes.Take(3);
            mensajeFinal += "\n• " + string.Join("\n• ", erroresAMostrar);
            
            // Si hay más de 3 errores importantes, agregar contador
            if (erroresImportantes.Count > 3)
            {
                var erroresRestantes = erroresImportantes.Count - 3;
                mensajeFinal += $"\n• ... y {erroresRestantes} error(es) adicional(es)";
            }
        }
        else if (erroresSecundarios.Any())
        {
            // Si no hay errores importantes, mostrar los primeros 3 secundarios
            var erroresAMostrar = erroresSecundarios.Take(3);
            mensajeFinal += "\n• " + string.Join("\n• ", erroresAMostrar);
            
            if (erroresSecundarios.Count > 3)
            {
                var erroresRestantes = erroresSecundarios.Count - 3;
                mensajeFinal += $"\n• ... y {erroresRestantes} error(es) adicional(es)";
            }
        }

        return mensajeFinal;
    }

    /// <summary>
    /// Determina si un error es importante (crítico para el usuario)
    /// </summary>
    private bool EsErrorImportante(string error)
    {
        var erroresImportantes = new[]
        {
            "es requerido",
            "no tiene un formato válido",
            "debe ser uno de:",
            "ya existe",
            "no encontrado",
            "no válido",
            "no es compatible",
            "no es válido",
            "debe tener",
            "solo puede contener",
            "formato válido",
            "nivel de acceso",
            "rol asignado"
        };

        return erroresImportantes.Any(importante => 
            error.Contains(importante, StringComparison.OrdinalIgnoreCase));
    }
}


