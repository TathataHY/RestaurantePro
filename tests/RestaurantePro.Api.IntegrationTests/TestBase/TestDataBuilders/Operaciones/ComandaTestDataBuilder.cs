using Bogus;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Operaciones;

/// <summary>
/// Builder para crear datos de prueba de comandas para tests de integración
/// </summary>
public class ComandaTestDataBuilder
{
    private readonly Faker _faker = new Faker("es");
    
    private Guid? _meseroId;
    private Guid? _clienteId;
    private Guid? _mesaId;
    private string? _observaciones;
    private readonly List<dynamic> _productos = new();
    private decimal? _descuentoFidelizacion;
    private RestaurantePro.Domain.Operaciones.Comandas.Enums.EstadoComanda _estado = RestaurantePro.Domain.Operaciones.Comandas.Enums.EstadoComanda.Creada;

    /// <summary>
    /// Establece el mesero responsable de la comanda
    /// </summary>
    public ComandaTestDataBuilder ConMesero(Guid meseroId)
    {
        _meseroId = meseroId;
        return this;
    }

    /// <summary>
    /// Establece el cliente asociado a la comanda
    /// </summary>
    public ComandaTestDataBuilder ConCliente(Guid clienteId)
    {
        _clienteId = clienteId;
        return this;
    }

    /// <summary>
    /// Establece la mesa asignada a la comanda
    /// </summary>
    public ComandaTestDataBuilder ConMesa(Guid mesaId)
    {
        _mesaId = mesaId;
        return this;
    }

    /// <summary>
    /// Establece las observaciones de la comanda
    /// </summary>
    public ComandaTestDataBuilder ConObservaciones(string observaciones)
    {
        _observaciones = observaciones;
        return this;
    }

    /// <summary>
    /// Agrega un producto a la comanda
    /// </summary>
    public ComandaTestDataBuilder ConProducto(Guid productoId, int cantidad = 1, string? observaciones = null)
    {
        _productos.Add(new
        {
            ProductoId = productoId,
            Cantidad = cantidad,
            Observaciones = observaciones ?? _faker.Lorem.Sentence(3, 5)
        });
        return this;
    }

    /// <summary>
    /// Establece el descuento de fidelización
    /// </summary>
    public ComandaTestDataBuilder ConDescuentoFidelizacion(decimal descuento)
    {
        _descuentoFidelizacion = descuento;
        return this;
    }

    /// <summary>
    /// Establece el estado de la comanda
    /// </summary>
    public ComandaTestDataBuilder ConEstado(RestaurantePro.Domain.Operaciones.Comandas.Enums.EstadoComanda estado)
    {
        _estado = estado;
        return this;
    }

    /// <summary>
    /// Construye el objeto de request para crear una comanda
    /// </summary>
    public object BuildCrearComandaRequest()
    {
        return new
        {
            MeseroId = _meseroId ?? Guid.NewGuid(),
            ClienteId = _clienteId,
            MesaId = _mesaId ?? Guid.NewGuid(),
            Observaciones = _observaciones ?? _faker.Lorem.Sentence(5, 10),
            Productos = _productos.Any() ? _productos : new List<dynamic> { new { ProductoId = Guid.NewGuid(), Cantidad = 1, Observaciones = "Test product" } }
        };
    }

    /// <summary>
    /// Construye el objeto de request para actualizar una comanda
    /// </summary>
    public object BuildActualizarComandaRequest()
    {
        return new
        {
            Observaciones = _observaciones ?? _faker.Lorem.Sentence(5, 10),
            Estado = _estado.ToString(),
            DescuentoFidelizacion = _descuentoFidelizacion
        };
    }

    /// <summary>
    /// Construye el objeto de request para cambiar estado de comanda
    /// </summary>
    public object BuildCambiarEstadoRequest(string? nuevoEstado = null)
    {
        return new
        {
            NuevoEstado = nuevoEstado ?? _estado.ToString(),
            Observaciones = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para asignar mesa
    /// </summary>
    public object BuildAsignarMesaRequest(Guid? mesaId = null)
    {
        return new
        {
            MesaId = mesaId ?? _mesaId ?? Guid.NewGuid()
        };
    }

    /// <summary>
    /// Construye el objeto de request para agregar producto
    /// </summary>
    public object BuildAgregarProductoRequest(Guid? productoId = null, int? cantidad = null)
    {
        return new
        {
            ProductoId = productoId ?? Guid.NewGuid(),
            Cantidad = cantidad ?? _faker.Random.Int(1, 3),
            Observaciones = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para modificar producto
    /// </summary>
    public object BuildModificarProductoRequest(Guid detalleId, int? cantidad = null, string? observaciones = null)
    {
        return new
        {
            Cantidad = cantidad ?? _faker.Random.Int(1, 3),
            Observaciones = observaciones ?? _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para aplicar descuento
    /// </summary>
    public object BuildAplicarDescuentoRequest(decimal? descuento = null)
    {
        return new
        {
            Descuento = descuento ?? _descuentoFidelizacion ?? _faker.Random.Decimal(5, 20),
            Motivo = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para dividir comanda
    /// </summary>
    public object BuildDividirComandaRequest()
    {
        return new
        {
            TipoDivision = "PorItems",
            Partes = new[]
            {
                new { ClienteId = Guid.NewGuid(), Items = new[] { 0 } },
                new { ClienteId = Guid.NewGuid(), Items = new[] { 1 } }
            }
        };
    }

    /// <summary>
    /// Construye el objeto de request para cerrar comanda
    /// </summary>
    public object BuildCerrarComandaRequest()
    {
        return new
        {
            MetodoPago = _faker.PickRandom("Efectivo", "Tarjeta", "Transferencia"),
            Propina = _faker.Random.Decimal(0, 50),
            Observaciones = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Resetea el builder a su estado inicial
    /// </summary>
    public ComandaTestDataBuilder Reset()
    {
        _meseroId = null;
        _clienteId = null;
        _mesaId = null;
        _observaciones = null;
        _productos.Clear();
        _descuentoFidelizacion = null;
        _estado = RestaurantePro.Domain.Operaciones.Comandas.Enums.EstadoComanda.Creada;
        return this;
    }

    /// <summary>
    /// Crea una nueva instancia del builder
    /// </summary>
    public static ComandaTestDataBuilder Nuevo()
    {
        return new ComandaTestDataBuilder();
    }
} 