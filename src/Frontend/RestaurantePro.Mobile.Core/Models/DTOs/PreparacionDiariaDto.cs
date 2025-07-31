using System;

namespace RestaurantePro.Mobile.Core.Models.DTOs
{
    /// <summary>
    /// DTO para representar una preparación diaria en mobile
    /// </summary>
    public class PreparacionDiariaDto
    {
        /// <summary>
        /// ID de la preparación diaria
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del producto que se preparó
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// ID del chef que realizó la preparación
        /// </summary>
        public Guid ChefId { get; set; }

        /// <summary>
        /// Nombre del chef
        /// </summary>
        public string NombreChef { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad inicial que se preparó
        /// </summary>
        public int CantidadPreparada { get; set; }

        /// <summary>
        /// Cantidad disponible actualmente
        /// </summary>
        public int CantidadDisponible { get; set; }

        /// <summary>
        /// Cantidad consumida (vendida)
        /// </summary>
        public int CantidadConsumida => CantidadPreparada - CantidadDisponible;

        /// <summary>
        /// Fecha de vencimiento de la preparación
        /// </summary>
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Observaciones adicionales sobre la preparación
        /// </summary>
        public string Observaciones { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora en que se realizó la preparación
        /// </summary>
        public DateTime FechaPreparacion { get; set; }

        /// <summary>
        /// Estado actual de la preparación
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Horas restantes para que venza la preparación
        /// </summary>
        public double HorasParaVencer => (FechaVencimiento - DateTime.Now).TotalHours;

        /// <summary>
        /// Indica si la preparación está por vencer (menos de 2 horas)
        /// </summary>
        public bool EstaPorVencer => HorasParaVencer > 0 && HorasParaVencer <= 2;

        /// <summary>
        /// Indica si la preparación está vencida
        /// </summary>
        public bool EstaVencida => HorasParaVencer <= 0;

        /// <summary>
        /// Indica si la preparación está disponible para consumo
        /// </summary>
        public bool EstaDisponible => Estado == "Disponible" && CantidadDisponible > 0 && !EstaVencida;

        /// <summary>
        /// Porcentaje de consumo de la preparación
        /// </summary>
        public decimal PorcentajeConsumo => CantidadPreparada > 0 ? (decimal)CantidadConsumida / CantidadPreparada * 100 : 0;

        /// <summary>
        /// Color del estado para la UI
        /// </summary>
        public string ColorEstado => Estado switch
        {
            "Preparando" => "#FFA500", // Naranja
            "Disponible" => "#4CAF50", // Verde
            "PorVencer" => "#FF9800",  // Naranja oscuro
            "Agotada" => "#F44336",    // Rojo
            "Vencida" => "#9E9E9E",    // Gris
            _ => "#757575"             // Gris por defecto
        };

        /// <summary>
        /// Icono del estado para la UI
        /// </summary>
        public string IconoEstado => Estado switch
        {
            "Preparando" => "⏳",
            "Disponible" => "✅",
            "PorVencer" => "⚠️",
            "Agotada" => "❌",
            "Vencida" => "⏰",
            _ => "❓"
        };

        /// <summary>
        /// Texto descriptivo del estado
        /// </summary>
        public string TextoEstado => Estado switch
        {
            "Preparando" => "En Preparación",
            "Disponible" => "Disponible",
            "PorVencer" => "Por Vencer",
            "Agotada" => "Agotada",
            "Vencida" => "Vencida",
            _ => "Desconocido"
        };
    }
} 