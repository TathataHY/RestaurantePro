using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Reportes.Dtos
{
    public class ReporteOcupacionDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalMesas { get; set; }
        public int TotalOcupaciones { get; set; }
        public decimal PromedioOcupacionDiaria { get; set; } // Porcentaje
        public TimeSpan TiempoPromedioOcupacion { get; set; }
        public List<OcupacionPorDiaDto> OcupacionPorDia { get; set; } = new List<OcupacionPorDiaDto>();
        public List<OcupacionPorMesaDto> OcupacionPorMesa { get; set; } = new List<OcupacionPorMesaDto>();
        public List<HoraPicoDto> HorasPico { get; set; } = new List<HoraPicoDto>();
    }

    public class OcupacionPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public int TotalOcupaciones { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
    }

    public class OcupacionPorMesaDto
    {
        public int MesaId { get; set; }
        public string NumeroMesa { get; set; }
        public int Capacidad { get; set; }
        public int VecesOcupada { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
        public TimeSpan TiempoPromedioOcupacion { get; set; }
    }

    public class HoraPicoDto
    {
        public int Hora { get; set; }
        public string RangoHoras { get; set; } // Ej: "13:00 - 14:00"
        public int TotalOcupaciones { get; set; }
        public decimal PorcentajeOcupacion { get; set; }
    }
} 