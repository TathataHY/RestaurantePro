# Servicios Transversales del Core

Este directorio contiene servicios transversales que son utilizados por múltiples contextos dentro del dominio.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben:

1. Ser utilizables por múltiples contextos dentro del dominio
2. Proporcionar funcionalidad base o transversal que no pertenece a un único agregado
3. Implementar patrones comunes reutilizables en todo el dominio

Ejemplos:
- Servicios de notificación basados en eventos
- Servicios de caching
- Servicios de auditoría
- Servicios de logging específicos del dominio

## Cuándo NO crear un servicio aquí

Si el servicio:
- Es específico de un único agregado o contexto
- No es reutilizable por otros contextos
- Implementa lógica de negocio específica

En esos casos, el servicio debe ubicarse en el contexto al que pertenece.

## Convenciones de nomenclatura

Los servicios en este directorio siguen una nomenclatura descriptiva según su funcionalidad, por ejemplo:

- `EventBasedNotificationService`
- `DomainAuditService`
- `DomainCacheService`

Las interfaces correspondientes siguen el mismo patrón con el prefijo "I":

- `IEventBasedNotificationService` 