namespace RestaurantePro.Domain.Comercial.Clientes.Interfaces;

/// <summary>
/// Repositorio para transacciones de puntos de fidelización
/// </summary>
public interface ITransaccionPuntosRepository : IRepository<TransaccionPuntos>
{
    /// <summary>
    /// Obtiene las transacciones de una tarjeta de fidelización
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <param name="desde">Fecha desde</param>
    /// <param name="hasta">Fecha hasta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de transacciones</returns>
    Task<List<TransaccionPuntos>> ObtenerPorTarjetaAsync(Guid tarjetaId, DateTime? desde = null, DateTime? hasta = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las transacciones de un cliente
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="desde">Fecha desde</param>
    /// <param name="hasta">Fecha hasta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de transacciones</returns>
    Task<List<TransaccionPuntos>> ObtenerPorClienteAsync(Guid clienteId, DateTime? desde = null, DateTime? hasta = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el saldo actual de puntos de una tarjeta
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Saldo actual de puntos</returns>
    Task<int> ObtenerSaldoPuntosAsync(Guid tarjetaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las transacciones por tipo
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta</param>
    /// <param name="tipo">Tipo de transacción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de transacciones del tipo especificado</returns>
    Task<List<TransaccionPuntos>> ObtenerPorTipoAsync(Guid tarjetaId, TipoTransaccionPuntos tipo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las transacciones asociadas a una factura
    /// </summary>
    /// <param name="facturaId">ID de la factura</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de transacciones asociadas a la factura</returns>
    Task<List<TransaccionPuntos>> ObtenerPorFacturaAsync(Guid facturaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca transacciones como vencidas
    /// </summary>
    /// <param name="fecha">Fecha de vencimiento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Cantidad de transacciones marcadas como vencidas</returns>
    Task<int> MarcarVencidasAsync(DateTime fecha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de transacciones para un período
    /// </summary>
    /// <param name="desde">Fecha desde</param>
    /// <param name="hasta">Fecha hasta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de transacciones</returns>
    Task<EstadisticasTransacciones> ObtenerEstadisticasAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default);
}

/// <summary>
/// Estadísticas de transacciones de puntos
/// </summary>
public class EstadisticasTransacciones
{
    /// <summary>
    /// Total de transacciones
    /// </summary>
    public int TotalTransacciones { get; set; }

    /// <summary>
    /// Total de puntos acumulados
    /// </summary>
    public int TotalPuntosAcumulados { get; set; }

    /// <summary>
    /// Total de puntos canjeados
    /// </summary>
    public int TotalPuntosCanjeados { get; set; }

    /// <summary>
    /// Total de puntos vencidos
    /// </summary>
    public int TotalPuntosVencidos { get; set; }

    /// <summary>
    /// Valor total de transacciones
    /// </summary>
    public decimal ValorTotalTransacciones { get; set; }

    /// <summary>
    /// Promedio de puntos por transacción
    /// </summary>
    public decimal PromedioPuntosPorTransaccion { get; set; }

    /// <summary>
    /// Estadísticas por tipo de transacción
    /// </summary>
    public Dictionary<TipoTransaccionPuntos, int> TransaccionesPorTipo { get; set; } = new();
} 