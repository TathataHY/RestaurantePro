# Pruebas del Módulo Operaciones

Este directorio contiene todas las pruebas unitarias para el módulo Operaciones del sistema RestaurantePro, siguiendo el enfoque TDD (Test-Driven Development).

## Estructura de las Pruebas

Las pruebas están organizadas siguiendo la misma estructura del dominio:

- **Comandas/**:
  - `ComandaTests.cs`: Pruebas para el agregado raíz Comanda
  - `ItemComandaTests.cs`: Pruebas para la entidad ItemComanda
  - `TotalComandaTests.cs`: Pruebas para el value object TotalComanda
  - `Events/`: Pruebas para los eventos de dominio de comandas
  - `Repositories/`: Pruebas para los repositorios

- **Reservaciones/**:
  - `ReservacionTests.cs`: Pruebas para el agregado raíz Reservacion
  - `MesaTests.cs`: Pruebas para las entidades relacionadas con mesas
  - `Events/`: Pruebas para los eventos de dominio de reservaciones
  - `Repositories/`: Pruebas para los repositorios

## Enfoque TDD

Las pruebas siguen el ciclo TDD:

1. **Red**: Escribir una prueba que falle
2. **Green**: Implementar el código más simple que haga pasar la prueba
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- Creación y validación de entidades
- Operaciones de negocio en las comandas y reservaciones
- Cambios de estado y validaciones
- Eventos de dominio generados
- Invariantes de dominio
- Comportamiento de repositorios

## Casos de Prueba Principales

### Comandas
- Creación de comandas con datos válidos
- Agregar, actualizar y eliminar productos
- Cambios de estado (creada, en proceso, lista, entregada, etc.)
- Cálculo de totales e impuestos
- Finalización y cancelación de comandas

### Reservaciones
- Creación de reservaciones para fechas y horas específicas
- Asignación y liberación de mesas
- Confirmación y cancelación de reservaciones
- Validación de disponibilidad
- Actualizaciones de estado

### Repositorios
- Búsqueda por diversos criterios (ID, estado, fecha, cliente, etc.)
- Operaciones CRUD (crear, leer, actualizar, eliminar)
- Consultas específicas del dominio

## Ejecución de Pruebas

Para ejecutar las pruebas, usar el comando desde la raíz del proyecto:

```bash
dotnet test
```

O para ejecutar solo las pruebas de este módulo:

```bash
dotnet test --filter "FullyQualifiedName~Operaciones"
```

Para un contexto específico:

```bash
dotnet test --filter "FullyQualifiedName~Operaciones.Comandas"
``` 