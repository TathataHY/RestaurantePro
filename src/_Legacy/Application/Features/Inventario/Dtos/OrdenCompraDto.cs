using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Inventario.Dtos
{
    public class OrdenCompraDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; }
        public int ProveedorId { get; set; }
        public string NombreProveedor { get; set; }
        public DateTime FechaOrden { get; set; }
        public DateTime? FechaEntregaEstimada { get; set; }
        public DateTime? FechaRecepcion { get; set; }
        public string Estado { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Observaciones { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public List<DetalleOrdenCompraDto> Detalles { get; set; } = new List<DetalleOrdenCompraDto>();
    }

    public class DetalleOrdenCompraDto
    {
        public int Id { get; set; }
        public int OrdenCompraId { get; set; }
        public int IngredienteId { get; set; }
        public string NombreIngrediente { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public decimal CantidadRecibida { get; set; }
        public bool Completado => CantidadRecibida >= Cantidad;
        public string Observaciones { get; set; }
    }
} 