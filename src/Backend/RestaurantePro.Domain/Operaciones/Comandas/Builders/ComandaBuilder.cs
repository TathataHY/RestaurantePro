namespace RestaurantePro.Domain.Operaciones.Comandas.Builders;

/// <summary>
/// Builder para construir instancias de Comanda paso a paso con validaciones fluidas.
/// Permite crear comandas complejas de manera segura y legible.
/// </summary>
public class ComandaBuilder
{
    private Guid? _meseroId;
    private Guid? _clienteId;
    private Guid? _mesaId;
    private string? _observaciones;
    private readonly List<ProductoItem> _productos = new();
    private decimal? _descuentoFidelizacion;
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<ComandaBuilder> _logger;

    /// <summary>
    /// Constructor del builder
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos del builder</param>
    public ComandaBuilder(INotificationManager notificationManager, ILogger<ComandaBuilder> logger)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Establece el mesero responsable de la comanda
    /// </summary>
    /// <param name="meseroId">ID del mesero</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ComandaBuilder ConMesero(Guid meseroId)
    {
        Guard.AgainstEmpty(meseroId, nameof(meseroId));
        _meseroId = meseroId;
        _logger.LogDebug("Mesero {MeseroId} asignado a la comanda", meseroId);
        return this;
    }

    /// <summary>
    /// Establece el cliente asociado a la comanda (opcional)
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ComandaBuilder ConCliente(Guid clienteId)
    {
        Guard.AgainstEmpty(clienteId, nameof(clienteId));
        _clienteId = clienteId;
        _logger.LogDebug("Cliente {ClienteId} asignado a la comanda", clienteId);
        return this;
    }

    /// <summary>
    /// Establece la mesa donde se sirve la comanda (opcional)
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ComandaBuilder EnMesa(Guid mesaId)
    {
        Guard.AgainstEmpty(mesaId, nameof(mesaId));
        _mesaId = mesaId;
        _logger.LogDebug("Mesa {MesaId} asignada a la comanda", mesaId);
        return this;
    }

