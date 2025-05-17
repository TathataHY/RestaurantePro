using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Comandas.Dtos
{
    public class ComandaDto
    {
        public int Id { get; set; }
        public int MesaId { get; set; }
        public string NumeroMesa { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public List<ComandaDetalleDto> Detalles { get; set; } = new List<ComandaDetalleDto>();
    }
} 