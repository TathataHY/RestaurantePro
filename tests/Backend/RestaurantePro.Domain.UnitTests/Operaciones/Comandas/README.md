# Pruebas del Módulo de Comandas

Este directorio contiene todas las pruebas unitarias para el módulo de Comandas del sistema RestaurantePro, siguiendo el enfoque TDD (Test-Driven Development).

## Estructura de las Pruebas

Las pruebas están organizadas siguiendo la misma estructura del dominio:

- **Entidades Principales**:
  - `ComandaTests.cs`: Pruebas para la entidad Comanda (agregado raíz)
  - `ItemComandaTests.cs`: Pruebas para la entidad ItemComanda

- **Repositorios**:
  - `Repositories/ComandaRepositoryTests.cs`: Pruebas para el repositorio de comandas

## Enfoque TDD

Las pruebas siguen el ciclo TDD:

1. **Red**: Escribir una prueba que falle
2. **Green**: Implementar el código más simple que haga pasar la prueba
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- Creación y validación de entidades
- Cambios de estado de comandas
- Operaciones principales del negocio (agregar productos, calcular totales)
- Manejo de excepciones y casos límite
- Eventos de dominio
- Interacción con repositorios

## Casos de Prueba Principales

### Comandas
- Creación con datos válidos
- Agregar productos a la comanda
- Actualizar estados (en proceso, lista, entregada, etc.)
- Validaciones de estado
- Cancelar comanda
- Cálculo de totales e impuestos

### ItemComanda
- Creación con datos válidos
- Validación de cantidad y precio
- Actualizar cantidad
- Actualizar precio unitario
- Recálculo de subtotales
- Actualizar observaciones

### Repositorios
- Buscar por ID, estado, mesa, mesero y cliente
- Consultar por rango de fechas
- Operaciones CRUD (agregar, actualizar, eliminar)

## Ejecución de Pruebas

Para ejecutar las pruebas, usar el comando desde la raíz del proyecto:

```bash
dotnet test
```

O para ejecutar solo las pruebas de este módulo:

```bash
dotnet test --filter "FullyQualifiedName~Operaciones.Comandas"
``` 