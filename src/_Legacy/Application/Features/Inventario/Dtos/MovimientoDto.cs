using System;

namespace RestaurantePro.Application.Features.Inventario.Dtos
{
    public class MovimientoDto
    {
        public int Id { get; set; }
        public string Ingrediente { get; set; }
        public string TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; }
        public DateTime Fecha { get; set; }
        public string Referencia { get; set; }
        public string Usuario { get; set; }
    }
} 