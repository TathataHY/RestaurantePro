using System;

namespace RestaurantePro.Application.Operaciones.Reportes.DTOs
{
    /// <summary>
    /// DTO con información de una mesa liberada
    /// </summary>
    public class MesaLiberadaDto
    {
        /// <summary>
        /// ID de la mesa
        /// </summary>
        public Guid MesaId { get; set; }
        
        /// <summary>
        /// Estado de la mesa después de liberarse
        /// </summary>
        public string EstadoMesa { get; set; } = string.Empty;
        
        /// <summary>
        /// Fecha y hora de liberación de la mesa
        /// </summary>
        public DateTime? FechaLiberacion { get; set; }
    }
} 