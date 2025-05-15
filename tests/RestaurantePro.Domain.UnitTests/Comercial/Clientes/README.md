# Pruebas del Módulo de Clientes

Este directorio contiene todas las pruebas unitarias para el módulo de Clientes del sistema RestaurantePro, siguiendo el enfoque TDD (Test-Driven Development).

## Estructura de las Pruebas

Las pruebas están organizadas siguiendo la misma estructura del dominio:

- **Entities**: Pruebas para las entidades principales
  - `ClienteTests.cs`: Pruebas para la entidad Cliente
  - `TarjetaFidelizacionTests.cs`: Pruebas para la tarjeta de fidelización
  - `HistorialPuntosTests.cs`: Pruebas para el historial de puntos

- **ValueObjects**: Pruebas para objetos de valor
  - `ClienteNombreTests.cs`: Pruebas para el VO ClienteNombre

- **Repositories**: Pruebas para los repositorios utilizando mocks
  - `ClienteRepositoryTests.cs`: Pruebas para el repositorio de clientes
  - `HistorialPuntosRepositoryTests.cs`: Pruebas para el repositorio de historial de puntos

## Enfoque TDD

Las pruebas siguen el ciclo TDD:

1. **Red**: Escribir una prueba que falle
2. **Green**: Implementar el código más simple que haga pasar la prueba
3. **Refactor**: Mejorar el código sin cambiar su comportamiento

## Cobertura de Pruebas

Las pruebas cubren:

- Creación y validación de entidades
- Operaciones principales del negocio
- Manejo de excepciones y casos límite
- Eventos de dominio
- Interacción con repositorios

## Casos de Prueba Principales

### Cliente
- Creación con datos válidos
- Agregar puntos a cliente activo
- Manejar errores al agregar puntos a cliente inactivo
- Desactivar/reactivar cliente
- Actualizar información de contacto

### TarjetaFidelizacion
- Creación con datos válidos
- Activación de tarjeta
- Manejo de puntos (agregar, canjear, vencer)
- Cambios de nivel automáticos según puntos
- Cambios de estado (suspender, cancelar)

### HistorialPuntos
- Registrar diferentes tipos de operaciones (agregados, canjeados, vencidos, ajuste)
- Validar datos de entrada
- Calcular puntos basados en monto de compra

## Ejecución de Pruebas

Para ejecutar las pruebas, usar el comando desde la raíz del proyecto:

```bash
dotnet test
```

O para ejecutar solo las pruebas de este módulo:

```bash
dotnet test --filter "FullyQualifiedName~Comercial.Clientes"
``` 