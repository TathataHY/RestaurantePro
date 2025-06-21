namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Comercial;

/// <summary>
/// Builder para crear datos de prueba de facturas comerciales
/// </summary>
public class FacturaTestDataBuilder
{
    private List<Guid> _comandasIds = new();
    private string _tipoFactura = "Normal";
    private string _nombreCliente = "Cliente Test";
    private Guid _clienteId = Guid.NewGuid();
    private string _identificacionFiscal = "12345678-9";
    private string _direccionCliente = "Dirección Test 123";
    private string _emailCliente = "cliente@test.com";
    private string _observaciones = "Factura de prueba";
    private int _diasCredito = 30;

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

    /// <summary>
    /// Construye un request para crear una factura
    /// </summary>
    public object BuildCrearFacturaRequest()
    {
        return new
        {
            ComandasIds = _comandasIds,
            TipoFactura = _tipoFactura,
            NombreCliente = _nombreCliente,
            ClienteId = _clienteId,
            IdentificacionFiscal = _identificacionFiscal,
            DireccionCliente = _direccionCliente,
            EmailCliente = _emailCliente,
            Observaciones = _observaciones,
            DiasCredito = _diasCredito
        };
    }
} 