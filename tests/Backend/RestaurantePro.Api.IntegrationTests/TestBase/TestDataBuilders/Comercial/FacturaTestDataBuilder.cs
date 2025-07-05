using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Comercial;

/// <summary>
/// Builder para crear datos de prueba de facturas comerciales
/// </summary>
public class FacturaTestDataBuilder
{
    private List<Guid> _comandasIds = new() { Guid.NewGuid() };
    private string _tipoFactura = "Normal";
    private string? _nombreCliente;
    private Guid _clienteId = Guid.NewGuid();
    private string? _identificacionFiscal;
    private string? _direccionCliente;
    private string? _emailCliente;
    private string? _observaciones;
    private int _diasCredito = 30;
    private string _metodoPagoPreferido = "Efectivo";
    private string _moneda = "CLP";
    private DateTime? _fechaEmision;

    /// <summary>
    /// Genera un nombre de cliente único
    /// </summary>
    private string GenerarNombreClienteUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"Cliente_{guid.Substring(0, 8)}";
    }

    /// <summary>
    /// Genera un email de cliente único
    /// </summary>
    private string GenerarEmailClienteUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"cliente_{guid.Substring(0, 8)}@test.com";
    }

    /// <summary>
    /// Genera una identificación fiscal única (RUT chileno válido)
    /// </summary>
    private string GenerarIdentificacionFiscalUnica()
    {
        var random = new Random();
        // Generar un número de RUT entre 1000000 y 99999999
        var numeroRut = random.Next(1000000, 99999999);
        
        // Calcular el dígito verificador usando el algoritmo chileno
        var digitoVerificador = CalcularDigitoVerificadorChileno(numeroRut);
        
        return $"{numeroRut}-{digitoVerificador}";
    }
    
    /// <summary>
    /// Calcula el dígito verificador para un RUT chileno
    /// </summary>
    private static char CalcularDigitoVerificadorChileno(int numero)
    {
        int suma = 0;
        int multiplicador = 2;
        
        // Calcular la suma ponderada
        int tempNumero = numero;
        while (tempNumero > 0)
        {
            int digito = tempNumero % 10;
            suma += digito * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
            tempNumero /= 10;
        }
        
        // Calcular el dígito verificador
        int resto = suma % 11;
        int resultado = 11 - resto;
        
        if (resultado == 11)
            return '0';
        else if (resultado == 10)
            return 'K';
        else
            return (char)('0' + resultado);
    }

    /// <summary>
    /// Genera una dirección de cliente única
    /// </summary>
    private string GenerarDireccionClienteUnica()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"Dirección Test {guid.Substring(0, 4)}";
    }

    /// <summary>
    /// Genera observaciones únicas
    /// </summary>
    private string GenerarObservacionesUnicas()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"Factura de prueba {guid.Substring(0, 4)}";
    }

    public FacturaTestDataBuilder ConComandasIds(params Guid[] comandasIds)
    {
        _comandasIds = comandasIds.ToList();
        return this;
    }

    public FacturaTestDataBuilder ConTipoFactura(string tipo)
    {
        _tipoFactura = tipo;
        return this;
    }

    public FacturaTestDataBuilder ConNombreCliente(string nombre)
    {
        _nombreCliente = nombre;
        return this;
    }

    public FacturaTestDataBuilder ConClienteId(Guid clienteId)
    {
        _clienteId = clienteId;
        return this;
    }

    public FacturaTestDataBuilder ConIdentificacionFiscal(string identificacion)
    {
        _identificacionFiscal = identificacion;
        return this;
    }

    public FacturaTestDataBuilder ConDireccionCliente(string direccion)
    {
        _direccionCliente = direccion;
        return this;
    }

    public FacturaTestDataBuilder ConEmailCliente(string email)
    {
        _emailCliente = email;
        return this;
    }

    public FacturaTestDataBuilder ConObservaciones(string observaciones)
    {
        _observaciones = observaciones;
        return this;
    }

    public FacturaTestDataBuilder ConDiasCredito(int dias)
    {
        _diasCredito = dias;
        return this;
    }

    public FacturaTestDataBuilder ConMetodoPagoPreferido(string metodoPago)
    {
        _metodoPagoPreferido = metodoPago;
        return this;
    }

    public FacturaTestDataBuilder ConMoneda(string moneda)
    {
        _moneda = moneda;
        return this;
    }

    public FacturaTestDataBuilder ConFechaEmision(DateTime fecha)
    {
        _fechaEmision = fecha;
        return this;
    }

    /// <summary>
    /// Construye un request para crear una factura
    /// </summary>
    public CrearFacturaCommand BuildCrearFacturaRequest()
    {
        return new CrearFacturaCommand
        {
            ComandasIds = _comandasIds,
            TipoFactura = _tipoFactura,
            NombreCliente = _nombreCliente ?? GenerarNombreClienteUnico(),
            ClienteId = _clienteId,
            IdentificacionFiscal = _identificacionFiscal ?? GenerarIdentificacionFiscalUnica(),
            DireccionCliente = _direccionCliente ?? GenerarDireccionClienteUnica(),
            EmailCliente = _emailCliente ?? GenerarEmailClienteUnico(),
            Observaciones = _observaciones ?? GenerarObservacionesUnicas(),
            DiasCredito = _diasCredito,
            MetodoPagoPreferido = _metodoPagoPreferido,
            Moneda = _moneda,
            EmitirInmediatamente = true,
            EnviarPorEmail = false,
            DescuentosAdicionales = new List<DescuentoAdicionalDto>(),
            TipoCambio = null,
            FechaEmision = _fechaEmision ?? DateTime.Today
        };
    }
} 