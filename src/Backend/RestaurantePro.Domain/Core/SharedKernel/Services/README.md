# Servicios del SharedKernel

Este directorio contiene servicios fundamentales que son utilizados en todo el dominio y representan abstracciones de infraestructura o utilidades básicas.

## Cuándo crear un servicio aquí

Los servicios en este directorio deben:

1. Proporcionar abstracciones sobre conceptos fundamentales o de infraestructura
2. Ser utilizados por múltiples contextos y componentes del dominio
3. No estar vinculados a una lógica de negocio específica

Ejemplos:
- Servicios de fecha/hora
- Servicios de identificadores únicos
- Servicios de serialización/deserialización

## Cuándo NO crear un servicio aquí

Si el servicio:
- Implementa lógica de negocio específica
- Es utilizado solo por un contexto particular
- Contiene reglas específicas de un agregado

En esos casos, el servicio debe ubicarse en el contexto correspondiente.

## Convenciones de nomenclatura

- Interfaces: `IXxxService` (ej: `IDateTimeService`)
- Implementaciones: `XxxService` (ej: `DateTimeService`) 