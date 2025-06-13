namespace RestaurantePro.Domain.Core.Base.Interfaces
{
    /// <summary>
    /// Interfaz para entidades con propiedades de auditoría
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Fecha de creación de la entidad
        /// </summary>
        DateTime FechaCreacion { get; set; }
        
        /// <summary>
        /// Identificador del usuario que creó la entidad
        /// </summary>
        string? CreadoPor { get; set; }
        
        /// <summary>
        /// Fecha de la última modificación de la entidad
        /// </summary>
        DateTime? FechaModificacion { get; set; }
        
        /// <summary>
        /// Identificador del usuario que realizó la última modificación
        /// </summary>
        string? ModificadoPor { get; set; }
    }
} 