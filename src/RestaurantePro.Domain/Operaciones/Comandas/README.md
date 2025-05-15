# Módulo Comandas - Vertical Slices

Este módulo implementa la arquitectura de Vertical Slices, organizando el código por funcionalidades o casos de uso en lugar de por capas técnicas.

## Estructura de carpetas

`
Comandas/
├── CrearComanda/                 # Funcionalidad de crear comanda
│   ├── Domain/                   # Modelos y reglas específicas
│   ├── Application/              # Comandos y handlers específicos
│   └── Infrastructure/           # Implementaciones específicas
│
├── ActualizarEstadoComanda/      # Funcionalidad de actualizar estado
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/
│
├── AgregarProductoComanda/       # Funcionalidad de agregar productos
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/
│
├── CancelarComanda/              # Funcionalidad de cancelar comanda
│   ├── Domain/
│   ├── Application/
│   └── Infrastructure/
│
└── FinalizarComanda/             # Funcionalidad de finalizar comanda
    ├── Domain/
    ├── Application/
    └── Infrastructure/
`

## Beneficios de Vertical Slices

1. **Cohesión por funcionalidad**: Todos los componentes relacionados con una funcionalidad están juntos
2. **Independencia entre slices**: Cambios en una funcionalidad no afectan a otras
3. **Productividad**: Facilita trabajar en funcionalidades específicas sin entender todo el sistema
4. **Evolución independiente**: Cada funcionalidad puede evolucionar a su propio ritmo
5. **Comprensión**: Es más fácil entender casos de uso completos cuando están agrupados

## Patrones adicionales utilizados

- **Command-Query Responsibility Segregation (CQRS)**: Separación entre comandos (escritura) y consultas (lectura)
- **Modelos de dominio específicos**: Cada slice tiene sus propios modelos específicos de dominio
- **Result Pattern**: Retorno de resultados estandarizados para cada operación
- **Repository Pattern**: Abstracción de la persistencia de datos
