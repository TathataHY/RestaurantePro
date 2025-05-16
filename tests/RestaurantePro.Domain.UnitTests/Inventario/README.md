# Pruebas del Mu00f3dulo Inventario

Este directorio contiene todas las pruebas unitarias para el mu00f3dulo Inventario del sistema RestaurantePro, siguiendo el enfoque TDD (Test-Driven Development).

## Estructura de las Pruebas

Las pruebas estu00e1n organizadas siguiendo la misma estructura que el dominio:

- **Entities/**: Pruebas para las entidades de inventario
  - `IngredienteTests.cs`: Pruebas para la entidad Ingrediente
  - `MovimientoInventarioTests.cs`: Pruebas para movimientos
  - `OrdenCompraTests.cs`: Pruebas para u00f3rdenes de compra
  - `ProveedorTests.cs`: Pruebas para proveedores

- **ValueObjects/**: Pruebas para objetos de valor
  - `CantidadTests.cs`: Pruebas para cantidades con unidades de medida

- **Services/**: Pruebas para servicios de dominio
  - `InventarioServiceTests.cs`: Pruebas para servicios de inventario

## Enfoque TDD

Cada prueba se crea siguiendo el ciclo TDD:

1. **Red**: Escribir prueba que falle
2. **Green**: Implementar el cu00f3digo mu00e1s simple que haga pasar la prueba
3. **Refactor**: Mejorar el cu00f3digo sin cambiar su comportamiento

## Casos de Prueba Principales

### Ingrediente
- Creaciu00f3n con valores vu00e1lidos
- Validaciu00f3n de nombre, unidad de medida, etc.
- Cu00e1lculo correcto de stock
- Eventos generados al crear/modificar

### MovimientoInventario
- Registro correcto de movimientos
- Validaciu00f3n de cantidades
- Cu00e1lculo correcto de stock resultante

### OrdenCompra
- Creaciu00f3n y modificaciu00f3n de u00f3rdenes
- Cu00e1lculos de totales
- Cambios de estado (pendiente, recibida, etc.)

## Cobertura de Pruebas

Las pruebas deben cubrir:
- Casos normales de u00e9xito
- Validaciones y excepciones
- Reglas de negocio complejas
- Eventos de dominio generados
