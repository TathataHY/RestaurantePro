using System;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

namespace RestaurantePro.Application.Operaciones.Reservaciones.DTOs
{
    /// <summary>
    /// DTO para representar una Mesa en la capa de aplicación
    /// </summary>
    public class MesaDto
    {
        /// <summary>
        /// Identificador único de la mesa
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Número de mesa (visible para usuarios)
        /// </summary>
        public int Numero { get; set; }
        
        /// <summary>
        /// Capacidad máxima de comensales
        /// </summary>
        public int Capacidad { get; set; }
        
        /// <summary>
        /// Ubicación de la mesa (sección del restaurante)
        /// </summary>
        public string Ubicacion { get; set; } = string.Empty;
        
        /// <summary>
        /// Estado actual de la mesa
        /// </summary>
        public EstadoMesa Estado { get; set; }
        
        /// <summary>
        /// Nombre del estado para mostrar en interfaces
        /// </summary>
        public string EstadoNombre => Estado.ToString();
        
        /// <summary>
        /// Descripción adicional de la mesa
        /// </summary>
        public string? Descripcion { get; set; }
        
        /// <summary>
        /// Indica si la mesa está reservada actualmente
        /// </summary>
        public bool EstaReservada { get; set; }
        
        /// <summary>
        /// ID de la comanda activa en esta mesa (si existe)
        /// </summary>
        public Guid? ComandaActivaId { get; set; }
        
        /// <summary>
        /// Fecha de la última reservación en esta mesa
        /// </summary>
        public DateTime? UltimaReservacion { get; set; }
    }
} 