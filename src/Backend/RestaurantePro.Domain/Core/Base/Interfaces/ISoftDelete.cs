namespace RestaurantePro.Domain.Core.Base.Interfaces
{
    /// <summary>
    /// Interfaz para entidades con borrado lógico
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Indica si la entidad está activa (no eliminada)
        /// </summary>
        bool Activo { get; set; }
        
        /// <summary>
        /// Fecha de eliminación de la entidad
        /// </summary>
        DateTime? FechaEliminacion { get; set; }
        
        /// <summary>
        /// Identificador del usuario que eliminó la entidad
        /// </summary>
        string? EliminadoPor { get; set; }
    }
} 