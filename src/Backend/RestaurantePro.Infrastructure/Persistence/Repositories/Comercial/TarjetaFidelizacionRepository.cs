using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial
{
    /// <summary>
    /// Implementación del repositorio de tarjetas de fidelización
    /// </summary>
    public class TarjetaFidelizacionRepository : Repository<TarjetaFidelizacion>, ITarjetaFidelizacionRepository
    {
        private readonly RestauranteProDbContext _dbContext;

        public TarjetaFidelizacionRepository(RestauranteProDbContext dbContext, ILogger<TarjetaFidelizacionRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Obtiene una tarjeta de fidelización por su ID
        /// </summary>
        public new async Task<TarjetaFidelizacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Obteniendo tarjeta de fidelización con ID: {TarjetaId}", id);

            return await _dbSet
                .Include(t => t.HistorialPuntos)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        /// <summary>
        /// Obtiene una tarjeta de fidelización por su código
        /// </summary>
        public async Task<TarjetaFidelizacion> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        {
            var entidad = await _dbSet
                .Include(t => t.HistorialPuntos)
                .FirstOrDefaultAsync(t => t.Codigo == codigo, cancellationToken);
                
            if (entidad == null)
                throw new KeyNotFoundException($"No se encontró la tarjeta de fidelización con código {codigo}");
                
            return entidad;
        }

        /// <summary>
        /// Obtiene una tarjeta de fidelización por su número
        /// </summary>
        public async Task<TarjetaFidelizacion?> ObtenerPorNumeroAsync(string numero, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.HistorialPuntos)
                .FirstOrDefaultAsync(t => t.Codigo == numero, cancellationToken);
        }

        /// <summary>
        /// Verifica si existe una tarjeta con el número especificado
        /// </summary>
        public async Task<bool> ExisteNumeroTarjetaAsync(string numero, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(t => t.Codigo == numero, cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las tarjetas de un cliente
        /// </summary>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.ClienteId == clienteId)
                .OrderByDescending(t => t.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene la tarjeta activa de un cliente
        /// </summary>
        public async Task<TarjetaFidelizacion> ObtenerTarjetaActivaPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            var entidad = await _dbSet
                .Include(t => t.HistorialPuntos)
                .FirstOrDefaultAsync(t => t.ClienteId == clienteId && t.Estado == EstadoTarjeta.Activa, cancellationToken);
                
            if (entidad == null)
                throw new KeyNotFoundException($"No se encontró una tarjeta de fidelización activa para el cliente con ID {clienteId}");
                
            return entidad;
        }

        /// <summary>
        /// Obtiene las tarjetas filtradas por estado
        /// </summary>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerPorEstadoAsync(EstadoTarjeta estado, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.Estado == estado)
                .OrderByDescending(t => t.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las tarjetas filtradas por nivel de fidelización
        /// </summary>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerPorNivelAsync(NivelFidelizacion nivel, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.NivelFidelizacion == nivel)
                .OrderByDescending(t => t.PuntosAcumulados)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las tarjetas filtradas por varios niveles de fidelización
        /// </summary>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerTarjetasPorNivelesAsync(IEnumerable<NivelFidelizacion> niveles, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => niveles.Contains(t.NivelFidelizacion))
                .OrderByDescending(t => t.PuntosAcumulados)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Agrega una nueva tarjeta de fidelización
        /// </summary>
        public new async Task AgregarAsync(TarjetaFidelizacion tarjeta, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(tarjeta, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Actualiza una tarjeta existente
        /// </summary>
        public new async Task ActualizarAsync(TarjetaFidelizacion tarjeta, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(tarjeta).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Genera un código único para una nueva tarjeta
        /// </summary>
        public async Task<string> GenerarCodigoUnicoAsync(string prefijo = "TF", CancellationToken cancellationToken = default)
        {
            string codigo;
            bool codigoExiste;

            do
            {
                // Generar una combinación aleatoria de números y letras
                var bytes = new byte[4];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(bytes);
                }
                
                // Convertir a una cadena hexadecimal y tomar los primeros 8 caracteres
                var codigoBase = BitConverter.ToString(bytes).Replace("-", "").Substring(0, 8);
                
                // Añadir el prefijo
                codigo = $"{prefijo}{codigoBase}";
                
                // Verificar si ya existe
                codigoExiste = await ExisteNumeroTarjetaAsync(codigo, cancellationToken);
            } while (codigoExiste);

            return codigo;
        }
        
        /// <summary>
        /// Elimina una tarjeta de fidelización
        /// </summary>
        public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var tarjeta = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (tarjeta != null)
            {
                _dbSet.Remove(tarjeta);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerConPuntosProximosAExpirarAsync(
            DateTime fechaLimite, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Obteniendo tarjetas de fidelización con puntos a expirar antes de: {FechaLimite}", fechaLimite);
            
            // Obtenemos tarjetas activas con puntos que expirarán antes de la fecha límite
            return await _dbSet
                .AsNoTracking()
                .Include(t => t.HistorialPuntos.Where(hp => 
                    hp.TipoOperacion != TipoOperacionPuntos.Vencidos && 
                    hp.FechaOperacion <= fechaLimite && 
                    hp.FechaOperacion > DateTime.Now))
                .Where(t => 
                    t.Estado == EstadoTarjeta.Activa &&
                    t.PuntosDisponibles > 0 &&
                    t.HistorialPuntos.Any(hp => 
                        hp.TipoOperacion != TipoOperacionPuntos.Vencidos && 
                        hp.FechaOperacion <= fechaLimite && 
                        hp.FechaOperacion > DateTime.Now)
                )
                .ToListAsync(cancellationToken);
        }
    }
} 