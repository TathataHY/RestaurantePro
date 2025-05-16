# Módulo de Proveedores

## Descripción
Este módulo gestiona toda la información relacionada con los proveedores del restaurante,
permitiendo mantener un registro detallado de quienes suministran insumos y materias primas.
Es un módulo crítico para la gestión de compras y la cadena de suministro.

## Estructura
- **Entidades**: Entidades principales del dominio de proveedores.
  - **Proveedor**: Información de contacto y gestión de proveedores.
  - **ProveedorCategoria**: Clasificación de proveedores por tipo de insumos.

## Contexto en DDD
Este módulo representa el Bounded Context de "Proveedores" según el Context Map del sistema,
con una relación de Partnership con el contexto de Inventario para la gestión de compras.

## Responsabilidades Principales
1. Mantener un catálogo actualizado de proveedores
2. Gestionar información de contacto y representantes
3. Clasificar proveedores por categorías de productos
4. Evaluar y calificar proveedores según su desempeño
5. Asociar proveedores con las órdenes de compra correspondientes

## Integración con otros módulos
- **Inventario/Compras**: Para asociar proveedores a órdenes de compra
- **Finanzas**: Para gestionar pagos y condiciones comerciales
- **Catálogo**: Para relacionar proveedores con los productos que suministran

## Futuras extensiones
- Portal de proveedores para gestión de cotizaciones
- Sistema de puntuación y evaluación formal de proveedores
- Gestión de contratos y acuerdos de nivel de servicio
- Automatización de selección de proveedores según criterios predefinidos 