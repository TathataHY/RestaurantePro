# Pruebas del Módulo Core/Base

Este directorio contiene todas las pruebas unitarias para el módulo Core/Base del sistema RestaurantePro, que incluye las clases e interfaces fundamentales que sustentan toda la arquitectura del dominio.

## Estructura de las Pruebas

Las pruebas están organizadas siguiendo la misma estructura del dominio:

- **Clases Base**:
  - `EntityBaseTests.cs`: Pruebas para la clase base de entidades
  - `ValueObjectTests.cs`: Pruebas para la clase base de objetos de valor

- **Interfaces/**:
  - `IAggregateRootTests.cs`: Pruebas para la interfaz de raíz de agregado
  - `IDomainEventTests.cs`: Pruebas para la interfaz de eventos de dominio

## Enfoque TDD

Las pruebas siguen el ciclo TDD:

1. **Red**: Escribir una prueba que falle
2. **Green**: Implementar el código más simple que haga pasar la prueba
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- **EntityBase**:
  - Generación correcta de ID
  - Comparación basada en identidad
  - Registro y publicación de eventos de dominio
  - Comportamiento de colecciones de entidades

- **ValueObject**:
  - Implementación de igualdad basada en valor
  - Implementación correcta de GetHashCode
  - Inmutabilidad de los objetos de valor
  - Comportamiento de colecciones de objetos de valor

- **Interfaces**:
  - Implementaciones correctas de las interfaces
  - Casos de uso de las interfaces en diferentes contextos

## Casos de Prueba Principales

### EntityBase
- Creación de entidad con ID generado automáticamente
- Creación de entidad con ID específico
- Comparación de entidades (igualdad por identidad)
- Registro de eventos de dominio
- Liberación de eventos de dominio
- Comportamiento en colecciones (Dictionary, HashSet)

### ValueObject
- Igualdad entre objetos con los mismos valores
- Desigualdad entre objetos con valores diferentes
- Consistencia de GetHashCode con Equals
- Comportamiento en colecciones (Dictionary, HashSet)
- Inmutabilidad (verificación de que no pueden modificarse)

### Interfaces de Dominio
- Marcado correcto de interfaces como IDomainEvent
- Implementación correcta de IAggregateRoot

## Importancia de estas Pruebas

Estas pruebas son fundamentales ya que verifican el comportamiento de las clases e interfaces más básicas del sistema, que son utilizadas por todos los módulos. Cualquier error en estos componentes base se propagaría a través de toda la aplicación.

## Ejecución de Pruebas

Para ejecutar las pruebas, usar el comando desde la raíz del proyecto:

```bash
dotnet test
```

O para ejecutar solo las pruebas de este módulo:

```bash
dotnet test --filter "FullyQualifiedName~Core.Base"
``` 