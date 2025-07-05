# Pruebas del Módulo Core

Este directorio contiene todas las pruebas unitarias para el módulo Core del sistema RestaurantePro, siguiendo el enfoque TDD (Test-Driven Development).

## Estructura de las Pruebas

Las pruebas están organizadas siguiendo la misma estructura del dominio:

- **Base/**:
  - `EntityBaseTests.cs`: Pruebas para la clase base de entidades
  - `ValueObjectTests.cs`: Pruebas para la clase base de objetos de valor
  - `DomainEventTests.cs`: Pruebas para eventos de dominio

- **Productos/**:
  - `ProductoTests.cs`: Pruebas para la entidad Producto
  - `PrecioProductoTests.cs`: Pruebas para el value object PrecioProducto
  - `CategoriaProductoTests.cs`: Pruebas para categorías de productos

## Enfoque TDD

Las pruebas siguen el ciclo TDD:

1. **Red**: Escribir una prueba que falle
2. **Green**: Implementar el código más simple que haga pasar la prueba
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- Creación y validación de entidades base
- Comportamiento correcto de Value Objects
- Comparación de objetos (igualdad por identidad vs. igualdad por valor)
- Eventos de dominio y su manejo
- Invariantes de dominio
- Manejo de excepciones

## Casos de Prueba Principales

### EntityBase
- Generación correcta de ID
- Comparación basada en identidad
- Registro y distribución de eventos de dominio

### ValueObject
- Igualdad basada en valores
- Inmutabilidad
- Comportamiento correcto de GetHashCode()

### Productos
- Creación de productos con valores válidos
- Validación de precio y existencias
- Cambios de estado (activación, desactivación)
- Actualización de precios

## Ejecución de Pruebas

Para ejecutar las pruebas, usar el comando desde la raíz del proyecto:

```bash
dotnet test
```

O para ejecutar solo las pruebas de este módulo:

```bash
dotnet test --filter "FullyQualifiedName~Core"
``` 