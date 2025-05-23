# Servicios del Contexto de Comandas

Este directorio contiene los servicios específicos para el agregado de Comandas.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben centrarse exclusivamente en operaciones específicas relacionadas con las comandas que:

1. No pueden ser encapsuladas adecuadamente como métodos dentro de la entidad Comanda
2. No requieren coordinación con otros agregados fuera del contexto de Comandas
3. Implementan lógica de negocio específica de las comandas

Ejemplos:
- Cálculos complejos sobre items de comanda
- Validaciones específicas de comandas
- Transformaciones o exportaciones de comandas

## Cuándo NO crear un servicio aquí

Si la operación:
- Coordina entre múltiples agregados (Comandas, Mesas, Reservaciones, etc.)
- Es una operación transversal que afecta a múltiples entidades
- Orquesta un flujo de negocio completo

En esos casos, el servicio debe ubicarse en `Operaciones/Services/`

## Convenciones de nomenclatura

- Servicios de dominio: `ServicioXxx`
- Interfaces: `IServicioXxx` 