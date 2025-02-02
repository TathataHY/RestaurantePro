using System.Collections.Generic;
using RestaurantePro.Core.Entities;

namespace RestaurantePro.Core.Entities.Aggregates
{
    public class ComandaAggregate
    {
        public Comanda Comanda { get; private set; }

        public List<ComandaDetalle> Detalles { get; private set; }

        public ComandaAggregate(Comanda comanda, List<ComandaDetalle> detalles)
        {
            Comanda = comanda;
            Detalles = detalles;
        }

        public void AddDetalle(ComandaDetalle detalle)
        {
            Detalles.Add(detalle);
            Comanda.Total += detalle.Subtotal;
        }

        public void RemoveDetalle(ComandaDetalle detalle)
        {
            Detalles.Remove(detalle);
            Comanda.Total -= detalle.Subtotal;
        }
    }
}