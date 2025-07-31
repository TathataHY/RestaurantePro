using System;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.DTOs
{
    /// <summary>
    /// DTO para estadísticas de preparaciones diarias
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
    }
} 