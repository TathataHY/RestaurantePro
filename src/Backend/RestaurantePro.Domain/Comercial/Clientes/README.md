# Módulo Clientes - Domain

Este módulo contiene las entidades, value objects, eventos y definiciones del dominio para el contexto de Clientes y fidelización, siguiendo los principios de Domain-Driven Design (DDD).

## Estructura del Módulo

```
Clientes/
├── Entities/                 # Entidades del dominio
│   ├── Cliente.cs            # Aggregate Root - Cliente
│   ├── TarjetaFidelizacion.cs # Entidad de tarjeta de fidelización
│   └── HistorialPuntos.cs    # Entidad para registro de operaciones de puntos
│
├── ValueObjects/             # Value Objects del dominio
│   ├── ClienteNombre.cs      # VO para nombre completo del cliente
│   ├── DatosContacto.cs      # VO para datos de contacto
│   └── Direccion.cs          # VO para dirección postal
│
├── Events/                   # Eventos de dominio
│   ├── ClienteCreado.cs
│   ├── ClienteActualizado.cs
│   ├── ClienteActivado.cs
│   ├── ClienteDesactivado.cs
│   ├── PuntosAgregados.cs
│   └── PuntosProcesados.cs
│
├── Interfaces/               # Interfaces del dominio
│   ├── IClienteRepository.cs # Repositorio de clientes
│   └── IHistorialPuntosRepository.cs # Repositorio de historial de puntos
│
└── Enums/                    # Enumeraciones
    ├── TipoCliente.cs        # Tipos de cliente
    ├── EstadoCliente.cs      # Estados posibles del cliente
    ├── NivelFidelizacion.cs  # Niveles de fidelización
    └── TipoOperacionPuntos.cs # Tipos de operaciones con puntos
```

## Contexto de Clientes

Este contexto maneja todo lo relacionado con los clientes del restaurante y su programa de fidelización:

- Registro y gestión de clientes
- Programa de fidelización con puntos y niveles
- Historial de transacciones y puntos
- Datos de contacto y preferencias
- Segmentación de clientes

## Principios implementados

1. **Aggregate Root**: `Cliente` como raíz de agregado
2. **Entidades**: Clientes y tarjetas de fidelización con identidad propia
3. **Value Objects**: Objetos inmutables como `ClienteNombre` o `DatosContacto`
4. **Eventos de dominio**: Comunicación de cambios importantes como registro de clientes
5. **Reglas de negocio**: Encapsuladas dentro de los agregados y entidades

## Reglas de Negocio Principales

- Los clientes deben tener datos de contacto válidos
- Los puntos tienen fechas de vencimiento configurables
- Existen diferentes niveles de fidelización basados en puntos acumulados
- Las operaciones con puntos deben registrarse con su tipo y justificación
- Los clientes pueden estar en diferentes estados (activo, inactivo, bloqueado)

## Operaciones Clave

- Registrar un nuevo cliente
- Actualizar datos de cliente
- Agregar puntos por consumo
- Canjear puntos por beneficios
- Calcular nivel de fidelización
- Gestionar vencimiento de puntos

## Relación con otros módulos

- Se integra con **Operaciones/Comandas** para asignar clientes a comandas
- Se integra con **Operaciones/Reservaciones** para gestionar reservas de clientes
- Puede proporcionar datos a un futuro módulo de **Marketing** para campañas 