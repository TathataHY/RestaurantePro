# RestaurantePro.Domain

Este proyecto contiene la definición del dominio del sistema RestaurantePro, siguiendo los principios de Domain-Driven Design (DDD) con una arquitectura limpia (Clean Architecture).

## Estructura del Proyecto

```
RestaurantePro.Domain/
├── Core/                      # Elementos centrales compartidos y base
│   ├── Base/                  # Clases e interfaces base
│   ├── BoundedContexts/       # Definición de contextos y sus límites
│   ├── SharedKernel/          # Elementos compartidos entre todos los contextos
│   └── Productos/             # Catálogo de productos del restaurante
│
├── Operaciones/               # Contexto de operaciones del restaurante
│   ├── Comandas/              # Gestión de comandas (órdenes)
│   └── Reservaciones/         # Gestión de reservaciones de mesas
│
├── Comercial/                 # Contexto comercial del restaurante
│   └── Clientes/              # Gestión de clientes
│
└── GlobalUsings.cs            # Importaciones globales para el proyecto
```

## Principios Arquitectónicos

1. **Domain-Driven Design (DDD)**: El dominio es el núcleo del sistema
2. **Clean Architecture**: Capas bien definidas con dependencias hacia el interior
3. **SOLID**: Principios aplicados en todas las clases e interfaces
4. **Bounded Contexts**: Contextos bien delimitados con lenguaje ubicuo propio
5. **Event-Driven**: Uso de eventos de dominio para comunicación entre agregados
6. **Inmutabilidad**: Value Objects inmutables para conceptos del dominio

## Conceptos DDD Implementados

- **Entidades**: Objetos con identidad y ciclo de vida
- **Value Objects**: Objetos inmutables sin identidad propia
- **Agregados**: Cluster de objetos tratados como una unidad para cambios
- **Eventos de Dominio**: Notificaciones de cambios importantes en el dominio
- **Repositorios**: Abstracciones para persistencia de agregados
- **Servicios de Dominio**: Operaciones que no pertenecen naturalmente a entidades
- **Especificaciones**: Reglas de negocio encapsuladas y reutilizables

## Patrones Utilizados

- **Factory Methods**: Para creación controlada de entidades
- **Specification Pattern**: Para validaciones complejas
- **Repository Pattern**: Abstracción de la persistencia
- **Domain Events**: Para comunicación desacoplada
- **Aggregate Root**: Para encapsulación y consistencia

## Cómo Trabajar con este Proyecto

1. Entender el **Bounded Context** en el que se va a trabajar
2. Revisar los **agregados** existentes y sus responsabilidades
3. Respetar la **encapsulación** de los agregados
4. Asegurar que toda la **lógica de negocio** esté en el dominio
5. Implementar nuevas funcionalidades usando **TDD**
6. Utilizar los **eventos de dominio** para comunicación entre agregados
