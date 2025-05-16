# Módulo de Compras

## Descripción
Este módulo gestiona el proceso completo de adquisición de insumos para el restaurante, 
desde la generación de solicitudes hasta la recepción y procesamiento de pedidos.

## Submódulos
- **OrdenesCompra**: Gestiona las órdenes enviadas a proveedores y su ciclo de vida.

## Responsabilidades Principales
1. Gestionar relaciones con proveedores
2. Crear y dar seguimiento a órdenes de compra
3. Procesar recepciones de mercancías
4. Validar entregas contra órdenes
5. Gestionar devoluciones y ajustes

## Integración con otros módulos
- **Ingredientes**: Para actualizar stock cuando se reciben mercancías
- **Proveedores**: Para obtener información de proveedores disponibles
- **Finanzas**: Para el procesamiento de pagos a proveedores

## Futuras extensiones
- Gestión de cotizaciones y comparativas de precios
- Automatización de órdenes basadas en niveles de stock
- Programación de compras recurrentes
- Evaluación de desempeño de proveedores 