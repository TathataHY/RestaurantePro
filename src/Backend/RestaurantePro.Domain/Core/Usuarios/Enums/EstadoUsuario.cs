namespace RestaurantePro.Domain.Core.Usuarios.Enums
{
    /// <summary>
    /// Estados posibles para un usuario en el sistema
    /// </summary>
    public enum EstadoUsuario
    {
        /// <summary>
        /// Usuario activo que puede iniciar sesión
        /// </summary>
        Activo = 1,
        
        /// <summary>
        /// Usuario inactivo que no puede iniciar sesión
        /// </summary>
        Inactivo = 2,
        
        /// <summary>
        /// Usuario bloqueado por infracciones o políticas de seguridad
        /// </summary>
        Bloqueado = 3,
        
        /// <summary>
        /// Usuario pendiente de confirmación de cuenta
        /// </summary>
        PendienteConfirmacion = 4,
        
        /// <summary>
        /// Usuario suspendido temporalmente
        /// </summary>
        Suspendido = 5
    }
} 