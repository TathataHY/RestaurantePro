using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Validation;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Builders;

/// <summary>
/// Builder para la construcción fluida y validada de entidades OrdenCompra.
/// 
/// Proporciona una interfaz fluida para construir órdenes de compra con validaciones
/// robustas de reglas de negocio y manejo de errores integrado.
/// 
/// Características:
/// - Fluent interface para construcción paso a paso
/// - Validaciones de reglas de negocio integradas
/// - Manejo robusto de errores con INotificationManager
/// - Logging detallado para debugging y auditoría
/// - Soporte para reutilización con método Reset()
/// - Patrón Result para comunicar éxito/fallo
/// 
/// Ejemplo de uso:
/// <code>
/// var resultado = new OrdenCompraBuilder(notificationManager, logger)
///     .ParaProveedor(proveedorId)
///     .ConFechaEmision(DateTime.Now)
///     .ConFechaEntregaEstimada(DateTime.Now.AddDays(7))
///     .AgregarItem(ingredienteId, "Harina", 50m, UnidadMedida.Kilogramo, 2.50m)
///     .ConObservaciones("Orden urgente")
///     .Construir();
/// 
/// if (resultado.Succeeded)
/// {
///     var orden = resultado.Value;
///     // Usar la orden construida
/// }
/// </code>
/// </summary>
public class OrdenCompraBuilder
{
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<OrdenCompraBuilder> _logger;
    
    // Estado interno del builder
    private Guid? _proveedorId;
    private DateTime? _fechaEmision;
    private DateTime? _fechaEntregaEstimada;
    private string? _observaciones;
    private readonly List<ItemDetalle> _items = new();
    
