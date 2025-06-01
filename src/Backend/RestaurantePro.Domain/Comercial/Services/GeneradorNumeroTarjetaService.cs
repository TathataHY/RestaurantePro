namespace RestaurantePro.Domain.Comercial.Services;

/// <summary>
/// Implementación del servicio de dominio para generar números únicos de tarjetas de fidelización
/// </summary>
public class GeneradorNumeroTarjetaService : IGeneradorNumeroTarjetaService
{
    private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
    private readonly ILogger<GeneradorNumeroTarjetaService> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public GeneradorNumeroTarjetaService(
        ITarjetaFidelizacionRepository tarjetaRepository,
        ILogger<GeneradorNumeroTarjetaService> logger)
    {
        _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Genera un número único para tarjeta de fidelización
    /// </summary>
    public async Task<string> GenerarNumeroTarjetaAsync(string tipoTarjeta, CancellationToken cancellationToken = default)
    {
        try
        {
            string numeroTarjeta;
            int intentos = 0;
            const int maxIntentos = 10;

            do
            {
                numeroTarjeta = GenerarNumeroConFormato(tipoTarjeta);
                intentos++;

                if (intentos > maxIntentos)
                {
                    _logger.LogWarning("Se alcanzó el máximo de intentos ({MaxIntentos}) generando número de tarjeta tipo {TipoTarjeta}", 
                        maxIntentos, tipoTarjeta);
                    throw new InvalidOperationException($"No se pudo generar un número único después de {maxIntentos} intentos");
                }

            } while (await ExisteNumeroTarjetaAsync(numeroTarjeta, cancellationToken));

            _logger.LogInformation("Número de tarjeta generado: {NumeroTarjeta} para tipo {TipoTarjeta}", 
                numeroTarjeta, tipoTarjeta);

            return numeroTarjeta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando número de tarjeta tipo {TipoTarjeta}", tipoTarjeta);
            throw;
        }
    }

    /// <summary>
    /// Valida si un número de tarjeta es válido según las reglas de negocio
    /// </summary>
    public async Task<bool> ValidarNumeroTarjetaAsync(string numeroTarjeta, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numeroTarjeta))
                return false;

            // Validar formato: debe tener 16 dígitos
            if (numeroTarjeta.Length != 16)
                return false;

            // Validar que solo contenga dígitos
            if (!numeroTarjeta.All(char.IsDigit))
                return false;

            // Validar algoritmo de Luhn (similar a tarjetas de crédito)
            if (!ValidarAlgoritmoLuhn(numeroTarjeta))
                return false;

