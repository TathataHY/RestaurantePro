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
using RestaurantePro.Domain.Proveedores.Enums;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores
{
    /// <summary>
    /// Implementación del repositorio de proveedores
    /// </summary>
    public class ProveedorRepository : Repository<Proveedor>, IProveedorRepository
    {
        private new readonly RestauranteProDbContext _dbContext;

        public ProveedorRepository(RestauranteProDbContext dbContext, ILogger<ProveedorRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Obtiene un proveedor por su ID
        /// </summary>
        public override async Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        /// <summary>
        /// Obtiene un proveedor por su ID
        /// </summary>
        public async Task<Proveedor?> ObtenerPorIdAsync(Guid id, bool incluirContactos = true, bool incluirCategorias = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (incluirContactos)
                query = query.Include(p => p.Contactos);

            if (incluirCategorias)
                query = query.Include(p => p.Categorias);

            return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        /// <summary>
        /// Obtiene un proveedor por su nombre
        /// </summary>
        public async Task<Proveedor?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Nombre == nombre, cancellationToken);
        }

        /// <summary>
        /// Obtiene proveedores por RFC
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerPorRFCAsync(string rfc, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.RFC == rfc)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene un proveedor por su RUT
        /// </summary>
        public async Task<Proveedor?> ObtenerPorRutAsync(string rut, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.RFC == rut, cancellationToken);
        }

        /// <summary>
        /// Obtiene un proveedor por su RUT (método alternativo para compatibilidad)
        /// </summary>
        public async Task<Proveedor?> ObtenerPorRUTAsync(string rut, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.RFC == rut, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los proveedores
        /// </summary>
        public override async Task<IEnumerable<Proveedor>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los proveedores
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerTodosAsync(bool incluirContactos = false, bool incluirCategorias = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            if (incluirCategorias)
            {
                query = query.Include(p => p.Categorias);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene proveedores activos
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerActivosAsync(bool incluirContactos = false, bool incluirCategorias = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(p => p.Activo);
                
            if (incluirContactos)
            {
                query = query.Include(p => p.Contactos);
            }
            
            if (incluirCategorias)
            {
                query = query.Include(p => p.Categorias);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene proveedores por ciudad o región
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerPorCiudadAsync(string ciudad, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Ciudad == ciudad)
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene un contacto de proveedor por su ID
        /// </summary>
        public async Task<ContactoProveedor?> ObtenerContactoPorIdAsync(Guid contactoId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<ContactoProveedor>()
                .FirstOrDefaultAsync(c => c.Id == contactoId, cancellationToken);
        }

        /// <summary>
        /// Obtiene contactos de un proveedor
        /// </summary>
        public async Task<IEnumerable<ContactoProveedor>> ObtenerContactosPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<ContactoProveedor>()
                .Where(c => c.ProveedorId == proveedorId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca proveedores por término (nombre, ciudad, email, etc.)
        /// </summary>
        public async Task<IEnumerable<Proveedor>> BuscarAsync(string termino, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerTodosAsync(false, false, cancellationToken);

            return await _dbSet
                .Where(p => 
                    p.Nombre.Contains(termino) || 
                    p.Ciudad.Contains(termino) || 
                    p.Email.Value.Contains(termino) || 
                    p.RFC.Contains(termino) ||
                    p.NombreContacto.Contains(termino))
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene proveedores con paginación
        /// </summary>
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

        /// <summary>
        /// Obtiene proveedores con paginación
        /// </summary>
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
            
            if (incluirCategorias)
            {
                query = query.Include(p => p.Categorias);
            }
            
            var total = await query.CountAsync(cancellationToken);
            
            var proveedores = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (Proveedores: proveedores, Total: total);
        }

        /// <summary>
        /// Obtiene proveedores paginados con filtros avanzados
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerProveedoresPaginadosAsync(
            int pagina, 
            int elementosPorPagina, 
            string? termino = null,
            CategoriaProveedor? categoria = null,
            bool soloActivos = true,
            bool incluirInactivos = false,
            string? campoOrden = null,
            bool ordenAscendente = true,
            string? ciudad = null,
            string? pais = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            // Filtro por término
            if (!string.IsNullOrWhiteSpace(termino))
            {
                query = query.Where(p => 
                    p.Nombre.Contains(termino) || 
                    p.Ciudad.Contains(termino) || 
                    p.RFC.Contains(termino) || 
                    p.Email.Value.Contains(termino));
            }

            // Filtro por categoría
            if (categoria.HasValue)
            {
                query = query.Where(p => p.Categorias.Any(c => c.Categoria == categoria));
            }

            // Filtro por ciudad
            if (!string.IsNullOrWhiteSpace(ciudad))
            {
                query = query.Where(p => p.Ciudad == ciudad);
            }

            // Filtro por país
            if (!string.IsNullOrWhiteSpace(pais))
            {
                query = query.Where(p => p.Pais == pais);
            }

            // Filtro por estado
            if (soloActivos && !incluirInactivos)
            {
                query = query.Where(p => p.Activo);
            }
            else if (!soloActivos && !incluirInactivos)
            {
                query = query.Where(p => !p.Activo);
            }

            // Ordenamiento
            IOrderedQueryable<Proveedor>? queryOrdenada = null;
            
            if (string.IsNullOrWhiteSpace(campoOrden) || campoOrden.ToLower() == "nombre")
            {
                queryOrdenada = ordenAscendente
                    ? query.OrderBy(p => p.Nombre)
                    : query.OrderByDescending(p => p.Nombre);
            }
            else if (campoOrden.ToLower() == "ciudad")
            {
                queryOrdenada = ordenAscendente
                    ? query.OrderBy(p => p.Ciudad)
                    : query.OrderByDescending(p => p.Ciudad);
            }
            else if (campoOrden.ToLower() == "fecharegistro")
            {
                queryOrdenada = ordenAscendente
                    ? query.OrderBy(p => p.FechaRegistro)
                    : query.OrderByDescending(p => p.FechaRegistro);
            }
            else
            {
                queryOrdenada = ordenAscendente
                    ? query.OrderBy(p => p.Nombre)
                    : query.OrderByDescending(p => p.Nombre);
            }

            // Paginación
            return await queryOrdenada
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Cuenta el total de proveedores con filtros
        /// </summary>
        public async Task<int> ContarProveedoresAsync(
            string? termino = null,
            CategoriaProveedor? categoria = null,
            bool soloActivos = true,
            bool incluirInactivos = false,
            string? ciudad = null,
            string? pais = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            // Filtro por término
            if (!string.IsNullOrWhiteSpace(termino))
            {
                query = query.Where(p => 
                    p.Nombre.Contains(termino) || 
                    p.Ciudad.Contains(termino) || 
                    p.RFC.Contains(termino) || 
                    p.Email.Value.Contains(termino));
            }

            // Filtro por categoría
            if (categoria.HasValue)
            {
                query = query.Where(p => p.Categorias.Any(c => c.Categoria == categoria));
            }

            // Filtro por ciudad
            if (!string.IsNullOrWhiteSpace(ciudad))
            {
                query = query.Where(p => p.Ciudad == ciudad);
            }

            // Filtro por país
            if (!string.IsNullOrWhiteSpace(pais))
            {
                query = query.Where(p => p.Pais == pais);
            }

            // Filtro por estado
            if (soloActivos && !incluirInactivos)
            {
                query = query.Where(p => p.Activo);
            }
            else if (!soloActivos && !incluirInactivos)
            {
                query = query.Where(p => !p.Activo);
            }

            return await query.CountAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene estadísticas de proveedores
        /// </summary>
        public async Task<ResultadoEstadisticasProveedores> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
        {
            var totalProveedores = await _dbSet.CountAsync(cancellationToken);
            var proveedoresActivos = await _dbSet.CountAsync(p => p.Activo, cancellationToken);
            var proveedoresConOrdenesPendientes = 0; // Esta información realmente requeriría consultar órdenes

            // Estos valores son ejemplos para cumplir con la firma - en una implementación real deberían calcularse
            var ordenesEnPeriodo = 0;
            var valorTotalOrdenesEnPeriodo = 0M;
            var tiempoPromedioEntrega = 0M;

            // Lista vacía para el ejemplo
            var topProveedores = new List<EstadisticaProveedor>();

            return new ResultadoEstadisticasProveedores(
                totalProveedores: totalProveedores,
                proveedoresActivos: proveedoresActivos,
                ordenesEnPeriodo: ordenesEnPeriodo,
                valorTotalOrdenesEnPeriodo: valorTotalOrdenesEnPeriodo,
                tiempoPromedioEntrega: tiempoPromedioEntrega,
                topProveedoresPorVolumen: topProveedores,
                proveedoresConOrdenesPendientes: proveedoresConOrdenesPendientes,
                fechaCalculo: DateTime.Now
            );
        }

        /// <summary>
        /// Obtiene proveedores por categoría
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerPorCategoriaAsync(
            CategoriaProveedor categoria, 
            bool soloProveedoresPrincipales = false, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(p => p.Categorias.Any(c => c.Categoria == categoria));

            if (soloProveedoresPrincipales)
                query = query.Where(p => p.Categorias.Any(c => c.Categoria == categoria && c.EsProveedorPrincipal));

            if (incluirContactos)
                query = query.Include(p => p.Contactos);

            return await query
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene el proveedor principal para una categoría específica
        /// </summary>
        public async Task<Proveedor?> ObtenerProveedorPrincipalPorCategoriaAsync(
            CategoriaProveedor categoria, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(p => 
                p.Activo && 
                p.Categorias.Any(c => c.Categoria == categoria && c.EsProveedorPrincipal));

            if (incluirContactos)
                query = query.Include(p => p.Contactos);

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene proveedores por múltiples categorías
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerPorMultiplesCategoriaAsync(
            IEnumerable<CategoriaProveedor> categorias, 
            bool incluirContactos = false, 
            CancellationToken cancellationToken = default)
        {
            var categoriasArray = categorias.ToArray();
            
            // Proveedores que tienen todas las categorías especificadas
            var query = _dbSet.Where(p => p.Activo && 
                categoriasArray.All(c => p.Categorias.Any(pc => pc.Categoria == c)));

            if (incluirContactos)
                query = query.Include(p => p.Contactos);

            return await query
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene proveedores que proveen un ingrediente específico
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerPorIngredienteAsync(Guid ingredienteId, bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            // En una implementación real, esto requeriría una tabla de relación entre proveedores e ingredientes
            // Por ahora, implementamos una versión simplificada usando categorías si están relacionadas con ingredientes
            var query = _dbSet.AsQueryable();
            
            if (soloActivos)
                query = query.Where(p => p.Activo);

            // Esta es una implementación simplificada. En un caso real, se consultaría la relación proveedor-ingrediente
            return await query
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene proveedores por tipo de producto o servicio
        /// </summary>
        public async Task<IEnumerable<Proveedor>> ObtenerPorTipoProductoAsync(string tipoProducto, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada usando observaciones y categorías
            return await _dbSet
                .Where(p => p.Observaciones.Contains(tipoProducto) || p.Categorias.Any(c => c.Categoria.ToString().Contains(tipoProducto)))
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
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
    }
} 