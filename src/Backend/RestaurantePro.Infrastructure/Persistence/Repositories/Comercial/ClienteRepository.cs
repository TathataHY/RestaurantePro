using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Specifications;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        private new readonly RestauranteProDbContext _dbContext;

        public ClienteRepository(RestauranteProDbContext dbContext, ILogger<ClienteRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        public override async Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Id == id && c.EstaActivo, cancellationToken);
        }

        public async Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Email.Value == email && c.EstaActivo, cancellationToken);
        }

        public async Task<Cliente?> ObtenerPorTelefonoAsync(string telefono, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Telefono.Value == telefono && c.EstaActivo, cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesFrecuentesAsync(int cantidad = 10, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.EstaActivo && c.TarjetaFidelizacionPrincipalId != null)
                .OrderByDescending(c => c.PuntosAcumulados)
                .Take(cantidad)
                .ToListAsync(cancellationToken);
        }

        public new async Task<(IEnumerable<Cliente> Items, int Total)> ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            // Aplicar filtros
            query = query.Where(c => c.EstaActivo);
            
            // Contar total
            var total = await query.CountAsync(cancellationToken);
            
            // Paginar resultados
            var clientes = await query
                .OrderBy(c => c.Nombre)
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
            
            return (Items: clientes, Total: total);
        }

        // Implementación explícita para la interfaz específica
        async Task<(IEnumerable<Cliente> Clientes, int Total)> IClienteRepository.ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            CancellationToken cancellationToken)
        {
            var result = await ObtenerPaginadoAsync(pagina, elementosPorPagina, cancellationToken);
            return (Clientes: result.Items, Total: result.Total);
        }

        public override async Task<Cliente> AgregarAsync(Cliente entity, CancellationToken cancellationToken = default)
        {
            var result = await _dbSet.AddAsync(entity, cancellationToken);
            return result.Entity;
        }

        public override async Task<IEnumerable<Cliente>> AgregarRangoAsync(IEnumerable<Cliente> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return entities;
        }

        public async Task<IEnumerable<Cliente>> BuscarPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.Nombre.NombreCompleto.Contains(nombre) && c.EstaActivo)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.Nombre.NombreCompleto.Contains(nombre))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorSegmentoAsync(SegmentoCliente segmento, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.Segmento == segmento)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorEstadoActivoAsync(bool activos, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.EstaActivo == activos)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerConTarjetaFidelizacionAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.TarjetaFidelizacionPrincipalId != null)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesMasFrecuentesAsync(int cantidad, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.EstaActivo)
                .OrderByDescending(c => c.CantidadVisitas)
                .Take(cantidad)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorPuntosMinimosAsync(int puntosMinimos, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.PuntosAcumulados >= puntosMinimos)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerPorRangoFechasRegistroAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => c.FechaCreacion >= fechaInicio && c.FechaCreacion <= fechaFin)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> VerificarExistenciaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .AnyAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Cliente>()
                .Where(c => ids.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesActivosConVisitasAsync(int cantidadMinimaVisitas, int cantidadDias, CancellationToken cancellationToken = default)
        {
            var fechaLimite = DateTime.Now.AddDays(-cantidadDias);
            
            return await _dbContext.Set<Cliente>()
                .Where(c => c.EstaActivo && c.CantidadVisitas >= cantidadMinimaVisitas)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerClientesConHistorialVisitasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            // Aquí asumo que tienes una relación entre Cliente y alguna entidad de Visitas
            // Si no existe tal relación, esto tendría que adaptarse
            return await _dbContext.Set<Cliente>()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosConHistorialVisitasAsync(int diasHistorial, CancellationToken cancellationToken = default)
        {
            var fechaLimite = DateTime.Now.AddDays(-diasHistorial);
            
            return await _dbContext.Set<Cliente>()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Factura>> ObtenerFacturasRecientesAsync(Guid clienteId, DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Factura>()
                .Where(f => f.ClienteId == clienteId && f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin)
                .ToListAsync(cancellationToken);
        }

        public new Task ActualizarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(cliente).State = EntityState.Modified;
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task GuardarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            if (cliente.Id == Guid.Empty)
            {
                _dbContext.Set<Cliente>().Add(cliente);
            }
            else
            {
                _dbContext.Entry(cliente).State = EntityState.Modified;
            }

            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public new Task<IEnumerable<Cliente>> BuscarAsync(Func<Cliente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Cliente>().Where(predicado).ToList();
            return Task.FromResult<IEnumerable<Cliente>>(result);
        }

        public new Task<bool> ExisteAsync(Func<Cliente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Cliente>().Any(predicado);
            return Task.FromResult(result);
        }

        public new Task<int> ContarAsync(Func<Cliente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Cliente>().Count(predicado);
            return Task.FromResult(result);
        }

        public new Task<Cliente?> PrimeroODefaultAsync(Func<Cliente, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbContext.Set<Cliente>().FirstOrDefault(predicado);
            return Task.FromResult(result);
        }

        public new Task<IEnumerable<Cliente>> ObtenerPorSpecAsync(ISpecification<Cliente> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var result = SpecificationEvaluator.GetQuery(query, spec).ToList();
            return Task.FromResult<IEnumerable<Cliente>>(result);
        }

        public new Task<int> ContarPorSpecAsync(ISpecification<Cliente> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var result = SpecificationEvaluator.GetQuery(query, spec).Count();
            return Task.FromResult(result);
        }

        public new Task<Cliente?> PrimeroODefaultPorSpecAsync(ISpecification<Cliente> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var result = SpecificationEvaluator.GetQuery(query, spec).FirstOrDefault();
            return Task.FromResult(result);
        }
    }
} 