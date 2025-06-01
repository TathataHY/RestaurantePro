namespace RestaurantePro.Domain.Operaciones.Services;

/// <summary>
/// Servicio de validación para reglas de negocio de reservaciones
/// Abstrae las validaciones complejas y políticas empresariales
/// </summary>
public interface IValidacionReservacionService
{
    /// <summary>
    /// Valida si el horario está dentro de las horas permitidas para reservaciones
    /// </summary>
    /// <param name="fechaHora">Fecha y hora a validar</param>
    /// <returns>Resultado de la validación</returns>
    Result ValidarHorarioPermitido(DateTime fechaHora);

    /// <summary>
    /// Valida si una mesa tiene capacidad suficiente para el número de personas
    /// </summary>
    /// <param name="mesa">Mesa a validar</param>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <returns>Resultado de la validación</returns>
    Result ValidarCapacidadMesa(Mesa mesa, int numeroPersonas);

    /// <summary>
    /// Valida el límite de reservaciones por cliente en una fecha específica
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="fecha">Fecha a validar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la validación asíncrona</returns>
    Task<Result> ValidarLimiteReservacionesPorCliente(Guid clienteId, DateTime fecha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si se puede hacer una reservación con la anticipación especificada
    /// </summary>
    /// <param name="fechaReservacion">Fecha de la reservación</param>
    /// <param name="fechaActual">Fecha actual (opcional, por defecto DateTime.Now)</param>
    /// <returns>Resultado de la validación</returns>
    Result ValidarAnticipacionMinima(DateTime fechaReservacion, DateTime? fechaActual = null);

    /// <summary>
    /// Valida las políticas especiales para ocasiones especiales
    /// </summary>
    /// <param name="tipoOcasion">Tipo de ocasión especial</param>
    /// <param name="fechaHora">Fecha y hora de la reservación</param>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <returns>Resultado de la validación</returns>
    Result ValidarOcasionEspecial(string tipoOcasion, DateTime fechaHora, int numeroPersonas);

    /// <summary>
    /// Valida si un cliente puede cancelar una reservación según las políticas
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="fechaCancelacion">Fecha de cancelación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la validación</returns>
    Task<Result> ValidarPoliticaCancelacionAsync(Guid reservacionId, Guid clienteId, DateTime fechaCancelacion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida las restricciones especiales de días festivos o eventos
    /// </summary>
    /// <param name="fechaHora">Fecha y hora a validar</param>
    /// <param name="numeroPersonas">Número de personas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la validación</returns>
    Task<Result> ValidarRestriccionesDiaEspecialAsync(DateTime fechaHora, int numeroPersonas, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si se puede modificar una reservación existente
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="nuevaFechaHora">Nueva fecha y hora propuesta</param>
    /// <param name="nuevoNumeroPersonas">Nuevo número de personas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la validación</returns>
    Task<Result> ValidarModificacionReservacionAsync(Guid reservacionId, DateTime nuevaFechaHora, int nuevoNumeroPersonas, CancellationToken cancellationToken = default);
} 