# Módulo Core/BoundedContexts - Domain

Este módulo define y documenta los diferentes Bounded Contexts (Contextos Delimitados) del sistema RestaurantePro, así como las relaciones entre ellos mediante el Context Map (Mapa de Contextos).

## Estructura del Módulo

```
BoundedContexts/
├── ContextMap/               # Definición de las relaciones entre contextos
│   ├── ContextMap.cs         # Configuración de relaciones entre contextos
│   └── ContextMapDiagram.md  # Documentación visual de relaciones
│
├── Operaciones/              # Definición del contexto Operaciones
│   └── OperacionesContext.cs
│
├── Comercial/                # Definición del contexto Comercial
│   └── ComercialContext.cs
│
└── [Otros Contextos]/        # Otros contextos del sistema
```

## Propósito de los Bounded Contexts

Los Bounded Contexts son una técnica central de Domain-Driven Design que define los límites explícitos dentro de los cuales un modelo de dominio particular es aplicable. Estos límites permiten:

1. Mantener la consistencia dentro de cada contexto
2. Definir un lenguaje ubicuo específico para cada contexto
3. Limitar la complejidad de cada modelo
4. Facilitar la evolución independiente de cada contexto

## Context Map

El Context Map documenta cómo los diferentes Bounded Contexts se relacionan entre sí, incluyendo:

### Tipos de Relaciones

- **Shared Kernel**: Parte del modelo compartida entre dos contextos
- **Customer-Supplier**: Relación upstream/downstream donde un contexto provee a otro
- **Conformist**: Un contexto se adapta al modelo de otro sin influenciarlo
- **Anti-Corruption Layer**: Capa de traducción entre dos contextos
- **Open Host Service**: API bien definida para integrarse con otros contextos
- **Published Language**: Esquema de datos compartido entre contextos

### Contextos Principales

#### Contexto Operaciones
- **Propósito**: Gestión de comandas, reservaciones y operación diaria
- **Subcontextos**: Comandas, Reservaciones
- **Relaciones**: 
  - Proveedor para Contexto Comercial (Customer-Supplier)
  - Consumidor de Contexto Productos (Conformist)

#### Contexto Comercial
- **Propósito**: Gestión de clientes, ventas y marketing
- **Subcontextos**: Clientes, Promociones
- **Relaciones**:
  - Consumidor de Contexto Operaciones (Customer-Supplier)
  - Usa Anti-Corruption Layer para integrarse con sistemas externos

## Implementación Práctica

En la implementación del código:

1. Cada contexto tiene su propia área en el proyecto (`Operaciones/`, `Comercial/`, etc.)
2. Las clases dentro de un contexto usan un lenguaje consistente
3. Las traducciones entre contextos ocurren en sus bordes
4. Los eventos de dominio se utilizan para comunicación entre contextos
5. El SharedKernel contiene solo lo estrictamente necesario compartir

## Evolución de los Bounded Contexts

Los contextos pueden evolucionar de formas diferentes:

1. Un contexto grande puede dividirse en varios más pequeños
2. Contextos separados pueden fusionarse si sus límites se difuminan
3. Las relaciones entre contextos pueden cambiar con el tiempo
4. Nuevos contextos pueden emerger con requisitos nuevos 