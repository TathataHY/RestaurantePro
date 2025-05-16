# Seguimiento del Desarrollo TDD - Capa de Dominio

## Propósito de este documento

Este documento sirve como guía y registro del desarrollo de la capa de dominio utilizando Test-Driven Development (TDD). Aquí encontrarás:

- El estado actual de implementación de cada módulo
- Las relaciones entre los diferentes contextos y agregados
- El seguimiento del proceso TDD aplicado
- La planificación de próximos desarrollos

## Estructura General del Dominio

```
RestaurantePro.Domain/
├── Core/                 # Componentes base y compartidos
├── Comercial/            # Gestión de clientes y fidelización
├── Operaciones/          # Comandas y reservaciones
└── [Pendiente] Inventario/  # Gestión de inventario y compras
```

## Estado de Implementación

### Core

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| EntityBase | ✅ Completo | ✅ Completas | Base para todas las entidades |
| ValueObject | ✅ Completo | ✅ Completas | Base para objetos de valor |
| DomainEvent | ✅ Completo | ✅ Completas | Eventos de dominio |
| Productos | ✅ Completo | ✅ Completas | Catálogo de productos |

### Comercial

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Cliente | ✅ Completo | ✅ Completas | Gestión de clientes |
| ClienteNombre | ✅ Completo | ✅ Completas | Value Object para nombres |
| TarjetaFidelizacion | ✅ Completo | ✅ Completas | Programa de fidelización |
| HistorialPuntos | ✅ Completo | ✅ Completas | Registro de puntos de fidelización |

### Operaciones

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Comanda | ✅ Completo | ✅ Completas | Gestión de órdenes |
| ItemComanda | ✅ Completo | ✅ Completas | Elementos de una comanda |
| Reservacion | ✅ Completo | ✅ Completas | Reservación de mesas |
| Mesa | ✅ Completo | ✅ Completas | Gestión de mesas |

### Inventario (Pendiente)

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Ingrediente | ⏳ Pendiente | ⏳ Pendiente | Materias primas |
| MovimientoInventario | ⏳ Pendiente | ⏳ Pendiente | Registro de movimientos |
| OrdenCompra | ⏳ Pendiente | ⏳ Pendiente | Órdenes a proveedores |
| Proveedor | ⏳ Pendiente | ⏳ Pendiente | Gestión de proveedores |

## Relaciones entre Contextos

### Comercial ↔ Operaciones
- Cliente puede hacer Reservaciones
- Cliente acumula puntos por Comandas
- TarjetaFidelizacion aplicable a Comandas

### Operaciones ↔ Core
- Comanda contiene Productos
- Reservacion asigna Mesas

### [Pendiente] Operaciones ↔ Inventario
- Comanda reduce stock de Ingredientes
- ItemComanda verifica disponibilidad de Ingredientes

## Próximos pasos (TDD)

1. **Implementar módulo de Inventario**:
   - Crear pruebas para Ingrediente
   - Implementar entidad Ingrediente
   - Crear pruebas para MovimientoInventario
   - Implementar entidad MovimientoInventario

2. **Implementar servicios de dominio**:
   - Servicio para verificar disponibilidad de ingredientes
   - Servicio para aplicar descuentos de fidelización

3. **Completar validaciones**:
   - Reglas de negocio para límites de reservas
   - Validaciones para creación de comandas

## Registro de ciclos TDD completados

| Fecha | Componente | Test → Implementación → Refactor |
|-------|------------|----------------------------------|
| [Pendiente] | - | - |

## Decisiones de Diseño

- Las entidades usan Factory Methods (Crear) en lugar de constructores públicos
- Se utiliza encapsulación estricta con propiedades privadas (set privado)
- Los cambios de estado se realizan mediante métodos específicos
- Cada cambio de estado genera eventos de dominio
- Se priorizan objetos inmutables para valores
