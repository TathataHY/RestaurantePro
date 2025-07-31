using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;

namespace RestaurantePro.Mobile.Core.Services.Commercial;

public class ClientesService : IClientesService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    public ClientesService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    public async Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesAsync(bool soloActivos = true)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes?soloActivos={soloActivos}", token);
            if (response.Succeeded && response.Data != null)
            {
                return ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(response.Data.Items.ToList(), "Clientes obtenidos exitosamente");
            }
            return ApiResponse<List<ClienteSummaryDto>>.Failure("No se pudieron obtener los clientes");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ClienteSummaryDto>>.Failure($"Error al obtener clientes: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ClienteDto>> ObtenerClienteAsync(Guid id)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<ClienteDto>($"api/comercial/clientes/{id}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ClienteDto>.Failure($"Error al obtener cliente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ClienteSummaryDto>>> BuscarClientesAsync(string terminoBusqueda)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes?filtroTexto={terminoBusqueda}", token);
            if (response.Succeeded && response.Data != null)
            {
                return ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(response.Data.Items.ToList(), "Clientes encontrados");
            }
            return ApiResponse<List<ClienteSummaryDto>>.Failure("No se pudieron buscar los clientes");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ClienteSummaryDto>>.Failure($"Error al buscar clientes: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ClienteDto>> CrearClienteAsync(ClienteDto cliente)
    {
        try
        {
            var response = await _apiService.PostAsync<ClienteDto>("api/comercial/clientes", cliente);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ClienteDto>.Failure($"Error al crear cliente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ClienteDto>> ActualizarClienteAsync(Guid id, ClienteDto cliente)
    {
        try
        {
            var response = await _apiService.PutAsync<ClienteDto>($"api/comercial/clientes/{id}", cliente);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<ClienteDto>.Failure($"Error al actualizar cliente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> EliminarClienteAsync(Guid id)
    {
        try
        {
            var response = await _apiService.DeleteAsync($"api/comercial/clientes/{id}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al eliminar cliente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EstadisticasClientesDto>> ObtenerEstadisticasAsync()
    {
        try
        {
            // Como no hay endpoint específico de estadísticas, retornamos un error
            return ApiResponse<EstadisticasClientesDto>.Failure("Endpoint de estadísticas no disponible");
        }
        catch (Exception ex)
        {
            return ApiResponse<EstadisticasClientesDto>.Failure($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesFrecuentesAsync(int cantidad = 10)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes?soloClientesFrecuentes=true&pageSize={cantidad}", token);
            if (response.Succeeded && response.Data != null)
            {
                return ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(response.Data.Items.ToList(), "Clientes frecuentes obtenidos");
            }
            return ApiResponse<List<ClienteSummaryDto>>.Failure("No se pudieron obtener los clientes frecuentes");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ClienteSummaryDto>>.Failure($"Error al obtener clientes frecuentes: {ex.Message}");
        }
    }

    // Métodos adicionales para ViewModels
    public async Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesAsync(FiltroClientesDto filtro)
    {
        try
        {
            // Usamos el endpoint principal con parámetros de consulta
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(filtro.Busqueda)) queryParams.Add($"filtroTexto={filtro.Busqueda}");
            if (filtro.SoloActivos.HasValue) queryParams.Add($"soloActivos={filtro.SoloActivos}");
            if (!string.IsNullOrEmpty(filtro.Segmento)) queryParams.Add($"segmento={filtro.Segmento}");
            if (filtro.SoloConTarjetaFidelizacion.HasValue) queryParams.Add($"soloConTarjetaFidelizacion={filtro.SoloConTarjetaFidelizacion}");
            if (filtro.FechaRegistroDesde.HasValue) queryParams.Add($"fechaRegistroDesde={filtro.FechaRegistroDesde:yyyy-MM-dd}");
            if (filtro.FechaRegistroHasta.HasValue) queryParams.Add($"fechaRegistroHasta={filtro.FechaRegistroHasta:yyyy-MM-dd}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes{queryString}", token);
            if (response.Succeeded && response.Data != null)
            {
                return ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(response.Data.Items.ToList(), "Clientes obtenidos");
            }
            return ApiResponse<List<ClienteSummaryDto>>.Failure("No se pudieron obtener los clientes");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ClienteSummaryDto>>.Failure($"Error al obtener clientes con filtro: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesPorSegmentoAsync(string segmento)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes?segmento={segmento}", token);
            if (response.Succeeded && response.Data != null)
            {
                return ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(response.Data.Items.ToList(), "Clientes obtenidos");
            }
            return ApiResponse<List<ClienteSummaryDto>>.Failure("No se pudieron obtener los clientes");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ClienteSummaryDto>>.Failure($"Error al obtener clientes por segmento: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesConTarjetaFidelizacionAsync()
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<PaginatedList<ClienteSummaryDto>>("api/comercial/clientes?soloConTarjetaFidelizacion=true", token);
            if (response.Succeeded && response.Data != null)
            {
                return ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(response.Data.Items.ToList(), "Clientes obtenidos");
            }
            return ApiResponse<List<ClienteSummaryDto>>.Failure("No se pudieron obtener los clientes");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ClienteSummaryDto>>.Failure($"Error al obtener clientes con tarjeta de fidelización: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ClienteSummaryDto>>> ObtenerClientesPorFechaRegistroAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<PaginatedList<ClienteSummaryDto>>($"api/comercial/clientes?fechaRegistroDesde={fechaDesde:yyyy-MM-dd}&fechaRegistroHasta={fechaHasta:yyyy-MM-dd}", token);
            if (response.Succeeded && response.Data != null)
            {
                return ApiResponse<List<ClienteSummaryDto>>.SuccessResponse(response.Data.Items.ToList(), "Clientes obtenidos");
            }
            return ApiResponse<List<ClienteSummaryDto>>.Failure("No se pudieron obtener los clientes");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ClienteSummaryDto>>.Failure($"Error al obtener clientes por fecha de registro: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DesactivarClienteAsync(Guid clienteId)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.DeleteAsync($"api/comercial/clientes/{clienteId}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al desactivar cliente: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<ComandaDto>>> ObtenerHistorialComandasAsync(Guid clienteId)
    {
        try
        {
            // Como no hay endpoint específico de historial de comandas, retornamos un error
            return ApiResponse<List<ComandaDto>>.Failure("Endpoint de historial de comandas no disponible");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ComandaDto>>.Failure($"Error al obtener historial de comandas: {ex.Message}");
        }
    }
} 