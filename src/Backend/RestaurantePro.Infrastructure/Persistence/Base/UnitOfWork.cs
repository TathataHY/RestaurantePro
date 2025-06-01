using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RestauranteProDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed;

        public UnitOfWork(RestauranteProDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool TieneTransaccionActiva => _transaction != null;

        public async Task IniciarTransaccionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task ConfirmarTransaccionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _transaction?.CommitAsync(cancellationToken);
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RevertirTransaccionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _transaction?.RollbackAsync(cancellationToken);
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        
        public async Task<int> GuardarEntidadesAsync(CancellationToken cancellationToken = default)
        {
            // Aquí publicaríamos eventos de dominio antes de guardar los cambios
            await PublicarEventosDominioAsync(cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken);
        }
        
        public async Task EjecutarEnTransaccionAsync(Func<Task> accion, CancellationToken cancellationToken = default)
        {
            if (TieneTransaccionActiva)
            {
                await accion();
                return;
            }
            
            await IniciarTransaccionAsync(cancellationToken);
            try
            {
                await accion();
                await ConfirmarTransaccionAsync(cancellationToken);
            }
            catch
            {
                await RevertirTransaccionAsync(cancellationToken);
                throw;
            }
        }
        
        public async Task<TResultado> EjecutarEnTransaccionAsync<TResultado>(Func<Task<TResultado>> funcion, CancellationToken cancellationToken = default)
        {
            if (TieneTransaccionActiva)
            {
                return await funcion();
            }
            
            await IniciarTransaccionAsync(cancellationToken);
            try
            {
                var resultado = await funcion();
                await ConfirmarTransaccionAsync(cancellationToken);
                return resultado;
            }
            catch
            {
                await RevertirTransaccionAsync(cancellationToken);
                throw;
            }
        }
        
        private async Task PublicarEventosDominioAsync(CancellationToken cancellationToken = default)
        {
            // Aquí implementaremos la publicación de eventos de dominio
            // Por ahora dejamos una implementación vacía
            await Task.CompletedTask;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
                _transaction?.Dispose();
            }
            _disposed = true;
        }

        // ============================================
        // ALIAS EN INGLÉS PARA COMPATIBILIDAD
        // ============================================

        /// <summary>
        /// Alias en inglés para IniciarTransaccionAsync
        /// </summary>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            await IniciarTransaccionAsync(cancellationToken);
        }

        /// <summary>
        /// Alias en inglés para ConfirmarTransaccionAsync
        /// </summary>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            await ConfirmarTransaccionAsync(cancellationToken);
        }

        /// <summary>
        /// Alias en inglés para RevertirTransaccionAsync
        /// </summary>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            await RevertirTransaccionAsync(cancellationToken);
        }

        /// <summary>
        /// Alias en inglés para GuardarCambiosAsync
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await GuardarCambiosAsync(cancellationToken);
        }
    }
} 