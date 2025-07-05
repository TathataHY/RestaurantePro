# Organización de Pruebas de Integración

## Estructura General

Las pruebas de integración del dominio se han reorganizado siguiendo un enfoque claro que distingue entre:

1. **Pruebas entre Contextos (BetweenContexts)**: Verifican la integración y comunicación entre dos o más contextos delimitados
2. **Pruebas dentro de un Contexto (WithinContext)**: Verifican la integración entre componentes dentro del mismo contexto

## Detalles de la Estructura

### Pruebas Entre Contextos (BetweenContexts)

Organizadas por pares de contextos que interactúan:

- **Comercial_Inventario**: Integración entre el contexto Comercial y el contexto de Inventario
  - Ejemplo: Facturación de órdenes de compra aprobadas

- **Comercial_Operaciones**: Integración entre el contexto Comercial y el contexto de Operaciones
  - Ejemplo: Cancelación de reservaciones al desactivar un cliente
  - Ejemplo: Acumulación de puntos de fidelización al finalizar una comanda

- **Core_Comercial**: Integración entre el contexto Core y el contexto Comercial
  - Ejemplo: Cálculo de descuentos para productos recomendados

- **Operaciones_Inventario**: Integración entre el contexto de Operaciones y el contexto de Inventario
  - Ejemplo: Actualización del inventario al modificar una comanda

### Pruebas Dentro de un Contexto (WithinContext)

Organizadas por contexto individual:

- **Core**: Pruebas de integración dentro del contexto Core
  - Ejemplo: Asignación de roles al crear un usuario

- **Comercial**: Pruebas de integración dentro del contexto Comercial

- **Inventario**: Pruebas de integración dentro del contexto de Inventario
  - Ejemplo: Generación automática de órdenes de compra al detectar stock bajo

- **Operaciones**: Pruebas de integración dentro del contexto de Operaciones
  - Ejemplo: Gestión de cancelaciones de reservaciones

- **Proveedores**: Pruebas de integración dentro del contexto de Proveedores

## Convenciones de Nomenclatura

Para las pruebas de integración se utilizan los siguientes patrones de nomenclatura:

1. Para eventos entre contextos: `[EventoDesencadenante]_[ResultadoEsperado]Tests.cs`
   Ejemplo: `ClienteDesactivado_CancelacionReservacionesTests.cs`

2. Para pruebas dentro de un contexto: `[OperacionPrincipal]_[ResultadoEsperado]Tests.cs`
   Ejemplo: `StockBajo_GeneracionOrdenCompraAutomaticaTests.cs`

## Beneficios de esta Organización

- **Mayor claridad**: Facilita la identificación de pruebas relacionadas con contextos específicos
- **Mejor mantenibilidad**: Reduce la fricción para encontrar y actualizar pruebas relacionadas
- **Documentación estructural**: La organización misma documenta las relaciones entre contextos
- **Cobertura visible**: Permite identificar fácilmente áreas con poca cobertura de pruebas de integración 