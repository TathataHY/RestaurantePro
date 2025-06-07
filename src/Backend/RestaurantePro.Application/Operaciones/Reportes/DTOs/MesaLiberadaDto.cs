using System;

namespace RestaurantePro.Application.Operaciones.Reportes.DTOs
{
    public class MesaLiberadaDto
    {
        public Guid MesaId { get; set; }
        public DateTime FechaLiberacion { get; set; }
        public string EstadoMesa { get; set; }
    }
} 