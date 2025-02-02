using System.Linq;
using RestaurantePro.Core.Specifications;
using RestaurantePro.Core.Specifications.Base;

namespace RestaurantePro.Core.Specifications
{
    public static class SpecificationEvaluator<T>
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
        {
            var query = inputQuery;

            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            if (specification.OrderBy != null)
            {
                query = specification.OrderBy(query);
            }
            else if (specification.OrderByDescending != null)
            {
                query = specification.OrderByDescending(query);
            }

            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip).Take(specification.Take);
            }

            return query;
        }
    }
} 