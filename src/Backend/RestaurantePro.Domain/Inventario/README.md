# Mu00f3dulo Inventario - Domain

Este mu00f3dulo contiene los elementos del dominio relacionados con la gestiu00f3n de inventario, ingredientes, compras y proveedores, siguiendo los principios de Domain-Driven Design (DDD).

## Estructura del Mu00f3dulo Inventario

```
Inventario/
u251cu2500u2500 Entities/              # Entidades del dominio (Ingrediente, MovimientoInventario, etc.)
u251cu2500u2500 ValueObjects/          # Value Objects especu00edficos (Cantidad, etc.)
u251cu2500u2500 Events/               # Eventos de dominio relacionados con inventario
u251cu2500u2500 Interfaces/           # Interfaces de repositorios y servicios
u251cu2500u2500 Enums/                # Enumeraciones especu00edficas
u2514u2500u2500 Services/             # Servicios de dominio para inventario
```

## Subcontextos

### Ingredientes

Maneja la definiciu00f3n de ingredientes, sus propiedades, unidades de medida y stock:

- Creaciu00f3n y mantenimiento de ingredientes
- Control de stock mu00ednimo y actual
- Asociaciu00f3n con productos/recetas

### Movimientos de Inventario

Registra todos los cambios en el inventario:

- Entradas de stock (compras, ajustes)
- Salidas de stock (uso en comandas, mermas)
- Auditoru00eda de cambios en el tiempo

### Proveedores y Compras

Gestiona los proveedores y el proceso de compra:

- Registro de proveedores
- u00d3rdenes de compra
- Recepciu00f3n de productos

## Principios implementados

1. **Agregados**: Entidades principales como `Ingrediente` y `OrdenCompra` como rau00edces de agregado
2. **Value Objects**: Conceptos inmutables como `Cantidad` con su unidad de medida
3. **Invariantes de dominio**: Reglas de negocio como "no permitir stock negativo"
4. **Eventos de dominio**: Notificaciones sobre cambios en el inventario
5. **Servicios de dominio**: Lu00f3gica que involucra mu00faltiples agregados

## Relaciu00f3n con otros mu00f3dulos

- Se integra con **Operaciones/Comandas** para descontar ingredientes al crear comandas
- Se relaciona con **Core/Productos** para asociar ingredientes a productos/recetas
- Puede integrarse con mu00f3dulos futuros como **Finanzas** para costos y valoraciu00f3n de inventario
