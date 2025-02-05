using System.Collections.Generic;
using System.Linq;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;
using RestaurantePro.Core.Specifications;
using RestaurantePro.Core.Specifications.Base;

namespace RestaurantePro.Core.Specifications
{
    public class MesasDisponiblesSpecification : BaseSpecification<Mesa>

    {
        public MesasDisponiblesSpecification()
            : base(x => x.Estado == EstadoMesa.Disponible)
        {
            AddOrderBy(x => x.Numero);
        }
    }
}