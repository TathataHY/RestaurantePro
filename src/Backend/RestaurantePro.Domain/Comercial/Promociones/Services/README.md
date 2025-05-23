# Servicios del Contexto de Promociones

Este directorio contiene los servicios específicos para el agregado de Promociones.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben centrarse exclusivamente en operaciones específicas relacionadas con las promociones que:

1. No pueden ser encapsuladas adecuadamente como métodos dentro de la entidad Promocion
2. No requieren coordinación con otros agregados fuera del contexto de Promociones
3. Implementan lógica de negocio específica para gestión de promociones

Ejemplos:
- Validación de aplicabilidad de promociones
- Cálculo de descuentos promocionales
- Gestión de reglas de negocio complejas para promociones
- Análisis de efectividad de promociones

## Cuándo NO crear un servicio aquí

Si la operación:
- Coordina entre múltiples agregados (Promociones, Clientes, Productos, etc.)
- Es una operación transversal que afecta a múltiples entidades
- Orquesta un flujo de negocio completo

En esos casos, el servicio debe ubicarse en `Comercial/Services/`

## Convenciones de nomenclatura

- Servicios de dominio: `ServicioXxx`
- Interfaces: `IServicioXxx` 