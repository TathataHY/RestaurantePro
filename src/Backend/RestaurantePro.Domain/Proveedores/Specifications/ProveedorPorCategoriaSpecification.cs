using System.Linq.Expressions;
using RestaurantePro.Domain.Core.SharedKernel.Specifications;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Enums;

namespace RestaurantePro.Domain.Proveedores.Specifications
{
    /// <summary>
    /// Especificación para buscar proveedores por categoría
    /// </summary>
    public class ProveedorPorCategoriaSpecification : Specification<Proveedor>
    {
        private readonly CategoriaProveedor _categoria;
        private readonly bool? _soloProveedoresPrincipales;
        
        /// <summary>
        /// Constructor para especificación de proveedores por categoría
        /// </summary>
        /// <param name="categoria">Categoría a buscar</param>
        /// <param name="soloProveedoresPrincipales">Si es true, sólo devuelve los proveedores marcados como principales. Si es null, devuelve todos.</param>
        public ProveedorPorCategoriaSpecification(CategoriaProveedor categoria, bool? soloProveedoresPrincipales = null)
        {
            _categoria = categoria;
            _soloProveedoresPrincipales = soloProveedoresPrincipales;
        }
        
        /// <summary>
        /// Devuelve la expresión LINQ para filtrar proveedores por categoría
        /// </summary>
        /// <remarks>
        /// Esta implementación asume que las colecciones están cargadas en memoria.
        /// Para consultas a base de datos, se recomienda usar un repositorio especializado.
        /// </remarks>
        public override Expression<Func<Proveedor, bool>> ToExpression()
        {
            if (_soloProveedoresPrincipales.HasValue && _soloProveedoresPrincipales.Value)
            {
                // Si se buscan solo proveedores principales
                return p => p.EstaActivo && 
                           p.Categorias.Any(c => c.Categoria == _categoria && c.EsProveedorPrincipal);
            }
            
            // Buscar todos los proveedores con la categoría
            return p => p.EstaActivo && 
                       p.Categorias.Any(c => c.Categoria == _categoria);
        }
    }
} 