using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Domain.Proveedores.Results;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores
{
    public class ProveedorRepository : Repository<Proveedor>, IProveedorRepository
    {
        public ProveedorRepository(RestauranteProDbContext context) : base(context)
        {
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(Guid id, bool incluirContactos = true, bool incluirCategorias = true, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Proveedor>().AsQueryable();

            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }

            return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Proveedor?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .FirstOrDefaultAsync(p => p.Nombre == nombre, cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorRFCAsync(string rfc, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .Where(p => p.RFC == rfc)
                .ToListAsync(cancellationToken);
        }

        public async Task<Proveedor?> ObtenerPorRutAsync(string rut, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .FirstOrDefaultAsync(p => p.RUT == rut, cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerTodosAsync(bool incluirContactos = false, bool incluirCategorias = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Proveedor>().AsQueryable();
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerActivosAsync(bool incluirContactos = false, bool incluirCategorias = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Proveedor>()
                .Where(p => p.Activo);
                
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorCiudadAsync(string ciudad, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .Where(p => p.Ciudad.Contains(ciudad))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorIngredienteAsync(Guid ingredienteId, bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Proveedor>()
                .Where(p => p.IngredientesProveidos.Any(i => i.IngredienteId == ingredienteId));
                
            if (soloActivos)
            {
                query = query.Where(p => p.Activo);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorTipoProductoAsync(string tipoProducto, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .Where(p => p.TiposProducto.Contains(tipoProducto))
                .ToListAsync(cancellationToken);
        }

        public async Task<ContactoProveedor?> ObtenerContactoPorIdAsync(Guid contactoId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ContactoProveedor>()
                .FindAsync(new object[] { contactoId }, cancellationToken);
        }

        public async Task<IEnumerable<ContactoProveedor>> ObtenerContactosPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ContactoProveedor>()
                .Where(c => c.ProveedorId == proveedorId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> BuscarAsync(string termino, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Proveedor>()
                .Where(p => p.Nombre.Contains(termino) || 
                           p.Ciudad.Contains(termino) ||
                           p.Email.Contains(termino) ||
                           p.Telefono.Contains(termino))
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Proveedor> Proveedores, int Total)> ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            bool incluirContactos = false, 
            bool incluirCategorias = false, 
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Proveedor>().AsQueryable();
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            var total = await query.CountAsync(cancellationToken);
            
            var proveedores = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (proveedores, total);
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
            var query = _context.Set<Proveedor>().AsQueryable();

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
            var query = _context.Set<Proveedor>().AsQueryable();

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
            // Implementación simplificada por ahora
            var totalProveedores = await _context.Set<Proveedor>().CountAsync(cancellationToken);
            var proveedoresActivos = await _context.Set<Proveedor>().CountAsync(p => p.Activo, cancellationToken);
            
            return new ResultadoEstadisticasProveedores(
                totalProveedores,
                proveedoresActivos,
                0,
                0m,
                0m,
                new List<EstadisticaProveedor>(),
                0,
                DateTime.Now
            );
        }

        public async Task<IEnumerable<Proveedor>> ObtenerPorCategoriaAsync(
            Domain.Proveedores.Enums.CategoriaProveedor categoria, 
            bool soloProveedoresPrincipales = false, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Proveedor>()
                .Where(p => p.Categoria == categoria);

            if (soloProveedoresPrincipales)
            {
                query = query.Where(p => p.EsPrincipal);
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
            var query = _context.Set<Proveedor>()
                .Where(p => p.Categoria == categoria && p.EsPrincipal);

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
            var query = _context.Set<Proveedor>()
                .Where(p => categorias.Contains(p.Categoria));

            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }

            return await query.ToListAsync(cancellationToken);
        }
    }
} 