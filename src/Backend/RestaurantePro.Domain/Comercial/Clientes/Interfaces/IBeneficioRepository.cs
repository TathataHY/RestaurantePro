namespace RestaurantePro.Domain.Comercial.Clientes.Interfaces;

/// <summary>
/// Repositorio para beneficios de fidelización
/// </summary>
public interface IBeneficioRepository : IRepository<Beneficio>
{
    /// <summary>
    /// Obtiene beneficios por tipo de tarjeta
    /// </summary>
    /// <param name="tipoTarjeta">Tipo de tarjeta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de beneficios</returns>
    Task<List<Beneficio>> ObtenerPorTipoTarjetaAsync(string tipoTarjeta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene beneficios activos
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de beneficios activos</returns>
    Task<List<Beneficio>> ObtenerActivosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene beneficios vigentes para una fecha específica
    /// </summary>
    /// <param name="fecha">Fecha a verificar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de beneficios vigentes</returns>
    Task<List<Beneficio>> ObtenerVigentesAsync(DateTime fecha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene beneficios por tipo
    /// </summary>
    /// <param name="tipo">Tipo de beneficio</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de beneficios del tipo especificado</returns>
    Task<List<Beneficio>> ObtenerPorTipoAsync(string tipo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un beneficio con el mismo nombre
    /// </summary>
    /// <param name="nombre">Nombre del beneficio</param>
    /// <param name="excluirId">ID a excluir de la búsqueda (para actualizaciones)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si existe un beneficio con el mismo nombre</returns>
    Task<bool> ExisteConNombreAsync(string nombre, Guid? excluirId = null, CancellationToken cancellationToken = default);
} 