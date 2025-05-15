# Módulo Clientes

Este módulo implementa patrones tácticos de Domain-Driven Design (DDD) para gestionar toda la lógica relacionada con clientes y fidelización.

## Estructura de Directorios

- **Entities/**: Entidades tradicionales (Cliente, TarjetaFidelizacion, etc.)
- **ValueObjects/**: Objetos de valor inmutables (ClienteNombre)
- **Events/**: Eventos de dominio para operaciones con clientes
- **Enums/**: Enumeraciones relacionadas con clientes
- **Interfaces/**: Contratos de repositorios y servicios

## Patrones DDD Implementados

### Value Objects
Objetos inmutables que encapsulan características con validaciones propias.
Ejemplo: `ClienteNombre` encapsula nombre y apellido con sus validaciones.

### Entidades y Agregados
Las entidades y agregados mantienen la consistencia del dominio.
Ejemplo: `Cliente` con sus operaciones y entidades relacionadas.

### Eventos de Dominio
Notifican cambios importantes en el estado del dominio.
Ejemplos: `ClienteCreadoEvent`, `PuntosAgregadosEvent`

### Especificaciones (Futuro)
Reglas de negocio encapsuladas como objetos independientes.
Ejemplo: `ClientePreferencialSpecification` determinará si un cliente es preferencial. 