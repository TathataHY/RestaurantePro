using AutoMapper;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.DTOs
{
    /// <summary>
    /// DTO para representar una preparación diaria
    /// </summary>
    public class PreparacionDto
    {
        /// <summary>
        /// ID de la preparación
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del producto preparado
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Nombre del producto (se popula mediante joins)
        /// </summary>
        public string? NombreProducto { get; set; }

        /// <summary>
        /// Cantidad total preparada
        /// </summary>
        public int CantidadPreparada { get; set; }

        /// <summary>
        /// Cantidad disponible actualmente
        /// </summary>
        public int CantidadDisponible { get; set; }

        /// <summary>
        /// Fecha y hora de preparación
        /// </summary>
        public DateTime FechaPreparacion { get; set; }

        /// <summary>
        /// Fecha y hora de vencimiento
        /// </summary>
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Estado actual de la preparación
        /// </summary>
        public EstadoPreparacion Estado { get; set; }

        /// <summary>
        /// Nombre del estado (para mostrar en UI)
        /// </summary>
        public string EstadoNombre => Estado.ToString();

        /// <summary>
        /// ID del chef que realizó la preparación
        /// </summary>
        public Guid ChefId { get; set; }

        /// <summary>
        /// Nombre del chef (se popula mediante joins)
        /// </summary>
        public string? NombreChef { get; set; }

        /// <summary>
        /// Observaciones sobre la preparación
        /// </summary>
        public string Observaciones { get; set; }

        /// <summary>
        /// Alias para Cantidad - usado en los tests
        /// </summary>
        public int Cantidad => CantidadPreparada;

        /// <summary>
        /// Alias para FechaPreparacion - usado en los tests
        /// </summary>
        public DateTime FechaCreacion => FechaPreparacion;

        /// <summary>
        /// Tiempo restante antes de vencer (en horas)
        /// </summary>
        public double? HorasParaVencer { get; set; }

        /// <summary>
        /// Porcentaje de producto consumido
        /// </summary>
        public int PorcentajeConsumido => CantidadPreparada > 0 
            ? (int)Math.Round(((double)(CantidadPreparada - CantidadDisponible) / CantidadPreparada) * 100) 
            : 0;

        /// <summary>
        /// Indica si la preparación está por vencer (menos de 2 horas)
        /// </summary>
        public bool EstaPorVencer => HorasParaVencer.HasValue && HorasParaVencer.Value <= 2 && HorasParaVencer.Value > 0;
    }
} 