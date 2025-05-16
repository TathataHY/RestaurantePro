# Submódulo Mesas - Domain

Este submódulo contiene las entidades, eventos y enumeraciones relacionadas con la gestión de mesas del restaurante, como parte del contexto de Reservaciones, siguiendo los principios de Domain-Driven Design (DDD).

## Estructura del Submódulo

```
Mesas/
├── Entities/                # Entidades del dominio
│   ├── Mesa.cs              # Entidad de mesa
│   └── SeccionMesa.cs       # Entidad para secciones de mesas
│
├── Events/                  # Eventos de dominio
│   ├── MesaCreada.cs
│   ├── MesaActualizada.cs
│   ├── MesaOcupada.cs
│   ├── MesaLiberada.cs
│   └── MesaFueraDeServicio.cs
│
├── Interfaces/              # Interfaces del dominio
│   └── IMesaRepository.cs   # Repositorio de mesas
│
└── Enums/                   # Enumeraciones
    ├── EstadoMesa.cs        # Estados posibles de una mesa
    ├── TipoMesa.cs          # Tipos de mesa (cuadrada, redonda, etc.)
    └── UbicacionMesa.cs     # Ubicaciones posibles (terraza, interior, etc.)
```

## Propósito del Submódulo

Este submódulo gestiona todo lo relacionado con las mesas físicas del restaurante:

- Definición y características de las mesas
- Estados de las mesas (disponible, ocupada, reservada, fuera de servicio)
- Ubicación y agrupación de las mesas (secciones)
- Capacidad y tipo de cada mesa
- Reglas de asignación y liberación

## Reglas de Negocio Principales

- Una mesa puede tener diferentes estados (disponible, ocupada, reservada, fuera de servicio)
- Las mesas tienen una capacidad definida (número de comensales)
- Las mesas pertenecen a una sección o área del restaurante
- Existen diferentes tipos de mesas según su forma o características
- Las mesas pueden tener restricciones específicas (para fumadores, accesibilidad, etc.)

## Operaciones Clave

- Crear una nueva mesa
- Cambiar el estado de una mesa
- Asignar una mesa a una reservación
- Marcar una mesa como ocupada
- Liberar una mesa
- Poner una mesa fuera de servicio temporalmente
- Reorganizar mesas en secciones

## Relación con otros submódulos

- Utilizado por **Reservaciones** para asignar mesas a las reservaciones
- Puede ser consultado por **Comandas** para asociar comandas a mesas
- Se relaciona con la distribución física del restaurante 