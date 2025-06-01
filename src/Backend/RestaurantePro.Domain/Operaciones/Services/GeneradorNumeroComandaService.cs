namespace RestaurantePro.Domain.Operaciones.Services;

/// <summary>
/// Implementación del servicio de dominio para generar números únicos de comandas
/// </summary>
public class GeneradorNumeroComandaService : IGeneradorNumeroComandaService
{
    private readonly IComandaRepository _comandaRepository;
    private readonly ILogger<GeneradorNumeroComandaService> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public GeneradorNumeroComandaService(
        IComandaRepository comandaRepository,
        ILogger<GeneradorNumeroComandaService> logger)
    {
        _comandaRepository = comandaRepository ?? throw new ArgumentNullException(nameof(comandaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Genera un nuevo número de comanda único
    /// </summary>
    public async Task<string> GenerarNumeroComandaAsync(
        Guid sucursalId, 
        DateTime fecha, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var parametros = new ParametrosGeneracionComanda
            {
                SucursalId = sucursalId,
                Fecha = fecha,
                LongitudSecuencial = 4
            };

            return await GenerarNumeroComandaAsync(parametros, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando número de comanda para sucursal {SucursalId}", sucursalId);
            throw;
        }
    }

    /// <summary>
    /// Genera un número de comanda con formato personalizado
    /// </summary>
    public async Task<string> GenerarNumeroComandaAsync(
        ParametrosGeneracionComanda parametros, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (parametros == null)
                throw new ArgumentNullException(nameof(parametros));

            var siguienteSecuencial = await ObtenerSiguienteSecuencialAsync(
                parametros.SucursalId, 
                parametros.Fecha, 
                cancellationToken);

            var numeroComanda = ConstruirNumeroComanda(parametros, siguienteSecuencial);

            // Validar que el número generado no exista (por seguridad)
            var existe = await _comandaRepository.ExisteNumeroComandaAsync(numeroComanda, cancellationToken);
            if (existe)
            {
                _logger.LogWarning("El número de comanda {NumeroComanda} ya existe, generando nuevo número", numeroComanda);
                
                // Incrementar secuencial y volver a intentar
                siguienteSecuencial++;
                numeroComanda = ConstruirNumeroComanda(parametros, siguienteSecuencial);
            }

            _logger.LogInformation("Número de comanda generado: {NumeroComanda} para sucursal {SucursalId}", 
                numeroComanda, parametros.SucursalId);

            return numeroComanda;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando número de comanda con parámetros personalizados");
            throw;
        }
    }

    /// <summary>
    /// Valida si un número de comanda es válido según las reglas de negocio
    /// </summary>
    public async Task<bool> ValidarNumeroComandaAsync(string numeroComanda, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numeroComanda))
                return false;

            // Validar longitud mínima
            if (numeroComanda.Length < 8)
                return false;

            // Validar formato general: debe contener al menos fecha y secuencial
            if (!ValidarFormatoGeneral(numeroComanda))
                return false;

            // Validar que no sea un número duplicado
            var existe = await _comandaRepository.ExisteNumeroComandaAsync(numeroComanda, cancellationToken);
            if (existe)
            {
                _logger.LogWarning("Número de comanda {NumeroComanda} ya existe en el sistema", numeroComanda);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando número de comanda {NumeroComanda}", numeroComanda);
            return false;
        }
    }

