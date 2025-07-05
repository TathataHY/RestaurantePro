# Pruebas del Módulo Comercial

Este directorio contiene todas las pruebas unitarias para el módulo Comercial del sistema RestaurantePro, siguiendo el enfoque TDD (Test-Driven Development).

## Estructura de las Pruebas

Las pruebas están organizadas siguiendo la misma estructura del dominio:

- **Clientes/**:
  - `ClienteTests.cs`: Pruebas para la entidad Cliente
  - `DatosContactoTests.cs`: Pruebas para value objects relacionados con clientes
  - `Events/`: Pruebas para los eventos de dominio de clientes
  - `Repositories/`: Pruebas para los repositorios

## Enfoque TDD

Las pruebas siguen el ciclo TDD:

1. **Red**: Escribir una prueba que falle
2. **Green**: Implementar el código más simple que haga pasar la prueba
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- Creación y validación de entidades relacionadas con clientes
- Validación de datos de contacto
- Operaciones específicas para clientes (actualización de datos, preferencias, etc.)
- Eventos de dominio generados
- Comportamiento de repositorios

## Casos de Prueba Principales

### Clientes
- Creación de clientes con datos válidos
- Validación de datos de contacto (correo, teléfono, etc.)
- Actualización de datos personales
- Gestión de preferencias
- Historial de visitas y consumos

### Repositorios
- Búsqueda por diversos criterios (ID, nombre, correo, etc.)
- Operaciones CRUD (crear, leer, actualizar, eliminar)
- Consultas específicas del dominio (clientes frecuentes, clientes con ciertas preferencias, etc.)

## Ejecución de Pruebas

Para ejecutar las pruebas, usar el comando desde la raíz del proyecto:

```bash
dotnet test
```

O para ejecutar solo las pruebas de este módulo:

```bash
dotnet test --filter "FullyQualifiedName~Comercial"
```

Para un contexto específico:

```bash
dotnet test --filter "FullyQualifiedName~Comercial.Clientes"
``` 