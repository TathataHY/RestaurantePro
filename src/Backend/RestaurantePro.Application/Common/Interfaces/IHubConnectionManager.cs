namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para gestionar las conexiones de usuarios en los Hubs de SignalR
/// </summary>
public interface IHubConnectionManager
{
    /// <summary>
    /// Agrega una nueva conexión para un usuario
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="connectionId">ID de la conexión SignalR</param>
    /// <param name="grupo">Grupo opcional al que pertenece el usuario</param>
    Task AgregarConexionAsync(Guid usuarioId, string connectionId, string? grupo = null);

    /// <summary>
    /// Remueve una conexión específica
    /// </summary>
    /// <param name="connectionId">ID de la conexión a remover</param>
    Task RemoverConexionAsync(string connectionId);

    /// <summary>
    /// Obtiene todas las conexiones de un usuario específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>Lista de connection IDs del usuario</returns>
    Task<List<string>> ObtenerConexionesUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Obtiene todas las conexiones de un grupo específico
    /// </summary>
    /// <param name="grupo">Nombre del grupo</param>
    /// <returns>Lista de connection IDs del grupo</returns>
    Task<List<string>> ObtenerConexionesGrupoAsync(string grupo);

    /// <summary>
    /// Verifica si un usuario está conectado
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>True si el usuario está conectado</returns>
    Task<bool> UsuarioEstaConectadoAsync(Guid usuarioId);

    /// <summary>
    /// Agrega un usuario a un grupo específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="grupo">Nombre del grupo</param>
    Task AgregarUsuarioAGrupoAsync(Guid usuarioId, string grupo);

    /// <summary>
    /// Remueve un usuario de un grupo específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="grupo">Nombre del grupo</param>
    Task RemoverUsuarioDeGrupoAsync(Guid usuarioId, string grupo);

    /// <summary>
    /// Obtiene todos los usuarios conectados en un grupo
    /// </summary>
    /// <param name="grupo">Nombre del grupo</param>
    /// <returns>Lista de IDs de usuarios en el grupo</returns>
    Task<List<Guid>> ObtenerUsuariosEnGrupoAsync(string grupo);

    /// <summary>
    /// Limpia las conexiones desconectadas (cleanup automático)
    /// </summary>
    Task LimpiarConexionesDesconectadasAsync();

    /// <summary>
    /// Obtiene estadísticas de las conexiones activas
    /// </summary>
    /// <returns>Estadísticas de conexiones</returns>
    Task<ConexionesEstadisticas> ObtenerEstadisticasConexionesAsync();

    /// <summary>
    /// Obtiene el ID del usuario asociado a una conexión
    /// </summary>
    /// <param name="connectionId">ID de la conexión</param>
    /// <returns>ID del usuario o Guid.Empty si no se encuentra</returns>
    Task<Guid> ObtenerUsuarioPorConexionAsync(string connectionId);

    /// <summary>
    /// Obtiene todos los grupos a los que pertenece un usuario
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>Lista de nombres de grupos</returns>
    Task<List<string>> ObtenerGruposDeUsuarioAsync(Guid usuarioId);

    /// <summary>
    /// Actualiza el timestamp de una conexión específica
    /// </summary>
    /// <param name="connectionId">ID de la conexión</param>
    Task ActualizarTimestampConexionAsync(string connectionId);

    /// <summary>
    /// Obtiene el timestamp de una conexión específica
    /// </summary>
    /// <param name="connectionId">ID de la conexión</param>
    /// <returns>Timestamp de la conexión o DateTime.MinValue si no se encuentra</returns>
    Task<DateTime> ObtenerTimestampConexionAsync(string connectionId);
}

/// <summary>
/// Estadísticas de las conexiones activas
/// </summary>
public class ConexionesEstadisticas
{
    /// <summary>
    /// Total de usuarios conectados
    /// </summary>
    public int TotalUsuariosConectados { get; set; }

    /// <summary>
    /// Total de conexiones activas
    /// </summary>
    public int TotalConexionesActivas { get; set; }

    /// <summary>
    /// Total de grupos activos
    /// </summary>
    public int TotalGruposActivos { get; set; }

    /// <summary>
    /// Usuarios por grupo
    /// </summary>
    public Dictionary<string, int> UsuariosPorGrupo { get; set; } = new();

    /// <summary>
    /// Timestamp de la última actualización
    /// </summary>
    public DateTime UltimaActualizacion { get; set; } = DateTime.UtcNow;
} 