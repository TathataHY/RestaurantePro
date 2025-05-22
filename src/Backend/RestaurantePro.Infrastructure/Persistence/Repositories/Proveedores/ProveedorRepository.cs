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

        public async Task<Proveedor?> ObtenerPorIdAsync(Guid id, bool incluirContactos = true, CancellationToken cancellationToken = default)
        {
            if (incluirContactos)
            {
                return await _context.Set<Proveedor>()
                    .Include(p => p.Contactos)
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            }
            
            return await _context.Set<Proveedor>()
                .FindAsync(new object[] { id }, cancellationToken);
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

        public async Task<IEnumerable<Proveedor>> ObtenerTodosAsync(bool incluirContactos = false, CancellationToken cancellationToken = default)
        {
            if (incluirContactos)
            {
                return await _context.Set<Proveedor>()
                    .Include(p => p.Contactos)
                    .ToListAsync(cancellationToken);
            }
            
            return await _context.Set<Proveedor>()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerActivosAsync(bool incluirContactos = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Proveedor>()
                .Where(p => p.EstaActivo);
                
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
            // Asumimos que existe una relación entre Proveedor e Ingrediente a través de una tabla intermedia
            // Esto es una implementación simplificada
            var query = _context.Set<Proveedor>()
                .Where(p => p.IngredientesProveidos.Any(i => i.IngredienteId == ingredienteId));
                
            if (soloActivos)
            {
                query = query.Where(p => p.EstaActivo);
            }
            
            return await query.ToListAsync(cancellationToken);
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

        public async Task<(IEnumerable<Proveedor> Proveedores, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, bool incluirContactos = false, CancellationToken cancellationToken = default)
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

        public async Task<ResultadoEstadisticasProveedores> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
        {
            // Calcular estadísticas básicas
            var totalProveedores = await _context.Set<Proveedor>().CountAsync(cancellationToken);
            var proveedoresActivos = await _context.Set<Proveedor>().CountAsync(p => p.EstaActivo, cancellationToken);
            
            // Obtener las órdenes de compra para estadísticas
            var fechaInicio = DateTime.Now.AddDays(-30); // último mes
            var ordenesCompra = await _context.Set<OrdenCompra>()
                .Where(o => o.FechaCreacion >= fechaInicio)
                .ToListAsync(cancellationToken);
                
            // Calcular estadísticas de órdenes
            var ordenesEnPeriodo = ordenesCompra.Count;
            var valorTotalOrdenes = ordenesCompra.Sum(o => o.ValorTotal);
            
            // Calcular tiempo promedio de entrega
            var tiempoPromedioEntrega = 0m;
            var ordenesEntregadas = ordenesCompra.Where(o => o.FechaEntregaReal.HasValue && o.FechaEntrega.HasValue).ToList();
            if (ordenesEntregadas.Any())
            {
                tiempoPromedioEntrega = ordenesEntregadas.Average(o => 
                    (decimal)(o.FechaEntregaReal!.Value - o.FechaEntrega!.Value).TotalDays);
            }
            
            // Obtener órdenes pendientes por proveedor
            var proveedoresConOrdenesPendientes = await _context.Set<Proveedor>()
                .CountAsync(p => p.OrdenesCompra.Any(o => o.Estado == Core.Inventario.Compras.OrdenesCompra.Enums.EstadoOrdenCompra.EnProceso), 
                    cancellationToken);
            
            // Obtener top proveedores por volumen
            var proveedoresTop = await _context.Set<Proveedor>()
                .Where(p => p.OrdenesCompra.Any(o => o.FechaCreacion >= fechaInicio))
                .Select(p => new 
                {
                    Proveedor = p,
                    TotalOrdenes = p.OrdenesCompra.Count(o => o.FechaCreacion >= fechaInicio),
                    ValorTotalOrdenes = p.OrdenesCompra.Where(o => o.FechaCreacion >= fechaInicio).Sum(o => o.ValorTotal),
                    TiempoPromedio = p.OrdenesCompra
                        .Where(o => o.FechaCreacion >= fechaInicio && o.FechaEntregaReal.HasValue && o.FechaEntrega.HasValue)
                        .Select(o => (decimal)(o.FechaEntregaReal!.Value - o.FechaEntrega!.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .OrderByDescending(x => x.ValorTotalOrdenes)
                .Take(5)
                .ToListAsync(cancellationToken);
                
            // Mapear a estadísticas de proveedor
            var topProveedores = proveedoresTop.Select(p => new EstadisticaProveedor(
                p.Proveedor.Id,
                p.Proveedor.Nombre,
                p.TotalOrdenes,
                p.ValorTotalOrdenes,
                p.TiempoPromedio
            )).ToList();
            
            // Crear y devolver el resultado
            return new ResultadoEstadisticasProveedores(
                totalProveedores,
                proveedoresActivos,
                ordenesEnPeriodo,
                valorTotalOrdenes,
                tiempoPromedioEntrega,
                topProveedores,
                proveedoresConOrdenesPendientes,
                DateTime.Now
            );
        }

        public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 