using RestaurantePro.Domain.Common;
using RestaurantePro.Domain.Enums;
using System;

namespace RestaurantePro.Domain.Entities
{
    public class MovimientoInventario : BaseEntity
    {
        public int InventarioId { get; set; }
        public virtual Inventario Inventario { get; set; }
        public int IngredienteId { get; set; }
        public virtual Ingrediente Ingrediente { get; set; }
        public TipoMovimientoInventario TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CantidadAnterior { get; set; }
        public decimal CantidadNueva { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotal { get; set; }
        public string Descripcion { get; set; }
        public string Referencia { get; set; }
        public DateTime Fecha { get; set; }
        public int? ComandaId { get; set; }
        public virtual Comanda Comanda { get; set; }
        public int? OrdenCompraId { get; set; }
        public virtual OrdenCompra OrdenCompra { get; set; }
        public string UsuarioId { get; set; }
    }
} 