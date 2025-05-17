using System;

namespace RestaurantePro.Application.Features.Mesas.Dtos
{
    public class MesaDto
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public int Capacidad { get; set; }
        public string Ubicacion { get; set; }
        public bool Activa { get; set; }
        public string Estado { get; set; }
        public string QrCode { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimaModificacion { get; set; }
    }
} 