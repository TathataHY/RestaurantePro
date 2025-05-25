# RestaurantePro.Domain.UnitTests

Este proyecto contiene pruebas unitarias para el dominio de RestaurantePro.

## Estructura de pruebas

Las pruebas siguen la misma estructura que el código de dominio:

- `Core`: Pruebas para clases base y componentes compartidos
- `Comercial`: Pruebas para la gestión de clientes y fidelización
- `Operaciones`: Pruebas para comandas y reservaciones
- `Inventario`: Pruebas para la gestión de inventario y compras
- `Proveedores`: Pruebas para la gestión de proveedores

## Pruebas desactivadas

Actualmente, existen algunos archivos de prueba que están desactivados debido a problemas con árboles de expresión en C#. Estos archivos son:

- `Inventario\Services\VerificadorStockTests.cs`
- `Inventario\Policies\StockBajoPolicyTests.cs`
- `Comercial\Services\ServicioFidelizacionTests.cs`
- `Inventario\Services\ServicioNotificacionesTests.cs`
- `Inventario\Services\GeneradorOrdenesCompraTests.cs`
- `Comercial\Policies\ClientesFrecuentesPolicyTests.cs`

## Sugerencias para resolver problemas con árboles de expresión

Para resolver los problemas con árboles de expresión en las pruebas, aquí hay algunas sugerencias:

1. **Usar It.Is<T>() en lugar de It.IsAny<T>()**: 
   ```csharp
   // En lugar de:
   _repo.Setup(r => r.MetodoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))

   // Usar:
   _repo.Setup(r => r.MetodoAsync(It.Is<Guid>(g => true), It.Is<CancellationToken>(t => true)))
   ```

2. **Extraer la lógica de setup a métodos auxiliares**:
   ```csharp
   private void SetupRepositorio(Guid id, Entidad returnValue)
   {
       var guid = id; // Variable local para capturar el valor
       _repo.Setup(r => r.MetodoAsync(It.Is<Guid>(g => g == guid), It.Is<CancellationToken>(t => true)))
           .ReturnsAsync(returnValue);
   }
   ```

3. **Usar Returns() en lugar de ReturnsAsync()**:
   ```csharp
   _repo.Setup(r => r.MetodoAsync(It.Is<Guid>(g => true), It.Is<CancellationToken>(t => true)))
       .Returns(Task.FromResult(returnValue));
   ```

4. **Considerar NSubstitute como alternativa a Moq**:
   Si los problemas con árboles de expresión persisten, considerar el uso de NSubstitute que a veces maneja mejor estos casos.

5. **Simplificar mocks**:
   Si las coincidencias exactas no son necesarias para la prueba, considerar usar una implementación directa de la interfaz en lugar de un mock.

## Consejos generales para las pruebas

- Usar `FluentAssertions` para pruebas más legibles
- Incluir pruebas positivas y negativas para cada caso
- Mantener pruebas independientes y autónomas
- Evitar dependencias entre pruebas

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
├── Integration/               # Pruebas de integración organizadas por tipo de interacción
│   ├── BetweenContexts/       # Pruebas entre diferentes contextos delimitados
│   │   ├── Comercial_Inventario/    # Integración entre contextos Comercial e Inventario
│   │   ├── Comercial_Operaciones/   # Integración entre contextos Comercial y Operaciones
│   │   ├── Core_Comercial/          # Integración entre contextos Core y Comercial
│   │   └── Operaciones_Inventario/  # Integración entre contextos Operaciones e Inventario
│   │
│   ├── WithinContext/         # Pruebas dentro del mismo contexto delimitado
│   │   ├── Core/              # Integración dentro del contexto Core
│   │   ├── Inventario/        # Integración dentro del contexto Inventario
│   │   └── Operaciones/       # Integración dentro del contexto Operaciones
│   │
│   └── README.md              # Documentación específica sobre la organización de pruebas de integración
│
└── GlobalUsings.cs            # Importaciones globales para el proyecto de pruebas
```

## Organización de Pruebas de Integración

Las pruebas de integración han sido reorganizadas siguiendo un enfoque claro que distingue entre:

1. **Pruebas entre Contextos (BetweenContexts)**: Verifican la integración y comunicación entre dos o más contextos delimitados.
   - Ejemplos: Comercial_Operaciones, Core_Comercial, Operaciones_Inventario.
   - Estas pruebas ayudan a validar que los límites entre contextos están bien definidos y que la comunicación entre ellos funciona correctamente.

2. **Pruebas dentro de un Contexto (WithinContext)**: Verifican la integración entre componentes dentro del mismo contexto.
   - Ejemplos: Core, Inventario, Operaciones.
   - Estas pruebas validan que los componentes internos de un contexto trabajan juntos correctamente.

Esta organización proporciona varias ventajas:
- Mayor claridad en la identificación de pruebas relacionadas con contextos específicos
- Mejor mantenibilidad al reducir la fricción para encontrar y actualizar pruebas relacionadas
- La estructura misma documenta las relaciones entre contextos
- Permite identificar fácilmente áreas con poca cobertura de pruebas de integración

Para más detalles, consultar el archivo `Integration/README.md`.

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