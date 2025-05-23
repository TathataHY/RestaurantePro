# Servicios del Contexto de Notificaciones

Este directorio contiene los servicios específicos para el agregado de Notificaciones dentro del Core.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben centrarse exclusivamente en operaciones específicas relacionadas con las notificaciones que:

1. No pueden ser encapsuladas adecuadamente como métodos dentro de la entidad Notificacion
2. No requieren coordinación con otros agregados fuera del contexto de Notificaciones
3. Implementan lógica de negocio específica para gestión de notificaciones

Ejemplos:
- Envío de notificaciones a diferentes canales
- Gestión de plantillas de notificaciones
- Configuración de preferencias de notificación

## Cuándo NO crear un servicio aquí

Si la operación:
- Coordina entre múltiples agregados no relacionados con notificaciones
- Es una operación transversal que afecta a múltiples entidades
- Orquesta un flujo de negocio complejo

En esos casos, el servicio debe ubicarse en un nivel superior.

## Convenciones de nomenclatura

- Servicios de dominio: `ServicioXxx`
- Interfaces: `IServicioXxx` 