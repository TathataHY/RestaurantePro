# Servicios del Contexto de Productos

Este directorio contiene los servicios específicos para el agregado de Productos dentro del Core.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben centrarse exclusivamente en operaciones específicas relacionadas con los productos que:

1. No pueden ser encapsuladas adecuadamente como métodos dentro de la entidad Producto
2. No requieren coordinación con otros agregados fuera del contexto de Productos
3. Implementan lógica de negocio específica para gestión de productos

Ejemplos:
- Gestión de categorías de productos
- Cálculos de precios y márgenes 
- Validaciones complejas de productos

## Cuándo NO crear un servicio aquí

Si la operación:
- Coordina entre múltiples agregados no relacionados con productos
- Es una operación transversal que afecta a múltiples entidades
- Orquesta un flujo de negocio complejo

En esos casos, el servicio debe ubicarse en un nivel superior.

## Convenciones de nomenclatura

- Servicios de dominio: `XxxService` (En inglés para ser consistente con el nombre del directorio)
- Interfaces: `IXxxService` 