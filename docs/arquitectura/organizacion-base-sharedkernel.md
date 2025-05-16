# Organización entre Core/Base y SharedKernel

## Propósito de este documento

Este documento explica la diferencia conceptual y práctica entre los componentes Core/Base y SharedKernel en la arquitectura de RestaurantePro. Sirve como guía para decidir dónde colocar nuevos componentes y cómo mantener una buena organización del código.

## Conceptos Básicos

### Core/Base

Core/Base contiene las **abstracciones fundamentales del dominio** que son utilizadas específicamente dentro de nuestro dominio. Estos componentes son la base sobre la que se construye nuestro modelo de dominio específico.

Ejemplos:
- EntityBase (clase base para todas las entidades)
- ValueObject (clase base para objetos de valor)
- DomainEvent (abstracción para eventos de dominio)
- IAggregateRoot (interfaz para raíces de agregado)

### SharedKernel

SharedKernel contiene **abstracciones y componentes compartidos** entre diferentes dominios o módulos. Son elementos que trascienden un solo contexto acotado y pueden ser utilizados en diferentes partes del sistema o incluso en diferentes sistemas.

Ejemplos:
- IRepository (interfaz genérica para repositorios)
- IUnitOfWork (interfaz para transacciones)
- Money (objeto de valor para manejo de dinero)
- Email (objeto de valor para correos electrónicos)

## Diferencias Clave

| Característica | Core/Base | SharedKernel |
|----------------|-----------|--------------|
| **Enfoque** | Específico del dominio | Genérico y compartido |
| **Alcance** | Interno al dominio | Puede traspasar límites de dominio |
| **Dependencias** | Mínimas o ninguna | Mínimas o ninguna |
| **Estabilidad** | Puede evolucionar con el dominio | Debe ser muy estable |
| **Reusabilidad** | Dentro del dominio | Entre múltiples contextos |

## Reglas para decidir dónde colocar componentes

### Colocar en Core/Base si:

1. El componente es fundamental para construir el modelo de dominio
2. Es específico de cómo modelamos nuestro dominio
3. No tiene sentido fuera del contexto de nuestro sistema
4. Es una abstracción base sobre la que se construyen las entidades y objetos de valor

### Colocar en SharedKernel si:

1. El componente es utilizado en múltiples contextos acotados
2. Representa un concepto universal (dinero, tiempo, ubicación, etc.)
3. Es una abstracción de infraestructura (repositorios, unidad de trabajo)
4. Proporciona funcionalidad común sin conocer detalles específicos del dominio

## Interfaces de Repositorio

Un caso particular es la organización de las interfaces de repositorio:

- **IRepository<T>**: Interfaz genérica que va en SharedKernel
- **IClienteRepository**, **IProductoRepository**, etc.: Interfaces específicas que van en sus respectivos módulos de dominio

Cada módulo de dominio define sus propias interfaces específicas de repositorio que extienden o se basan en la interfaz genérica de SharedKernel:

```csharp
// En Core/SharedKernel
public interface IRepository<T> { ... }

// En Comercial/Clientes/Interfaces
public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente> ObtenerPorEmail(string email);
    // Métodos específicos para el repositorio de clientes
}
```

## Buenas Prácticas

1. Mantener SharedKernel lo más pequeño posible
2. No incluir lógica de negocio específica en SharedKernel
3. Los cambios en SharedKernel deben ser muy cuidadosos ya que afectan a todo el sistema
4. Preferir composición sobre herencia cuando sea posible
5. Las interfaces en SharedKernel deben ser estables y bien definidas

## Diagramas de Organización

```
Core/
├── Base/                      # Abstracciones específicas del dominio
│   ├── EntityBase.cs          # Base para entidades
│   ├── ValueObject.cs         # Base para objetos de valor
│   └── Interfaces/            # Interfaces específicas del dominio
│       ├── IAggregateRoot.cs  # Interfaz para raíces de agregado
│       └── IDomainEvent.cs    # Interfaz para eventos de dominio
│
└── SharedKernel/              # Componentes compartidos
    ├── ValueObjects/          # Objetos de valor universales
    │   ├── Money.cs           # Representa dinero con moneda
    │   └── Email.cs           # Representa emails
    │
    └── Interfaces/            # Interfaces universales
        ├── IRepository.cs     # Interfaz genérica para repositorios
        └── IUnitOfWork.cs     # Interfaz para transacciones
``` 