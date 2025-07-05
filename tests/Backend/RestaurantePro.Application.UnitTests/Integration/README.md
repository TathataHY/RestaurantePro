# Organización de Pruebas de Integración en la Capa de Aplicación

## Estructura General

Las pruebas de integración en la capa de aplicación verifican la interacción entre distintos componentes del sistema, tanto dentro de un mismo contexto como entre diferentes contextos delimitados. Se organizan siguiendo esta estructura:

1. **Pruebas entre Contextos (BetweenContexts)**: Verifican la integración y comunicación entre dos o más contextos delimitados, enfocándose en cómo los comandos y consultas de un contexto afectan a otro contexto.

2. **Pruebas dentro de un Contexto (WithinContext)**: Verifican el flujo completo de operaciones dentro del mismo contexto, asegurando que los diferentes componentes (comandos, eventos, consultas) trabajan juntos correctamente.

## Ejemplos Implementados

### Dentro del Contexto de Operaciones
Se ha implementado una prueba de integración que verifica el flujo completo:
1. Creación de preparación mediante `CrearPreparacionCommand`
2. Marcado como disponible mediante `MarcarComoDisponibleCommand`
3. Consumo a través de `AgregarItemComandaHandler`

### Entre Contextos de Operaciones e Inventario
Se ha implementado una prueba de integración entre contextos que verifica:
1. Que al crear una preparación se actualiza correctamente el inventario (stock de ingredientes)
2. Que al consumir una preparación no se vuelve a actualizar el inventario (solo se actualiza la preparación)

## Estado Actual

> **NOTA IMPORTANTE**: Los archivos de prueba de integración han sido creados, pero actualmente no compilan correctamente debido a discrepancias entre las interfaces y tipos del proyecto. Estos archivos requieren ajustes adicionales para adaptarse a los cambios en las interfaces, tipos de datos y parámetros de los métodos en el sistema.

## Cómo Ejecutar las Pruebas

Una vez ajustados los archivos, las pruebas pueden ejecutarse con el siguiente comando:

```powershell
dotnet test tests\RestaurantePro.Application.UnitTests\RestaurantePro.Application.UnitTests.csproj --filter "Integration"
```

## Pasos Pendientes

1. Corregir errores de compilación:
   - Ajustar parámetros de métodos
   - Actualizar tipos y enumeraciones
   - Corregir llamadas a métodos mock

2. Crear pruebas de integración adicionales:
   - Entre Operaciones y Comercial (ej: Comandas y Facturación)
   - Entre Inventario y Proveedores

## Detalles de la Estructura

### Pruebas Entre Contextos (BetweenContexts)

Organizadas por pares de contextos que interactúan:

- **Operaciones_Inventario**: Integración entre el contexto de Operaciones y el contexto de Inventario
  - Ejemplo: Actualización del inventario al crear preparaciones
  - Ejemplo: Consumo de preparaciones y su efecto en el inventario

### Pruebas Dentro de un Contexto (WithinContext)

Organizadas por contexto individual:

- **Operaciones**: Pruebas de integración dentro del contexto de Operaciones
  - Ejemplo: Flujo completo de creación, marcado y consumo de preparaciones
  - Ejemplo: Integración entre preparaciones y comandas

## Beneficios de las Pruebas de Integración

- **Verificación de flujos completos**: Prueban escenarios reales de uso que atraviesan múltiples componentes
- **Detección de problemas de integración**: Encuentran errores que las pruebas unitarias individuales no pueden detectar
- **Documentación viva**: Demuestran cómo se integran diferentes partes del sistema
- **Confianza en cambios**: Aseguran que modificaciones en un área no rompen la integración con otras áreas

## Convenciones de Nomenclatura

Para las pruebas de integración se utilizan los siguientes patrones de nomenclatura:

1. Para flujos completos dentro de un contexto: `[EntidadPrincipal]_[AccionPrincipal]_[ResultadoEsperado]`
   Ejemplo: `PreparacionComanda_DebeCompletarseExitosamente`

2. Para integración entre contextos: `[EntidadDelContextoA]_[AccionSobreContextoB]_[Condición]`
   Ejemplo: `Preparacion_DebeActualizarInventario_CuandoSeConsume`

## Implementación Técnica

Las pruebas de integración utilizan:

- **Mocks controlados**: Para simular el comportamiento de los componentes externos al flujo probado
- **Eventos de dominio**: Para verificar que los eventos desencadenan las acciones esperadas en otros contextos
- **Flujos completos**: Ejecutando comandos en secuencia para verificar el comportamiento del sistema completo 