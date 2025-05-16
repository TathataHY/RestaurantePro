# Módulo Operaciones - Domain

Este módulo contiene los diferentes contextos de negocio relacionados con las operaciones del restaurante, siguiendo los principios de Domain-Driven Design (DDD).

## Estructura del Módulo Operaciones

```
Operaciones/
├── Comandas/                   # Gestión de comandas (órdenes) del restaurante
│   ├── Entities/               # Entidades del dominio (Comanda, ItemComanda)
│   ├── ValueObjects/           # Value Objects específicos de comandas
│   ├── Events/                 # Eventos de dominio de comandas
│   ├── Interfaces/             # Interfaces de repositorios y servicios
│   └── Enums/                  # Enumeraciones del dominio
│
└── Reservaciones/              # Gestión de reservaciones de mesas
    ├── Entities/               # Entidades de reservaciones
    ├── ValueObjects/           # Value Objects de reservaciones
    ├── Events/                 # Eventos de dominio de reservaciones
    ├── Interfaces/             # Interfaces de repositorios y servicios
    └── Enums/                  # Enumeraciones específicas
```

## Contexto Comandas

Este contexto maneja todo lo relacionado con las órdenes o comandas que se generan cuando un cliente ordena productos en el restaurante. Incluye:

- Creación y gestión del ciclo de vida de las comandas
- Seguimiento de estados (creada, en proceso, lista, entregada, etc.)
- Cálculo de totales e impuestos
- Gestión de productos asociados a la comanda

## Contexto Reservaciones

Este contexto maneja la reservación de mesas en el restaurante, incluyendo:

- Programación de reservaciones
- Asignación de mesas
- Confirmación y cancelación de reservaciones
- Gestión de disponibilidad

## Principios implementados

1. **Aggregate Roots**: Entidades principales como `Comanda` y `Reservacion`
2. **Encapsulación**: Toda la lógica de negocio está dentro de las entidades
3. **Inmutabilidad**: Value Objects inmutables para conceptos como `TotalComanda`
4. **Eventos de dominio**: Comunicación dentro y entre agregados mediante eventos
5. **Repositorios**: Interfaces para persistencia específicas a cada agregado

## Relación con otros módulos

- Este módulo forma parte del **Bounded Context** de Operaciones
- Se integra con el módulo **Comercial** para la gestión de clientes
- Utiliza los **Productos** definidos en el módulo Core 