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
    public async Task<Result<bool>> ValidarNumeroComandaAsync(string numeroComanda, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numeroComanda))
                return Result.Success(false);

            // Validar longitud mínima
            if (numeroComanda.Length < 8)
                return Result.Success(false);

            // Validar formato general: debe contener al menos fecha y secuencial
            if (!ValidarFormatoGeneral(numeroComanda))
                return Result.Success(false);

            // Validar que no sea un número duplicado
            var existe = await _comandaRepository.ExisteNumeroComandaAsync(numeroComanda, cancellationToken);
            if (existe)
            {
                _logger.LogWarning("Número de comanda {NumeroComanda} ya existe en el sistema", numeroComanda);
                return Result.Success(false);
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando número de comanda {NumeroComanda}", numeroComanda);
            return Result.Failure<bool>($"Error validando número de comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Extrae información de un número de comanda
    /// </summary>
    public async Task<Result<InformacionComanda>> ExtraerInformacionNumeroAsync(string numeroComanda, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numeroComanda))
                return Result.Failure<InformacionComanda>("Número de comanda no puede estar vacío");

            var partes = numeroComanda.Split('-');
            if (partes.Length < 5 || partes.Length > 6)
                return Result.Failure<InformacionComanda>("Formato de número de comanda inválido");

            var informacion = new InformacionComanda();

            // Extraer código de sucursal (primera parte)
            informacion.SucursalId = partes[0];

            // Extraer fecha (segunda parte - YYYYMMDD)
            var fechaStr = partes[1];
            if (DateTime.TryParseExact(fechaStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var fecha))
            {
                informacion.Fecha = fecha;
            }
            else
            {
                return Result.Failure<InformacionComanda>("Formato de fecha inválido en número de comanda");
            }

            // Extraer tipo de comanda (tercera parte)
            informacion.TipoComanda = partes[2];

            // Determinar si tiene mesa o no según la cantidad de partes
            if (partes.Length == 6)
            {
                // Formato: código-fecha-tipo-mesa-canal-secuencial
                if (int.TryParse(partes[3], out var numeroMesa))
                {
                    informacion.NumeroMesa = numeroMesa;
                }
                informacion.CanalOrden = partes[4];
                if (int.TryParse(partes[5], out var secuencial))
                {
                    informacion.Secuencial = secuencial;
                }
            }
            else
            {
                // Formato: código-fecha-tipo-canal-secuencial
                informacion.NumeroMesa = null;
                informacion.CanalOrden = partes[3];
                if (int.TryParse(partes[4], out var secuencial))
                {
                    informacion.Secuencial = secuencial;
                }
            }

            return Result.Success(informacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extrayendo información del número de comanda {NumeroComanda}", numeroComanda);
            return Result.Failure<InformacionComanda>($"Error extrayendo información: {ex.Message}");
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
                fecha.Date, 
                cancellationToken);

            var siguienteSecuencial = (ultimoSecuencial ?? 0) + 1;

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

    /// <summary>
    /// Genera un número de comanda con tipo y canal específicos
    /// </summary>
    public async Task<Result<string>> GenerarNumeroAsync(
        Guid sucursalId, 
        DateTime fecha, 
        TipoComanda tipoComanda, 
        CanalOrden canalOrden, 
        int? numeroMesa = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ultimoSecuencial = await _comandaRepository.ObtenerUltimoSecuencialAsync(sucursalId, fecha, cancellationToken);
            var secuencial = ultimoSecuencial + 1;

            var numeroComanda = ConstruirNumeroComanda(sucursalId, fecha, tipoComanda, canalOrden, numeroMesa, secuencial);

            _logger.LogInformation("Número de comanda generado: {NumeroComanda} para sucursal {SucursalId}", 
                numeroComanda, sucursalId);

            return Result<string>.Success(numeroComanda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando número de comanda para sucursal {SucursalId}", sucursalId);
            return Result.Failure<string>($"Error generando número de comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera un número de comanda con prefijo personalizado
    /// </summary>
    public async Task<Result<string>> GenerarNumeroConPrefijoAsync(
        string prefijo, 
        Guid sucursalId, 
        DateTime fecha, 
        TipoComanda tipoComanda, 
        CanalOrden canalOrden, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(prefijo))
            {
                return Result.Failure<string>("Prefijo no puede estar vacío");
            }

            var ultimoSecuencial = await _comandaRepository.ObtenerUltimoSecuencialAsync(sucursalId, fecha, cancellationToken);
            var secuencial = ultimoSecuencial + 1;

            var numeroComanda = ConstruirNumeroComandaConPrefijo(prefijo, sucursalId, fecha, tipoComanda, canalOrden, secuencial);

            _logger.LogInformation("Número de comanda con prefijo generado: {NumeroComanda} para sucursal {SucursalId}", 
                numeroComanda, sucursalId);

            return Result<string>.Success(numeroComanda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando número de comanda con prefijo para sucursal {SucursalId}", sucursalId);
            return Result.Failure<string>($"Error generando número de comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el formato de número de comanda para parámetros específicos
    /// </summary>
    public async Task<Result<string>> ObtenerFormatoNumeroAsync(
        Guid sucursalId, 
        DateTime fecha, 
        TipoComanda tipoComanda, 
        CanalOrden canalOrden, 
        int? numeroMesa, 
        int secuencial, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var numeroComanda = ConstruirNumeroComanda(sucursalId, fecha, tipoComanda, canalOrden, numeroMesa, secuencial);
            return Result<string>.Success(numeroComanda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo formato de número de comanda");
            return Result.Failure<string>($"Error obteniendo formato: {ex.Message}");
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
    /// Construye el número de comanda usando enums
    /// </summary>
    private string ConstruirNumeroComanda(
        Guid sucursalId, 
        DateTime fecha, 
        TipoComanda tipoComanda, 
        CanalOrden canalOrden, 
        int? numeroMesa, 
        int secuencial)
    {
        var builder = new StringBuilder();

        // Agregar código de sucursal (primeros 8 caracteres del GUID)
        var codigoSucursal = sucursalId.ToString("N")[..8].ToUpperInvariant();
        builder.Append(codigoSucursal);

        // Separador
        builder.Append("-");

        // Agregar fecha en formato YYYYMMDD  
        builder.Append(fecha.ToString("yyyyMMdd"));

        // Separador
        builder.Append("-");

        // Agregar tipo de comanda
        var tipoAbreviado = ObtenerAbreviaturaTipoComanda(tipoComanda);
        builder.Append(tipoAbreviado);

        // Agregar número de mesa si existe
        if (numeroMesa.HasValue)
        {
            builder.Append("-");
            if (numeroMesa.Value < 100)
            {
                builder.Append($"{numeroMesa.Value:D3}");
            }
            else
            {
                builder.Append(numeroMesa.Value.ToString().PadLeft(3, '0'));
            }
        }

        // Separador
        builder.Append("-");

        // Agregar canal
        var canalAbreviado = ObtenerAbreviaturaCanal(canalOrden);
        builder.Append(canalAbreviado);

        // Separador
        builder.Append("-");

        // Agregar secuencial con padding de 4 dígitos
        builder.Append(secuencial.ToString("D4"));

        return builder.ToString();
    }

    /// <summary>
    /// Construye el número de comanda con prefijo personalizado
    /// </summary>
    private string ConstruirNumeroComandaConPrefijo(
        string prefijo,
        Guid sucursalId, 
        DateTime fecha, 
        TipoComanda tipoComanda, 
        CanalOrden canalOrden, 
        int secuencial)
    {
        var builder = new StringBuilder();

        // Agregar prefijo
        builder.Append(prefijo.ToUpperInvariant());
        builder.Append("-");

        // Agregar código de sucursal (primeros 8 caracteres del GUID)
        var codigoSucursal = sucursalId.ToString("N")[..8].ToUpperInvariant();
        builder.Append(codigoSucursal);

        // Agregar fecha en formato YYYYMMDD
        builder.Append(fecha.ToString("yyyyMMdd"));

        // Agregar tipo de comanda
        var tipoAbreviado = ObtenerAbreviaturaTipoComanda(tipoComanda);
        builder.Append(tipoAbreviado);

        // Agregar canal
        var canalAbreviado = ObtenerAbreviaturaCanal(canalOrden);
        builder.Append(canalAbreviado);

        // Agregar separador
        builder.Append("-");

        // Agregar secuencial con padding de 4 dígitos
        builder.Append(secuencial.ToString("D4"));

        return builder.ToString();
    }

    /// <summary>
    /// Obtiene la abreviatura del tipo de comanda (enum)
    /// </summary>
    private string ObtenerAbreviaturaTipoComanda(TipoComanda tipoComanda)
    {
        return tipoComanda switch
        {
            TipoComanda.Delivery => "DLV",
            TipoComanda.TakeAway => "TKW", 
            TipoComanda.Mesa => "MSA",
            _ => "GEN"
        };
    }

    /// <summary>
    /// Obtiene la abreviatura del canal (enum)
    /// </summary>
    private string ObtenerAbreviaturaCanal(CanalOrden canalOrden)
    {
        return canalOrden switch
        {
            CanalOrden.Web => "W",
            CanalOrden.Movil => "M",
            CanalOrden.Telefono => "T",
            CanalOrden.Presencial => "P",
            _ => "O"
        };
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
            
            // Debe tener entre 5 y 6 partes: código-fecha-tipo-[mesa]-canal-secuencial
            if (partes.Length < 5 || partes.Length > 6)
                return false;

            // Validar código de sucursal (8 caracteres hexadecimales)
            var codigoSucursal = partes[0];
            if (codigoSucursal.Length != 8 || !codigoSucursal.All(c => char.IsLetterOrDigit(c)))
                return false;

            // Validar fecha (8 caracteres YYYYMMDD)
            var fechaStr = partes[1];
            if (fechaStr.Length != 8 || !DateTime.TryParseExact(fechaStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out _))
                return false;

            // Validar tipo de comanda (3 caracteres)
            var tipoComanda = partes[2];
            if (!EsTipoComandaValido(tipoComanda))
                return false;

            // Determinar si tiene mesa o no
            string canal, secuencial;
            if (partes.Length == 6)
            {
                // Formato: código-fecha-tipo-mesa-canal-secuencial
                var mesa = partes[3];
                if (!int.TryParse(mesa, out _))
                    return false;
                
                canal = partes[4];
                secuencial = partes[5];
            }
            else
            {
                // Formato: código-fecha-tipo-canal-secuencial
                canal = partes[3];
                secuencial = partes[4];
            }

            // Validar canal
            if (!EsCanalValido(canal))
                return false;

            // Validar secuencial (debe ser numérico)
            if (!int.TryParse(secuencial, out _))
                return false;

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Valida si el tipo de comanda es válido
    /// </summary>
    private bool EsTipoComandaValido(string tipoComanda)
    {
        return tipoComanda switch
        {
            "DLV" => true,  // Delivery
            "TKW" => true,  // TakeAway  
            "MSA" => true,  // Mesa
            "GEN" => true,  // General
            _ => false
        };
    }

    /// <summary>
    /// Valida si el canal es válido
    /// </summary>
    private bool EsCanalValido(string canal)
    {
        return canal switch
        {
            "W" => true,   // Web
            "M" => true,   // Móvil
            "T" => true,   // Teléfono
            "P" => true,   // Presencial
            "O" => true,   // Otros
            _ => false
        };
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