    /// <summary>
    /// Clase interna para almacenar temporalmente los datos de ítems
    /// antes de crear las entidades ItemOrdenCompra
    /// </summary>
    private class ItemDetalle
    {
        public Guid IngredienteId { get; set; }
        public string NombreIngrediente { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public UnidadMedida UnidadMedida { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
    
    public OrdenCompraBuilder(INotificationManager notificationManager, ILogger<OrdenCompraBuilder> logger)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogDebug("OrdenCompraBuilder inicializado");
    }
    
    /// <summary>
    /// Factory method estático para crear una nueva instancia del builder
    /// </summary>
    public static OrdenCompraBuilder Nuevo(INotificationManager notificationManager, ILogger<OrdenCompraBuilder> logger)
    {
        return new OrdenCompraBuilder(notificationManager, logger);
    }
    
    /// <summary>
    /// Establece el proveedor para la orden de compra
    /// </summary>
    /// <param name="proveedorId">ID del proveedor (requerido)</param>
    /// <returns>Instancia del builder para encadenamiento fluido</returns>
    public OrdenCompraBuilder ParaProveedor(Guid proveedorId)
    {
        if (proveedorId == Guid.Empty)
        {
            _notificationManager.AddError("ProveedorId", "El ID del proveedor no puede estar vacío");
            _logger.LogWarning("Intento de asignar proveedor con ID vacío");
            return this;
        }
        
        _proveedorId = proveedorId;
        _logger.LogDebug("Proveedor asignado: {ProveedorId}", proveedorId);
        return this;
    }
    
    /// <summary>
    /// Establece la fecha de emisión de la orden
    /// </summary>
    /// <param name="fechaEmision">Fecha de emisión (no puede ser futura)</param>
    /// <returns>Instancia del builder para encadenamiento fluido</returns>
    public OrdenCompraBuilder ConFechaEmision(DateTime fechaEmision)
    {
        if (fechaEmision > DateTime.Now)
        {
            _notificationManager.AddError("FechaEmision", "La fecha de emisión no puede ser futura");
            _logger.LogWarning("Fecha de emisión futura rechazada: {FechaEmision}", fechaEmision);
            return this;
        }
        
        if (fechaEmision < DateTime.Now.AddYears(-1))
        {
            _notificationManager.AddError("FechaEmision", "La fecha de emisión no puede ser mayor a 1 año de antigüedad");
            _logger.LogWarning("Fecha de emisión muy antigua rechazada: {FechaEmision}", fechaEmision);
            return this;
        }
        
        _fechaEmision = fechaEmision;
        _logger.LogDebug("Fecha de emisión establecida: {FechaEmision}", fechaEmision);
        return this;
    }
    
    /// <summary>
    /// Establece la fecha estimada de entrega
    /// </summary>
    /// <param name="fechaEntregaEstimada">Fecha estimada de entrega (debe ser posterior a la emisión)</param>
    /// <returns>Instancia del builder para encadenamiento fluido</returns>
    public OrdenCompraBuilder ConFechaEntregaEstimada(DateTime fechaEntregaEstimada)
    {
        if (_fechaEmision.HasValue && fechaEntregaEstimada < _fechaEmision.Value)
        {
            _notificationManager.AddError("FechaEntregaEstimada", "La fecha de entrega estimada debe ser posterior a la fecha de emisión");
            _logger.LogWarning("Fecha de entrega anterior a emisión: Entrega={FechaEntrega}, Emisión={FechaEmision}", 
                fechaEntregaEstimada, _fechaEmision);
            return this;
        }
        
        if (fechaEntregaEstimada > DateTime.Now.AddYears(1))
        {
            _notificationManager.AddError("FechaEntregaEstimada", "La fecha de entrega estimada no puede ser mayor a 1 año en el futuro");
            _logger.LogWarning("Fecha de entrega muy lejana rechazada: {FechaEntrega}", fechaEntregaEstimada);
            return this;
        }
        
        _fechaEntregaEstimada = fechaEntregaEstimada;
        _logger.LogDebug("Fecha de entrega estimada establecida: {FechaEntrega}", fechaEntregaEstimada);
        return this;
    }
    
    /// <summary>
    /// Agrega observaciones a la orden de compra
    /// </summary>
    /// <param name="observaciones">Observaciones (máximo 1000 caracteres)</param>
    /// <returns>Instancia del builder para encadenamiento fluido</returns>
    public OrdenCompraBuilder ConObservaciones(string observaciones)
    {
        if (string.IsNullOrWhiteSpace(observaciones))
        {
            _logger.LogDebug("Observaciones vacías, no se asignan");
            return this;
        }
        
        if (observaciones.Length > 1000)
        {
            _notificationManager.AddError("Observaciones", "Las observaciones no pueden exceder 1000 caracteres");
            _logger.LogWarning("Observaciones muy largas rechazadas: {Longitud} caracteres", observaciones.Length);
            return this;
        }
        
        _observaciones = observaciones.Trim();
        _logger.LogDebug("Observaciones establecidas: {Longitud} caracteres", _observaciones.Length);
        return this;
    }
    
    /// <summary>
    /// Agrega un ítem a la orden de compra
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="nombreIngrediente">Nombre del ingrediente</param>
    /// <param name="cantidad">Cantidad a solicitar (debe ser mayor a 0)</param>
    /// <param name="unidadMedida">Unidad de medida</param>
    /// <param name="precioUnitario">Precio unitario (debe ser mayor o igual a 0)</param>
    /// <returns>Instancia del builder para encadenamiento fluido</returns>
    public OrdenCompraBuilder AgregarItem(Guid ingredienteId, string nombreIngrediente, decimal cantidad, 
        UnidadMedida unidadMedida, decimal precioUnitario)
    {
        // Validaciones básicas
        if (ingredienteId == Guid.Empty)
        {
            _notificationManager.AddError("IngredienteId", "El ID del ingrediente no puede estar vacío");
            _logger.LogWarning("Intento de agregar ítem con ingrediente ID vacío");
            return this;
        }
        
        if (string.IsNullOrWhiteSpace(nombreIngrediente))
        {
            _notificationManager.AddError("NombreIngrediente", "El nombre del ingrediente es requerido");
            _logger.LogWarning("Intento de agregar ítem sin nombre para ingrediente {IngredienteId}", ingredienteId);
            return this;
        }
        
        if (nombreIngrediente.Length > 200)
        {
            _notificationManager.AddError("NombreIngrediente", "El nombre del ingrediente no puede exceder 200 caracteres");
            _logger.LogWarning("Nombre de ingrediente muy largo rechazado: {Longitud} caracteres", nombreIngrediente.Length);
            return this;
        }
        
        if (cantidad <= 0)
        {
            _notificationManager.AddError("Cantidad", "La cantidad debe ser mayor a 0");
            _logger.LogWarning("Cantidad inválida para ingrediente {IngredienteId}: {Cantidad}", ingredienteId, cantidad);
            return this;
        }
        
        if (cantidad > 999999.99m)
        {
            _notificationManager.AddError("Cantidad", "La cantidad no puede exceder 999,999.99");
            _logger.LogWarning("Cantidad excesiva para ingrediente {IngredienteId}: {Cantidad}", ingredienteId, cantidad);
            return this;
        }
        
        if (precioUnitario < 0)
        {
            _notificationManager.AddError("PrecioUnitario", "El precio unitario no puede ser negativo");
            _logger.LogWarning("Precio unitario negativo para ingrediente {IngredienteId}: {Precio}", ingredienteId, precioUnitario);
            return this;
        }
        
        if (precioUnitario > 999999.99m)
        {
            _notificationManager.AddError("PrecioUnitario", "El precio unitario no puede exceder 999,999.99");
            _logger.LogWarning("Precio unitario excesivo para ingrediente {IngredienteId}: {Precio}", ingredienteId, precioUnitario);
            return this;
        }
        
        // Verificar duplicados
        var itemExistente = _items.FirstOrDefault(i => i.IngredienteId == ingredienteId);
        if (itemExistente != null)
        {
            _notificationManager.AddError("ItemDuplicado", $"Ya existe un ítem para el ingrediente '{nombreIngrediente}'");
            _logger.LogWarning("Intento de agregar ítem duplicado para ingrediente {IngredienteId}", ingredienteId);
            return this;
        }
        
        // Agregar el ítem
        var nuevoItem = new ItemDetalle
        {
            IngredienteId = ingredienteId,
            NombreIngrediente = nombreIngrediente.Trim(),
            Cantidad = cantidad,
            UnidadMedida = unidadMedida,
            PrecioUnitario = precioUnitario
        };
        
        _items.Add(nuevoItem);
        _logger.LogDebug("Ítem agregado: {Ingrediente} - {Cantidad} {Unidad} a ${Precio}",
            nombreIngrediente, cantidad, unidadMedida, precioUnitario);
        
        return this;
    }
    
    /// <summary>
    /// Agrega un ítem a la orden de compra con precio unitario 0 (para cotizar después)
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <param name="nombreIngrediente">Nombre del ingrediente</param>
    /// <param name="cantidad">Cantidad a solicitar (debe ser mayor a 0)</param>
    /// <param name="unidadMedida">Unidad de medida</param>
    /// <returns>Instancia del builder para encadenamiento fluido</returns>
    public OrdenCompraBuilder AgregarItem(Guid ingredienteId, string nombreIngrediente, decimal cantidad, 
        UnidadMedida unidadMedida)
    {
        return AgregarItem(ingredienteId, nombreIngrediente, cantidad, unidadMedida, 0m);
    }
    
    /// <summary>
    /// Actualiza el precio unitario de un ítem existente
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente del ítem a actualizar</param>
    /// <param name="nuevoPrecioUnitario">Nuevo precio unitario</param>
    /// <returns>Instancia del builder para encadenamiento fluido</returns>
    public OrdenCompraBuilder ActualizarPrecioItem(Guid ingredienteId, decimal nuevoPrecioUnitario)
    {
        if (ingredienteId == Guid.Empty)
        {
            _notificationManager.AddError("IngredienteId", "El ID del ingrediente no puede estar vacío");
            _logger.LogWarning("Intento de actualizar precio con ingrediente ID vacío");
            return this;
        }
        
        if (nuevoPrecioUnitario < 0)
        {
            _notificationManager.AddError("PrecioUnitario", "El precio unitario no puede ser negativo");
            _logger.LogWarning("Precio unitario negativo para ingrediente {IngredienteId}: {Precio}", ingredienteId, nuevoPrecioUnitario);
            return this;
        }
        
        if (nuevoPrecioUnitario > 999999.99m)
        {
            _notificationManager.AddError("PrecioUnitario", "El precio unitario no puede exceder 999,999.99");
            _logger.LogWarning("Precio unitario excesivo para ingrediente {IngredienteId}: {Precio}", ingredienteId, nuevoPrecioUnitario);
            return this;
        }
        
        var item = _items.FirstOrDefault(i => i.IngredienteId == ingredienteId);
        if (item == null)
        {
            _notificationManager.AddError("ItemNoEncontrado", $"No se encontró un ítem con el ingrediente ID {ingredienteId}");
            _logger.LogWarning("Intento de actualizar precio de ítem inexistente: {IngredienteId}", ingredienteId);
            return this;
        }
        
        var precioAnterior = item.PrecioUnitario;
        item.PrecioUnitario = nuevoPrecioUnitario;
        
        _logger.LogDebug("Precio actualizado para {Ingrediente}: ${PrecioAnterior} → ${PrecioNuevo}",
            item.NombreIngrediente, precioAnterior, nuevoPrecioUnitario);
        
        return this;
    }
    
    /// <summary>
    /// Construye la orden de compra con todas las validaciones aplicadas
    /// </summary>
    /// <returns>Result con la orden de compra creada o información de errores</returns>
    public Result<OrdenCompra> Construir()
    {
        _logger.LogDebug("Iniciando construcción de OrdenCompra");
        
        // Validaciones finales antes de construir
        if (!_proveedorId.HasValue)
        {
            _notificationManager.AddError("ProveedorRequerido", "Debe especificar un proveedor para la orden de compra");
        }
        
        if (!_fechaEmision.HasValue)
        {
            _notificationManager.AddError("FechaEmisionRequerida", "Debe especificar la fecha de emisión de la orden");
        }
        
        if (!_fechaEntregaEstimada.HasValue)
        {
            _notificationManager.AddError("FechaEntregaRequerida", "Debe especificar la fecha de entrega estimada");
        }
        
        if (_items.Count == 0)
        {
            _notificationManager.AddError("ItemsRequeridos", "La orden debe tener al menos un ítem");
        }
        
        // Verificar si hay errores acumulados
        if (_notificationManager.HasErrors)
        {
            _logger.LogWarning("Error en construcción de OrdenCompra: {ErrorCount} errores encontrados", 
                _notificationManager.GetErrors().Count);
            return Result.Failure<OrdenCompra>("Errores de validación en la construcción de la orden de compra");
        }
        
        try
        {
            // Crear la orden de compra
            var orden = OrdenCompra.Crear(_proveedorId!.Value, _observaciones ?? string.Empty, _fechaEmision!.Value);
            
            // Establecer fecha de entrega estimada
            orden.EstablecerFechaEntrega(_fechaEntregaEstimada!.Value);
            
            // Agregar todos los ítems
            foreach (var item in _items)
            {
                var itemOrden = orden.AgregarItem(item.IngredienteId, item.NombreIngrediente, item.Cantidad, item.UnidadMedida);
                
                // Actualizar el precio unitario si es mayor a 0
                if (item.PrecioUnitario > 0)
                {
                    itemOrden.Actualizar(item.Cantidad, item.PrecioUnitario);
                }
            }
            
            _logger.LogInformation("OrdenCompra construida exitosamente: {OrdenId} para proveedor {ProveedorId} con {ItemCount} ítems",
                orden.Id, _proveedorId.Value, _items.Count);
            
            return Result.Success(orden);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al construir OrdenCompra");
            return Result.Failure<OrdenCompra>($"Error inesperado durante la construcción: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Reinicia el builder a su estado inicial para reutilización
    /// </summary>
    /// <returns>Instancia del builder reiniciada</returns>
    public OrdenCompraBuilder Reset()
    {
        _logger.LogDebug("Reiniciando OrdenCompraBuilder");
        
        _proveedorId = null;
        _fechaEmision = null;
        _fechaEntregaEstimada = null;
        _observaciones = null;
        _items.Clear();
        
        // Crear nueva notificación para empezar limpio
        _notificationManager.CreateNewNotification();
        
        _logger.LogDebug("OrdenCompraBuilder reiniciado exitosamente");
        return this;
    }
} 