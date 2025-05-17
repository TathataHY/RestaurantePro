using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Clientes.Dtos
{
    public class TarjetaFidelizacionDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public int ClienteId { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public string Estado { get; set; }
        public int NivelId { get; set; }
        public string NombreNivel { get; set; }
        public int PuntosAcumulados { get; set; }
        public int PuntosDisponibles { get; set; }
        public int PuntosCanjeados { get; set; }
        public decimal DescuentoDisponible { get; set; }
        public List<MovimientoPuntosDto> UltimosMovimientos { get; set; } = new List<MovimientoPuntosDto>();
    }
    
    public class MovimientoPuntosDto
    {
        public int Id { get; set; }
        public string TipoMovimiento { get; set; }
        public int Puntos { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string Origen { get; set; }
    }
} 