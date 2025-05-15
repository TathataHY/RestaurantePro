# Módulo Comandas - Domain

Este módulo contiene las entidades, value objects, eventos y definiciones del dominio para el contexto de Comandas, siguiendo los principios de Domain-Driven Design (DDD).

## Estructura de carpetas del Dominio

```
Comandas/
├── Entities/                   # Entidades del dominio de comandas
│   ├── Comanda.cs             # Aggregate Root
│   └── ItemComanda.cs         # Entidad dentro del agregado
│
├── ValueObjects/               # Value Objects del dominio
│   └── TotalComanda.cs        # VO para totales (inmutable)
│
├── Events/                     # Eventos de dominio
│   ├── ComandaCreada.cs
│   ├── ProductoAgregadoAComanda.cs
│   ├── EstadoComandaActualizado.cs
│   ├── ComandaFinalizada.cs
│   └── ComandaCancelada.cs
│
├── Interfaces/                 # Interfaces del dominio (repositorios)
│   └── IComandaRepository.cs
│
└── Enums/                      # Enumeraciones
    └── EstadoComanda.cs
```

## Vertical Slices en Application

Los Vertical Slices (cortes verticales por funcionalidad) se implementarán en la capa de **Application**, no en Domain. La estructura será:

```
Application/
└── Operaciones/
    └── Comandas/
        ├── CrearComanda/
        │   ├── CrearComandaCommand.cs
        │   ├── CrearComandaValidator.cs
        │   └── CrearComandaHandler.cs
        │
        ├── ActualizarEstadoComanda/
        │   ├── ActualizarEstadoComandaCommand.cs
        │   ├── ActualizarEstadoComandaValidator.cs
        │   └── ActualizarEstadoComandaHandler.cs
        │
        ├── AgregarProductoComanda/
        │   ├── AgregarProductoComandaCommand.cs
        │   ├── AgregarProductoComandaValidator.cs
        │   └── AgregarProductoComandaHandler.cs
        │
        ├── CancelarComanda/
        │   ├── CancelarComandaCommand.cs
        │   ├── CancelarComandaValidator.cs
        │   └── CancelarComandaHandler.cs
        │
        └── FinalizarComanda/
            ├── FinalizarComandaCommand.cs
            ├── FinalizarComandaValidator.cs
            └── FinalizarComandaHandler.cs
```

## Beneficios de esta estructura

1. **Dominio puro**: El dominio contiene solo reglas de negocio, sin detalles de implementación
2. **Vertical Slices en Application**: Organiza los casos de uso por funcionalidad
3. **Separación clara**: Cada capa tiene su responsabilidad bien definida
4. **Desarrollo en paralelo**: Múltiples desarrolladores pueden trabajar en diferentes funcionalidades
