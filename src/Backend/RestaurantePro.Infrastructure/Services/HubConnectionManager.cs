using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using System.Collections.Concurrent;
using System.Collections;

namespace RestaurantePro.Infrastructure.Services;

/// <summary>
/// Implementación del gestor de conexiones de usuarios en los Hubs de SignalR
/// </summary>
public class HubConnectionManager : IHubConnectionManager
{
    private readonly ILogger<HubConnectionManager> _logger;
    private readonly ConcurrentDictionary<Guid, ConcurrentHashSet<string>> _usuarioConexiones;
    private readonly ConcurrentDictionary<string, Guid> _conexionUsuario;
    private readonly ConcurrentDictionary<string, ConcurrentHashSet<Guid>> _gruposUsuarios;
    private readonly ConcurrentDictionary<Guid, ConcurrentHashSet<string>> _usuarioGrupos;
    private readonly ConcurrentDictionary<string, DateTime> _conexionesTimestamp;

    public HubConnectionManager(ILogger<HubConnectionManager> logger)
    {
        _logger = logger;
        _usuarioConexiones = new ConcurrentDictionary<Guid, ConcurrentHashSet<string>>();
        _conexionUsuario = new ConcurrentDictionary<string, Guid>();
        _gruposUsuarios = new ConcurrentDictionary<string, ConcurrentHashSet<Guid>>();
        _usuarioGrupos = new ConcurrentDictionary<Guid, ConcurrentHashSet<string>>();
        _conexionesTimestamp = new ConcurrentDictionary<string, DateTime>();

        _logger.LogInformation("HubConnectionManager inicializado");
    }

    /// <summary>
    /// Agrega una nueva conexión para un usuario
    /// </summary>
    public async Task AgregarConexionAsync(Guid usuarioId, string connectionId, string? grupo = null)
    {
        // Agregar conexión al usuario
        _usuarioConexiones.AddOrUpdate(
            usuarioId,
            new ConcurrentHashSet<string> { connectionId },
            (key, existing) =>
            {
                existing.Add(connectionId);
                return existing;
            });

        // Mapear conexión a usuario
        _conexionUsuario.TryAdd(connectionId, usuarioId);

        // Registrar timestamp de la conexión
        _conexionesTimestamp.TryAdd(connectionId, DateTime.UtcNow);

        // Agregar a grupo si se especifica
        if (!string.IsNullOrEmpty(grupo))
        {
            await AgregarUsuarioAGrupoAsync(usuarioId, grupo);
        }

        _logger.LogInformation("Conexión agregada: Usuario {UsuarioId}, Connection {ConnectionId}, Grupo {Grupo}", 
            usuarioId, connectionId, grupo ?? "Ninguno");
    }

    /// <summary>
    /// Remueve una conexión específica
    /// </summary>
    public async Task RemoverConexionAsync(string connectionId)
    {
        if (_conexionUsuario.TryRemove(connectionId, out var usuarioId))
        {
            // Remover conexión del usuario
            if (_usuarioConexiones.TryGetValue(usuarioId, out var conexiones))
            {
                conexiones.Remove(connectionId);
                
                // Si el usuario no tiene más conexiones, removerlo completamente
                if (conexiones.Count == 0)
                {
                    _usuarioConexiones.TryRemove(usuarioId, out _);
                    _usuarioGrupos.TryRemove(usuarioId, out _);
                }
            }

            // Remover de todos los grupos
            if (_usuarioGrupos.TryGetValue(usuarioId, out var grupos))
            {
                foreach (var grupo in grupos.ToList())
                {
                    await RemoverUsuarioDeGrupoAsync(usuarioId, grupo);
                }
            }

            // Remover timestamp
            _conexionesTimestamp.TryRemove(connectionId, out _);

            _logger.LogInformation("Conexión removida: Usuario {UsuarioId}, Connection {ConnectionId}", 
                usuarioId, connectionId);
        }
    }

    /// <summary>
    /// Obtiene todas las conexiones de un usuario específico
    /// </summary>
    public async Task<List<string>> ObtenerConexionesUsuarioAsync(Guid usuarioId)
    {
        if (_usuarioConexiones.TryGetValue(usuarioId, out var conexiones))
        {
            return conexiones.ToList();
        }
        return new List<string>();
    }

    /// <summary>
    /// Obtiene todas las conexiones de un grupo específico
    /// </summary>
    public async Task<List<string>> ObtenerConexionesGrupoAsync(string grupo)
    {
        var conexiones = new List<string>();
        
        if (_gruposUsuarios.TryGetValue(grupo, out var usuarios))
        {
            foreach (var usuarioId in usuarios)
            {
                var conexionesUsuario = await ObtenerConexionesUsuarioAsync(usuarioId);
                conexiones.AddRange(conexionesUsuario);
            }
        }

        return conexiones;
    }

    /// <summary>
    /// Verifica si un usuario está conectado
    /// </summary>
    public async Task<bool> UsuarioEstaConectadoAsync(Guid usuarioId)
    {
        return _usuarioConexiones.ContainsKey(usuarioId) && 
               _usuarioConexiones[usuarioId].Count > 0;
    }

