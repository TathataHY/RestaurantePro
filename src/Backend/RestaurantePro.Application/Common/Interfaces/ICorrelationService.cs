namespace RestaurantePro.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de correlación, que permite rastrear operaciones relacionadas
    /// </summary>
    public interface ICorrelationService
    {
        /// <summary>
        /// Identificador único de correlación que permite rastrear un flujo completo entre servicios
        /// </summary>
        string CorrelationId { get; }
        
        /// <summary>
        /// Identificador único de la solicitud HTTP actual
        /// </summary>
        string RequestId { get; }
        
        /// <summary>
        /// Identificador de la sesión del usuario
        /// </summary>
        string SessionId { get; }
        
        /// <summary>
        /// Establece el ID de correlación actual
        /// </summary>
        /// <param name="correlationId">ID de correlación a establecer</param>
        void SetCorrelationId(string correlationId);
        
        /// <summary>
        /// Establece el ID de solicitud actual
        /// </summary>
        /// <param name="requestId">ID de solicitud a establecer</param>
        void SetRequestId(string requestId);
        
        /// <summary>
        /// Establece el ID de sesión actual
        /// </summary>
        /// <param name="sessionId">ID de sesión a establecer</param>
        void SetSessionId(string sessionId);
    }
} 