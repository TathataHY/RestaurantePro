using System;

namespace RestaurantePro.Application.Features.Clientes.Dtos
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public int Edad => CalcularEdad(FechaNacimiento);
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimaVisita { get; set; }
        public int TotalVisitas { get; set; }
        public decimal TotalGastado { get; set; }
        public int PuntosAcumulados { get; set; }
        public int PuntosDisponibles { get; set; }
        public int PuntosRedimidos { get; set; }
        public string NivelFidelizacion { get; set; }
        public string NumeroTarjeta { get; set; }
        public string EstadoTarjeta { get; set; }
        public bool Activo { get; set; }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;
            
            // Ajustar si aún no se ha cumplido el año de nacimiento en este año
            if (fechaNacimiento.Date > hoy.AddYears(-edad))
                edad--;
                
            return edad;
        }
    }
} 