using AutoMapper;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.DTOs
{
    /// <summary>
    /// DTO para representar una preparación de cocina
    /// </summary>
    public class PreparacionDto
    {
        /// <summary>
        /// ID de la preparación
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID de la comanda
        /// </summary>
        public Guid ComandaId { get; set; }

        /// <summary>
        /// Número de la comanda
        /// </summary>
        public string NumeroComanda { get; set; } = string.Empty;

        /// <summary>
        /// ID del producto preparado
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Nombre del producto (se popula mediante joins)
        /// </summary>
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad total preparada
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Estado actual de la preparación
        /// </summary>
        public EstadoPreparacion Estado { get; set; }

        /// <summary>
        /// Fecha y hora de preparación
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha y hora de inicio de la preparación
        /// </summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Fecha y hora de finalización de la preparación
        /// </summary>
        public DateTime? FechaCompletado { get; set; }

        /// <summary>
        /// Fecha y hora de cancelación de la preparación
        /// </summary>
        public DateTime? FechaCancelacion { get; set; }

        /// <summary>
        /// ID del chef que realizó la preparación
        /// </summary>
        public Guid? ChefId { get; set; }

        /// <summary>
        /// Nombre del chef (se popula mediante joins)
        /// </summary>
        public string? NombreChef { get; set; }

        /// <summary>
        /// Observaciones sobre la preparación
        /// </summary>
        public string? Observaciones { get; set; }

        /// <summary>
        /// Prioridad de la preparación
        /// </summary>
        public int Prioridad { get; set; }

        /// <summary>
        /// Tiempo estimado para la preparación
        /// </summary>
        public TimeSpan? TiempoEstimado { get; set; }

        /// <summary>
        /// Tiempo real de la preparación
        /// </summary>
        public TimeSpan? TiempoReal { get; set; }

        /// <summary>
        /// Motivo de cancelación de la preparación
        /// </summary>
        public string? MotivoCancelacion { get; set; }
        
        /// <summary>
        /// Fecha de vencimiento de la preparación
        /// </summary>
        public DateTime? FechaVencimiento { get; set; }
        
        /// <summary>
        /// Horas restantes para que venza la preparación
        /// </summary>
        public double HorasParaVencer { get; set; }
    }

    /// <summary>
    /// Estados posibles de una preparación
    /// </summary>
    public enum PreparacionEstado
    {
        Pendiente = 1,
        EnPreparacion = 2,
        Completada = 3,
        Cancelada = 4,
        Entregada = 5
    }
} 