using System;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Usuarios.Events.Usuario;
using RestaurantePro.Domain.Core.Usuarios.Services;

namespace RestaurantePro.Domain.Core.Usuarios.Events
{
    /// <summary>
    /// Manejador de eventos para invalidar la caché de usuarios cuando se producen eventos de dominio relacionados con usuarios
    /// </summary>
    public class CacheInvalidationUsuarioEventHandler : 
        IDomainEventHandler<UsuarioCreado>,
        IDomainEventHandler<UsuarioActualizado>,
        IDomainEventHandler<UsuarioDesactivado>,
        IDomainEventHandler<UsuarioActivado>,
        IDomainEventHandler<UsuarioBloqueado>,
        IDomainEventHandler<UsuarioDesbloqueado>,
        IDomainEventHandler<RolAsignado>,
        IDomainEventHandler<RolRemovido>
    {
        private readonly IUsuarioServiceCached _usuarioService;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="usuarioService">Servicio de usuario con caché</param>
        public CacheInvalidationUsuarioEventHandler(IUsuarioServiceCached usuarioService)
        {
            _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        }
        
        /// <summary>
        /// Maneja el evento UsuarioCreado
        /// </summary>
        public Task Handle(UsuarioCreado domainEvent, CancellationToken cancellationToken)
        {
            // Invalidar listas de usuarios ya que se agregó uno nuevo
            _usuarioService.InvalidarCacheUsuarios();
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Maneja el evento UsuarioActualizado
        /// </summary>
        public Task Handle(UsuarioActualizado domainEvent, CancellationToken cancellationToken)
        {
            // Invalidar caché de usuario específico
            _usuarioService.InvalidarCacheUsuario(domainEvent.UsuarioId);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Maneja el evento UsuarioDesactivado
        /// </summary>
        public Task Handle(UsuarioDesactivado domainEvent, CancellationToken cancellationToken)
        {
            // Invalidar caché de usuario específico y listas
            _usuarioService.InvalidarCacheUsuario(domainEvent.UsuarioId);
            _usuarioService.InvalidarCacheUsuarios();
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Maneja el evento UsuarioActivado
        /// </summary>
        public Task Handle(UsuarioActivado domainEvent, CancellationToken cancellationToken)
        {
            // Invalidar caché de usuario específico y listas
            _usuarioService.InvalidarCacheUsuario(domainEvent.UsuarioId);
            _usuarioService.InvalidarCacheUsuarios();
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Maneja el evento UsuarioBloqueado
        /// </summary>
        public Task Handle(UsuarioBloqueado domainEvent, CancellationToken cancellationToken)
        {
            // Invalidar caché de usuario específico
            _usuarioService.InvalidarCacheUsuario(domainEvent.UsuarioId);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Maneja el evento UsuarioDesbloqueado
        /// </summary>
        public Task Handle(UsuarioDesbloqueado domainEvent, CancellationToken cancellationToken)
        {
            // Invalidar caché de usuario específico
            _usuarioService.InvalidarCacheUsuario(domainEvent.UsuarioId);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Maneja el evento RolAsignado
        /// </summary>
        public Task Handle(RolAsignado domainEvent, CancellationToken cancellationToken)
        {
            // Invalidar caché de usuario específico y listas por rol
            _usuarioService.InvalidarCacheUsuario(domainEvent.UsuarioId);
            _usuarioService.InvalidarCacheUsuarios();
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Maneja el evento RolRemovido
        /// </summary>
        public Task Handle(RolRemovido domainEvent, CancellationToken cancellationToken)
        {
            // Invalidar caché de usuario específico y listas por rol
            _usuarioService.InvalidarCacheUsuario(domainEvent.UsuarioId);
            _usuarioService.InvalidarCacheUsuarios();
            return Task.CompletedTask;
        }
    }
} 