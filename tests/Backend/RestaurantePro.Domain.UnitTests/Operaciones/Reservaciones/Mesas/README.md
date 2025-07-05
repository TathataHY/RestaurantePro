# Pruebas del Submódulo Mesas

Este directorio contiene todas las pruebas unitarias para el submódulo Mesas dentro del contexto de Reservaciones del sistema RestaurantePro, siguiendo el enfoque TDD (Test-Driven Development).

## Estructura de las Pruebas

Las pruebas están organizadas siguiendo la misma estructura del dominio:

- **Entidades**:
  - `MesaTests.cs`: Pruebas para la entidad Mesa
  - `SeccionMesaTests.cs`: Pruebas para las secciones de mesas

- **Repositories/**:
  - `MesaRepositoryTests.cs`: Pruebas para el repositorio de mesas

## Enfoque TDD

Las pruebas siguen el ciclo TDD:

1. **Red**: Escribir una prueba que falle
2. **Green**: Implementar el código más simple que haga pasar la prueba
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- Creación y validación de mesas
- Cambios de estado de las mesas (disponible, ocupada, reservada, etc.)
- Operaciones de negocio con mesas
- Validaciones y reglas específicas de mesas
- Eventos de dominio generados por operaciones con mesas
- Comportamiento de los repositorios

## Casos de Prueba Principales

### Entidad Mesa
- Creación de mesa con datos válidos
- Validación de capacidad de mesa
- Cambios de estado (disponible, ocupada, reservada, fuera de servicio)
- Asignación/desasignación a reservaciones
- Validación de reglas de negocio específicas

### SeccionMesa
- Creación de sección con datos válidos
- Asignación de mesas a secciones
- Validación de restricciones por sección
- Cálculo de capacidad total por sección

### Repositorios
- Búsqueda por diversos criterios (ID, estado, sección, capacidad)
- Verificación de disponibilidad en horarios específicos
- Operaciones CRUD (crear, leer, actualizar, eliminar)
- Consultas específicas del negocio (mesas disponibles por capacidad, etc.)

## Verificación de Eventos

- **MesaCreada**: Al crear una nueva mesa
- **MesaActualizada**: Al actualizar propiedades de la mesa
- **MesaOcupada**: Al cambiar el estado a ocupada
- **MesaLiberada**: Al liberar una mesa ocupada
- **MesaFueraDeServicio**: Al marcar una mesa como no disponible

## Ejecución de Pruebas

Para ejecutar las pruebas, usar el comando desde la raíz del proyecto:

```bash
dotnet test
```

O para ejecutar solo las pruebas de este submódulo:

```bash
dotnet test --filter "FullyQualifiedName~Operaciones.Reservaciones.Mesas"
``` 