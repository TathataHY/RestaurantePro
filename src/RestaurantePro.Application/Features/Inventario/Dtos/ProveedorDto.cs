using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Inventario.Dtos
{
    public class ProveedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string NombreContacto { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Pais { get; set; }
        public int DiasCredito { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimaOrden { get; set; }
        public List<string> Categorias { get; set; } = new List<string>();
        public int TotalOrdenes { get; set; }
    }
} 