    /// <summary>
    /// Establece observaciones para la comanda
    /// </summary>
    /// <param name="observaciones">Observaciones o notas especiales</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ComandaBuilder ConObservaciones(string observaciones)
    {
        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            Guard.AgainstTooLong(observaciones, 500, nameof(observaciones));
            _observaciones = observaciones;
            _logger.LogDebug("Observaciones agregadas: {Observaciones}", observaciones[..Math.Min(50, observaciones.Length)]);
        }
        return this;
    }

    /// <summary>
    /// Agrega un producto a la comanda
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="cantidad">Cantidad del producto</param>
    /// <param name="precioUnitario">Precio unitario</param>
    /// <param name="observaciones">Observaciones específicas del producto</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ComandaBuilder AgregarProducto(Guid productoId, int cantidad, decimal precioUnitario, string? observaciones = null)
    {
        Guard.AgainstEmpty(productoId, nameof(productoId));
        Guard.AgainstNegativeOrZero(cantidad, nameof(cantidad));
        Guard.AgainstNegativeOrZero(precioUnitario, nameof(precioUnitario));
        
        // Validaciones del builder que deben impedir el agregado
        var hayErrores = false;
        
        if (cantidad > 50)
        {
            _notificationManager.AddError("La cantidad no puede exceder 50 unidades por producto", "Cantidad");
            hayErrores = true;
        }

        if (precioUnitario > 1000000m)
        {
            _notificationManager.AddError("El precio unitario no puede exceder $1,000,000", "PrecioUnitario");
            hayErrores = true;
        }

        if (!string.IsNullOrEmpty(observaciones) && observaciones.Length > 200)
        {
            _notificationManager.AddError("Las observaciones del producto no pueden exceder 200 caracteres", "ObservacionesProducto");
            hayErrores = true;
        }

        // Verificar que no exista ya el producto
        if (_productos.Any(p => p.ProductoId == productoId))
        {
            _notificationManager.AddError($"El producto {productoId} ya existe en la comanda", "ProductoDuplicado");
            hayErrores = true;
        }

        // Solo agregar si no hay errores
        if (!hayErrores)
        {
            _productos.Add(new ProductoItem(productoId, cantidad, precioUnitario, observaciones));
            _logger.LogDebug("Producto {ProductoId} agregado: {Cantidad} x ${PrecioUnitario}", 
                productoId, cantidad, precioUnitario);
        }
        else
        {
            _logger.LogWarning("Producto {ProductoId} no agregado debido a errores de validación", productoId);
        }
        
        return this;
    }

    /// <summary>
    /// Agrega múltiples productos a la comanda
    /// </summary>
    /// <param name="productos">Lista de productos a agregar</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ComandaBuilder AgregarProductos(params (Guid ProductoId, int Cantidad, decimal PrecioUnitario, string? Observaciones)[] productos)
    {
        foreach (var (productoId, cantidad, precioUnitario, observaciones) in productos)
        {
            AgregarProducto(productoId, cantidad, precioUnitario, observaciones);
        }
        return this;
    }

    /// <summary>
    /// Establece un descuento de fidelización
    /// </summary>
    /// <param name="porcentajeDescuento">Porcentaje de descuento (0-0.5)</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ComandaBuilder ConDescuentoFidelizacion(decimal porcentajeDescuento)
    {
        if (porcentajeDescuento < 0 || porcentajeDescuento > 0.5m)
        {
            _notificationManager.AddError("El descuento debe estar entre 0% y 50%", "DescuentoFidelizacion");
            return this; // No aplicar el descuento si es inválido
        }

        if (!_clienteId.HasValue)
        {
            _notificationManager.AddError("No se puede aplicar descuento sin un cliente asociado", "DescuentoSinCliente");
            return this; // No aplicar el descuento si no hay cliente
        }

        // Solo aplicar el descuento si las validaciones pasan
        _descuentoFidelizacion = porcentajeDescuento;
        _logger.LogDebug("Descuento de fidelización del {Porcentaje}% configurado", porcentajeDescuento * 100);
        return this;
    }

    /// <summary>
    /// Construye la comanda validando todos los datos
    /// </summary>
    /// <returns>Resultado con la comanda creada o errores de validación</returns>
    public Result<Comanda> Construir()
    {
        // NO limpiar notificaciones si ya hay errores acumulados
        // Solo crear nueva notificación si no hay errores previos
        if (!_notificationManager.HasErrors)
        {
            _notificationManager.CreateNewNotification();
        }

        try
        {
            // Validaciones obligatorias
            ValidarDatosObligatorios();

            // Si hay errores (previos o nuevos), retornar resultado fallido
            if (_notificationManager.HasErrors)
            {
                var errores = string.Join(", ", _notificationManager.GetErrors().Select(e => e.Message));
                _logger.LogWarning("Error al construir comanda: {Errores}", errores);
                return _notificationManager.ToResult<Comanda>(null!);
            }

            // Crear la comanda base - la mesa ahora es obligatoria
            var comanda = Comanda.Crear(
                _meseroId!.Value,
                _clienteId,
                _mesaId!.Value,
                _observaciones,
                null,
                RestaurantePro.Domain.Operaciones.Comandas.Enums.TipoComanda.Mesa);
            
            // Agregar productos
            foreach (var producto in _productos)
            {
                comanda.AgregarProducto(producto.ProductoId, producto.Cantidad, 
                    producto.PrecioUnitario, producto.Observaciones);
            }

            // Aplicar descuento si está configurado
            if (_descuentoFidelizacion.HasValue && _clienteId.HasValue)
            {
                comanda.AplicarDescuentoFidelizacion(_descuentoFidelizacion.Value);
            }

            _logger.LogInformation("Comanda {ComandaId} construida exitosamente con {CantidadProductos} productos", 
                comanda.Id, _productos.Count);

            return Result.Success(comanda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al construir comanda");
            _notificationManager.AddError($"Error al construir la comanda: {ex.Message}", "ErrorInesperado");
            return _notificationManager.ToResult<Comanda>(null!);
        }
    }

    /// <summary>
    /// Valida que todos los datos obligatorios estén presentes
    /// </summary>
    private void ValidarDatosObligatorios()
    {
        if (!_meseroId.HasValue)
        {
            _notificationManager.AddError("El mesero es obligatorio para crear una comanda", "MeseroRequerido");
        }

        if (!_mesaId.HasValue)
        {
            _notificationManager.AddError("La mesa es obligatoria para crear una comanda", "MesaRequerida");
        }

        if (_productos.Count == 0)
        {
            _notificationManager.AddInformation("Se está creando una comanda sin productos", "SinProductos");
        }

        if (_productos.Count > 50)
        {
            _notificationManager.AddError("Una comanda no puede tener más de 50 productos", "DemasiadosProductos");
        }
    }

    /// <summary>
    /// Reinicia el builder para construir una nueva comanda
    /// </summary>
    /// <returns>Builder reiniciado</returns>
    public ComandaBuilder Reset()
    {
        _meseroId = null;
        _clienteId = null;
        _mesaId = null;
        _observaciones = null;
        _productos.Clear();
        _descuentoFidelizacion = null;
        
        // Limpiar notificaciones acumuladas
        _notificationManager.CreateNewNotification();
        
        _logger.LogDebug("Builder reiniciado");
        return this;
    }

    /// <summary>
    /// Método de conveniencia para crear un builder configurado
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger</param>
    /// <returns>Nuevo builder configurado</returns>
    public static ComandaBuilder Nuevo(INotificationManager notificationManager, ILogger<ComandaBuilder> logger)
    {
        return new ComandaBuilder(notificationManager, logger);
    }
}

/// <summary>
/// Representa un producto en el proceso de construcción de la comanda
/// </summary>
internal record ProductoItem(
    Guid ProductoId,
    int Cantidad,
    decimal PrecioUnitario,
    string? Observaciones); 