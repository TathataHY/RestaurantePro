# Pruebas del Módulo Core/Productos

Este directorio contiene todas las pruebas unitarias para el módulo Core/Productos del sistema RestaurantePro, siguiendo el enfoque TDD (Test-Driven Development).

## Estructura de las Pruebas

Las pruebas están organizadas siguiendo la misma estructura del dominio:

- **Entities/**:
  - `ProductoTests.cs`: Pruebas para la entidad Producto
  - `CategoriaProductoTests.cs`: Pruebas para la entidad CategoriaProducto

- **ValueObjects/**:
  - `PrecioProductoTests.cs`: Pruebas para el value object PrecioProducto
  - `DescripcionProductoTests.cs`: Pruebas para el value object DescripcionProducto

- `Events/`: Pruebas para los eventos de dominio relacionados con productos

## Enfoque TDD

Las pruebas siguen el ciclo TDD:

1. **Red**: Escribir una prueba que falle
2. **Green**: Implementar el código más simple que haga pasar la prueba
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- Creación y validación de productos
- Reglas de negocio sobre precios y categorías
- Comportamiento de los value objects
- Cambios de estado (activación/desactivación)
- Eventos de dominio generados

## Casos de Prueba Principales

### Entidad Producto
- Creación de productos con datos válidos e inválidos
- Cambios de precio
- Actualización de información
- Activación y desactivación
- Categorización

### Value Objects
- PrecioProducto: validación de valores permitidos
- DescripcionProducto: validación de formato y longitud

### Eventos
- Verificación de que se generen los eventos apropiados:
  - ProductoCreado
  - ProductoActualizado
  - ProductoActivado
  - ProductoDesactivado

## Ejecución de Pruebas

Para ejecutar las pruebas, usar el comando desde la raíz del proyecto:

```bash
dotnet test
```

O para ejecutar solo las pruebas de este módulo:

```bash
dotnet test --filter "FullyQualifiedName~Core.Productos"
```

## Utilidades de Prueba

- **Fixtures**: Configuraciones reutilizables para pruebas
- **Builders**: Implementación del patrón Builder para crear instancias de prueba
- **Comparadores personalizados**: Para verificar igualdad semántica 