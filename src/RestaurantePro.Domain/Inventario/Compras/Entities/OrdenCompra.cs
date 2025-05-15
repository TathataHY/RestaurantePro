using RestaurantePro.Domain.Common;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    public class OrdenCompra : BaseEntity
    {
        public string NumeroOrden { get; set; }
        public int ProveedorId { get; set; }
        public virtual Proveedor Proveedor { get; set; }
        public DateTime FechaOrden { get; set; }
        public DateTime? FechaEntregaEstimada { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public EstadoOrdenCompra Estado { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Observaciones { get; set; }
        public string UsuarioId { get; set; }
        
        public virtual ICollection<DetalleOrdenCompra> DetallesOrden { get; set; } = new List<DetalleOrdenCompra>();
        public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();
    }
} 