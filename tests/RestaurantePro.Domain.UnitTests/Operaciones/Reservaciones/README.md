# Pruebas del Módulo de Reservaciones

Este directorio contiene todas las pruebas unitarias para el módulo de Reservaciones del sistema RestaurantePro, siguiendo el enfoque TDD (Test-Driven Development).

## Estructura de las Pruebas

Las pruebas están organizadas siguiendo la misma estructura del dominio:

- **Entidades Principales**:
  - `ReservacionTests.cs`: Pruebas para la entidad Reservacion
  - `Mesas/MesaTests.cs`: Pruebas para la entidad Mesa

- **Repositorios**:
  - `Repositories/ReservacionRepositoryTests.cs`: Pruebas para el repositorio de reservaciones
  - `Mesas/Repositories/MesaRepositoryTests.cs`: Pruebas para el repositorio de mesas

## Enfoque TDD

Las pruebas siguen el ciclo TDD:

1. **Red**: Escribir una prueba que falle
2. **Green**: Implementar el código más simple que haga pasar la prueba
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- Creación y validación de entidades
- Cambios de estado de reservaciones y mesas
- Operaciones principales del negocio
- Manejo de excepciones y casos límite
- Eventos de dominio
- Interacción con repositorios

## Casos de Prueba Principales

### Reservaciones
- Creación con datos válidos
- Confirmar reservación
- Cancelar reservación
- Completar reservación
- Marcar como no-show
- Validaciones de fechas y número de personas

### Mesas
- Creación con datos válidos
- Marcar como ocupada
- Marcar como reservada
- Marcar como disponible
- Marcar como fuera de servicio
- Validaciones de número y capacidad

### Repositorios
- Buscar por ID, fecha, cliente, estado
- Verificar disponibilidad en rangos horarios
- Obtener mesas por ubicación y estado
- Calcular total de comensales actuales

## Ejecución de Pruebas

Para ejecutar las pruebas, usar el comando desde la raíz del proyecto:

```bash
dotnet test
```

O para ejecutar solo las pruebas de este módulo:

```bash
dotnet test --filter "FullyQualifiedName~Operaciones.Reservaciones"
``` 