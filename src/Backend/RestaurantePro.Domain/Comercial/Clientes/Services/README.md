# Servicios del Contexto de Clientes

Este directorio contiene los servicios específicos para el agregado de Clientes.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben centrarse exclusivamente en operaciones específicas relacionadas con los clientes que:

1. No pueden ser encapsuladas adecuadamente como métodos dentro de la entidad Cliente
2. No requieren coordinación con otros agregados fuera del contexto de Clientes

## Cuándo NO crear un servicio aquí

Si la operación:
- Coordina entre múltiples agregados de diferentes contextos
- Es una operación transversal que afecta a múltiples entidades

En esos casos, el servicio debe ubicarse en `Comercial/Services/`

## Convenciones de nomenclatura

- Servicios de dominio: `ServicioXxx`
- Interfaces: `IServicioXxx` 