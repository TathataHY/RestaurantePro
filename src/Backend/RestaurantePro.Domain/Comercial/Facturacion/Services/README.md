# Servicios del Contexto de Facturación

Este directorio contiene los servicios específicos para el agregado de Facturación.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben centrarse exclusivamente en operaciones específicas relacionadas con las facturas que:

1. No pueden ser encapsuladas adecuadamente como métodos dentro de la entidad Factura
2. No requieren coordinación con otros agregados fuera del contexto de Facturación
3. Implementan lógica de negocio específica para facturación

Ejemplos:
- Generación de facturas a partir de comandas
- Gestión de facturas vencidas
- Cálculos fiscales complejos
- Exportación de facturas a formatos específicos

## Cuándo NO crear un servicio aquí

Si la operación:
- Coordina entre múltiples agregados (Facturas, Clientes, Pagos, etc.)
- Es una operación transversal que afecta a múltiples entidades
- Orquesta un flujo de negocio completo

En esos casos, el servicio debe ubicarse en `Comercial/Services/`

## Convenciones de nomenclatura

- Servicios de dominio: `ServicioXxx`
- Interfaces: `IServicioXxx` 