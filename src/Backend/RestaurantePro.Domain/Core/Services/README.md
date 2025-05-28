# Servicios Transversales del Core

Este directorio contiene servicios transversales que son utilizados por múltiples contextos dentro del dominio.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben:

1. Ser utilizables por múltiples contextos dentro del dominio
2. Proporcionar funcionalidad base o transversal que no pertenece a un único agregado
3. Implementar patrones comunes reutilizables en todo el dominio

Ejemplos:
- Servicios de notificación basados en eventos
- Servicios de caching
- Servicios de auditoría
- Servicios de logging específicos del dominio

## Cuándo NO crear un servicio aquí

Si el servicio:
- Es específico de un único agregado o contexto
- No es reutilizable por otros contextos
- Implementa lógica de negocio específica

En esos casos, el servicio debe ubicarse en el contexto al que pertenece.

## Convenciones de nomenclatura

Los servicios en este directorio siguen una nomenclatura descriptiva según su funcionalidad, por ejemplo:

- `EventBasedNotificationService`
- `DomainAuditService`
- `DomainCacheService`

Las interfaces correspondientes siguen el mismo patrón con el prefijo "I":

- `IEventBasedNotificationService`

# Guía de Uso de CoreServiceFacade

## Introducción

`CoreServiceFacade` es la implementación del patrón Fachada para el contexto Core de RestaurantePro. Esta fachada proporciona una interfaz unificada para acceder a todas las funcionalidades principales del módulo Core, simplificando la interacción desde la capa de Aplicación y ocultando la complejidad interna del dominio.

## Características principales

- **Gestión de Productos**: Operaciones CRUD para productos y categorías
- **Gestión de Recetas**: Registro y consulta de recetas, verificación de disponibilidad de ingredientes
- **Gestión de Usuarios**: Administración de usuarios y roles
- **Sistema de Notificaciones**: Envío y gestión de notificaciones

## Instalación y configuración

Para utilizar CoreServiceFacade en la capa de Aplicación, debes registrarlo en el contenedor de dependencias:

```csharp
// En Program.cs o Startup.cs
services.AddScoped<ICoreServiceFacade, CoreServiceFacade>();
```

Asegúrate de que todas las dependencias requeridas estén registradas:

```csharp
services.AddScoped<IProductoRepository, ProductoRepository>();
services.AddScoped<IProductoCategoriaRepository, ProductoCategoriaRepository>();
services.AddScoped<IRecetaRepository, RecetaRepository>();
services.AddScoped<IUsuarioRepository, UsuarioRepository>();
services.AddScoped<IRolRepository, RolRepository>();
services.AddScoped<INotificacionRepository, NotificacionRepository>();
services.AddScoped<IProductoCategoriaService, ProductoCategoriaService>();
services.AddScoped<IRecetaService, RecetaService>();
services.AddScoped<IEventBasedNotificationService, EventBasedNotificationService>();
services.AddScoped<IDateTimeService, DateTimeService>();
services.AddScoped<INotificationManager, NotificationManager>();
```

## Ejemplos de uso

### Gestión de productos

```csharp
// Registrar un nuevo producto
var resultado = await _coreServiceFacade.RegistrarProductoAsync(
    "Ensalada César", 
    "Ensalada clásica con lechuga romana, crutones, parmesano y aderezo César",
    12.99m,
    categoriaId);

if (resultado.Succeeded)
{
    var nuevoProducto = resultado.Value;
    // Usar el producto creado
}

// Actualizar un producto existente
var resultadoActualizacion = await _coreServiceFacade.ActualizarProductoAsync(
    productoId,
    nombre: "Ensalada César Especial",
    precio: 14.99m);

// Obtener productos por categoría
var productos = await _coreServiceFacade.ObtenerProductosPorCategoriaAsync(
    categoriaId, 
    soloActivos: true);
```

### Gestión de recetas

```csharp
// Registrar una receta para un producto
var ingredientes = new Dictionary<Guid, decimal>
{
    { ingredienteId1, 0.2m }, // 200g
    { ingredienteId2, 0.05m } // 50g
};

var resultadoReceta = await _coreServiceFacade.RegistrarRecetaProductoAsync(
    productoId,
    "Mezclar todos los ingredientes y servir frío",
    15, // tiempo en minutos
    ingredientes);

// Verificar disponibilidad de ingredientes
var hayDisponibilidad = await _coreServiceFacade.VerificarDisponibilidadProductoAsync(
    productoId,
    cantidad: 5); // para 5 unidades

// Obtener ingredientes faltantes
var faltantes = await _coreServiceFacade.ObtenerIngredientesFaltantesProductoAsync(
    productoId,
    cantidad: 10);

// Calcular rentabilidad
var rentabilidad = await _coreServiceFacade.CalcularRentabilidadProductoAsync(productoId);
```

