using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Promociones.Dtos
{
    public class PromocionDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public decimal ValorDescuento { get; set; }
        public decimal MontoMinimo { get; set; }
        public int PuntosRequeridos { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int? MaximoUsos { get; set; }
        public int VecesUsada { get; set; }
        public bool Activa { get; set; }
        public List<string> CategoriasAplicables { get; set; } = new List<string>();
        public List<string> ProductosAplicables { get; set; } = new List<string>();
    }
} 