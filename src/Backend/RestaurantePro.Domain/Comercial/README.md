# Módulo Comercial - Domain

Este módulo contiene los elementos del dominio relacionados con el área comercial del restaurante, siguiendo los principios de Domain-Driven Design (DDD).

## Estructura del Módulo Comercial

```
Comercial/
└── Clientes/                  # Gestión de clientes del restaurante
    ├── Entities/              # Entidades del dominio (Cliente)
    ├── ValueObjects/          # Value Objects específicos (DatosContacto, etc.)
    ├── Events/                # Eventos de dominio relacionados con clientes
    ├── Interfaces/            # Interfaces de repositorios
    └── Enums/                 # Enumeraciones específicas
```

## Contexto Clientes

Este contexto maneja todo lo relacionado con los clientes del restaurante:

- Registro y gestión de clientes
- Gestión de datos de contacto
- Preferencias y opciones personalizadas
- Historial de visitas
- Programa de fidelización (si existe)

## Principios implementados

1. **Agregados**: Entidad principal `Cliente` como raíz de agregado
2. **Value Objects**: Conceptos inmutables como `DatosContacto`, `Direccion`, etc.
3. **Invariantes de dominio**: Reglas de negocio aplicadas a las entidades
4. **Eventos de dominio**: Notificaciones sobre cambios en los clientes
5. **Repositorios**: Interfaces para persistencia específicas al agregado Cliente

## Casos de Uso Principales

- Registro de nuevo cliente
- Actualización de datos de cliente
- Consulta de historial de visitas y pedidos
- Asignación de promociones o descuentos
- Gestión de preferencias

## Relación con otros módulos

- Se integra con el módulo **Operaciones** para asociar clientes a comandas y reservaciones
- Puede integrarse con un módulo de **Marketing** (si existe) para campañas específicas
- Se relaciona con **Facturación** para generar facturas a nombre del cliente 