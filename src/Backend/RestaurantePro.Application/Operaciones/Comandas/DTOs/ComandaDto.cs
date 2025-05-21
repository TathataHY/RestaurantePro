using System;
using System.Collections.Generic;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.DTOs
{
    /// <summary>
    /// DTO para representar una Comanda en la capa de aplicación
    /// </summary>
    public class ComandaDto
    {
        /// <summary>
        /// Identificador único de la comanda
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// ID de la mesa asociada
        /// </summary>
        public Guid MesaId { get; set; }
        
        /// <summary>
        /// Número de la mesa (para mostrar en interfaces)
        /// </summary>
        public int? NumeroMesa { get; set; }
        
        /// <summary>
        /// ID del mesero responsable
        /// </summary>
        public Guid MeseroId { get; set; }
        
        /// <summary>
        /// Nombre del mesero (para mostrar en interfaces)
        /// </summary>
        public string? NombreMesero { get; set; }
        
        /// <summary>
        /// ID del cliente (opcional)
        /// </summary>
        public Guid? ClienteId { get; set; }
        
        /// <summary>
        /// Nombre del cliente (opcional, para mostrar en interfaces)
        /// </summary>
        public string? NombreCliente { get; set; }
        
        /// <summary>
        /// Fecha de creación de la comanda
        /// </summary>
        public DateTime FechaCreacion { get; set; }
        
        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? FechaActualizacion { get; set; }
        
        /// <summary>
        /// Estado actual de la comanda
        /// </summary>
        public EstadoComanda Estado { get; set; }
        
        /// <summary>
        /// Nombre del estado para mostrar en interfaces
        /// </summary>
        public string EstadoNombre => Estado.ToString();
        
        /// <summary>
        /// Observaciones de la comanda
        /// </summary>
        public string? Observaciones { get; set; }
        
        /// <summary>
        /// Subtotal (sin impuestos)
        /// </summary>
        public decimal Subtotal { get; set; }
        
        /// <summary>
        /// Total de impuestos
        /// </summary>
        public decimal Impuestos { get; set; }
        
        /// <summary>
        /// Descuento por fidelización (si aplica)
        /// </summary>
        public decimal? DescuentoFidelizacion { get; set; }
        
        /// <summary>
        /// Total a pagar (subtotal + impuestos - descuentos)
        /// </summary>
        public decimal Total { get; set; }
        
        /// <summary>
        /// Items de la comanda
        /// </summary>
        public List<ItemComandaDto> Items { get; set; } = new List<ItemComandaDto>();
        
        /// <summary>
        /// Duración total desde la creación hasta el momento actual o la finalización
        /// </summary>
        public TimeSpan Duracion => FechaActualizacion.HasValue 
            ? FechaActualizacion.Value - FechaCreacion 
            : DateTime.Now - FechaCreacion;
    }
} 