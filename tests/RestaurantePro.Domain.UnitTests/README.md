# RestaurantePro.Domain.UnitTests

Este proyecto contiene todas las pruebas unitarias para el dominio del sistema RestaurantePro, implementadas siguiendo el enfoque TDD (Test-Driven Development).

## Estructura del Proyecto

```
RestaurantePro.Domain.UnitTests/
├── Core/                      # Pruebas para el módulo Core
│   ├── Base/                  # Pruebas para las clases base
│   └── Productos/             # Pruebas para el catálogo de productos
│
├── Operaciones/               # Pruebas para el contexto de operaciones
│   ├── Comandas/              # Pruebas para la gestión de comandas
│   └── Reservaciones/         # Pruebas para la gestión de reservaciones
│
├── Comercial/                 # Pruebas para el contexto comercial
│   └── Clientes/              # Pruebas para la gestión de clientes
│
└── GlobalUsings.cs            # Importaciones globales para el proyecto de pruebas
```

## Tecnologías Utilizadas

- **xUnit**: Framework de pruebas unitarias
- **FluentAssertions**: Biblioteca para escribir aserciones más legibles
- **Moq**: Framework de mocking para simular dependencias

## Principios de Pruebas

1. **Independencia**: Cada prueba es independiente de las demás
2. **Aislamiento**: Se utilizan mocks para aislar la unidad bajo prueba
3. **Repetibilidad**: Las pruebas producen el mismo resultado en cada ejecución
4. **Legibilidad**: Uso del patrón AAA (Arrange-Act-Assert) para estructurar las pruebas
5. **Nomenclatura**: Convención [Método_Escenario_ResultadoEsperado] para nombrar pruebas

## Metodología TDD

Las pruebas implementan el ciclo de desarrollo TDD:

1. **Red**: Escribir una prueba que falle para una funcionalidad
2. **Green**: Implementar el código mínimo para que la prueba pase
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- **Escenarios positivos**: Flujo normal de operaciones
- **Escenarios negativos**: Manejo de condiciones de error
- **Casos límite**: Comportamiento en situaciones extremas
- **Invariantes de dominio**: Reglas de negocio que deben cumplirse
- **Eventos de dominio**: Verificación de eventos publicados

## Ejecución de Pruebas

Para ejecutar todas las pruebas:

```bash
dotnet test
```

Para ejecutar pruebas específicas por categoría:

```bash
dotnet test --filter "Category=Comandas"
```

Para ejecutar pruebas específicas por namespace:

```bash
dotnet test --filter "FullyQualifiedName~RestaurantePro.Domain.UnitTests.Operaciones"
```

## Convenciones de Pruebas

1. Cada clase de prueba tiene un nombre que coincide con la clase bajo prueba más el sufijo "Tests"
2. Las pruebas se agrupan por funcionalidad usando Facts y Theories
3. Los datos de prueba se proporcionan mediante:
   - InlineData para casos simples
   - ClassData para conjuntos de datos complejos
   - MemberData para datos generados dinámicamente
4. Se utilizan traits para categorizar las pruebas 