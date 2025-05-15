using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para transferencia de datos de mesa
    /// </summary>
    public class MesaDto
    {
        /// <summary>
        /// ID de la mesa
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número de la mesa
        /// </summary>
        public int Numero { get; set; }

        /// <summary>
        /// Capacidad de personas
        /// </summary>
        public int Capacidad { get; set; }

        /// <summary>
        /// Ubicación de la mesa
        /// </summary>
        public string Ubicacion { get; set; }

        /// <summary>
        /// Estado actual de la mesa
        /// </summary>
        public EstadoMesa Estado { get; set; }

        /// <summary>
        /// Nombre del estado para mostrar
        /// </summary>
        public string EstadoNombre => Estado.ToString();

        /// <summary>
        /// Posición X en el plano
        /// </summary>
        public int PosicionX { get; set; }

        /// <summary>
        /// Posición Y en el plano
        /// </summary>
        public int PosicionY { get; set; }

        /// <summary>
        /// Indica si la mesa está activa
        /// </summary>
        public bool Activa { get; set; }

        /// <summary>
        /// ID de la comanda activa (si existe)
        /// </summary>
        public int? ComandaActivaId { get; set; }
    }
} 