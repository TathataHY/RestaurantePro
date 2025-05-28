# Servicios de Integración de Operaciones

Este directorio contiene los servicios que actúan como intermediarios entre el contexto de Operaciones y otros contextos del dominio.

## OperacionesInventarioIntegrationService

### Propósito

El servicio `OperacionesInventarioIntegrationService` implementa el patrón Anticorruption Layer (ACL) entre los contextos de Operaciones e Inventario. Su objetivo principal es gestionar la integración entre comandas (órdenes) y el inventario de ingredientes, permitiendo:

1. Verificar la disponibilidad de ingredientes para una comanda
2. Reservar ingredientes cuando se crea una comanda
3. Confirmar el consumo de ingredientes cuando se entregan productos
4. Liberar reservas de ingredientes cuando se cancelan ítems

### Funcionalidades principales

#### Verificación de disponibilidad

```csharp
// Verifica si hay suficientes ingredientes para todos los productos de una comanda
var resultado = await _operacionesInventarioIntegrationService.VerificarDisponibilidadIngredientesComandaAsync(comandaId);

if (resultado.Succeeded && resultado.Value.TodosDisponibles)
{
    // Hay disponibilidad para todos los productos
}
else
{
    // Obtener lista de ingredientes faltantes
    foreach (var faltante in resultado.Value.IngredientesFaltantes)
    {
        Console.WriteLine($"Falta {faltante.Value} de {faltante.Key}");
    }
    
    // Obtener productos no disponibles
    foreach (var noDisponible in resultado.Value.ProductosNoDisponibles)
    {
        Console.WriteLine($"Producto {noDisponible.Key} no disponible: {noDisponible.Value}");
    }
}
```

#### Reserva de ingredientes

```csharp
// Reserva los ingredientes necesarios para una comanda (reduce temporalmente el stock)
var reservaResult = await _operacionesInventarioIntegrationService.ReservarIngredientesComandaAsync(comandaId);

if (reservaResult.Succeeded)
{
    // Reserva exitosa
}
else
{
    // Error en la reserva
    Console.WriteLine(reservaResult.Errors.First().Message);
}
```

#### Confirmación de consumo

```csharp
// Confirma el consumo definitivo de ingredientes para una comanda entregada
var consumoResult = await _operacionesInventarioIntegrationService.ConfirmarConsumoIngredientesAsync(comandaId);

if (consumoResult.Succeeded)
{
    // Consumo registrado correctamente
}
```

#### Liberación de reserva

```csharp
// Libera la reserva de ingredientes para una comanda cancelada
string motivo = "Cancelación por cliente";
var liberacionResult = await _operacionesInventarioIntegrationService.LiberarReservaIngredientesAsync(comandaId, motivo);

if (liberacionResult.Succeeded)
{
    // Reserva liberada correctamente, ingredientes devueltos al stock
}
```

### Registro del servicio

El servicio está registrado en el contenedor de dependencias en la clase `DomainServiceCollection`:

```csharp
services.AddScoped<IOperacionesInventarioIntegrationService, OperacionesInventarioIntegrationService>();
```

### Patrones implementados

1. **Anticorruption Layer**: Protege la integridad de ambos contextos (Operaciones e Inventario)
2. **Facade**: Proporciona una interfaz unificada para todas las operaciones relacionadas
3. **Result Pattern**: Todos los métodos devuelven objetos `Result<T>` para manejar errores de forma elegante
4. **Repository**: Utiliza repositorios para acceder a las entidades de ambos contextos

### Beneficios

- **Desacoplamiento**: Los contextos no dependen directamente uno de otro
- **Cohesión**: Cada contexto mantiene su responsabilidad específica
- **Transaccionalidad**: Las operaciones que afectan a ambos contextos se realizan de forma consistente
- **Mantenibilidad**: Facilita los cambios en cualquiera de los contextos sin afectar al otro

### Diagrama simplificado

```
Operaciones                      ACL                         Inventario
+------------------+    +---------------------+    +------------------+
| Comanda          |    |                     |    | Ingrediente      |
| ItemComanda      | -> | OperacionesInventarioIntegrationService | -> | Stock            |
| EventHandlers    |    |                     |    | MovimientoInventario |
+------------------+    +---------------------+    +------------------+
```

### Ejemplo de resultado de verificación

```csharp
var resultado = await _integrationService.VerificarDisponibilidadIngredientesComandaAsync(comandaId);

if (resultado.Succeeded) {
    if (resultado.Value.TodosDisponibles) {
        Console.WriteLine("Todos los ingredientes están disponibles");
    } else {
        Console.WriteLine("Faltan los siguientes ingredientes:");
        foreach (var item in resultado.Value.IngredientesFaltantes) {
            Console.WriteLine($"- {item.Key}: Falta {item.Value}");
        }
        
        Console.WriteLine("Los siguientes productos no están disponibles:");
        foreach (var item in resultado.Value.ProductosNoDisponibles) {
            Console.WriteLine($"- Producto {item.Key}: {item.Value}");
        }
    }
}
``` 