# Servicios del Contexto de Usuarios

Este directorio contiene los servicios específicos para el agregado de Usuarios dentro del Core.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben centrarse exclusivamente en operaciones específicas relacionadas con los usuarios que:

1. No pueden ser encapsuladas adecuadamente como métodos dentro de la entidad Usuario
2. No requieren coordinación con otros agregados fuera del contexto de Usuarios
3. Implementan lógica de negocio específica para gestión de usuarios

Ejemplos:
- Autenticación y autorización a nivel de dominio
- Gestión de perfiles y roles
- Validaciones complejas de usuarios
- Cambios de contraseña y recuperación de cuentas

## Cuándo NO crear un servicio aquí

Si la operación:
- Coordina entre múltiples agregados no relacionados con usuarios
- Es una operación transversal que afecta a múltiples entidades
- Orquesta un flujo de negocio complejo

En esos casos, el servicio debe ubicarse en un nivel superior.

## Convenciones de nomenclatura

- Servicios de dominio: `ServicioXxx`
- Interfaces: `IServicioXxx` 