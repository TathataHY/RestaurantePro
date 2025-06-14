using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Domain.Proveedores.Results;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Specifications;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores
{
    public class ProveedorRepository : Repository<Proveedor>, IProveedorRepository
    {
        public ProveedorRepository(DbContext context, ILogger<ProveedorRepository> logger) 
            : base(context, logger)
        {
        }

        public override async Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(Guid id, bool incluirContactos = true, bool incluirCategorias = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }

            return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Proveedor?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Nombre == nombre, cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorRFCAsync(string rfc, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.RFC == rfc)
                .ToListAsync(cancellationToken);
        }

        public override async Task<IEnumerable<Proveedor>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerTodosAsync(bool incluirContactos = false, bool incluirCategorias = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerActivosAsync(bool incluirContactos = false, bool incluirCategorias = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(p => p.Activo);
                
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorCiudadAsync(string ciudad, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Ciudad.Contains(ciudad))
                .ToListAsync(cancellationToken);
        }

        public async Task<ContactoProveedor?> ObtenerContactoPorIdAsync(Guid contactoId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<ContactoProveedor>()
                .FindAsync(new object[] { contactoId }, cancellationToken);
        }

        public async Task<IEnumerable<ContactoProveedor>> ObtenerContactosPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<ContactoProveedor>()
                .Where(c => c.ProveedorId == proveedorId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> BuscarAsync(string termino, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Nombre.Contains(termino) || 
                           p.Ciudad.Contains(termino) ||
                           p.Email.ToString().Contains(termino) ||
                           p.Telefono.ToString().Contains(termino))
                .ToListAsync(cancellationToken);
        }

        public override async Task<(IEnumerable<Proveedor> Items, int Total)> ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var total = await query.CountAsync(cancellationToken);
            
            var items = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (Items: items, Total: total);
        }

        public async Task<(IEnumerable<Proveedor> Proveedores, int Total)> ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            bool incluirContactos = false, 
            bool incluirCategorias = false, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            var total = await query.CountAsync(cancellationToken);
            
            var proveedores = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (Proveedores: proveedores, Total: total);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerProveedoresPaginadosAsync(
            int pagina, 
            int elementosPorPagina, 
            string? termino = null,
            Domain.Proveedores.Enums.CategoriaProveedor? categoria = null,
            bool soloActivos = true,
            bool incluirInactivos = false,
            string? campoOrden = null,
            bool ordenAscendente = true,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            // Aplicar filtros de estado
            if (soloActivos && !incluirInactivos)
            {
                query = query.Where(p => p.Activo);
            }
            else if (!soloActivos && !incluirInactivos)
            {
                query = query.Where(p => !p.Activo);
            }

            // Aplicar filtro de término de búsqueda
            if (!string.IsNullOrWhiteSpace(termino))
            {
                query = query.Where(p => 
                    p.Nombre.Contains(termino) || 
                    p.Observaciones.Contains(termino) ||
                    p.Email.ToString().Contains(termino) ||
                    p.Telefono.ToString().Contains(termino) ||
                    p.Direccion.Contains(termino) ||
                    p.Ciudad.Contains(termino));
            }

            // Aplicar filtro de categoría
            if (categoria.HasValue)
            {
                query = query.Where(p => p.Categorias.Any(c => c.Categoria == categoria.Value));
            }

            // Aplicar ordenamiento
            if (!string.IsNullOrWhiteSpace(campoOrden))
            {
                query = campoOrden.ToLower() switch
                {
                    "nombre" => ordenAscendente 
                        ? query.OrderBy(p => p.Nombre) 
                        : query.OrderByDescending(p => p.Nombre),
                    "fechacreacion" => ordenAscendente 
                        ? query.OrderBy(p => p.FechaRegistro) 
                        : query.OrderByDescending(p => p.FechaRegistro),
                    "activo" => ordenAscendente 
                        ? query.OrderBy(p => p.Activo) 
                        : query.OrderByDescending(p => p.Activo),
                    _ => query.OrderBy(p => p.Nombre)
                };
            }
            else
            {
                query = query.OrderBy(p => p.Nombre);
            }

            // Aplicar paginación (páginas basadas en 1)
            var skip = (pagina - 1) * elementosPorPagina;
            query = query.Skip(skip).Take(elementosPorPagina);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<int> ContarProveedoresAsync(
            string? termino = null,
            Domain.Proveedores.Enums.CategoriaProveedor? categoria = null,
            bool soloActivos = true,
            bool incluirInactivos = false,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            // Aplicar filtros de estado
            if (soloActivos && !incluirInactivos)
            {
                query = query.Where(p => p.Activo);
            }
            else if (!soloActivos && !incluirInactivos)
            {
                query = query.Where(p => !p.Activo);
            }

            // Aplicar filtro de término de búsqueda
            if (!string.IsNullOrWhiteSpace(termino))
            {
                query = query.Where(p => 
                    p.Nombre.Contains(termino) || 
                    p.Observaciones.Contains(termino) ||
                    p.Email.ToString().Contains(termino) ||
                    p.Telefono.ToString().Contains(termino) ||
                    p.Direccion.Contains(termino) ||
                    p.Ciudad.Contains(termino));
            }

            // Aplicar filtro de categoría
            if (categoria.HasValue)
            {
                query = query.Where(p => p.Categorias.Any(c => c.Categoria == categoria.Value));
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<ResultadoEstadisticasProveedores> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
        {
            var totalProveedores = await _dbSet.CountAsync(cancellationToken);
            var proveedoresActivos = await _dbSet.CountAsync(p => p.Activo, cancellationToken);
            var proveedoresInactivos = totalProveedores - proveedoresActivos;

            var categorias = await _dbSet
                .SelectMany(p => p.Categorias)
                .GroupBy(c => c.Categoria)
                .Select(g => new { Categoria = g.Key, Total = g.Count() })
                .ToListAsync(cancellationToken);

            var estadisticasPorCategoria = categorias
                .Select(c => new EstadisticaProveedor(
                    Guid.Empty, // ProveedorId (usamos Empty ya que son estadísticas por categoría)
                    c.Categoria.ToString(), // Nombre de la categoría como nombre
                    0, // Total de órdenes (no disponible en este contexto)
                    0, // Valor total de órdenes (no disponible en este contexto)
                    0)) // Tiempo promedio de entrega (no disponible en este contexto)
                .ToList();

            return new ResultadoEstadisticasProveedores(
                totalProveedores,
                proveedoresActivos,
                proveedoresInactivos,
                0, // Promedio de pedidos por proveedor
                0, // Promedio de tiempo de entrega
                estadisticasPorCategoria,
                0, // Total de proveedores principales
                DateTime.Now // Fecha de generación
            );
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorCategoriaAsync(
            Domain.Proveedores.Enums.CategoriaProveedor categoria, 
            bool soloProveedoresPrincipales = false, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(p => p.Categorias.Any(c => c.Categoria == categoria));
                
            if (soloProveedoresPrincipales)
            {
                query = query.Where(p => p.Categorias.Any(c => c.Categoria == categoria && c.EsProveedorPrincipal));
            }
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<Proveedor?> ObtenerProveedorPrincipalPorCategoriaAsync(
            Domain.Proveedores.Enums.CategoriaProveedor categoria, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(p => p.Categorias.Any(c => c.Categoria == categoria && c.EsProveedorPrincipal));
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorMultiplesCategoriaAsync(
            IEnumerable<Domain.Proveedores.Enums.CategoriaProveedor> categorias, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default)
        {
            var categoriasArray = categorias.ToArray();
            
            var query = _dbSet
                .Where(p => p.Categorias.Any(c => categoriasArray.Contains(c.Categoria)));
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public new async Task AgregarAsync(Proveedor entity, CancellationToken cancellationToken = default)
        {
            await base.AgregarAsync(entity, cancellationToken);
        }

        public new async Task AgregarRangoAsync(IEnumerable<Proveedor> entities, CancellationToken cancellationToken = default)
        {
            await base.AgregarRangoAsync(entities, cancellationToken);
        }

        public new Task<IEnumerable<Proveedor>> BuscarAsync(Func<Proveedor, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Proveedor>().Where(predicado).ToList();
            return Task.FromResult<IEnumerable<Proveedor>>(result);
        }

        public new Task<bool> ExisteAsync(Func<Proveedor, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Proveedor>().Any(predicado);
            return Task.FromResult(result);
        }

        public new Task<int> ContarAsync(Func<Proveedor, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Proveedor>().Count(predicado);
            return Task.FromResult(result);
        }

        public new Task<Proveedor?> PrimeroODefaultAsync(Func<Proveedor, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Proveedor>().FirstOrDefault(predicado);
            return Task.FromResult(result);
        }

        public new Task<IEnumerable<Proveedor>> ObtenerPorSpecAsync(ISpecification<Proveedor> specification, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var result = SpecificationEvaluator.GetQuery(query, specification).ToList();
            return Task.FromResult<IEnumerable<Proveedor>>(result);
        }

        public new Task<int> ContarPorSpecAsync(ISpecification<Proveedor> specification, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var result = SpecificationEvaluator.GetQuery(query, specification).Count();
            return Task.FromResult(result);
        }

        public new Task<Proveedor?> PrimeroODefaultPorSpecAsync(ISpecification<Proveedor> specification, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var result = SpecificationEvaluator.GetQuery(query, specification).FirstOrDefault();
            return Task.FromResult(result);
        }

        public async Task<Proveedor?> ObtenerPorRutAsync(string rut, CancellationToken cancellationToken = default)
        {
            // Implementación vacía ya que la entidad Proveedor no tiene la propiedad RUT
            // Devolvemos null para cumplir con la interfaz
            return null;
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorIngredienteAsync(Guid ingredienteId, bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            // Como la entidad Proveedor no tiene la propiedad IngredientesProveidos,
            // implementamos una versión simplificada que devuelve una lista vacía
            return new List<Proveedor>();
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorTipoProductoAsync(string tipoProducto, CancellationToken cancellationToken = default)
        {
            // Como la entidad Proveedor no tiene la propiedad TiposProducto,
            // implementamos una versión simplificada que devuelve una lista vacía
            return new List<Proveedor>();
        }
    }
} 