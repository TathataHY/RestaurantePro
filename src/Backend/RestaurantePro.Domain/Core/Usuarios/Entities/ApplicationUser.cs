namespace RestaurantePro.Domain.Core.Usuarios.Entities
{
    /// <summary>
    /// Representa una entidad de usuario de Identity con propiedades extendidas.
    /// Esta entidad es parte de la infraestructura pero se define en el dominio
    /// para mantener la coherencia y facilitar referencias.
    /// 
    /// La implementación real se realizará en la capa de Infraestructura/Identity,
    /// heredando de IdentityUser y añadiendo estas propiedades.
    /// </summary>
    public class ApplicationUser
    {
        /// <summary>
        /// Identificador del usuario en el sistema de Identity
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Nombre de usuario para login
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Email del usuario
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string NombreCompleto { get; set; }
        
        /// <summary>
        /// Referencia al Usuario de dominio
        /// </summary>
        public Guid UsuarioId { get; set; }
        
        /// <summary>
        /// Fecha de último acceso al sistema
        /// </summary>
        public DateTime? UltimoAcceso { get; set; }
        
        /// <summary>
        /// Indica si el usuario está bloqueado en el sistema
        /// </summary>
        public bool Bloqueado { get; set; }
        
        /// <summary>
        /// Motivo de bloqueo si el usuario está bloqueado
        /// </summary>
        public string? MotivoBloqueo { get; set; }
    }
} 