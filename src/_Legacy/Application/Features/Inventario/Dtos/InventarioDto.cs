using System;

namespace RestaurantePro.Application.Features.Inventario.Dtos
{
    public class InventarioDto
    {
        public int Id { get; set; }
        public int IngredienteId { get; set; }
        public string NombreIngrediente { get; set; }
        public string Categoria { get; set; }
        public decimal CantidadDisponible { get; set; }
        public decimal CantidadMinima { get; set; }
        public decimal CantidadOptima { get; set; }
        public string UnidadMedida { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal ValorTotal => CantidadDisponible * CostoUnitario;
        public string Ubicacion { get; set; }
        public string Estado { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public string UltimoInventarioPor { get; set; }
        public string Observaciones { get; set; }
        public bool RequiereReposicion => CantidadDisponible <= CantidadMinima;
        public decimal PorcentajeDisponible => CantidadOptima > 0 
            ? Math.Round((CantidadDisponible / CantidadOptima) * 100, 2) 
            : 0;
    }
} 