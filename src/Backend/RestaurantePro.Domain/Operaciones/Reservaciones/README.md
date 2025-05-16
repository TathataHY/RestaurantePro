# Módulo Reservaciones - Domain

Este módulo contiene las entidades, value objects, eventos y definiciones del dominio para el contexto de Reservaciones de mesas, siguiendo los principios de Domain-Driven Design (DDD).

## Estructura del Módulo

```
Reservaciones/
├── Entities/                # Entidades del dominio
│   ├── Reservacion.cs       # Aggregate Root
│   └── ReservacionItem.cs   # Entidades dentro del agregado
│
├── Mesas/                   # Gestión de mesas
│   ├── Mesa.cs              # Entidad de mesa
│   └── SeccionRestaurante.cs # Entidad para secciones del restaurante
│
├── ValueObjects/            # Value Objects del dominio
│   ├── HorarioReservacion.cs # VO para horarios (inmutable)
│   └── DatosContactoReserva.cs # VO para datos de contacto
│
├── Events/                  # Eventos de dominio
│   ├── ReservacionCreada.cs
│   ├── ReservacionConfirmada.cs
│   ├── ReservacionCancelada.cs
│   ├── MesaAsignada.cs
│   └── ReservacionFinalizada.cs
│
├── Interfaces/              # Interfaces del dominio
│   ├── IReservacionRepository.cs # Repositorio de reservaciones
│   └── IMesaRepository.cs    # Repositorio de mesas
│
└── Enums/                   # Enumeraciones
    ├── EstadoReservacion.cs  # Estados de la reservación
    ├── TipoMesa.cs           # Tipos de mesa
    └── SeccionRestaurante.cs # Secciones del restaurante
```

## Contexto de Reservaciones

Este contexto maneja todo lo relacionado con las reservaciones de mesas en el restaurante:

- Creación y gestión de reservaciones
- Asignación de mesas a las reservaciones
- Seguimiento de estados (pendiente, confirmada, cancelada, finalizada)
- Gestión de la disponibilidad de mesas
- Reglas para confirmar, cancelar o modificar reservaciones

## Principios implementados

1. **Aggregate Root**: `Reservacion` como raíz de agregado
2. **Entidades**: Mesas y secciones con identidad propia
3. **Value Objects**: Objetos inmutables para conceptos como horarios
4. **Eventos de dominio**: Comunicación de cambios importantes
5. **Reglas de negocio**: Validación de disponibilidad, tiempos de reserva, etc.

## Reglas de Negocio Principales

- Una mesa no puede tener dos reservaciones para el mismo horario
- Las reservaciones deben hacerse con un mínimo de horas de anticipación
- Reservaciones no confirmadas pueden ser canceladas automáticamente
- Ciertas secciones del restaurante pueden tener políticas especiales
- Las mesas tienen capacidades específicas que deben respetarse

## Relación con otros módulos

- Se integra con **Comercial/Clientes** para asociar clientes a las reservaciones
- Puede comunicarse con **Operaciones/Comandas** cuando una reservación genera una comanda
- Consulta el catálogo de **Core/Productos** para servicios especiales en reservaciones 