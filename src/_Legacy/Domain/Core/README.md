# Core - Bounded Contexts y SharedKernel

Esta parte del dominio implementa el concepto de Bounded Contexts (Contextos Delimitados) de Domain-Driven Design (DDD), facilitando la clara separación entre diferentes modelos de dominio y sus límites.

## Estructura de carpetas

`
Core/
├── SharedKernel/               # Elementos compartidos entre todos los contextos
│   ├── ValueObjects/           # ValueObjects genéricos (Money, Email, etc.)
│   ├── Interfaces/             # Interfaces compartidas (IRepository, etc.)
│   ├── Services/               # Servicios base compartidos
│   └── Exceptions/             # Excepciones comunes
│
├── BoundedContexts/            # Definición de contextos y sus límites
│   ├── Catalogo/               # Contexto de Catálogo de Productos
│   ├── Identidad/              # Contexto de Gestión de Identidad
│   └── ContextMap/             # Mapeo de relaciones entre contextos
`

## Conceptos implementados

### SharedKernel
Conjunto mínimo de conceptos, interfaces y entidades que son comunes a todos los Bounded Contexts del sistema. Estos elementos son utilizados sin traducción por todos los contextos.

### Bounded Contexts
Fronteras explícitas alrededor de un modelo de dominio, dentro de las cuales los términos y conceptos tienen un significado específico y consistente.

### Context Map
Documentación de las relaciones entre diferentes Bounded Contexts, incluidos patrones como:
- **Conformist**: Un contexto se adapta al modelo de otro
- **Anti-corruption Layer**: Capa de traducción entre contextos
- **Partnership**: Colaboración en la definición del modelo
- **Customer-Supplier**: Relación de consumo de servicios

## Beneficios

1. **Claridad conceptual**: Cada equipo puede trabajar con su propio modelo sin confusiones
2. **Flexibilidad**: Permite evolucionar partes del sistema de forma independiente
3. **Escalabilidad organizacional**: Diferentes equipos pueden trabajar en diferentes contextos
4. **Consistencia local**: Cada contexto mantiene consistencia dentro de sus límites
5. **Integridad global**: El Context Map documenta cómo se relacionan los diferentes contextos
