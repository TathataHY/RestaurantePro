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

        public async Task<Proveedor?> ObtenerPorRutAsync(string rut, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.RUT == rut, cancellationToken);
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

        public async Task<IEnumerable<Proveedor>> ObtenerPorIngredienteAsync(Guid ingredienteId, bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(p => p.IngredientesProveidos.Any(i => i.IngredienteId == ingredienteId));
                
            if (soloActivos)
            {
                query = query.Where(p => p.Activo);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorTipoProductoAsync(string tipoProducto, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.TiposProducto.Contains(tipoProducto))
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
                           p.Email.Contains(termino) ||
                           p.Telefono.Contains(termino))
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
                    p.Descripcion.Contains(termino) ||
                    p.Email.Contains(termino) ||
                    p.Telefono.Contains(termino) ||
                    p.Direccion.Contains(termino) ||
                    p.Ciudad.Contains(termino));
            }

            // Aplicar filtro de categoría
            if (categoria.HasValue)
            {
                query = query.Where(p => p.Categoria == categoria.Value);
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
                        ? query.OrderBy(p => p.FechaCreacion) 
                        : query.OrderByDescending(p => p.FechaCreacion),
                    "categoria" => ordenAscendente 
                        ? query.OrderBy(p => p.Categoria) 
                        : query.OrderByDescending(p => p.Categoria),
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
                    p.Descripcion.Contains(termino) ||
                    p.Email.Contains(termino) ||
                    p.Telefono.Contains(termino) ||
                    p.Direccion.Contains(termino) ||
                    p.Ciudad.Contains(termino));
            }

            // Aplicar filtro de categoría
            if (categoria.HasValue)
            {
                query = query.Where(p => p.Categoria == categoria.Value);
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<ResultadoEstadisticasProveedores> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
        {
            var totalProveedores = await _dbSet.CountAsync(cancellationToken);
            var proveedoresActivos = await _dbSet.CountAsync(p => p.Activo, cancellationToken);
            var proveedoresInactivos = totalProveedores - proveedoresActivos;
            
            var categorias = await _dbSet
                .GroupBy(p => p.Categoria)
                .Select(g => new { Categoria = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);
                
            return new ResultadoEstadisticasProveedores
            {
                TotalProveedores = totalProveedores,
                ProveedoresActivos = proveedoresActivos,
                ProveedoresInactivos = proveedoresInactivos,
                ProveedoresPorCategoria = categorias.ToDictionary(x => x.Categoria, x => x.Cantidad)
            };
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorCategoriaAsync(
            Domain.Proveedores.Enums.CategoriaProveedor categoria, 
            bool soloProveedoresPrincipales = false, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Where(p => p.Categoria == categoria);
                
            if (soloProveedoresPrincipales)
            {
                query = query.Where(p => p.EsProveedorPrincipal);
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
                .Where(p => p.Categoria == categoria && p.EsProveedorPrincipal);
                
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
            var query = _dbSet.AsQueryable();
            
            // Si hay categorías para filtrar
            if (categoriasArray.Length > 0)
            {
                query = query.Where(p => categoriasArray.Contains(p.Categoria));
            }
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        // Implementación de métodos de IRepository<Proveedor>
        public new async Task AgregarAsync(Proveedor entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public new async Task AgregarRangoAsync(IEnumerable<Proveedor> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> BuscarAsync(Func<Proveedor, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            return _dbSet.AsEnumerable().Where(predicado).ToList();
        }

        public async Task<bool> ExisteAsync(Func<Proveedor, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            return _dbSet.AsEnumerable().Any(predicado);
        }

        public async Task<int> ContarAsync(Func<Proveedor, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            return _dbSet.AsEnumerable().Count(predicado);
        }

        public async Task<Proveedor?> PrimeroODefaultAsync(Func<Proveedor, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            return _dbSet.AsEnumerable().FirstOrDefault(predicado);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorSpecAsync(ISpecification<Proveedor> specification, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada - debería traducir la especificación a consulta EF Core
            var query = _dbSet.AsQueryable();
            // Aplica la especificación (esto dependería de cómo se implementen las especificaciones)
            // Por ahora, simplemente devolvemos todos los elementos
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<int> ContarPorSpecAsync(ISpecification<Proveedor> specification, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada
            var query = _dbSet.AsQueryable();
            // Aplica la especificación
            return await query.CountAsync(cancellationToken);
        }

        public async Task<Proveedor?> PrimeroODefaultPorSpecAsync(ISpecification<Proveedor> specification, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada
            var query = _dbSet.AsQueryable();
            // Aplica la especificación
            return await query.FirstOrDefaultAsync(cancellationToken);
        }
    }
} 