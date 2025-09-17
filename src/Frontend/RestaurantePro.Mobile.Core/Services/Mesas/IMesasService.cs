using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Mesas;

/// <summary>
/// Servicio para gestión de mesas del restaurante
/// </summary>
public interface IMesasService
{
    /// <summary>
    /// Obtiene todas las mesas con filtros opcionales
    /// </summary>
    /// <param name="estado">Filtro opcional por estado</param>
    /// <param name="ubicacion">Filtro opcional por ubicación</param>
    /// <param name="capacidadMinima">Filtro opcional por capacidad mínima</param>
    /// <returns>Lista de mesas</returns>
    Task<ApiResponse<PaginatedList<MesaDto>>> ObtenerMesasAsync(
        string? estado = null, 
        string? ubicacion = null, 
        int? capacidadMinima = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una mesa específica por ID
    /// </summary>
    /// <param name="id">ID de la mesa</param>
    /// <returns>Información de la mesa</returns>
    Task<ApiResponse<MesaDto>> ObtenerMesaAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las mesas disponibles
    /// </summary>
    /// <param name="capacidadMinima">Filtro opcional por capacidad mínima</param>
    /// <param name="ubicacion">Filtro opcional por ubicación</param>
    /// <returns>Lista de mesas disponibles</returns>
    Task<ApiResponse<List<MesaDto>>> ObtenerMesasDisponiblesAsync(
        int? capacidadMinima = null, 
        string? ubicacion = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el estado general de ocupación de las mesas
    /// </summary>
    /// <returns>Estado de ocupación</returns>
    Task<ApiResponse<EstadoMesasDto>> ObtenerEstadoOcupacionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asigna (ocupa) una mesa
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="clienteId">ID del cliente (opcional)</param>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <param name="observaciones">Observaciones opcionales</param>
    /// <returns>Resultado de la operación</returns>
    Task<ApiResponse<object>> AsignarMesaAsync(
        Guid mesaId, 
        Guid? clienteId = null, 
        int? numeroPersonas = null, 
        string? observaciones = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Libera una mesa
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="motivo">Motivo de liberación</param>
    /// <param name="observaciones">Observaciones opcionales</param>
    /// <returns>Información de la mesa liberada</returns>
    Task<ApiResponse<MesaDto>> LiberarMesaAsync(
        Guid mesaId, 
        string motivo = "Mesa liberada desde móvil", 
        string? observaciones = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia el estado de una mesa
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="nuevoEstado">Nuevo estado para la mesa</param>
    /// <param name="motivo">Motivo del cambio</param>
    /// <returns>Información de la mesa actualizada</returns>
    Task<ApiResponse<MesaDto>> CambiarEstadoMesaAsync(
        Guid mesaId, 
        string nuevoEstado, 
        string? motivo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca la mejor mesa disponible para un número de personas
    /// </summary>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <param name="ubicacionPreferida">Ubicación preferida (opcional)</param>
    /// <returns>La mejor mesa disponible</returns>
    Task<ApiResponse<MesaDto>> BuscarMejorMesaAsync(
        int numeroPersonas, 
        string? ubicacionPreferida = null,
        CancellationToken cancellationToken = default);
} 