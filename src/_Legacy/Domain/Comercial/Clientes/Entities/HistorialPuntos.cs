using RestaurantePro.Domain.Common;
using RestaurantePro.Domain.Enums;
using System;

namespace RestaurantePro.Domain.Entities
{
    public class HistorialPuntos : BaseEntity
    {
        public int TarjetaFidelizacionId { get; set; }
        public virtual TarjetaFidelizacion TarjetaFidelizacion { get; set; }
        public int ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; }
        public TipoMovimientoPuntos TipoMovimiento { get; set; }
        public int Puntos { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public int? ComandaId { get; set; }
        public virtual Comanda Comanda { get; set; }
        public int? PromocionId { get; set; }
        public virtual Promocion Promocion { get; set; }
        public string UsuarioId { get; set; }
    }
} 