using SQLite;
using System;
using System.Collections.Generic;

namespace RestaurantePro.App.Models
{
    public class Comanda
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int NumeroMesa { get; set; }
        public DateTime FechaHora { get; set; }
        public int MesaId { get; set; } 
        public string MeseroId { get; set; }
        public decimal Total { get; set; }
        public EstadoComanda Estado { get; set; }
        public string Observaciones { get; set; }

        [Ignore]
        public List<ComandaDetalle> Detalles { get; set; } // Nueva propiedad para los detalles de la comanda
    }

    public enum EstadoComanda
    {
        Pendiente,
        EnPreparacion,
        Lista,
        Entregada,
        Cancelada
    }
}