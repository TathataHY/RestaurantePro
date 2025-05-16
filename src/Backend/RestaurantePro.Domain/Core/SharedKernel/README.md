# Módulo Core/SharedKernel - Domain

Este módulo implementa el concepto de "Shared Kernel" (Núcleo Compartido) de Domain-Driven Design (DDD), conteniendo elementos que son compartidos entre todos los Bounded Contexts del sistema.

## Estructura del Módulo

```
SharedKernel/
├── ValueObjects/             # Value Objects compartidos
│   ├── Money.cs              # VO para manejo de dinero
│   ├── Email.cs              # VO para validación de correos
│   ├── PhoneNumber.cs        # VO para números telefónicos
│   └── Address.cs            # VO para direcciones
│
├── Services/                 # Servicios compartidos
│   ├── DateTimeService.cs    # Servicio de manejo de fechas/horas
│   └── CurrencyService.cs    # Servicio de conversión de monedas
│
├── Interfaces/               # Interfaces compartidas
│   ├── IPaginationService.cs # Interfaz para paginación
│   └── IDateTimeProvider.cs  # Proveedor de fecha/hora
│
└── Exceptions/               # Excepciones comunes
    ├── InvalidValueException.cs
    └── ValidationException.cs
```

## Propósito del SharedKernel

El SharedKernel es el conjunto mínimo de conceptos que son compartidos entre todos los Bounded Contexts sin traducción. Es un acuerdo explícito entre todos los equipos sobre:

1. Conceptos comunes que aparecen en múltiples dominios
2. Reglas de validación universales
3. Comportamientos consistentes en todo el sistema

## Principales Componentes

### Value Objects

- **Money**: Encapsula el concepto de dinero con moneda y valor
- **Email**: Encapsula y valida direcciones de correo electrónico
- **PhoneNumber**: Encapsula y valida números telefónicos
- **Address**: Representa direcciones físicas con sus componentes

### Servicios

- **DateTimeService**: Abstracción para manejo de fechas y horas
- **CurrencyService**: Conversión y manejo de diferentes monedas

### Interfaces

- **IPaginationService**: Interfaces para paginación de resultados
- **IDateTimeProvider**: Interface para obtener fecha/hora actual

### Excepciones

- **InvalidValueException**: Para valores inválidos en el dominio
- **ValidationException**: Para fallos en validaciones de reglas

## Consideraciones Importantes

1. **Cambios cuidadosos**: Modificaciones al SharedKernel impactan múltiples Bounded Contexts
2. **Mínimo indispensable**: Solo incluir lo que realmente es necesario compartir
3. **Sin dependencias externas**: El SharedKernel no debe depender de contextos específicos
4. **Estabilidad**: Debe ser muy estable, con cambios poco frecuentes
5. **Inmutabilidad**: Preferir objetos inmutables para evitar efectos secundarios 