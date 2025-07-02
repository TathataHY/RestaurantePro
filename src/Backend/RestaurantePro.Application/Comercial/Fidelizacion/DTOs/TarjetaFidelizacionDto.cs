namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs
{
    /// <summary>
    /// DTO para representar una tarjeta de fidelización
    /// </summary>
    public class TarjetaFidelizacionDto
    {
        /// <summary>
        /// ID único de la tarjeta
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Número único de la tarjeta
        /// </summary>
        public string NumeroTarjeta { get; set; } = string.Empty;

        /// <summary>
        /// ID del cliente propietario
        /// </summary>
        public Guid ClienteId { get; set; }

        /// <summary>
        /// Nombre del cliente
        /// </summary>
        public string NombreCliente { get; set; } = string.Empty;

        /// <summary>
        /// Nivel de fidelización de la tarjeta
        /// </summary>
        public NivelFidelizacion Nivel { get; set; }

        /// <summary>
        /// Nombre del nivel para visualización
        /// </summary>
        public string NivelTexto => Nivel.ToString();

        /// <summary>
        /// Puntos actuales acumulados
        /// </summary>
        public int PuntosActuales { get; set; }

        /// <summary>
        /// Total de puntos ganados históricamente
        /// </summary>
        public int TotalPuntosGanados { get; set; }

        /// <summary>
        /// Total de puntos canjeados
        /// </summary>
        public int TotalPuntosCanjeados { get; set; }

        /// <summary>
        /// Fecha de emisión de la tarjeta
        /// </summary>
        public DateTime FechaEmision { get; set; }

        /// <summary>
        /// Fecha de vencimiento
        /// </summary>
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>
        /// Estado de la tarjeta
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de última actividad
        /// </summary>
        public DateTime? FechaUltimaActividad { get; set; }

        /// <summary>
        /// Código QR de la tarjeta
        /// </summary>
        public string? CodigoQr { get; set; }

        /// <summary>
        /// Observaciones
        /// </summary>
        public string? Observaciones { get; set; }

        /// <summary>
        /// Indica si la tarjeta está activa
        /// </summary>
        public bool Activa { get; set; }

        /// <summary>
        /// Beneficios disponibles para el nivel actual
        /// </summary>
        public List<BeneficioDto> BeneficiosDisponibles { get; set; } = new();

        /// <summary>
        /// Historial reciente de transacciones
        /// </summary>
        public List<TransaccionPuntosDto> TransaccionesRecientes { get; set; } = new();

        /// <summary>
        /// Versión de la entidad para control de concurrencia - comentada para tests con SQLite
        /// </summary>
        // public string? RowVersion { get; set; }
    }

    /// <summary>
    /// DTO para representar un beneficio
    /// </summary>
    public class BeneficioDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int PuntosRequeridos { get; set; }
        public string TipoBeneficio { get; set; } = string.Empty;
        public decimal? ValorDescuento { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaExpiracion { get; set; }
    }
} 