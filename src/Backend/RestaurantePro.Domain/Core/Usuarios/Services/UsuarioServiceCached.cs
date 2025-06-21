using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;

namespace RestaurantePro.Domain.Core.Usuarios.Services
{
    /// <summary>
    /// Implementación con caché del servicio de gestión de usuarios
    /// </summary>
    public class UsuarioServiceCached : IUsuarioServiceCached
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ICacheService _cacheService;
        
        // Claves de caché
        private const string CACHE_KEY_USUARIO_ID = "Usuario_Id_{0}";
        private const string CACHE_KEY_USUARIO_NOMBRE = "Usuario_Nombre_{0}";
        private const string CACHE_KEY_USUARIO_EMAIL = "Usuario_Email_{0}";
        private const string CACHE_KEY_USUARIOS_ROL = "Usuarios_Rol_{0}";
        private const string CACHE_KEY_USUARIOS_TODOS = "Usuarios_Todos_{0}"; // {0} = soloActivos
        private const string CACHE_PATTERN_USUARIO = "Usuario_";
        
        // TTL (Time-To-Live) en minutos
        private const int TTL_USUARIO_INDIVIDUAL = 60;     // Una hora para datos de usuario individual
        private const int TTL_USUARIOS_LISTA = 15;         // 15 minutos para listas de usuarios
        private const int TTL_VERIFICACIONES = 5;          // 5 minutos para verificaciones (existencia)
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="usuarioService">Servicio de usuarios original</param>
        /// <param name="cacheService">Servicio de caché</param>
        public UsuarioServiceCached(IUsuarioService usuarioService, ICacheService cacheService)
        {
            _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        
        /// <inheritdoc />
        public async Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_USUARIO_ID, id);
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async ct => await _usuarioService.ObtenerPorIdAsync(id, ct),
                TTL_USUARIO_INDIVIDUAL,
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_USUARIO_NOMBRE, nombreUsuario);
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async ct => await _usuarioService.ObtenerPorNombreUsuarioAsync(nombreUsuario, ct),
                TTL_USUARIO_INDIVIDUAL,
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_USUARIO_EMAIL, email);
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async ct => await _usuarioService.ObtenerPorEmailAsync(email, ct),
                TTL_USUARIO_INDIVIDUAL,
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Usuario>> ObtenerPorRolAsync(RolUsuario rol, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_USUARIOS_ROL, rol);
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async ct => await _usuarioService.ObtenerPorRolAsync(rol, ct),
                TTL_USUARIOS_LISTA,
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync(bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_USUARIOS_TODOS, soloActivos);
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async ct => await _usuarioService.ObtenerTodosAsync(soloActivos, ct),
                TTL_USUARIOS_LISTA,
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<Usuario> CrearUsuarioAsync(string nombreUsuario, string nombreCompleto, string email, RolUsuario rol, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            var usuario = await _usuarioService.CrearUsuarioAsync(nombreUsuario, nombreCompleto, email, rol, cancellationToken);
            
            // Invalidar caché de listas
            _cacheService.InvalidatePattern(CACHE_PATTERN_USUARIO);
            
            return usuario;
        }
        
        /// <inheritdoc />
        public async Task<Usuario?> ActualizarUsuarioAsync(Guid id, string? nombreCompleto, string? email, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            var usuario = await _usuarioService.ActualizarUsuarioAsync(id, nombreCompleto, email, cancellationToken);
            
            if (usuario != null)
            {
                // Invalidar caché del usuario específico
                InvalidarCacheUsuario(id);
                
                // Si se cambió el email, invalidar la caché por email anterior (si está en el caché)
                if (email != null)
                {
                    _cacheService.InvalidatePattern(CACHE_KEY_USUARIO_EMAIL);
                }
            }
            
            return usuario;
        }
        
        /// <inheritdoc />
        public async Task<bool> ActivarUsuarioAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            bool resultado = await _usuarioService.ActivarUsuarioAsync(id, cancellationToken);
            
            if (resultado)
            {
                // Invalidar caché del usuario específico y listas
                InvalidarCacheUsuario(id);
                _cacheService.InvalidatePattern(CACHE_KEY_USUARIOS_TODOS);
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<bool> DesactivarUsuarioAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            bool resultado = await _usuarioService.DesactivarUsuarioAsync(id, cancellationToken);
            
            if (resultado)
            {
                // Invalidar caché del usuario específico y listas
                InvalidarCacheUsuario(id);
                _cacheService.InvalidatePattern(CACHE_KEY_USUARIOS_TODOS);
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<bool> BloquearUsuarioAsync(Guid id, string motivo, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            bool resultado = await _usuarioService.BloquearUsuarioAsync(id, motivo, cancellationToken);
            
            if (resultado)
            {
                // Invalidar caché del usuario específico
                InvalidarCacheUsuario(id);
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<bool> DesbloquearUsuarioAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            bool resultado = await _usuarioService.DesbloquearUsuarioAsync(id, cancellationToken);
            
            if (resultado)
            {
                // Invalidar caché del usuario específico
                InvalidarCacheUsuario(id);
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<bool> AsignarRolAsync(Guid usuarioId, RolUsuario rol, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            bool resultado = await _usuarioService.AsignarRolAsync(usuarioId, rol, cancellationToken);
            
            if (resultado)
            {
                // Invalidar caché del usuario específico y listas por rol
                InvalidarCacheUsuario(usuarioId);
                _cacheService.InvalidatePattern(CACHE_KEY_USUARIOS_ROL);
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<bool> EliminarRolAsync(Guid usuarioId, RolUsuario rol, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            bool resultado = await _usuarioService.EliminarRolAsync(usuarioId, rol, cancellationToken);
            
            if (resultado)
            {
                // Invalidar caché del usuario específico y listas por rol
                InvalidarCacheUsuario(usuarioId);
                _cacheService.InvalidatePattern(CACHE_KEY_USUARIOS_ROL);
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"Usuario_Existe_Nombre_{nombreUsuario}";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async ct => await _usuarioService.ExisteNombreUsuarioAsync(nombreUsuario, ct),
                TTL_VERIFICACIONES,
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"Usuario_Existe_Email_{email}";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async ct => await _usuarioService.ExisteEmailAsync(email, ct),
                TTL_VERIFICACIONES,
                cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<bool> EliminarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            bool resultado = await _usuarioService.EliminarAsync(id, cancellationToken);
            
            if (resultado)
            {
                // Invalidar caché del usuario específico y listas
                InvalidarCacheUsuario(id);
                _cacheService.InvalidatePattern(CACHE_KEY_USUARIOS_TODOS);
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<bool> CambiarRolAsync(Guid id, RolUsuario nuevoRol, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            bool resultado = await _usuarioService.CambiarRolAsync(id, nuevoRol, cancellationToken);
            
            if (resultado)
            {
                // Invalidar caché del usuario específico y listas por rol
                InvalidarCacheUsuario(id);
                _cacheService.InvalidatePattern(CACHE_KEY_USUARIOS_ROL);
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<bool> ResetearPasswordAsync(Guid id, string nuevaPassword, CancellationToken cancellationToken = default)
        {
            // Operación de escritura, no se cachea pero se invalida la caché
            bool resultado = await _usuarioService.ResetearPasswordAsync(id, nuevaPassword, cancellationToken);
            
            if (resultado)
            {
                // Invalidar caché del usuario específico
                InvalidarCacheUsuario(id);
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public void InvalidarCacheUsuario(Guid usuarioId)
        {
            string cacheKey = string.Format(CACHE_KEY_USUARIO_ID, usuarioId);
            _cacheService.Remove(cacheKey);
        }
        
        /// <inheritdoc />
        public void InvalidarCacheUsuarios()
        {
            _cacheService.InvalidatePattern(CACHE_PATTERN_USUARIO);
        }
        
        /// <inheritdoc />
        public Dictionary<string, object> ObtenerEstadisticasCache()
        {
            var telemetry = _cacheService as ICacheTelemetry;
            
            var estadisticas = new Dictionary<string, object>
            {
                ["FechaReporte"] = DateTime.Now,
                ["TotalEntradas"] = 0,
                ["TasaAciertos"] = 0.0
            };
            
            if (telemetry != null)
            {
                var metricas = telemetry.GetMetrics();
                estadisticas["TotalAccesos"] = metricas.TotalAccesses;
                estadisticas["TotalAciertos"] = metricas.TotalHits;
                estadisticas["TasaAciertos"] = metricas.HitRate;
                estadisticas["TiempoPromedioAcceso"] = metricas.AverageAccessTimeMs;
                estadisticas["InvalidacionesRecientes"] = metricas.RecentInvalidations;
            }
            
            return estadisticas;
        }
    }
} 