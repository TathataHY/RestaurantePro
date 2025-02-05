using System.Collections.Generic;
using System.Linq.Expressions;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Specifications.Base;

namespace RestaurantePro.Core.Specifications
{
    public class ComandaSpecification : BaseSpecification<Comanda>
    {
        public ComandaSpecification() : base()
        {
            AddInclude(x => x.Mesa);
            AddInclude(x => x.Mesero);
            AddInclude(x => x.Detalles);
            AddOrderByDescending(x => x.FechaHora);
        }

        public ComandaSpecification(int id) : base(x => x.Id == id)
        {
            AddInclude(x => x.Mesa);
            AddInclude(x => x.Mesero);
            AddInclude(x => x.Detalles);
        }
    }

    public class ComandaConDetallesSpecification : BaseSpecification<Comanda>
    {
        public ComandaConDetallesSpecification(int comandaId) 
            : base(x => x.Id == comandaId)
        {
            AddInclude(x => x.Detalles);
            AddInclude(x => x.Mesa);
            AddInclude(x => x.Mesero);
        }
    }
}