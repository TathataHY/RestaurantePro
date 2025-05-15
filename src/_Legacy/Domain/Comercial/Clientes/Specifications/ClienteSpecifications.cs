using System;
using System.Linq.Expressions;
using RestaurantePro.Domain.Comercial.Clientes.Aggregates;

namespace RestaurantePro.Domain.Comercial.Clientes.Specifications
{
    /// <summary>
    /// Interface base para especificaciones
    /// </summary>
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
        Expression<Func<T, bool>> ToExpression();
    }

    /// <summary>
    /// Especificación para clientes activos con más de cierta cantidad de puntos
    /// </summary>
    public class ClientePreferencialSpecification : ISpecification<ClienteAggregate>
    {
        private readonly int _puntosMinimos;

        public ClientePreferencialSpecification(int puntosMinimos)
        {
            _puntosMinimos = puntosMinimos;
        }

        public bool IsSatisfiedBy(ClienteAggregate cliente)
        {
            return cliente.Activo && cliente.PuntosAcumulados >= _puntosMinimos;
        }

        public Expression<Func<ClienteAggregate, bool>> ToExpression()
        {
            return cliente => cliente.Activo && cliente.PuntosAcumulados >= _puntosMinimos;
        }
    }

    /// <summary>
    /// Especificación para clientes elegibles para promociones especiales
    /// </summary>
    public class ClienteElegiblePromocionSpecification : ISpecification<ClienteAggregate>
    {
        private readonly DateTime _fechaLimite;

        public ClienteElegiblePromocionSpecification(int diasAntiguedad)
        {
            _fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);
        }

        public bool IsSatisfiedBy(ClienteAggregate cliente)
        {
            return cliente.Activo && cliente.FechaCreacion <= _fechaLimite;
        }

        public Expression<Func<ClienteAggregate, bool>> ToExpression()
        {
            return cliente => cliente.Activo && cliente.FechaCreacion <= _fechaLimite;
        }
    }
}
