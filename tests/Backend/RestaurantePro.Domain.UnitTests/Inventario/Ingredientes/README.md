# Tests de Ingredientes

## Descripción
Este directorio contiene las pruebas unitarias para el submódulo de Ingredientes, que verifica la correcta implementación de las entidades, lógica de negocio y eventos relacionados con los ingredientes utilizados en el restaurante.

## Estructura

- **Entities**: Pruebas de las entidades principales como `Ingrediente`
- **Movimientos**: Pruebas relacionadas con el registro y aplicación de movimientos de inventario
  - **Entities**: Pruebas de la entidad `MovimientoInventario`

## Pruebas Principales

### IngredienteTests
- Verificación de la creación de ingredientes con datos válidos
- Validación de reglas de negocio para datos inválidos
- Operaciones de stock (incremento, decremento)
- Validación de eventos de dominio generados

### MovimientoInventarioTests
- Verificación de creación de movimientos
- Aplicación correcta de movimientos al stock
- Validación de tipos de movimientos

## Convenciones de Nomenclatura
- Los nombres de las pruebas siguen el patrón `Método_Condición_ResultadoEsperado`
- Cada test sigue la estructura Arrange-Act-Assert 