    /// <summary>
    /// Obtiene el siguiente número secuencial para una sucursal
    /// </summary>
    public async Task<int> ObtenerSiguienteSecuencialAsync(
        Guid sucursalId, 
        DateTime fecha, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Obtener el último número secuencial del día para la sucursal
            var ultimoSecuencial = await _comandaRepository.ObtenerUltimoSecuencialDelDiaAsync(
                sucursalId, 
                fecha.Date, 
                cancellationToken);

            var siguienteSecuencial = ultimoSecuencial + 1;

            _logger.LogDebug("Siguiente secuencial para sucursal {SucursalId} en fecha {Fecha}: {Secuencial}", 
                sucursalId, fecha.Date, siguienteSecuencial);

            return siguienteSecuencial;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo siguiente secuencial para sucursal {SucursalId}", sucursalId);
            throw;
        }
    }

    #region Métodos Privados de Lógica de Negocio

    /// <summary>
    /// Construye el número de comanda según los parámetros
    /// </summary>
    private string ConstruirNumeroComanda(ParametrosGeneracionComanda parametros, int secuencial)
    {
        var builder = new StringBuilder();

        // Agregar prefijo personalizado si existe
        if (!string.IsNullOrWhiteSpace(parametros.Prefijo))
        {
            builder.Append(parametros.Prefijo.ToUpperInvariant());
            builder.Append("-");
        }

        // Agregar código de sucursal (primeros 2 caracteres del GUID)
        var codigoSucursal = parametros.SucursalId.ToString("N")[..2].ToUpperInvariant();
        builder.Append(codigoSucursal);

        // Agregar fecha en formato YYMMDD
        builder.Append(parametros.Fecha.ToString("yyMMdd"));

        // Agregar tipo de comanda si existe
        if (!string.IsNullOrWhiteSpace(parametros.TipoComanda))
        {
            var tipoAbreviado = ObtenerAbreviaturaTipoComanda(parametros.TipoComanda);
            builder.Append(tipoAbreviado);
        }

        // Agregar número de mesa si existe
        if (parametros.NumeroMesa.HasValue)
        {
            builder.Append($"M{parametros.NumeroMesa.Value:D2}");
        }

        // Agregar canal si existe
        if (!string.IsNullOrWhiteSpace(parametros.Canal))
        {
            var canalAbreviado = ObtenerAbreviaturaCanal(parametros.Canal);
            builder.Append(canalAbreviado);
        }

        // Agregar separador
        builder.Append("-");

        // Agregar secuencial con padding
        var formatoSecuencial = new string('0', parametros.LongitudSecuencial);
        builder.Append(secuencial.ToString(formatoSecuencial));

        return builder.ToString();
    }

    /// <summary>
    /// Obtiene la abreviatura del tipo de comanda
    /// </summary>
    private string ObtenerAbreviaturaTipoComanda(string tipoComanda)
    {
        return tipoComanda?.ToUpperInvariant() switch
        {
            "DELIVERY" => "DLV",
            "TAKEAWAY" => "TKW",
            "MESA" => "MSA",
            "BUFFET" => "BFT",
            "EVENTO" => "EVT",
            _ => "GEN" // General
        };
    }

    /// <summary>
    /// Obtiene la abreviatura del canal
    /// </summary>
    private string ObtenerAbreviaturaCanal(string canal)
    {
        return canal?.ToUpperInvariant() switch
        {
            "WEB" => "W",
            "MOVIL" => "M",
            "TELEFONO" => "T",
            "PRESENCIAL" => "P",
            "WHATSAPP" => "WA",
            _ => "O" // Otros
        };
    }

    /// <summary>
    /// Valida el formato general del número de comanda
    /// </summary>
    private bool ValidarFormatoGeneral(string numeroComanda)
    {
        try
        {
            // Debe contener al menos un guión separador
            if (!numeroComanda.Contains('-'))
                return false;

            var partes = numeroComanda.Split('-');
            
            // Debe tener al menos 2 partes (identificador y secuencial)
            if (partes.Length < 2)
                return false;

            // La última parte debe ser numérica (secuencial)
            var secuencialParte = partes[^1];
            if (!int.TryParse(secuencialParte, out _))
                return false;

            // La parte principal debe contener fecha válida
            var parteIdentificador = partes[0];
            if (parteIdentificador.Length < 8) // Mínimo para código sucursal + fecha
                return false;

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Extrae la fecha del número de comanda
    /// </summary>
    private DateTime? ExtraerFechaDeNumero(string numeroComanda)
    {
        try
        {
            var partes = numeroComanda.Split('-');
            if (partes.Length < 1)
                return null;

            var parteIdentificador = partes[0];
            
            // Buscar patrón de fecha YYMMDD (6 dígitos consecutivos)
            for (int i = 0; i <= parteIdentificador.Length - 6; i++)
            {
                var posibleFecha = parteIdentificador.Substring(i, 6);
                if (posibleFecha.All(char.IsDigit))
                {
                    var año = int.Parse("20" + posibleFecha.Substring(0, 2));
                    var mes = int.Parse(posibleFecha.Substring(2, 2));
                    var dia = int.Parse(posibleFecha.Substring(4, 2));

                    if (mes >= 1 && mes <= 12 && dia >= 1 && dia <= 31)
                    {
                        try
                        {
                            return new DateTime(año, mes, dia);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            continue;
                        }
                    }
                }
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    #endregion
} 