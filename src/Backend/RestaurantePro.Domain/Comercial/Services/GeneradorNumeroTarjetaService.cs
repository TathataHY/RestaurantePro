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

    #region Métodos Privados de Lógica de Negocio

    /// <summary>
    /// Genera un número con formato estándar
    /// </summary>
    private string GenerarNumeroConFormato(string tipoTarjeta)
    {
        var prefijo = ObtenerPrefijoPorTipo(tipoTarjeta);
        var random = new Random();
        
        // Generar 12 dígitos aleatorios (16 total - 4 del prefijo)
        var digitosAleatorios = string.Join("", Enumerable.Range(0, 12)
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
        var digitosAleatorios = string.Join("", Enumerable.Range(0, 7)
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

    #endregion
} 