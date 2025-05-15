using MediatR;
using System;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.CrearReservacion
{
    public class CrearReservacionCommand : IRequest<int>
    {
        public int MesaId { get; set; }
        public string NombreCliente { get; set; }
        public string TelefonoCliente { get; set; }
        public string EmailCliente { get; set; }
        public int CantidadPersonas { get; set; }
        public DateTime FechaReservacion { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public int DuracionMinutos { get; set; } = 90; // Por defecto 1.5 horas
        public string Notas { get; set; }
    }
} 