    /// <summary>
    /// Agrega un usuario a un grupo específico
    /// </summary>
    public async Task AgregarUsuarioAGrupoAsync(Guid usuarioId, string grupo)
    {
        // Agregar usuario al grupo
        _gruposUsuarios.AddOrUpdate(
            grupo,
            new ConcurrentHashSet<Guid> { usuarioId },
            (key, existing) =>
            {
                existing.Add(usuarioId);
                return existing;
            });

        // Agregar grupo al usuario
        _usuarioGrupos.AddOrUpdate(
            usuarioId,
            new ConcurrentHashSet<string> { grupo },
            (key, existing) =>
            {
                existing.Add(grupo);
                return existing;
            });

        _logger.LogInformation("Usuario {UsuarioId} agregado al grupo {Grupo}", usuarioId, grupo);
    }

    /// <summary>
    /// Remueve un usuario de un grupo específico
    /// </summary>
    public async Task RemoverUsuarioDeGrupoAsync(Guid usuarioId, string grupo)
    {
        // Remover usuario del grupo
        if (_gruposUsuarios.TryGetValue(grupo, out var usuarios))
        {
            usuarios.Remove(usuarioId);
            if (usuarios.Count == 0)
            {
                _gruposUsuarios.TryRemove(grupo, out _);
            }
        }

        // Remover grupo del usuario
        if (_usuarioGrupos.TryGetValue(usuarioId, out var grupos))
        {
            grupos.Remove(grupo);
            if (grupos.Count == 0)
            {
                _usuarioGrupos.TryRemove(usuarioId, out _);
            }
        }

        _logger.LogInformation("Usuario {UsuarioId} removido del grupo {Grupo}", usuarioId, grupo);
    }

    /// <summary>
    /// Obtiene todos los usuarios conectados en un grupo
    /// </summary>
    public async Task<List<Guid>> ObtenerUsuariosEnGrupoAsync(string grupo)
    {
        if (_gruposUsuarios.TryGetValue(grupo, out var usuarios))
        {
            return usuarios.ToList();
        }
        return new List<Guid>();
    }

    /// <summary>
    /// Limpia las conexiones desconectadas (cleanup automático)
    /// </summary>
    public async Task LimpiarConexionesDesconectadasAsync()
    {
        var conexionesARemover = new List<string>();
        var ahora = DateTime.UtcNow;

        foreach (var kvp in _conexionesTimestamp)
        {
            // Considerar conexión como desconectada si tiene más de 5 minutos sin actividad
            if (ahora - kvp.Value > TimeSpan.FromMinutes(5))
            {
                conexionesARemover.Add(kvp.Key);
            }
        }

        foreach (var connectionId in conexionesARemover)
        {
            await RemoverConexionAsync(connectionId);
        }

        if (conexionesARemover.Count > 0)
        {
            _logger.LogInformation("Limpiadas {Count} conexiones desconectadas", conexionesARemover.Count);
        }
    }

    /// <summary>
    /// Obtiene estadísticas de las conexiones activas
    /// </summary>
    public async Task<ConexionesEstadisticas> ObtenerEstadisticasConexionesAsync()
    {
        var estadisticas = new ConexionesEstadisticas
        {
            TotalUsuariosConectados = _usuarioConexiones.Count,
            TotalConexionesActivas = _conexionUsuario.Count,
            TotalGruposActivos = _gruposUsuarios.Count,
            UltimaActualizacion = DateTime.UtcNow
        };

        // Calcular usuarios por grupo
        foreach (var kvp in _gruposUsuarios)
        {
            estadisticas.UsuariosPorGrupo[kvp.Key] = kvp.Value.Count;
        }

        return estadisticas;
    }

    /// <summary>
    /// Obtiene el ID del usuario asociado a una conexión
    /// </summary>
    public async Task<Guid> ObtenerUsuarioPorConexionAsync(string connectionId)
    {
        if (_conexionUsuario.TryGetValue(connectionId, out var usuarioId))
        {
            return usuarioId;
        }
        return Guid.Empty;
    }

    /// <summary>
    /// Obtiene todos los grupos a los que pertenece un usuario
    /// </summary>
    public async Task<List<string>> ObtenerGruposDeUsuarioAsync(Guid usuarioId)
    {
        if (_usuarioGrupos.TryGetValue(usuarioId, out var grupos))
        {
            return grupos.ToList();
        }
        return new List<string>();
    }

    /// <summary>
    /// Actualiza el timestamp de una conexión (para mantenerla activa)
    /// </summary>
    public async Task ActualizarTimestampConexionAsync(string connectionId)
    {
        _conexionesTimestamp.AddOrUpdate(connectionId, DateTime.UtcNow, (key, oldValue) => DateTime.UtcNow);
    }

    /// <summary>
    /// Obtiene el timestamp de una conexión
    /// </summary>
    public async Task<DateTime> ObtenerTimestampConexionAsync(string connectionId)
    {
        if (_conexionesTimestamp.TryGetValue(connectionId, out var timestamp))
        {
            return timestamp;
        }
        return DateTime.MinValue;
    }
}

/// <summary>
/// Implementación thread-safe de HashSet usando ConcurrentDictionary
/// </summary>
public class ConcurrentHashSet<T> : IEnumerable<T>
{
    private readonly ConcurrentDictionary<T, byte> _dictionary = new();

    public bool Add(T item)
    {
        return _dictionary.TryAdd(item, 0);
    }

    public bool Remove(T item)
    {
        return _dictionary.TryRemove(item, out _);
    }

    public bool Contains(T item)
    {
        return _dictionary.ContainsKey(item);
    }

    public int Count => _dictionary.Count;

    public IEnumerator<T> GetEnumerator()
    {
        return _dictionary.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public List<T> ToList()
    {
        return _dictionary.Keys.ToList();
    }
} 