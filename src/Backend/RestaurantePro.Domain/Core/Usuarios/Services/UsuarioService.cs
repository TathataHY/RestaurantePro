using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;

namespace RestaurantePro.Domain.Core.Usuarios.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de usuarios
    /// </summary>
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="usuarioRepository">Repositorio de usuarios</param>
        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        }
        
        /// <inheritdoc />
        public async Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _usuarioRepository.ObtenerPorIdAsync(id, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
        {
            return await _usuarioRepository.ObtenerPorNombreUsuarioAsync(nombreUsuario, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _usuarioRepository.ObtenerPorEmailAsync(email, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Usuario>> ObtenerPorRolAsync(RolUsuario rol, CancellationToken cancellationToken = default)
        {
            return await _usuarioRepository.ObtenerPorRolAsync(rol, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync(bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            return await _usuarioRepository.ObtenerTodosAsync(soloActivos, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<Usuario> CrearUsuarioAsync(string nombreUsuario, string nombreCompleto, string email, RolUsuario rol, CancellationToken cancellationToken = default)
        {
            // Verificar que el nombre de usuario no exista
            if (await _usuarioRepository.ExisteNombreUsuarioAsync(nombreUsuario, cancellationToken))
            {
                throw new InvalidOperationException($"El nombre de usuario '{nombreUsuario}' ya está en uso");
            }
            
            // Verificar que el email no exista
            if (await _usuarioRepository.ExisteEmailAsync(email, cancellationToken))
            {
                throw new InvalidOperationException($"El email '{email}' ya está en uso");
            }
            
            // Crear el usuario
            var usuario = Usuario.Crear(nombreUsuario, nombreCompleto, email, rol);
            
            // Guardar en repositorio
            await _usuarioRepository.AgregarAsync(usuario, cancellationToken);
            
            return usuario;
        }
        
        /// <inheritdoc />
        public async Task<Usuario?> ActualizarUsuarioAsync(Guid id, string? nombreCompleto, string? email, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (usuario == null)
            {
                return null;
            }
            
            if (nombreCompleto != null)
            {
                usuario.Actualizar(nombreCompleto, email ?? usuario.Email);
            }
            else if (email != null)
            {
                usuario.Actualizar(usuario.NombreCompleto, email);
            }
            
            await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
            
            return usuario;
        }
        
        /// <inheritdoc />
        public async Task<bool> ActivarUsuarioAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (usuario == null)
            {
                return false;
            }
            
            usuario.Activar();
            await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<bool> DesactivarUsuarioAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (usuario == null)
            {
                return false;
            }
            
            usuario.Desactivar();
            await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<bool> BloquearUsuarioAsync(Guid id, string motivo, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (usuario == null)
            {
                return false;
            }
            
            usuario.Bloquear(motivo);
            await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<bool> DesbloquearUsuarioAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (usuario == null)
            {
                return false;
            }
            
            usuario.Desbloquear();
            await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<bool> AsignarRolAsync(Guid usuarioId, RolUsuario rol, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
            {
                return false;
            }
            
            usuario.AsignarRol(rol);
            await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
            
            return true;
        }
        
        /// <inheritdoc />
        public async Task<bool> EliminarRolAsync(Guid usuarioId, RolUsuario rol, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
            {
                return false;
            }
            
            try
            {
                usuario.RemoverRol(rol);
                await _usuarioRepository.ActualizarAsync(usuario, cancellationToken);
                return true;
            }
            catch (InvalidOperationException)
            {
                // Se lanza cuando se intenta eliminar el único rol
                return false;
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
        {
            return await _usuarioRepository.ExisteNombreUsuarioAsync(nombreUsuario, cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _usuarioRepository.ExisteEmailAsync(email, cancellationToken);
        }
    }
} 