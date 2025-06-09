# Organización de Pruebas de Integración en la Capa de Aplicación

## Estructura General

Las pruebas de integración en la capa de aplicación verifican la interacción entre distintos componentes del sistema, tanto dentro de un mismo contexto como entre diferentes contextos delimitados. Se organizan siguiendo esta estructura:

1. **Pruebas entre Contextos (BetweenContexts)**: Verifican la integración y comunicación entre dos o más contextos delimitados, enfocándose en cómo los comandos y consultas de un contexto afectan a otro contexto.

2. **Pruebas dentro de un Contexto (WithinContext)**: Verifican el flujo completo de operaciones dentro del mismo contexto, asegurando que los diferentes componentes (comandos, eventos, consultas) trabajan juntos correctamente.

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