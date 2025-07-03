using System;
using System.Collections.Generic;

namespace RestaurantePro.Infrastructure.DTOs.SignalR
{
    /// <summary>
    /// DTO optimizado para envío por SignalR de comandas
    /// Contiene solo los datos necesarios para la comunicación en tiempo real
    /// </summary>
    public class ComandaSignalRDto
    {
        /// <summary>
        /// ID único de la comanda
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Número visible de la comanda (ej: "COM-001")
        /// </summary>
        public string NumeroComanda { get; set; } = string.Empty;

        /// <summary>
        /// ID de la mesa asociada
        /// </summary>
        public Guid MesaId { get; set; }

        /// <summary>
        /// Número de la mesa (ej: "Mesa 5")
        /// </summary>
        public string NumeroMesa { get; set; } = string.Empty;

        /// <summary>
        /// Estado actual de la comanda (Creada, EnProceso, Lista, Entregada, Cancelada)
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Lista de items de la comanda
        /// </summary>
        public List<ItemComandaSignalRDto> Items { get; set; } = new();

        /// <summary>
        /// Fecha y hora de creación de la comanda
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Tiempo estimado de entrega (opcional)
        /// </summary>
        public DateTime? FechaEstimadaEntrega { get; set; }

        /// <summary>
        /// Observaciones especiales de la comanda
        /// </summary>
        public string? Observaciones { get; set; }

        /// <summary>
        /// Nombre del mesero responsable
        /// </summary>
        public string NombreMesero { get; set; } = string.Empty;

        /// <summary>
        /// Prioridad de la comanda (Normal, Alta, Urgente)
        /// </summary>
        public string Prioridad { get; set; } = "Normal";

        /// <summary>
        /// ID del mesero responsable
        /// </summary>
        public Guid MeseroId { get; set; }

        /// <summary>
        /// ID del cliente (si aplica)
        /// </summary>
        public Guid? ClienteId { get; set; }

        /// <summary>
        /// Total de la comanda
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Constructor por defecto requerido para serialización
        /// </summary>
        public ComandaSignalRDto() { }

        /// <summary>
        /// Constructor con parámetros principales
        /// </summary>
        public ComandaSignalRDto(
            Guid id,
            string numeroComanda,
            Guid mesaId,
            string numeroMesa,
            string estado,
            DateTime fechaCreacion,
            string nombreMesero,
            Guid meseroId)
        {
            Id = id;
            NumeroComanda = numeroComanda;
            MesaId = mesaId;
            NumeroMesa = numeroMesa;
            Estado = estado;
            FechaCreacion = fechaCreacion;
            NombreMesero = nombreMesero;
            MeseroId = meseroId;
            Items = new List<ItemComandaSignalRDto>();
        }
    }

    /// <summary>
    /// DTO para items de comanda optimizado para SignalR
    /// </summary>
    public class ItemComandaSignalRDto
    {
        /// <summary>
        /// ID del item
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del producto
        /// </summary>
        public Guid ProductoId { get; set; }

        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad solicitada
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario
        /// </summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Subtotal del item
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Observaciones específicas del item
        /// </summary>
        public string? Observaciones { get; set; }

        /// <summary>
        /// Estado del item (Pendiente, EnPreparacion, Listo, Entregado)
        /// </summary>
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public ItemComandaSignalRDto() { }

        /// <summary>
        /// Constructor con parámetros principales
        /// </summary>
        public ItemComandaSignalRDto(
            Guid id,
            Guid productoId,
            string nombreProducto,
            int cantidad,
            decimal precioUnitario,
            string? observaciones = null)
        {
            Id = id;
            ProductoId = productoId;
            NombreProducto = nombreProducto;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Subtotal = cantidad * precioUnitario;
            Observaciones = observaciones;
        }
    }
} 