using RestaurantePro.Domain.Common;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    public class NivelFidelizacion : BaseEntity
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Orden { get; set; }
        public int PuntosRequeridos { get; set; }
        public int MultiplicadorPuntos { get; set; }
        public decimal DescuentoBase { get; set; }
        public bool PermiteAccesoPromociones { get; set; }
        public bool PermiteReservaPreferencial { get; set; }
        public bool Activo { get; set; }
        
        // Relaciones
        public virtual ICollection<TarjetaFidelizacion> Tarjetas { get; set; } = new List<TarjetaFidelizacion>();
        public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
} 