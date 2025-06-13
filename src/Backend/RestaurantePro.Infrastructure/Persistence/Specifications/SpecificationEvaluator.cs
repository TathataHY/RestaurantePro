using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RestaurantePro.Infrastructure.Persistence.Specifications
{
    /// <summary>
    /// Evaluador de especificaciones que convierte ISpecification en consultas IQueryable
    /// </summary>
    public static class SpecificationEvaluator
    {
        /// <summary>
        /// Aplica una especificación a un IQueryable
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad</typeparam>
        /// <param name="query">Consulta base</param>
        /// <param name="specification">Especificación a aplicar</param>
        /// <returns>IQueryable resultante de aplicar la especificación</returns>
        public static IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) 
            where T : class
        {
            // Aplicar la expresión principal de la especificación
            if (specification != null)
            {
                query = query.Where(specification.ToExpression());
            }
            
            return query;
        }

        /// <summary>
        /// Aplica una especificación a un IQueryable y permite paginar el resultado
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad</typeparam>
        /// <param name="query">Consulta base</param>
        /// <param name="specification">Especificación a aplicar</param>
        /// <param name="skip">Número de elementos a omitir</param>
        /// <param name="take">Número de elementos a tomar</param>
        /// <returns>IQueryable paginado</returns>
        public static IQueryable<T> GetPaginatedQuery<T>(IQueryable<T> query, ISpecification<T> specification, int skip, int take) 
            where T : class
        {
            // Aplicar especificación y luego paginar
            query = GetQuery(query, specification);
            return query.Skip(skip).Take(take);
        }
    }
} 