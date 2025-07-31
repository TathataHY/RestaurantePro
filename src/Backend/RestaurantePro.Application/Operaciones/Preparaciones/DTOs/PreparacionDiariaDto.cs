using System;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.DTOs
{
    /// <summary>
    /// DTO para representar una preparación diaria
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
        /// Nombre del producto (se popula mediante joins)
        /// </summary>
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// ID del chef que realizó la preparación
        /// </summary>
        public Guid ChefId { get; set; }

        /// <summary>
        /// Nombre del chef (se popula mediante joins)
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
        public EstadoPreparacion Estado { get; set; }

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
        public bool EstaDisponible => Estado == EstadoPreparacion.Disponible && CantidadDisponible > 0 && !EstaVencida;

        /// <summary>
        /// Porcentaje de consumo de la preparación
        /// </summary>
        public decimal PorcentajeConsumo => CantidadPreparada > 0 ? (decimal)CantidadConsumida / CantidadPreparada * 100 : 0;
    }
} 