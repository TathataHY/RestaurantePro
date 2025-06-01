namespace RestaurantePro.Domain.Operaciones.Services;

/// <summary>
/// Servicio de disponibilidad para reservaciones y mesas
/// Abstrae las validaciones de disponibilidad de mesas y horarios
/// </summary>
public interface IDisponibilidadService
{
    /// <summary>
    /// Verifica la disponibilidad de una mesa específica en una fecha y hora
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="fechaHora">Fecha y hora deseada</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la verificación</returns>
    Task<Result> VerificarDisponibilidadAsync(Guid mesaId, DateTime fechaHora, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica la disponibilidad general para un número de personas en una fecha
    /// </summary>
    /// <param name="fechaHora">Fecha y hora deseada</param>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado con mesas disponibles</returns>
    Task<Result<IEnumerable<Guid>>> VerificarDisponibilidadGeneralAsync(DateTime fechaHora, int numeroPersonas, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene alternativas de horarios disponibles cerca de la hora solicitada
    /// </summary>
    /// <param name="fechaHora">Fecha y hora deseada originalmente</param>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <param name="rangoMinutos">Rango en minutos para buscar alternativas (por defecto 120)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de horarios alternativos disponibles</returns>
    Task<Result<IEnumerable<DateTime>>> ObtenerAlternativasHorarioAsync(DateTime fechaHora, int numeroPersonas, int rangoMinutos = 120, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si una mesa tiene la capacidad adecuada para el número de personas
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la validación de capacidad</returns>
    Task<Result> ValidarCapacidadMesaAsync(Guid mesaId, int numeroPersonas, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula la ocupación actual del restaurante para una fecha y hora
    /// </summary>
    /// <param name="fechaHora">Fecha y hora a evaluar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Porcentaje de ocupación (0-100)</returns>
    Task<Result<decimal>> CalcularOcupacionAsync(DateTime fechaHora, CancellationToken cancellationToken = default);
} 