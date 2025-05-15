using System;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Clase base para todas las entidades del dominio
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Identificador único de la entidad
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última actualización del registro
        /// </summary>
        public DateTime? FechaActualizacion { get; set; }

        /// <summary>
        /// Indica si el registro ha sido eliminado lógicamente
        /// </summary>
        public bool EstaEliminado { get; set; }
    }
} 