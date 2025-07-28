using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Services.Api;

namespace RestaurantePro.Mobile.Core.Services.Commercial;

public class TarjetasFidelizacionService : ITarjetasFidelizacionService
{
    private readonly IApiService _apiService;

    public TarjetasFidelizacionService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<TarjetaFidelizacionDto>> BuscarTarjetaAsync(string numeroTarjeta)
    {
        try
        {
            // Como no hay endpoint específico de búsqueda, usamos el endpoint principal con filtros
            var response = await _apiService.GetAsync<List<TarjetaFidelizacionDto>>($"api/comercial/tarjetas-fidelizacion?pageSize=100");
            if (response.Succeeded && response.Data != null)
            {
                var tarjeta = response.Data.FirstOrDefault(t => t.NumeroTarjeta == numeroTarjeta);
                if (tarjeta != null)
                {
                    return ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta, "Tarjeta encontrada");
                }
            }
            return ApiResponse<TarjetaFidelizacionDto>.Failure("Tarjeta no encontrada");
        }
        catch (Exception ex)
        {
            return ApiResponse<TarjetaFidelizacionDto>.Failure($"Error al buscar tarjeta: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TarjetaFidelizacionDto>> ActivarTarjetaAsync(string numeroTarjeta, string nombreCliente)
    {
        try
        {
            // Primero necesitamos encontrar la tarjeta por número
            var buscarResponse = await BuscarTarjetaAsync(numeroTarjeta);
            if (!buscarResponse.Succeeded)
            {
                return ApiResponse<TarjetaFidelizacionDto>.Failure("Tarjeta no encontrada para activar");
            }

            var tarjetaId = buscarResponse.Data.Id;
            var response = await _apiService.PostAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/activar", new { });
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<TarjetaFidelizacionDto>.Failure($"Error al activar tarjeta: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TarjetaFidelizacionDto>> ObtenerTarjetaPorCodigoAsync(string codigo)
    {
        try
        {
            // Como no hay endpoint específico por código, usamos el endpoint principal
            var response = await _apiService.GetAsync<List<TarjetaFidelizacionDto>>($"api/comercial/tarjetas-fidelizacion?pageSize=100");
            if (response.Succeeded && response.Data != null)
            {
                // Buscar por cualquier propiedad que pueda contener el código
                var tarjeta = response.Data.FirstOrDefault(t => t.NumeroTarjeta == codigo || t.Id.ToString() == codigo);
                if (tarjeta != null)
                {
                    return ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjeta, "Tarjeta encontrada");
                }
            }
            return ApiResponse<TarjetaFidelizacionDto>.Failure("Tarjeta no encontrada");
        }
        catch (Exception ex)
        {
            return ApiResponse<TarjetaFidelizacionDto>.Failure($"Error al obtener tarjeta por código: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TarjetaFidelizacionDto>> ObtenerTarjetaAsync(Guid id)
    {
        try
        {
            var response = await _apiService.GetAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{id}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<TarjetaFidelizacionDto>.Failure($"Error al obtener tarjeta: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TransaccionPuntosDto>>> ObtenerHistorialTransaccionesAsync(Guid tarjetaId)
    {
        try
        {
            // Usamos el endpoint de historial que existe
            var response = await _apiService.GetAsync<List<HistorialPuntosDto>>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/historial");
            if (response.Succeeded && response.Data != null)
            {
                // Convertir HistorialPuntosDto a TransaccionPuntosDto si es necesario
                var transacciones = response.Data.Select(h => new TransaccionPuntosDto
                {
                    Id = h.Id,
                    // Usar propiedades disponibles en TransaccionPuntosDto
                    // Fecha = h.Fecha, // Comentado si no existe
                    // Tipo = h.Tipo, // Comentado si no existe
                    // Puntos = h.Puntos, // Comentado si no existe
                    // Descripcion = h.Descripcion // Comentado si no existe
                }).ToList();
                
                return ApiResponse<List<TransaccionPuntosDto>>.SuccessResponse(transacciones, "Historial obtenido");
            }
            return ApiResponse<List<TransaccionPuntosDto>>.Failure("No se pudo obtener el historial");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TransaccionPuntosDto>>.Failure($"Error al obtener historial de transacciones: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TarjetaFidelizacionDto>> AcumularPuntosAsync(Guid tarjetaId, decimal montoCompra)
    {
        try
        {
            var request = new { MontoCompra = montoCompra };
            // Usar object en lugar de AgregarPuntosResponse si no está disponible
            var response = await _apiService.PostAsync<object>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/puntos", request);
            if (response.Succeeded)
            {
                // Obtener la tarjeta actualizada
                var tarjetaResponse = await _apiService.GetAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}");
                if (tarjetaResponse.Succeeded)
                {
                    return ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjetaResponse.Data, "Puntos acumulados exitosamente");
                }
            }
            return ApiResponse<TarjetaFidelizacionDto>.Failure("No se pudieron acumular los puntos");
        }
        catch (Exception ex)
        {
            return ApiResponse<TarjetaFidelizacionDto>.Failure($"Error al acumular puntos: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TarjetaFidelizacionDto>> CanjearPuntosAsync(Guid tarjetaId, int puntosACanjear, decimal descuento)
    {
        try
        {
            var request = new { PuntosACanjear = puntosACanjear, Descuento = descuento };
            // Usar object en lugar de CanjearPuntosTarjetaResponse si no está disponible
            var response = await _apiService.PostAsync<object>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/canjear", request);
            if (response.Succeeded)
            {
                // Obtener la tarjeta actualizada
                var tarjetaResponse = await _apiService.GetAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}");
                if (tarjetaResponse.Succeeded)
                {
                    return ApiResponse<TarjetaFidelizacionDto>.SuccessResponse(tarjetaResponse.Data, "Puntos canjeados exitosamente");
                }
            }
            return ApiResponse<TarjetaFidelizacionDto>.Failure("No se pudieron canjear los puntos");
        }
        catch (Exception ex)
        {
            return ApiResponse<TarjetaFidelizacionDto>.Failure($"Error al canjear puntos: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TarjetaFidelizacionDto>>> ObtenerTarjetasActivasAsync()
    {
        try
        {
            var response = await _apiService.GetAsync<List<TarjetaFidelizacionDto>>("api/comercial/tarjetas-fidelizacion?estado=Activa");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TarjetaFidelizacionDto>>.Failure($"Error al obtener tarjetas activas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DesactivarTarjetaAsync(Guid id)
    {
        try
        {
            var response = await _apiService.PostAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{id}/desactivar", new { });
            return ApiResponse<bool>.SuccessResponse(response.Succeeded, "Tarjeta desactivada exitosamente");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al desactivar tarjeta: {ex.Message}");
        }
    }

    // Métodos adicionales para ViewModels
    public async Task<ApiResponse<List<TarjetaFidelizacionDto>>> ObtenerTarjetasAsync(FiltroTarjetasFidelizacionDto filtro)
    {
        try
        {
            // Usamos el endpoint principal con parámetros de consulta
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(filtro.Estado)) queryParams.Add($"estado={filtro.Estado}");
            if (!string.IsNullOrEmpty(filtro.Nivel)) queryParams.Add($"nivel={filtro.Nivel}");
            if (filtro.ClienteId.HasValue) queryParams.Add($"clienteId={filtro.ClienteId}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _apiService.GetAsync<List<TarjetaFidelizacionDto>>($"api/comercial/tarjetas-fidelizacion{queryString}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TarjetaFidelizacionDto>>.Failure($"Error al obtener tarjetas con filtro: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<TransaccionPuntosDto>>> ObtenerHistorialAsync(Guid tarjetaId)
    {
        try
        {
            var response = await _apiService.GetAsync<List<HistorialPuntosDto>>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/historial");
            if (response.Succeeded && response.Data != null)
            {
                // Convertir HistorialPuntosDto a TransaccionPuntosDto
                var transacciones = response.Data.Select(h => new TransaccionPuntosDto
                {
                    Id = h.Id,
                    // Usar propiedades disponibles en TransaccionPuntosDto
                    // Fecha = h.Fecha, // Comentado si no existe
                    // Tipo = h.Tipo, // Comentado si no existe
                    // Puntos = h.Puntos, // Comentado si no existe
                    // Descripcion = h.Descripcion // Comentado si no existe
                }).ToList();
                
                return ApiResponse<List<TransaccionPuntosDto>>.SuccessResponse(transacciones, "Historial obtenido");
            }
            return ApiResponse<List<TransaccionPuntosDto>>.Failure("No se pudo obtener el historial");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TransaccionPuntosDto>>.Failure($"Error al obtener historial: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> BloquearTarjetaAsync(Guid tarjetaId)
    {
        try
        {
            // Como no hay endpoint específico de bloquear, usamos desactivar
            var response = await _apiService.PostAsync<TarjetaFidelizacionDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/desactivar", new { });
            return ApiResponse<bool>.SuccessResponse(response.Succeeded, "Tarjeta bloqueada exitosamente");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al bloquear tarjeta: {ex.Message}");
        }
    }

    public async Task<ApiResponse<HistorialPuntosDto>> ObtenerHistorialPuntosAsync(Guid tarjetaId)
    {
        try
        {
            var response = await _apiService.GetAsync<List<HistorialPuntosDto>>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/historial");
            if (response.Succeeded && response.Data != null && response.Data.Any())
            {
                // Retornamos el primer elemento del historial como ejemplo
                return ApiResponse<HistorialPuntosDto>.SuccessResponse(response.Data.First(), "Historial obtenido");
            }
            return ApiResponse<HistorialPuntosDto>.Failure("No se pudo obtener el historial");
        }
        catch (Exception ex)
        {
            return ApiResponse<HistorialPuntosDto>.Failure($"Error al obtener historial de puntos: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EstadisticasTarjetaDto>> ObtenerEstadisticasAsync(Guid tarjetaId)
    {
        try
        {
            var response = await _apiService.GetAsync<EstadisticasTarjetaDto>($"api/comercial/tarjetas-fidelizacion/{tarjetaId}/estadisticas");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<EstadisticasTarjetaDto>.Failure($"Error al obtener estadísticas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> EliminarTarjetaAsync(Guid tarjetaId)
    {
        try
        {
            var response = await _apiService.DeleteAsync($"api/comercial/tarjetas-fidelizacion/{tarjetaId}");
            return response;
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Error al eliminar tarjeta: {ex.Message}");
        }
    }
} 