using RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;
using RestaurantePro.Application.Comercial.Fidelizacion.Commands.ActualizarTarjetaFidelizacion;
using RestaurantePro.Domain.Comercial.Clientes.Enums;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Comercial;

/// <summary>
/// Builder para crear datos de prueba de tarjetas de fidelización
/// </summary>
public class TarjetaFidelizacionTestDataBuilder
{
    private Guid _clienteId = Guid.NewGuid();
    private TipoTarjetaFidelizacion _tipoTarjeta = TipoTarjetaFidelizacion.Estandar;
    private int _puntosIniciales = 0;
    private bool _activarInmediatamente = true;
    private decimal _multiplicadorPuntos = 1.0m;
    private int? _limitePuntosMensual = 1000;
    private string? _codigoTarjeta = null;
    private Guid _usuarioId = Guid.NewGuid();
    private string? _observaciones = "Tarjeta de prueba";
    private NivelFidelizacion _nivelActualizar = NivelFidelizacion.Platino;

    public TarjetaFidelizacionTestDataBuilder ConClienteId(Guid clienteId)
    {
        _clienteId = clienteId;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConTipoTarjeta(TipoTarjetaFidelizacion tipoTarjeta)
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

    public TarjetaFidelizacionTestDataBuilder ConLimitePuntosMensual(int? limitePuntosMensual)
    {
        _limitePuntosMensual = limitePuntosMensual;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConCodigoTarjeta(string? codigoTarjeta)
    {
        _codigoTarjeta = codigoTarjeta;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConUsuarioId(Guid usuarioId)
    {
        _usuarioId = usuarioId;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConObservaciones(string? observaciones)
    {
        _observaciones = observaciones;
        return this;
    }

    public TarjetaFidelizacionTestDataBuilder ConNivelActualizar(NivelFidelizacion nivel)
    {
        _nivelActualizar = nivel;
        return this;
    }

    public CrearTarjetaFidelizacionCommand BuildCrearTarjetaRequest() => new()
    {
        ClienteId = _clienteId,
        CodigoTarjeta = _codigoTarjeta,
        PuntosIniciales = _puntosIniciales,
        TipoTarjeta = _tipoTarjeta,
        ActivarInmediatamente = _activarInmediatamente,
        UsuarioId = _usuarioId,
        Observaciones = _observaciones,
        Configuracion = new CrearTarjetaConfiguracion
        {
            PuntosIniciales = _puntosIniciales,
            MultiplicadorPuntos = _multiplicadorPuntos,
            LimitePuntosMensual = _limitePuntosMensual
        }
    };

    public ActualizarTarjetaFidelizacionCommand BuildActualizarTarjetaRequest() => new()
    {
        Nivel = _nivelActualizar,
        MultiplicadorPuntos = _multiplicadorPuntos,
        LimiteMensual = _limitePuntosMensual,
        Observaciones = _observaciones,
        UsuarioId = _usuarioId
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