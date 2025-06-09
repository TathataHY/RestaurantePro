namespace RestaurantePro.Application.Operaciones.Preparaciones.DTOs
{
    /// <summary>
    /// DTO para estadísticas de preparaciones
    /// </summary>
    public class EstadisticasPreparacionesDto
    {
        /// <summary>
        /// Número total de preparaciones del día
        /// </summary>
        public int TotalPreparaciones { get; set; }
        
        /// <summary>
        /// Número de preparaciones disponibles
        /// </summary>
        public int PreparacionesDisponibles { get; set; }
        
        /// <summary>
        /// Número de preparaciones por vencer
        /// </summary>
        public int PreparacionesPorVencer { get; set; }
        
        /// <summary>
        /// Número de preparaciones agotadas
        /// </summary>
        public int PreparacionesAgotadas { get; set; }
        
        /// <summary>
        /// Número de preparaciones vencidas
        /// </summary>
        public int PreparacionesVencidas { get; set; }
        
        /// <summary>
        /// Cantidad total de productos preparados
        /// </summary>
        public int CantidadTotalPreparada { get; set; }
        
        /// <summary>
        /// Cantidad actual disponible
        /// </summary>
        public int CantidadDisponible { get; set; }
        
        /// <summary>
        /// Cantidad consumida (vendida)
        /// </summary>
        public int CantidadConsumida { get; set; }
        
        /// <summary>
        /// Cantidad desperdiciada (vencida)
        /// </summary>
        public int CantidadDesperdiciada { get; set; }
        
        /// <summary>
        /// Porcentaje de eficiencia (consumido vs preparado)
        /// </summary>
        public decimal PorcentajeEficiencia { get; set; }
    }
} 