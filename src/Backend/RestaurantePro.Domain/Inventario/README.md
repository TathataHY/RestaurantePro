# Módulo Inventario - Domain

Este módulo contiene los elementos del dominio relacionados con la gestión de inventario, ingredientes, compras y proveedores, siguiendo los principios de Domain-Driven Design (DDD).

## Estructura del Módulo Inventario

```
Inventario/
├── Entities/              # Entidades del dominio (Ingrediente, MovimientoInventario, etc.)
├── ValueObjects/          # Value Objects específicos (Cantidad, etc.)
├── Events/                # Eventos de dominio relacionados con inventario
├── Interfaces/            # Interfaces de repositorios y servicios
├── Enums/                 # Enumeraciones específicas
└── Services/              # Servicios de dominio para inventario
```

## Subcontextos

### Ingredientes

Maneja la definición de ingredientes, sus propiedades, unidades de medida y stock:

- Creación y mantenimiento de ingredientes
- Control de stock mínimo y actual
- Asociación con productos/recetas

**Estado actual**: ✅ Implementado

### Movimientos de Inventario

Registra todos los cambios en el inventario:

- Entradas de stock (compras, ajustes)
- Salidas de stock (uso en comandas, mermas)
- Auditoría de cambios en el tiempo

**Estado actual**: ✅ Implementado

La entidad `MovimientoInventario` permite:
- Crear movimientos de ingreso y egreso con motivo
- Aplicar los movimientos al stock de ingredientes
- Validar reglas de negocio (no permitir stock negativo)
- Mantener un historial de cambios de stock

### Proveedores y Compras

Gestiona los proveedores y el proceso de compra:

- Registro de proveedores
- Órdenes de compra
- Recepción de productos

**Estado actual**: ⏳ Pendiente

## Principios implementados

1. **Agregados**: Entidades principales como `Ingrediente` y `OrdenCompra` como raíces de agregado
2. **Value Objects**: Conceptos inmutables como `Cantidad` con su unidad de medida
3. **Invariantes de dominio**: Reglas de negocio como "no permitir stock negativo"
4. **Eventos de dominio**: Notificaciones sobre cambios en el inventario
5. **Servicios de dominio**: Lógica que involucra múltiples agregados

## Relación con otros módulos

- Se integra con **Operaciones/Comandas** para descontar ingredientes al crear comandas
- Se relaciona con **Core/Productos** para asociar ingredientes a productos/recetas
- Puede integrarse con módulos futuros como **Finanzas** para costos y valoración de inventario
