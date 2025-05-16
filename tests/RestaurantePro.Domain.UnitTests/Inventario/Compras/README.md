# Tests de Compras

## Descripción
Este directorio contiene las pruebas unitarias para el submódulo de Compras, que verifica la correcta implementación de las entidades, lógica de negocio y eventos relacionados con el proceso de adquisición de insumos para el restaurante.

## Estructura

- **OrdenesCompra**: Pruebas relacionadas con las órdenes de compra a proveedores
  - **Entities**: Pruebas de las entidades `OrdenCompra` e `ItemOrdenCompra`
  - **Repositories**: Pruebas del repositorio de órdenes de compra

## Pruebas Principales

### OrdenCompraTests
- Verificación de la creación de órdenes con datos válidos
- Pruebas del flujo de estados (enviar, recibir, cancelar)
- Manejo de ítems (agregar, eliminar)
- Cálculo de totales
- Validación de eventos de dominio generados

### OrdenCompraRepositoryTests
- Operaciones CRUD sobre órdenes de compra
- Consultas filtradas por estado y proveedor
- Verificación de persistencia de cambios

## Convenciones de Nomenclatura
- Los nombres de las pruebas siguen el patrón `Método_Condición_ResultadoEsperado`
- Cada test sigue la estructura Arrange-Act-Assert
- Se utiliza Moq para simular dependencias externas 