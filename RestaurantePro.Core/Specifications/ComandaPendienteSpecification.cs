using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Specifications.Base;

namespace RestaurantePro.Core.Specifications
{
    public class ComandaPendienteSpecification : BaseSpecification<Comanda>
    {
        public ComandaPendienteSpecification() 
            : base(c => c.Estado == EstadoComanda.Pendiente)
        {
            AddInclude(x => x.Mesa);
            AddInclude(x => x.Mesero);
            AddInclude(x => x.Detalles);
            AddOrderByDescending(x => x.FechaHora);
        }
    }
} 