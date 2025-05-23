# Servicios del Contexto de Pagos

Este directorio contiene los servicios específicos para el agregado de Pagos.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben centrarse exclusivamente en operaciones específicas relacionadas con los pagos que:

1. No pueden ser encapsuladas adecuadamente como métodos dentro de la entidad Pago
2. No requieren coordinación con otros agregados fuera del contexto de Pagos
3. Interactúan con sistemas de pago externos o proveedores de procesamiento de pagos

Ejemplos:
- Integración con pasarelas de pago externas
- Validación de pagos con reglas complejas
- Generación de reportes específicos de pagos

## Cuándo NO crear un servicio aquí

Si la operación:
- Coordina entre múltiples agregados (Pagos, Facturas, Clientes, etc.)
- Es una operación transversal que afecta a múltiples entidades

En esos casos, el servicio debe ubicarse en `Comercial/Services/`

## Convenciones de nomenclatura

- Servicios de dominio: `ServicioXxx`
- Interfaces: `IServicioXxx` 