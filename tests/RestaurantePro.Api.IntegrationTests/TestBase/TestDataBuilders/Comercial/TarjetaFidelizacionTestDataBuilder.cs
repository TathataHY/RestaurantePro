namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Comercial;

/// <summary>
/// Builder para crear datos de prueba de tarjetas de fidelización
/// </summary>
public class TarjetaFidelizacionTestDataBuilder
{
    private Guid _clienteId = Guid.NewGuid();
    private string _tipoTarjeta = "Estandar";
    private int _puntosIniciales = 0;
    private bool _activarInmediatamente = true;
    private decimal _multiplicadorPuntos = 1.0m;
    private int _limiteMensual = 1000;
    private string _numeroTarjeta = "TF-001-001";

    public TarjetaFidelizacionTestDataBuilder ConClienteId(Guid clienteId)
    {
        _clienteId = clienteId;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConTipoTarjeta(string tipoTarjeta)
    {
        _tipoTarjeta = tipoTarjeta;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConPuntosIniciales(int puntosIniciales)
    {
        _puntosIniciales = puntosIniciales;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConActivarInmediatamente(bool activarInmediatamente)
    {
        _activarInmediatamente = activarInmediatamente;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConMultiplicadorPuntos(decimal multiplicadorPuntos)
    {
        _multiplicadorPuntos = multiplicadorPuntos;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConLimiteMensual(int limiteMensual)
    {
        _limiteMensual = limiteMensual;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConNumeroTarjeta(string numeroTarjeta)
    {
        _numeroTarjeta = numeroTarjeta;
        return this;
    }

    public object BuildCrearTarjetaRequest() => new
    {
        ClienteId = _clienteId,
        TipoTarjeta = _tipoTarjeta,
        PuntosIniciales = _puntosIniciales,
        ActivarInmediatamente = _activarInmediatamente
    };

    public object BuildActualizarTarjetaRequest() => new
    {
        TipoTarjeta = _tipoTarjeta,
        MultiplicadorPuntos = _multiplicadorPuntos,
        LimiteMensual = _limiteMensual
    };

    public object BuildAgregarPuntosRequest() => new
    {
        Puntos = 250,
        Motivo = "Compra en restaurante",
        ComandaId = Guid.NewGuid(),
        MontoCompra = 125.50m
    };

    public object BuildCanjearPuntosRequest() => new
    {
        PuntosACanjear = 500,
        TipoCanje = "Descuento",
        MontoDescuento = 25.00m,
        Observaciones = "Canje por descuento en comanda"
    };
} 