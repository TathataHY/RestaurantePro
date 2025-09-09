using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Commercial;

public class FacturasService : IFacturasService
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    public FacturasService(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    public async Task<ApiResponse<List<FacturaDto>>> ObtenerFacturasAsync(DateTime fecha, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<FacturaDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<FacturaDto>>($"api/comercial/facturas?fecha={fecha:yyyy-MM-dd}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<FacturaDto>>.Failure($"Error al obtener facturas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FacturaDto>> ObtenerFacturaAsync(Guid id)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<FacturaDto>($"api/comercial/facturas/{id}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<FacturaDto>.Failure($"Error al obtener factura: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<FacturaDto>>> BuscarFacturasAsync(string busqueda, DateTime fecha, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<FacturaDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<FacturaDto>>($"api/comercial/facturas/buscar-por-termino?busqueda={busqueda}&fecha={fecha:yyyy-MM-dd}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<FacturaDto>>.Failure($"Error al buscar facturas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<FacturaDto>>> ObtenerFacturasPorFechaAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<FacturaDto>>($"api/comercial/facturas?fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<FacturaDto>>.Failure($"Error al obtener facturas por fecha: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EstadisticasFacturasDto>> ObtenerEstadisticasAsync(DateTime fecha, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<EstadisticasFacturasDto>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<EstadisticasFacturasDto>($"api/comercial/facturas/estadisticas?fecha={fecha:yyyy-MM-dd}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<EstadisticasFacturasDto>.Failure($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ImprimirFacturaAsync(Guid facturaId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<bool>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PostAsync<bool>($"api/comercial/facturas/{facturaId}/imprimir", null, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al imprimir factura: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> EnviarFacturaPorEmailAsync(Guid id, string email, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<string>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var request = new { Email = email };
            var response = await _apiService.PostAsync<string>($"api/comercial/facturas/{id}/enviar-email", request, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure($"Error al enviar factura por email: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<FacturaDto>>> ObtenerFacturasPendientesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<List<FacturaDto>>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<List<FacturaDto>>("api/comercial/facturas/pendientes", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<FacturaDto>>.Failure($"Error al obtener facturas pendientes: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> RegistrarPagoAsync(Guid facturaId, RegistrarPagoDto pagoDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<bool>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.PostAsync<bool>($"api/comercial/facturas/{facturaId}/pagar", pagoDto, token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al registrar pago: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> AnularFacturaAsync(Guid facturaId, AnularFacturaDto anulacionDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<bool>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.DeleteAsync($"api/comercial/facturas/{facturaId}", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al anular factura: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DescargarFacturaPdfAsync(Guid facturaId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ApiResponse<string>.Failure("Operación cancelada por el usuario");
            var token = await _authService.GetTokenAsync();
            var response = await _apiService.GetAsync<string>($"api/comercial/facturas/{facturaId}/pdf", token);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure($"Error al descargar factura PDF: {ex.Message}");
        }
    }
} 