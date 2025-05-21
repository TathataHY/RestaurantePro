using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Proveedores.DTOs
{
    /// <summary>
    /// Estadísticas globales de proveedores
    /// </summary>
    public class EstadisticasProveedores
    {
        /// <summary>
        /// Cantidad total de proveedores registrados
        /// </summary>
        public int TotalProveedores { get; set; }

        /// <summary>
        /// Cantidad de proveedores activos
        /// </summary>
        public int ProveedoresActivos { get; set; }

        /// <summary>
        /// Cantidad de órdenes realizadas en el periodo
        /// </summary>
        public int OrdenesEnPeriodo { get; set; }

        /// <summary>
        /// Valor total de las órdenes realizadas en el periodo
        /// </summary>
        public decimal ValorTotalOrdenesEnPeriodo { get; set; }

        /// <summary>
        /// Tiempo promedio de entrega en días
        /// </summary>
        public decimal TiempoPromedioEntrega { get; set; }

        /// <summary>
        /// Top 5 proveedores por volumen de compra
        /// </summary>
        public List<ProveedorEstadistica> TopProveedoresPorVolumen { get; set; } = new List<ProveedorEstadistica>();

        /// <summary>
        /// Proveedores con órdenes pendientes
        /// </summary>
        public int ProveedoresConOrdenesPendientes { get; set; }

        /// <summary>
        /// Fecha de último cálculo de las estadísticas
        /// </summary>
        public DateTime FechaCalculo { get; set; }
    }

    /// <summary>
    /// Estadísticas individuales de un proveedor
    /// </summary>
    public class ProveedorEstadistica
    {
        /// <summary>
        /// ID del proveedor
        /// </summary>
        public Guid ProveedorId { get; set; }

        /// <summary>
        /// Nombre del proveedor
        /// </summary>
        public string NombreProveedor { get; set; }

        /// <summary>
        /// Total de órdenes realizadas al proveedor
        /// </summary>
        public int TotalOrdenes { get; set; }

        /// <summary>
        /// Valor total de las órdenes realizadas al proveedor
        /// </summary>
        public decimal ValorTotalOrdenes { get; set; }

        /// <summary>
        /// Tiempo promedio de entrega del proveedor en días
        /// </summary>
        public decimal TiempoPromedioEntrega { get; set; }
    }
} 