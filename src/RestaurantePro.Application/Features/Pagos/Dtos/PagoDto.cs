using System;

namespace RestaurantePro.Application.Features.Pagos.Dtos
{
    public class PagoDto
    {
        public int Id { get; set; }
        public int ComandaId { get; set; }
        public string NumeroComanda { get; set; }
        public decimal Monto { get; set; }
        public string FormaPago { get; set; }
        public string Estado { get; set; }
        public string ReferenciaPago { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
} 