using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Reportes.Dtos
{
    public class ReporteReservacionesDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalReservaciones { get; set; }
        public int ReservacionesCompletadas { get; set; }
        public int ReservacionesCanceladas { get; set; }
        public int ReservacionesNoShow { get; set; }
        public decimal PorcentajeCompletadas { get; set; }
        public decimal PorcentajeCanceladas { get; set; }
        public decimal PorcentajeNoShow { get; set; }
        public List<ReservacionPorDiaDto> ReservacionesPorDia { get; set; } = new List<ReservacionPorDiaDto>();
        public List<ReservacionPorMesaDto> ReservacionesPorMesa { get; set; } = new List<ReservacionPorMesaDto>();
        public List<HoraPopularReservacionDto> HorasPopularesReservacion { get; set; } = new List<HoraPopularReservacionDto>();
    }

    public class ReservacionPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public int TotalReservaciones { get; set; }
        public int Completadas { get; set; }
        public int Canceladas { get; set; }
        public int NoShow { get; set; }
    }

    public class ReservacionPorMesaDto
    {
        public int MesaId { get; set; }
        public string NumeroMesa { get; set; }
        public int Capacidad { get; set; }
        public int TotalReservaciones { get; set; }
        public int Completadas { get; set; }
        public int Canceladas { get; set; }
        public int NoShow { get; set; }
    }

    public class HoraPopularReservacionDto
    {
        public int Hora { get; set; }
        public string RangoHoras { get; set; }
        public int TotalReservaciones { get; set; }
        public decimal Porcentaje { get; set; }
    }
} 