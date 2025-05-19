# Sistema de Suscripción a Eventos entre Agregados

Este módulo implementa un sistema de suscripción a eventos de dominio que permite a los agregados
suscribirse a eventos emitidos por otros agregados sin crear dependencias directas entre ellos.

## Conceptos clave

- **EventSubscriptionCriteria**: Define los criterios para filtrar eventos (tipo, entidad origen, contexto delimitado)
- **EventSubscriptionHandler**: Delegado que representa el manejador de eventos por suscripción
- **EventSubscriptionManager**: Administra las suscripciones y notifica a los suscriptores

## Cómo funciona

1. Un agregado se suscribe a eventos específicos mediante el `IEventSubscriptionManager`
2. Cuando se emite un evento, el `DomainEventDispatcher` lo distribuye a los manejadores registrados y a los suscriptores
3. El `EventSubscriptionManager` filtra los eventos según los criterios de suscripción
4. Invoca los manejadores de los suscriptores que cumplan con los criterios

## Ejemplos de uso

### Registrando los servicios

```csharp
// En Program.cs o Startup.cs
services.AddDomainEventServicesWithSubscriptions();
```

### Creando una suscripción entre agregados

```csharp
// En un servicio de dominio o manejador de comandos
public class ProductoStockWatcher
{
    private readonly IEventSubscriptionManager _subscriptionManager;
    private readonly IIngredienteRepository _ingredienteRepository;
    private Guid _subscriptionId;
    
    public ProductoStockWatcher(
        IEventSubscriptionManager subscriptionManager,
        IIngredienteRepository ingredienteRepository)
    {
        _subscriptionManager = subscriptionManager;
        _ingredienteRepository = ingredienteRepository;
    }
    
    public void Initialize()
    {
        // Suscribirse a eventos StockBajoMinimo desde el contexto de Inventario
        _subscriptionId = _subscriptionManager.Subscribe<StockBajoMinimo>(
            "Inventario",
            HandleStockBajoMinimo);
    }
    
    public void Dispose()
    {
        // Cancelar la suscripción
        _subscriptionManager.Unsubscribe(_subscriptionId);
    }
    
    private async Task HandleStockBajoMinimo(DomainEvent evento, CancellationToken cancellationToken)
    {
        var stockBajoEvento = (StockBajoMinimo)evento;
        
        // Buscar productos que usan el ingrediente con stock bajo
        var productosAfectados = await _ingredienteRepository.ObtenerProductosQueUsanIngrediente(
            stockBajoEvento.IngredienteId,
            cancellationToken);
            
        // Realizar alguna acción con los productos afectados...
    }
}
```

### Suscribiéndose desde un agregado

```csharp
public class ServicioNotificaciones
{
    private readonly IEventSubscriptionManager _subscriptionManager;
    private readonly List<Guid> _suscripciones = new List<Guid>();
    
    public ServicioNotificaciones(IEventSubscriptionManager subscriptionManager)
    {
        _subscriptionManager = subscriptionManager;
    }
    
    public void SuscribirseAEventosCriticos()
    {
        // Suscribirse a cualquier orden de compra generada
        _suscripciones.Add(_subscriptionManager.Subscribe<OrdenCompraGenerada>(
            async (evento, cancellationToken) => 
            {
                var ordenEvento = (OrdenCompraGenerada)evento;
                await EnviarNotificacionOrdenCompra(ordenEvento.OrdenCompraId);
            }));
            
        // Suscribirse a stock bajo de un ingrediente específico
        var ingredienteEspecifico = Guid.Parse("1234...");
        _suscripciones.Add(_subscriptionManager.Subscribe<StockBajoMinimo>(
            ingredienteEspecifico,
            async (evento, cancellationToken) => 
            {
                var stockEvento = (StockBajoMinimo)evento;
                await EnviarAlertaUrgente(stockEvento.IngredienteId, stockEvento.Stock);
            }));
    }
    
    // Métodos para enviar notificaciones...
}
```

## Buenas prácticas

1. **Evitar suscripciones circulares**: No crear ciclos donde A → B → A, ya que podría provocar recursión infinita
2. **Mantener manejadores ligeros**: Los manejadores de eventos por suscripción deben ser rápidos 
3. **Cancelar suscripciones**: Siempre cancelar las suscripciones cuando ya no se necesiten
4. **Usar criterios específicos**: Filtrar por tipo de evento, entidad o contexto para minimizar el procesamiento
5. **Manejo de errores**: Implementar estrategias de recuperación ante errores en los manejadores

## Ventajas

- **Desacoplamiento**: Evita dependencias directas entre agregados
- **Flexibilidad**: Permite crear interacciones entre contextos delimitados
- **Testabilidad**: Facilita la prueba de interacciones entre agregados
- **Escalabilidad**: Mecanismo sencillo para añadir nuevos comportamientos sin modificar código existente 