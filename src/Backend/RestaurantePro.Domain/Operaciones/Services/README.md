# Servicios de Integración en el Dominio Operaciones

## OperacionesInventarioIntegrationService

El servicio `OperacionesInventarioIntegrationService` implementa el patrón de Capa Anticorrupción (Anticorruption Layer) para mantener la integridad entre los contextos de Operaciones e Inventario.

### Propósito

Este servicio abstrae todas las interacciones entre los módulos de Operaciones (comandas, reservas, etc.) e Inventario (ingredientes, stock, etc.), permitiendo que ambos contextos evolucionen de manera independiente sin acoplarse directamente entre sí.

### Principales funcionalidades

1. **Verificación de disponibilidad**
   - Comprueba si hay suficiente stock de ingredientes para una comanda
   - Proporciona información detallada sobre productos no disponibles e ingredientes faltantes

2. **Reserva de ingredientes**
   - Reserva temporalmente los ingredientes necesarios para una comanda
   - Decrementa el stock de los ingredientes correspondientes

3. **Confirmación de consumo**
   - Confirma el consumo definitivo de ingredientes cuando se entrega la comanda
   - Registra el movimiento para fines de auditoría

4. **Liberación de reservas**
   - Libera la reserva de ingredientes cuando se cancela una comanda
   - Incrementa el stock de los ingredientes correspondientes
   - Registra el motivo de la cancelación

### Uso en manejadores de eventos

El servicio está diseñado para ser utilizado en los siguientes manejadores de eventos:

1. **ComandaCreada_VerificarDisponibilidadHandler**
   ```csharp
   // Verificar disponibilidad de ingredientes al crear una comanda
   var resultado = await _integrationService.VerificarDisponibilidadIngredientesComandaAsync(comandaId);
   if (resultado.Succeeded && resultado.Value.TodosDisponibles) {
       // Proceder con la creación de la comanda
   }
   ```

2. **ComandaModificada_ActualizarInventarioHandler**
   ```csharp
   // Reservar ingredientes al agregar un producto a la comanda
   await _integrationService.ReservarIngredientesComandaAsync(comandaId);
   
   // Liberar ingredientes al eliminar un producto de la comanda
   await _integrationService.LiberarReservaIngredientesAsync(comandaId, "Cliente canceló el producto");
   ```

3. **ItemComandaEntregado_ActualizarInventarioHandler**
   ```csharp
   // Confirmar el consumo de ingredientes cuando se entrega un ítem
   await _integrationService.ConfirmarConsumoIngredientesAsync(comandaId);
   ```

### Ventajas del enfoque

- **Desacoplamiento**: Operaciones e Inventario no dependen directamente entre sí
- **Adaptabilidad**: Se pueden cambiar las implementaciones de cualquier contexto sin afectar al otro
- **Responsabilidad única**: Cada contexto se enfoca en su propia lógica de negocio
- **Integridad**: Se preserva la integridad de los datos entre contextos
- **Testabilidad**: Facilita la escritura de pruebas unitarias para cada contexto por separado

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