namespace RestaurantePro.Domain.Comercial.Services;

/// <summary>
/// Servicio de dominio para cálculo de puntos de fidelización
/// </summary>
public interface ICalculadoraPuntosService
{
    /// <summary>
    /// Calcula los puntos a acumular por una compra
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta de fidelización</param>
    /// <param name="montoCompra">Monto de la compra</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del cálculo de puntos</returns>
    Task<Result<CalculoResultadoPuntos>> CalcularPuntosPorCompraAsync(Guid tarjetaId, decimal montoCompra, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula los puntos por una promoción específica
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta de fidelización</param>
    /// <param name="promocionId">ID de la promoción</param>
    /// <param name="parametros">Parámetros adicionales</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del cálculo de puntos</returns>
    Task<Result<CalculoResultadoPuntos>> CalcularPuntosPorPromocionAsync(Guid tarjetaId, Guid promocionId, Dictionary<string, object>? parametros = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula el valor en dinero de una cantidad de puntos
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta de fidelización</param>
    /// <param name="puntos">Cantidad de puntos</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Valor en dinero de los puntos</returns>
    Task<Result<decimal>> CalcularValorPuntosAsync(Guid tarjetaId, int puntos, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la tasa de conversión actual para una tarjeta
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta de fidelización</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tasa de conversión (puntos por peso)</returns>
    Task<Result<decimal>> ObtenerTasaConversionAsync(Guid tarjetaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si se pueden canjear una cantidad específica de puntos
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta de fidelización</param>
    /// <param name="puntos">Cantidad de puntos a canjear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si se pueden canjear, false en caso contrario</returns>
    Task<Result<bool>> ValidarCanjePuntosAsync(Guid tarjetaId, int puntos, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula los puntos de bonificación por nivel de cliente
    /// </summary>
    /// <param name="tarjetaId">ID de la tarjeta de fidelización</param>
    /// <param name="puntosBase">Puntos base calculados</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Puntos de bonificación</returns>
    Task<Result<int>> CalcularBonificacionPorNivelAsync(Guid tarjetaId, int puntosBase, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado del cálculo de puntos
/// </summary>
public class CalculoResultadoPuntos
{
    /// <summary>
    /// Puntos calculados
    /// </summary>
    public int Puntos { get; set; }

    /// <summary>
    /// Tasa de conversión utilizada
    /// </summary>
    public decimal TasaConversion { get; set; }

    /// <summary>
    /// Bonificación aplicada
    /// </summary>
    public int Bonificacion { get; set; }

    /// <summary>
    /// Total de puntos (puntos + bonificación)
    /// </summary>
    public int TotalPuntos => Puntos + Bonificacion;

    /// <summary>
    /// Detalles del cálculo
    /// </summary>
    public string DetalleCalculo { get; set; } = string.Empty;

    /// <summary>
    /// Constructor
    /// </summary>
    public CalculoResultadoPuntos(int puntos, decimal tasaConversion, int bonificacion = 0, string detalleCalculo = "")
    {
        Puntos = puntos;
        TasaConversion = tasaConversion;
        Bonificacion = bonificacion;
        DetalleCalculo = detalleCalculo;
    }
} 