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
├── Inventario/           # Gestión de inventario y compras
└── Proveedores/          # Gestión de proveedores
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

### Inventario

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Ingrediente | ✅ Completo | ✅ Completas | Materias primas |
| MovimientoInventario | ✅ Completo | ✅ Completas | Registro de movimientos |
| OrdenCompra | ✅ Completo | ✅ Completas | Órdenes a proveedores |

### Proveedores

| Componente | Estado | Pruebas | Notas |
|------------|--------|---------|-------|
| Proveedor | ✅ Completo | ✅ Completas | Gestión de proveedores |
| ContactoProveedor | ✅ Completo | ✅ Completas | Contactos de proveedores |

## Relaciones entre Contextos

### Comercial ↔ Operaciones
- Cliente puede hacer Reservaciones
- Cliente acumula puntos por Comandas
- TarjetaFidelizacion aplicable a Comandas

### Operaciones ↔ Core
- Comanda contiene Productos
- Reservacion asigna Mesas

### Operaciones ↔ Inventario
- Comanda reduce stock de Ingredientes
- ItemComanda verifica disponibilidad de Ingredientes

### Inventario ↔ Proveedores
- OrdenCompra se genera para un Proveedor específico
- Proveedor suministra Ingredientes

## Próximos pasos (TDD)

1. **Completar integración entre contextos**:
   - Implementar servicios de dominio para la integración entre contextos
   - Crear eventos de integración entre Inventario y Proveedores
   - Implementar políticas de manejo de eventos entre contextos

2. **Implementar servicios de dominio**:
   - Servicio para verificar disponibilidad de ingredientes
   - Servicio para aplicar descuentos de fidelización
   - Servicio para gestión de órdenes de compra automáticas

3. **Completar validaciones**:
   - Reglas de negocio para límites de reservas
   - Validaciones para creación de comandas
   - Validaciones para gestión de proveedores

## Registro de ciclos TDD completados

| Fecha | Componente | Test → Implementación → Refactor |
|-------|------------|----------------------------------|
| 2023-10-15 | Ingrediente | Pruebas → Implementación → Refactor |
| 2023-10-20 | MovimientoInventario | Pruebas → Implementación → Refactor |
| 2023-10-25 | OrdenCompra | Pruebas → Implementación → Refactor |
| 2023-11-05 | Proveedor | Pruebas → Implementación → Refactor |

## Decisiones de Diseño

- Las entidades usan Factory Methods (Crear) en lugar de constructores públicos
- Se utiliza encapsulación estricta con propiedades privadas (set privado)
- Los cambios de estado se realizan mediante métodos específicos
- Cada cambio de estado genera eventos de dominio
- Se priorizan objetos inmutables para valores
- Se separan interfaces de repositorio por contexto
