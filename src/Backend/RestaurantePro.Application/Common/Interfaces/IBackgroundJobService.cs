namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de trabajos en segundo plano y recordatorios
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Programa un recordatorio de reservación
    /// </summary>
    /// <param name="reservacionId">ID de la reservación</param>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="fechaEnvio">Fecha y hora para enviar el recordatorio</param>
    /// <param name="tipoRecordatorio">Tipo de recordatorio (24h, 2h, etc.)</param>
    /// <param name="email">Email del cliente</param>
    /// <param name="telefono">Teléfono del cliente</param>
    /// <returns>ID del trabajo programado</returns>
    Task<string> ProgramarRecordatorioReservacionAsync(
        Guid reservacionId, 
        Guid clienteId, 
        DateTime fechaEnvio, 
        string tipoRecordatorio,
        string? email = null, 
        string? telefono = null);
    
    /// <summary>
    /// Programa un recordatorio de vencimiento de factura
    /// </summary>
    /// <param name="facturaId">ID de la factura</param>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="fechaVencimiento">Fecha de vencimiento</param>
    /// <param name="diasAnticipacion">Días de anticipación para el recordatorio</param>
    /// <returns>ID del trabajo programado</returns>
    Task<string> ProgramarRecordatorioFacturaAsync(
        Guid facturaId, 
        Guid clienteId, 
        DateTime fechaVencimiento, 
        int diasAnticipacion = 3);
    
    /// <summary>
    /// Programa verificación automática de stock bajo
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="intervaloVerificacion">Intervalo en horas para verificar</param>
    /// <returns>ID del trabajo programado</returns>
    Task<string> ProgramarVerificacionStockAsync(Guid ingredienteId, int intervaloVerificacion = 24);
    
    /// <summary>
    /// Programa limpieza automática de datos temporales
    /// </summary>
    /// <param name="tipoLimpieza">Tipo de limpieza (logs, cache, temp, etc.)</param>
    /// <param name="antigüedadDias">Antigüedad en días de los datos a limpiar</param>
    /// <param name="programacion">Expresión cron para la programación</param>
    /// <returns>ID del trabajo programado</returns>
    Task<string> ProgramarLimpiezaAutomaticaAsync(
        string tipoLimpieza, 
        int antigüedadDias, 
        string programacion = "0 2 * * *"); // Diario a las 2 AM por defecto
    
    /// <summary>
    /// Programa generación automática de reportes
    /// </summary>
    /// <param name="tipoReporte">Tipo de reporte (ventas, inventario, etc.)</param>
    /// <param name="destinatarios">Lista de emails destinatarios</param>
    /// <param name="programacion">Expresión cron para la programación</param>
    /// <returns>ID del trabajo programado</returns>
    Task<string> ProgramarReporteAutomaticoAsync(
        string tipoReporte, 
        List<string> destinatarios, 
        string programacion);
    
    /// <summary>
    /// Cancela un trabajo programado
    /// </summary>
    /// <param name="jobId">ID del trabajo a cancelar</param>
    /// <returns>True si se canceló correctamente</returns>
    Task<bool> CancelarTrabajoAsync(string jobId);
    
    /// <summary>
    /// Obtiene el estado de un trabajo programado
    /// </summary>
    /// <param name="jobId">ID del trabajo</param>
    /// <returns>Estado del trabajo (Pending, Running, Completed, Failed, Cancelled)</returns>
    Task<string> ObtenerEstadoTrabajoAsync(string jobId);
    
    /// <summary>
    /// Obtiene la lista de trabajos programados activos
    /// </summary>
    /// <returns>Lista de trabajos programados con su información</returns>
    Task<List<object>> ObtenerTrabajosActivosAsync();
    
    /// <summary>
    /// Programa envío inmediato de trabajo (sin delay)
    /// </summary>
    /// <param name="tipoTrabajo">Tipo de trabajo</param>
    /// <param name="parametros">Parámetros del trabajo</param>
    /// <returns>ID del trabajo</returns>
    Task<string> EjecutarTrabajoInmediatoAsync(string tipoTrabajo, object parametros);
    
    /// <summary>
    /// Programa trabajo recurrente con intervalo
    /// </summary>
    /// <param name="tipoTrabajo">Tipo de trabajo</param>
    /// <param name="parametros">Parámetros del trabajo</param>
    /// <param name="intervaloDias">Intervalo en días</param>
    /// <returns>ID del trabajo recurrente</returns>
    Task<string> ProgramarTrabajoRecurrenteAsync(string tipoTrabajo, object parametros, int intervaloDias);
} 