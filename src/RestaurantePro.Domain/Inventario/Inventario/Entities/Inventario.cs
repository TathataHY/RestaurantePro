using RestaurantePro.Domain.Common;
using RestaurantePro.Domain.Enums;
using System;

namespace RestaurantePro.Domain.Entities
{
    public class Inventario : BaseEntity
    {
        public int IngredienteId { get; set; }
        public virtual Ingrediente Ingrediente { get; set; }
        public decimal CantidadDisponible { get; set; }
        public decimal CantidadMinima { get; set; }
        public decimal CantidadOptima { get; set; }
        public string UnidadMedida { get; set; }
        public decimal CostoUnitario { get; set; }
        public string Ubicacion { get; set; }
        public EstadoInventario Estado { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public string UltimoInventarioPor { get; set; }
        public string Observaciones { get; set; }
    }
} 