### Gestión de usuarios

```csharp
// Crear un usuario
var resultadoUsuario = await _coreServiceFacade.CrearUsuarioAsync(
    "juanperez",
    "Juan Pérez",
    "juan.perez@ejemplo.com",
    "Mesero");

// Asignar un rol adicional
await _coreServiceFacade.AsignarRolUsuarioAsync(
    usuarioId,
    "Cajero");

// Desactivar un usuario
await _coreServiceFacade.CambiarEstadoUsuarioAsync(
    usuarioId,
    activar: false);
```

### Sistema de notificaciones

```csharp
// Enviar una notificación a un usuario específico
var notificacion = await _coreServiceFacade.EnviarNotificacionAsync(
    usuarioId,
    "Alerta",
    "Stock bajo",
    "Hay productos con stock bajo que requieren reposición",
    prioridad: 2);

// Enviar notificación global (a todos los usuarios)
await _coreServiceFacade.EnviarNotificacionAsync(
    null, // sin destinatario específico = global
    "Información",
    "Mantenimiento programado",
    "El sistema estará en mantenimiento mañana de 2AM a 4AM");

// Obtener notificaciones de un usuario
var notificaciones = await _coreServiceFacade.ObtenerNotificacionesUsuarioAsync(
    usuarioId,
    soloNoLeidas: true);

// Marcar notificación como leída
await _coreServiceFacade.MarcarNotificacionComoLeidaAsync(notificacionId);
```

## Integración con CoreOperacionesIntegrationService

El `CoreServiceFacade` puede trabajar en conjunto con `CoreOperacionesIntegrationService` para integrar las operaciones entre contextos:

```csharp
// Verificar disponibilidad de productos para una comanda
var productosIdCantidad = new Dictionary<Guid, int>
{
    { productoId1, 2 },
    { productoId2, 3 }
};

var disponibilidad = await _coreOperacionesIntegrationService.VerificarDisponibilidadProductosAsync(
    productosIdCantidad);

if (disponibilidad.Succeeded && disponibilidad.Value.TodosDisponibles)
{
    // Proceder con la creación de la comanda
}
else
{
    // Mostrar productos no disponibles
    foreach (var kvp in disponibilidad.Value.ProductosNoDisponibles)
    {
        Console.WriteLine($"Producto {kvp.Key} no disponible: {kvp.Value}");
    }
}

// Calcular precios para productos de una comanda
var precios = await _coreOperacionesIntegrationService.CalcularPreciosTotalesAsync(
    productosIdCantidad);

if (precios.Succeeded)
{
    decimal total = precios.Value.PrecioTotal;
    // Usar el precio total calculado
}
```

## Manejo de errores

Todos los métodos que retornan `Result<T>` incluyen información detallada en caso de error:

```csharp
var resultado = await _coreServiceFacade.RegistrarProductoAsync(
    "Ensalada César", 
    "Ensalada clásica con lechuga romana, crutones, parmesano y aderezo César",
    -12.99m, // precio inválido
    categoriaId);

if (!resultado.Succeeded)
{
    // Acceder a los errores
    foreach (var error in resultado.Errors)
    {
        Console.WriteLine(error);
    }
}
```

## Buenas prácticas

1. **Validaciones previas**: Aunque la fachada incluye validaciones, es recomendable realizar validaciones básicas antes de llamar a los métodos para proporcionar una mejor experiencia al usuario.

2. **Transacciones**: La fachada no gestiona transacciones, es responsabilidad de la capa de aplicación manejarlas cuando se realizan múltiples operaciones que deben ser atómicas.

3. **Cachés**: Considerar implementar cachés en la capa de aplicación para operaciones de solo lectura frecuentes para mejorar el rendimiento.

4. **Gestión de IDs**: Asegúrate de que los IDs proporcionados son válidos antes de pasar al servicio (no nulos y no empty).

5. **Manejo de nulos**: La fachada está diseñada para manejar nulos de forma segura, pero es recomendable evitar pasar valores nulos cuando no corresponde.

## Resolución de problemas comunes

- **Errores de validación**: Revisa los mensajes de error retornados en `Result.Errors`.
- **Productos no encontrados**: Verifica que el ID del producto sea correcto y que el producto no esté desactivado.
- **Problemas con recetas**: Asegúrate de que todos los ingredientes referenciados existan y tengan un ID válido.
- **Errores en asignación de roles**: Verifica que el rol especificado exista en el sistema.

## Contacto y soporte

Para reportar problemas o sugerir mejoras, contacta al equipo de desarrollo en [correo@ejemplo.com](mailto:correo@ejemplo.com). 