# Tests de Proveedores

## Descripción
Este directorio contiene las pruebas unitarias para el módulo de Proveedores, que verifica la correcta implementación de las entidades, lógica de negocio y eventos relacionados con los proveedores que suministran insumos al restaurante.

## Estructura

- **Entidades**: Pruebas de las entidades principales como `Proveedor` y `ProveedorCategoria`

## Pruebas Planificadas

### ProveedorTests
- Verificación de la creación de proveedores con datos válidos
- Validación de reglas de negocio para datos inválidos
- Operaciones de activación/desactivación
- Asociación con categorías
- Validación de eventos de dominio generados

### ProveedorCategoriaTests
- Verificación de la creación de categorías de proveedores
- Manejo de asociaciones entre proveedores y categorías

## Estado Actual
Este módulo está en fase de implementación conforme a la metodología TDD. Primero se crearán las pruebas y luego se desarrollará el código que las satisfaga.

## Convenciones de Nomenclatura
- Los nombres de las pruebas siguen el patrón `Método_Condición_ResultadoEsperado`
- Cada test sigue la estructura Arrange-Act-Assert 