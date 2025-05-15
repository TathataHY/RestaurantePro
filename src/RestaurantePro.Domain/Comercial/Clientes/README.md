# Módulo Clientes

Este módulo implementa patrones tácticos de Domain-Driven Design (DDD) para gestionar toda la lógica relacionada con clientes y fidelización.

## Estructura de carpetas

- **Entities/**: Entidades tradicionales (Cliente, TarjetaFidelizacion, etc.)
- **ValueObjects/**: Objetos de valor inmutables (ClienteNombre)
- **Aggregates/**: Raíces de agregados que garantizan consistencia (ClienteAggregate)
- **DomainEvents/**: Eventos de dominio para comunicación entre módulos
- **Specifications/**: Encapsulación de reglas de negocio reutilizables
- **Services/**: Servicios que operan sobre varios agregados
- **Enums/**: Enumeraciones relacionadas con clientes
- **Interfaces/**: Interfaces para repositorios y servicios

## Patrones implementados

### ValueObject
Objetos inmutables identificados por el valor de sus atributos, no por identidad.
Ejemplo: \ClienteNombre\ encapsula nombre y apellido con sus validaciones.

### Aggregate
Agrupación de entidades y objetos de valor que mantiene consistencia.
Ejemplo: \ClienteAggregate\ controla todas las operaciones sobre un cliente.

### Domain Events
Notificaciones sobre cambios importantes en el dominio.
Ejemplos: \ClienteCreadoEvent\, \PuntosAgregadosEvent\

### Specification
Encapsula reglas de negocio reutilizables y combinables.
Ejemplo: \ClientePreferencialSpecification\ determina si un cliente es preferencial.

### Domain Service
Operaciones de dominio que no pertenecen a una sola entidad.
Ejemplo: \ClienteFidelizacionService\ calcula nivel de fidelización basado en múltiples factores.
