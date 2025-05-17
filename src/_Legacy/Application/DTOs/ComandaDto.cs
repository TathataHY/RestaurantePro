using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para transferencia de datos de comanda
    /// </summary>
    public class ComandaDto
    {
        /// <summary>
        /// ID de la comanda
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la mesa
        /// </summary>
        public int MesaId { get; set; }

        /// <summary>
        /// Número de la mesa
        /// </summary>
        public int NumeroMesa { get; set; }

        /// <summary>
        /// ID del usuario que creó la comanda
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Nombre del usuario que creó la comanda
        /// </summary>
        public string NombreUsuario { get; set; }

        /// <summary>
        /// Número de comanda
        /// </summary>
        public string NumeroComanda { get; set; }

        /// <summary>
        /// Fecha de creación de la comanda
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Subtotal de la comanda
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// IVA de la comanda
        /// </summary>
        public decimal Iva { get; set; }

        /// <summary>
        /// Total de la comanda
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Estado de la comanda
        /// </summary>
        public EstadoComanda Estado { get; set; }

        /// <summary>
        /// Nombre del estado para mostrar
        /// </summary>
        public string EstadoNombre => Estado.ToString();

        /// <summary>
        /// Notas adicionales
        /// </summary>
        public string Notas { get; set; }

        /// <summary>
        /// Cantidad de productos en la comanda
        /// </summary>
        public int CantidadProductos { get; set; }

        /// <summary>
        /// Tiempo estimado de preparación en minutos
        /// </summary>
        public int TiempoEstimadoPreparacion { get; set; }

        /// <summary>
        /// Lista de detalles de la comanda
        /// </summary>
        public List<ComandaDetalleDto> Detalles { get; set; } = new List<ComandaDetalleDto>();

        /// <summary>
        /// Lista de pagos asociados a la comanda
        /// </summary>
        public List<PagoDto> Pagos { get; set; } = new List<PagoDto>();
    }
} 