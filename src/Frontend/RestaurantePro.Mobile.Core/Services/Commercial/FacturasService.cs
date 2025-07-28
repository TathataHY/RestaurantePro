using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;

namespace RestaurantePro.Mobile.Core.Services.Commercial;

public class FacturasService : IFacturasService
{
    private readonly IApiService _apiService;

    public FacturasService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<List<FacturaDto>>> ObtenerFacturasAsync(DateTime fecha)
    {
        try
        {
            var response = await _apiService.GetAsync<List<FacturaDto>>($"api/facturas?fecha={fecha:yyyy-MM-dd}");
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
            var response = await _apiService.GetAsync<FacturaDto>($"api/facturas/{id}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<FacturaDto>.Failure($"Error al obtener factura: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<FacturaDto>>> BuscarFacturasAsync(string busqueda, DateTime fecha)
    {
        try
        {
            var response = await _apiService.GetAsync<List<FacturaDto>>($"api/facturas/buscar?busqueda={busqueda}&fecha={fecha:yyyy-MM-dd}");
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
            var response = await _apiService.GetAsync<List<FacturaDto>>($"api/facturas/por-fecha?fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<FacturaDto>>.Failure($"Error al obtener facturas por fecha: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EstadisticasFacturasDto>> ObtenerEstadisticasAsync(DateTime fecha)
    {
        try
        {
            var response = await _apiService.GetAsync<EstadisticasFacturasDto>($"api/facturas/estadisticas?fecha={fecha:yyyy-MM-dd}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<EstadisticasFacturasDto>.Failure($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ImprimirFacturaAsync(Guid facturaId)
    {
        try
        {
            var response = await _apiService.PostAsync<bool>($"api/facturas/{facturaId}/imprimir", null);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al imprimir factura: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> EnviarFacturaPorEmailAsync(Guid id, string email)
    {
        try
        {
            var request = new { Email = email };
            var response = await _apiService.PostAsync<string>($"api/facturas/{id}/enviar-email", request);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure($"Error al enviar factura por email: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<FacturaDto>>> ObtenerFacturasPendientesAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<List<FacturaDto>>("api/facturas/pendientes");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<FacturaDto>>.Failure($"Error al obtener facturas pendientes: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> RegistrarPagoAsync(Guid facturaId, RegistrarPagoDto pagoDto)
    {
        try
        {
            var response = await _apiService.PostAsync<bool>($"api/facturas/{facturaId}/registrar-pago", pagoDto);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al registrar pago: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> AnularFacturaAsync(Guid facturaId, AnularFacturaDto anulacionDto)
    {
        try
        {
            var response = await _apiService.PostAsync<bool>($"api/facturas/{facturaId}/anular", anulacionDto);
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al anular factura: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DescargarFacturaPdfAsync(Guid facturaId)
    {
        try
        {
            var response = await _apiService.GetAsync<string>($"api/facturas/{facturaId}/descargar-pdf");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure($"Error al descargar factura PDF: {ex.Message}");
        }
    }
} 