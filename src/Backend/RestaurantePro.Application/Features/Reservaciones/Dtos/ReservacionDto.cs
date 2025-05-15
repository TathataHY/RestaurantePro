using System;

namespace RestaurantePro.Application.Features.Reservaciones.Dtos
{
    public class ReservacionDto
    {
        public int Id { get; set; }
        public int MesaId { get; set; }
        public string NumeroMesa { get; set; }
        public string NombreCliente { get; set; }
        public string TelefonoCliente { get; set; }
        public string EmailCliente { get; set; }
        public int CantidadPersonas { get; set; }
        public DateTime FechaReservacion { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Estado { get; set; }
        public string Notas { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimaModificacion { get; set; }
    }
} 