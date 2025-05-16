# Core/Base - Componentes Base del Dominio

Este directorio contiene las clases base, interfaces y abstracciones fundamentales que sustentan toda la arquitectura del dominio de RestaurantePro.

## Contenido Principal

```
Base/
├── EntityBase.cs              # Clase base para todas las entidades
├── ValueObject.cs             # Clase base para objetos de valor
├── Interfaces/                # Interfaces fundamentales
│   ├── IAggregateRoot.cs      # Interface para raíces de agregado
│   ├── IDomainEvent.cs        # Interface para eventos de dominio
│   ├── IRepository.cs         # Interface base para repositorios
│   └── IUnitOfWork.cs         # Interface para unidad de trabajo
│
├── Exceptions/                # Excepciones específicas del dominio
│   ├── DomainException.cs     # Excepción base de dominio
│   └── BusinessRuleViolationException.cs
```

## Componentes Principales

### EntityBase

Clase base para todas las entidades del dominio que proporciona:
- Gestión de identidad única mediante Guid
- Implementación de Equals y GetHashCode basados en Id
- Funcionalidad para registrar y publicar eventos de dominio

### ValueObject

Clase base abstracta para todos los objetos de valor que:
- Implementa igualdad basada en la igualdad de todos los campos
- Asegura que los objetos de valor sean inmutables
- Proporciona GetHashCode basado en valores

### Interfaces

- **IAggregateRoot**: Marca las entidades que son raíces de agregado
- **IDomainEvent**: Interface base para todos los eventos de dominio
- **IRepository<T>**: Interface genérica para repositorios de agregados
- **IUnitOfWork**: Interface para coordinar transacciones

### Excepciones

- **DomainException**: Excepción base para errores de dominio
- **BusinessRuleViolationException**: Excepción específica para violaciones de reglas de negocio

## Buenas Prácticas

1. Todas las entidades deben heredar de `EntityBase`
2. Todos los objetos de valor deben heredar de `ValueObject`
3. Solo las raíces de agregado deben implementar `IAggregateRoot`
4. Utilizar eventos de dominio para comunicación entre agregados
5. Las excepciones específicas del dominio deben derivar de `DomainException` 