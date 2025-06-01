namespace RestaurantePro.Domain.Operaciones.Services;

/// <summary>
/// Servicio de dominio para generar números únicos de comandas
/// </summary>
public interface IGeneradorNumeroComandaService
{
    /// <summary>
    /// Genera un nuevo número de comanda único
    /// </summary>
    /// <param name="sucursalId">ID de la sucursal</param>
    /// <param name="fecha">Fecha de la comanda</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de comanda generado</returns>
    Task<string> GenerarNumeroComandaAsync(Guid sucursalId, DateTime fecha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un número de comanda con formato personalizado
    /// </summary>
    /// <param name="parametros">Parámetros para la generación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de comanda generado</returns>
    Task<string> GenerarNumeroComandaAsync(ParametrosGeneracionComanda parametros, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un número de comanda con tipo y canal específicos
    /// </summary>
    /// <param name="sucursalId">ID de la sucursal</param>
    /// <param name="fecha">Fecha de la comanda</param>
    /// <param name="tipoComanda">Tipo de comanda</param>
    /// <param name="canalOrden">Canal de orden</param>
    /// <param name="numeroMesa">Número de mesa (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado con el número de comanda generado</returns>
    Task<Result<string>> GenerarNumeroAsync(Guid sucursalId, DateTime fecha, TipoComanda tipoComanda, CanalOrden canalOrden, int? numeroMesa = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un número de comanda con prefijo personalizado
    /// </summary>
    /// <param name="prefijo">Prefijo personalizado</param>
    /// <param name="sucursalId">ID de la sucursal</param>
    /// <param name="fecha">Fecha de la comanda</param>
    /// <param name="tipoComanda">Tipo de comanda</param>
    /// <param name="canalOrden">Canal de orden</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado con el número de comanda generado</returns>
    Task<Result<string>> GenerarNumeroConPrefijoAsync(string prefijo, Guid sucursalId, DateTime fecha, TipoComanda tipoComanda, CanalOrden canalOrden, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el formato de número de comanda para parámetros específicos
    /// </summary>
    /// <param name="sucursalId">ID de la sucursal</param>
    /// <param name="fecha">Fecha de la comanda</param>
    /// <param name="tipoComanda">Tipo de comanda</param>
    /// <param name="canalOrden">Canal de orden</param>
    /// <param name="numeroMesa">Número de mesa (opcional)</param>
    /// <param name="secuencial">Secuencial</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado con el formato del número</returns>
    Task<Result<string>> ObtenerFormatoNumeroAsync(Guid sucursalId, DateTime fecha, TipoComanda tipoComanda, CanalOrden canalOrden, int? numeroMesa, int secuencial, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si un número de comanda es válido según las reglas de negocio
    /// </summary>
    /// <param name="numeroComanda">Número de comanda a validar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si es válido</returns>
    Task<Result<bool>> ValidarNumeroComandaAsync(string numeroComanda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extrae información de un número de comanda
    /// </summary>
    /// <param name="numeroComanda">Número de comanda a analizar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información extraída del número de comanda</returns>
    Task<Result<InformacionComanda>> ExtraerInformacionNumeroAsync(string numeroComanda, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el siguiente número secuencial para una sucursal
    /// </summary>
    /// <param name="sucursalId">ID de la sucursal</param>
    /// <param name="fecha">Fecha</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Siguiente número secuencial</returns>
    Task<int> ObtenerSiguienteSecuencialAsync(Guid sucursalId, DateTime fecha, CancellationToken cancellationToken = default);
}

/// <summary>
/// Parámetros para la generación de números de comanda
/// </summary>
public class ParametrosGeneracionComanda
{
    /// <summary>
    /// ID de la sucursal
    /// </summary>
    public Guid SucursalId { get; set; }

    /// <summary>
    /// Fecha de la comanda
    /// </summary>
    public DateTime Fecha { get; set; } = DateTime.Now;

    /// <summary>
    /// Prefijo personalizado
    /// </summary>
    public string? Prefijo { get; set; }

    /// <summary>
    /// Longitud del número secuencial
    /// </summary>
    public int LongitudSecuencial { get; set; } = 4;

    /// <summary>
    /// Tipo de comanda
    /// </summary>
    public string? TipoComanda { get; set; }

    /// <summary>
    /// Canal de origen
    /// </summary>
    public string? Canal { get; set; }

    /// <summary>
    /// Mesa asociada (si aplica)
    /// </summary>
    public int? NumeroMesa { get; set; }
}

/// <summary>
/// Información extraída de un número de comanda
/// </summary>
public class InformacionComanda
{
    /// <summary>
    /// ID de la sucursal
    /// </summary>
    public string SucursalId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de la comanda
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Tipo de comanda
    /// </summary>
    public string TipoComanda { get; set; } = string.Empty;

    /// <summary>
    /// Número de mesa (si aplica)
    /// </summary>
    public int? NumeroMesa { get; set; }

    /// <summary>
    /// Canal de orden
    /// </summary>
    public string CanalOrden { get; set; } = string.Empty;

    /// <summary>
    /// Número secuencial
    /// </summary>
    public int Secuencial { get; set; }
} 