            // Validar prefijo según tipo de tarjeta
            if (!ValidarPrefijo(numeroTarjeta))
                return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando número de tarjeta {NumeroTarjeta}", numeroTarjeta);
            return false;
        }
    }

    /// <summary>
    /// Verifica si un número ya existe
    /// </summary>
    public async Task<bool> ExisteNumeroTarjetaAsync(string numeroTarjeta, CancellationToken cancellationToken = default)
    {
        try
        {
            var tarjeta = await _tarjetaRepository.ObtenerPorNumeroAsync(numeroTarjeta, cancellationToken);
            return tarjeta != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando existencia de número de tarjeta {NumeroTarjeta}", numeroTarjeta);
            throw;
        }
    }

    /// <summary>
    /// Genera un número de tarjeta con formato específico
    /// </summary>
    public async Task<string> GenerarNumeroTarjetaConFormatoAsync(
        string tipoTarjeta, 
        Guid clienteId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            string numeroTarjeta;
            int intentos = 0;
            const int maxIntentos = 10;

            do
            {
                numeroTarjeta = GenerarNumeroConFormatoPersonalizado(tipoTarjeta, clienteId);
                intentos++;

                if (intentos > maxIntentos)
                {
                    _logger.LogWarning("Se alcanzó el máximo de intentos generando número personalizado para cliente {ClienteId}", clienteId);
                    // Fallback al método estándar
                    return await GenerarNumeroTarjetaAsync(tipoTarjeta, cancellationToken);
                }

            } while (await ExisteNumeroTarjetaAsync(numeroTarjeta, cancellationToken));

            _logger.LogInformation("Número de tarjeta personalizado generado: {NumeroTarjeta} para cliente {ClienteId}", 
                numeroTarjeta, clienteId);

            return numeroTarjeta;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando número personalizado para cliente {ClienteId}", clienteId);
            throw;
        }
    }

    /// <summary>
    /// Genera un número único para tarjeta de fidelización basado en el nivel
    /// </summary>
    public async Task<Result<string>> GenerarNumeroAsync(NivelFidelizacion nivel, Guid clienteId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tipoTarjeta = ConvertirNivelATipo(nivel);
            string numeroTarjeta;
            int intentos = 0;
            const int maxIntentos = 10;
            bool existe;

            do
            {
                numeroTarjeta = GenerarNumeroConFormato(tipoTarjeta);
                intentos++;
                existe = await _tarjetaRepository.ExisteNumeroTarjetaAsync(numeroTarjeta, cancellationToken);

                // Si después de este intento aún existe y hemos alcanzado el máximo, fallar
                if (existe && intentos >= maxIntentos)
                {
                    _logger.LogWarning("Se alcanzó el máximo de intentos ({MaxIntentos}) generando número para nivel {Nivel} y cliente {ClienteId}", 
                        maxIntentos, nivel, clienteId);
                    return Result.Failure<string>("No se pudo generar un número único después de múltiples intentos");
                }

            } while (existe);

            _logger.LogInformation("Número de tarjeta generado: {NumeroTarjeta} para nivel {Nivel} y cliente {ClienteId} en {Intentos} intentos", 
                numeroTarjeta, nivel, clienteId, intentos);

            return Result.Success(numeroTarjeta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando número para nivel {Nivel} y cliente {ClienteId}", nivel, clienteId);
            return Result.Failure<string>($"Error interno generando número de tarjeta: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida un número de tarjeta usando el algoritmo de Luhn
    /// </summary>
    public async Task<Result<bool>> ValidarNumeroLuhnAsync(string numeroTarjeta, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numeroTarjeta))
                return Result.Failure<bool>("El número de tarjeta no puede estar vacío");

            if (numeroTarjeta.Length < 13 || numeroTarjeta.Length > 19)
                return Result.Failure<bool>("El número de tarjeta tiene formato inválido");

            if (!numeroTarjeta.All(char.IsDigit))
                return Result.Failure<bool>("El número de tarjeta debe contener solo dígitos");

            var esValido = ValidarAlgoritmoLuhn(numeroTarjeta);
            return Result.Success(esValido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando número con algoritmo Luhn {NumeroTarjeta}", numeroTarjeta);
            return Result.Failure<bool>($"Error validando número: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el prefijo correspondiente a un nivel de fidelización
    /// </summary>
    public async Task<Result<string>> ObtenerPrefijoByNivelAsync(NivelFidelizacion nivel, CancellationToken cancellationToken = default)
    {
        try
        {
            var tipoTarjeta = ConvertirNivelATipo(nivel);
            var prefijo = ObtenerPrefijoPorTipo(tipoTarjeta);
            return Result.Success(prefijo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo prefijo para nivel {Nivel}", nivel);
            return Result.Failure<string>($"Error obteniendo prefijo: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera un número de tarjeta personalizado con prefijo y sufijo específicos
    /// </summary>
    public async Task<Result<string>> GenerarNumeroPersonalizadoAsync(Guid clienteId, string prefijo, string sufijo, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar parámetros - primero validar si están vacíos
            if (string.IsNullOrWhiteSpace(prefijo) || string.IsNullOrWhiteSpace(sufijo))
                return Result.Failure<string>("Prefijo y sufijo no pueden estar vacíos");

            // Luego validar si son numéricos
            if (!prefijo.All(char.IsDigit) || !sufijo.All(char.IsDigit))
                return Result.Failure<string>("Prefijo y sufijo deben ser numéricos");

            if (prefijo.Length + sufijo.Length >= 16)
                return Result.Failure<string>("Prefijo y sufijo son demasiado largos");

            string numeroTarjeta;
            int intentos = 0;
            const int maxIntentos = 10;

            do
            {
                numeroTarjeta = GenerarNumeroPersonalizado(prefijo, sufijo, clienteId);
                intentos++;

                if (intentos >= maxIntentos)
                {
                    _logger.LogWarning("Se alcanzó el máximo de intentos generando número personalizado para cliente {ClienteId}", clienteId);
                    return Result.Failure<string>("No se pudo generar un número único después de múltiples intentos");
                }

            } while (await _tarjetaRepository.ExisteNumeroTarjetaAsync(numeroTarjeta, cancellationToken));

            _logger.LogInformation("Número personalizado generado: {NumeroTarjeta} para cliente {ClienteId}", 
                numeroTarjeta, clienteId);

            return Result.Success(numeroTarjeta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando número personalizado para cliente {ClienteId}", clienteId);
            return Result.Failure<string>($"Error generando número personalizado: {ex.Message}");
        }
    }

    #region Métodos Privados de Lógica de Negocio

    /// <summary>
    /// Genera un número con formato estándar
    /// </summary>
    private string GenerarNumeroConFormato(string tipoTarjeta)
    {
        var prefijo = ObtenerPrefijoPorTipo(tipoTarjeta);
        var random = new Random();
        
        // Generar 11 dígitos aleatorios (16 total - 4 del prefijo - 1 de verificación)
        var digitosAleatorios = string.Join("", Enumerable.Range(0, 11)
            .Select(_ => random.Next(0, 10).ToString()));
        
        var numeroSinChecksum = prefijo + digitosAleatorios;
        
        // Calcular dígito de verificación usando algoritmo de Luhn
        var digitoVerificacion = CalcularDigitoVerificacionLuhn(numeroSinChecksum);
        
        return numeroSinChecksum + digitoVerificacion;
    }

    /// <summary>
    /// Genera un número con formato personalizado basado en el cliente
    /// </summary>
    private string GenerarNumeroConFormatoPersonalizado(string tipoTarjeta, Guid clienteId)
    {
        var prefijo = ObtenerPrefijoPorTipo(tipoTarjeta);
        
        // Usar parte del GUID del cliente para personalizar
        var clienteHash = Math.Abs(clienteId.GetHashCode());
        var clientePart = (clienteHash % 100000).ToString("D5"); // 5 dígitos del cliente
        
        var random = new Random(clienteHash); // Usar hash como semilla para consistencia
        var digitosAleatorios = string.Join("", Enumerable.Range(0, 6)
            .Select(_ => random.Next(0, 10).ToString()));
        
        var numeroSinChecksum = prefijo + clientePart + digitosAleatorios;
        var digitoVerificacion = CalcularDigitoVerificacionLuhn(numeroSinChecksum);
        
        return numeroSinChecksum + digitoVerificacion;
    }

    /// <summary>
    /// Obtiene el prefijo según el tipo de tarjeta
    /// </summary>
    private string ObtenerPrefijoPorTipo(string tipoTarjeta)
    {
        return tipoTarjeta?.ToUpperInvariant() switch
        {
            "BRONCE" => "4001",
            "PLATA" => "4002", 
            "ORO" => "4003",
            "PLATINO" => "4004",
            _ => "4000" // Prefijo por defecto
        };
    }

    /// <summary>
    /// Valida el prefijo del número de tarjeta
    /// </summary>
    private bool ValidarPrefijo(string numeroTarjeta)
    {
        if (numeroTarjeta.Length < 4)
            return false;

        var prefijo = numeroTarjeta.Substring(0, 4);
        
        // Validar que el prefijo corresponda a nuestros tipos de tarjeta
        return prefijo is "4000" or "4001" or "4002" or "4003" or "4004";
    }

    /// <summary>
    /// Valida el número usando el algoritmo de Luhn
    /// </summary>
    private bool ValidarAlgoritmoLuhn(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            return false;

        var suma = 0;
        var alternar = false;

        // Procesar dígitos de derecha a izquierda
        for (int i = numero.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(numero[i]))
                return false;

            var digito = numero[i] - '0';

            if (alternar)
            {
                digito *= 2;
                if (digito > 9)
                    digito = digito / 10 + digito % 10;
            }

            suma += digito;
            alternar = !alternar;
        }

        return suma % 10 == 0;
    }

    /// <summary>
    /// Calcula el dígito de verificación usando algoritmo de Luhn
    /// </summary>
    private string CalcularDigitoVerificacionLuhn(string numeroSinChecksum)
    {
        var suma = 0;
        var alternar = true; // Empezamos alternando porque el checksum será el último dígito

        // Procesar dígitos de derecha a izquierda
        for (int i = numeroSinChecksum.Length - 1; i >= 0; i--)
        {
            var digito = numeroSinChecksum[i] - '0';

            if (alternar)
            {
                digito *= 2;
                if (digito > 9)
                    digito = digito / 10 + digito % 10;
            }

            suma += digito;
            alternar = !alternar;
        }

        var checksum = (10 - (suma % 10)) % 10;
        return checksum.ToString();
    }

    /// <summary>
    /// Convierte un nivel de fidelización a tipo de tarjeta
    /// </summary>
    private string ConvertirNivelATipo(NivelFidelizacion nivel)
    {
        return nivel switch
        {
            NivelFidelizacion.Basico => "BRONCE",
            NivelFidelizacion.Plata => "PLATA",
            NivelFidelizacion.Oro => "ORO",
            NivelFidelizacion.Platino => "PLATINO",
            _ => "BRONCE"
        };
    }

    /// <summary>
    /// Genera un número personalizado con prefijo y sufijo específicos
    /// </summary>
    private string GenerarNumeroPersonalizado(string prefijo, string sufijo, Guid clienteId)
    {
        const int longitudTotal = 16;
        const int longitudDigitoVerificacion = 1;
        var longitudDisponible = longitudTotal - prefijo.Length - sufijo.Length - longitudDigitoVerificacion;
        
        // Garantizar que tengamos al menos espacio para algunos dígitos aleatorios
        if (longitudDisponible <= 0)
        {
            // Si prefijo y sufijo son muy largos, ajustar para que quepan en 15 posiciones + verificación
            var longitudMaximaSinVerificacion = longitudTotal - longitudDigitoVerificacion;
            var prefijoTruncado = prefijo.Length > 8 ? prefijo.Substring(0, 8) : prefijo;
            var sufijoTruncado = sufijo.Length > 4 ? sufijo.Substring(0, 4) : sufijo;
            longitudDisponible = longitudMaximaSinVerificacion - prefijoTruncado.Length - sufijoTruncado.Length;
            
            if (longitudDisponible <= 0)
                longitudDisponible = 2; // Mínimo 2 dígitos aleatorios
                
            prefijo = prefijoTruncado;
            sufijo = sufijoTruncado;
        }
        
        var random = new Random(clienteId.GetHashCode());
        var digitosAleatorios = string.Join("", Enumerable.Range(0, longitudDisponible)
            .Select(_ => random.Next(0, 10).ToString()));
        
        // Construir el número con el sufijo antes del dígito de verificación: prefijo + aleatorios + sufijo
        var numeroSinChecksum = prefijo + digitosAleatorios + sufijo;
        
        // Asegurar que el número sea exactamente de 15 dígitos (16 - 1 de verificación)
        var longitudEsperada = longitudTotal - longitudDigitoVerificacion;
        if (numeroSinChecksum.Length > longitudEsperada)
        {
            // Si es muy largo, truncar los dígitos aleatorios para mantener prefijo y sufijo
            var longitudAleatoriosPermitida = longitudEsperada - prefijo.Length - sufijo.Length;
            if (longitudAleatoriosPermitida > 0)
            {
                var digitosAleatoriosTruncados = digitosAleatorios.Substring(0, longitudAleatoriosPermitida);
                numeroSinChecksum = prefijo + digitosAleatoriosTruncados + sufijo;
            }
            else
            {
                // En caso extremo, usar solo prefijo + sufijo y rellenar con ceros
                var longitudRelleno = longitudEsperada - prefijo.Length - sufijo.Length;
                var relleno = new string('0', Math.Max(0, longitudRelleno));
                numeroSinChecksum = prefijo + relleno + sufijo;
            }
        }
        else if (numeroSinChecksum.Length < longitudEsperada)
        {
            // Si es muy corto, agregar ceros al final (antes del sufijo)
            var longitudFaltante = longitudEsperada - numeroSinChecksum.Length;
            var relleno = new string('0', longitudFaltante);
            // Insertar el relleno antes del sufijo
            numeroSinChecksum = prefijo + digitosAleatorios + relleno + sufijo;
        }
        
        var digitoVerificacion = CalcularDigitoVerificacionLuhn(numeroSinChecksum);
        return numeroSinChecksum + digitoVerificacion;
    }

    #endregion
} 