using System;

namespace RestaurantePro.Mobile.Core.Models.DTOs
{
    /// <summary>
    /// DTO para estadísticas de preparaciones diarias en mobile
    /// </summary>
    public class EstadisticasPreparacionesDiariasDto
    {
        /// <summary>
        /// Número total de preparaciones diarias
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
        /// Número de preparaciones en preparación
        /// </summary>
        public int PreparacionesEnPreparacion { get; set; }
        
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
        
        /// <summary>
        /// Fecha de generación del reporte
        /// </summary>
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Resumen de estados
        /// </summary>
        public string ResumenEstados => $"Total: {TotalPreparaciones}, Disponibles: {PreparacionesDisponibles}, Por Vencer: {PreparacionesPorVencer}, Agotadas: {PreparacionesAgotadas}, Vencidas: {PreparacionesVencidas}";
        
        /// <summary>
        /// Resumen de cantidades
        /// </summary>
        public string ResumenCantidades => $"Preparado: {CantidadTotalPreparada}, Disponible: {CantidadDisponible}, Consumido: {CantidadConsumida}, Desperdiciado: {CantidadDesperdiciada}";
        
        /// <summary>
        /// Eficiencia formateada
        /// </summary>
        public string EficienciaFormateada => $"{PorcentajeEficiencia:F1}%";

        /// <summary>
        /// Color de la eficiencia para la UI
        /// </summary>
        public string ColorEficiencia => PorcentajeEficiencia switch
        {
            >= 80 => "#4CAF50", // Verde - Excelente
            >= 60 => "#FF9800", // Naranja - Buena
            >= 40 => "#FFC107", // Amarillo - Regular
            _ => "#F44336"       // Rojo - Mala
        };

        /// <summary>
        /// Icono de la eficiencia para la UI
        /// </summary>
        public string IconoEficiencia => PorcentajeEficiencia switch
        {
            >= 80 => "🏆",
            >= 60 => "👍",
            >= 40 => "⚠️",
            _ => "❌"
        };

        /// <summary>
        /// Texto descriptivo de la eficiencia
        /// </summary>
        public string TextoEficiencia => PorcentajeEficiencia switch
        {
            >= 80 => "Excelente",
            >= 60 => "Buena",
            >= 40 => "Regular",
            _ => "Necesita Mejora"
        };
    }
} 