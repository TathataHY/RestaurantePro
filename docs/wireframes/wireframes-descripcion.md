# Wireframes - RestaurantePro

Este documento describe los wireframes principales para el sistema RestaurantePro, proporcionando una guía visual de las interfaces de usuario antes de comenzar el desarrollo. Los wireframes están organizados por aplicación (móvil y web) y por funcionalidad principal.

## Aplicación Móvil (Meseros y Cocineros)

### 1. Pantalla de Inicio de Sesión
![Login](../imagenes/wireframes/movil-login.png)
- Campo de correo electrónico
- Campo de contraseña
- Botón de inicio de sesión
- Enlace para recuperar contraseña
- Logo de RestaurantePro

### 2. Dashboard Principal - Mesero
![Dashboard Mesero](../imagenes/wireframes/movil-dashboard-mesero.png)
- Resumen de mesas asignadas
- Indicadores de estado (mesas ocupadas, comandas pendientes)
- Acceso rápido a funciones principales:
  - Ver mesas
  - Tomar comanda
  - Procesar pagos
  - Ver notificaciones

### 3. Visualización de Mesas
![Mesas](../imagenes/wireframes/movil-mesas.png)
- Plano visual del restaurante
- Mesas codificadas por color según estado
- Información resumida al tocar cada mesa
- Filtros para visualizar por sección o estado
- Botón de acción flotante para cambiar vista o filtrar

### 4. Detalle de Mesa
![Detalle Mesa](../imagenes/wireframes/movil-detalle-mesa.png)
- Información completa de la mesa
- Estado actual
- Comandas activas con tiempos
- Botones de acción:
  - Nueva comanda
  - Ver comandas
  - Cambiar estado
  - Generar cuenta

### 5. Toma de Comanda
![Toma Comanda](../imagenes/wireframes/movil-toma-comanda.png)
- Selector de categorías en la parte superior
- Lista de productos con imágenes y precios
- Botón de búsqueda
- Panel lateral o inferior con resumen de la comanda actual
- Contador de productos
- Botones para personalizar productos
- Botón para confirmar comanda

### 6. Personalización de Producto
![Personalización](../imagenes/wireframes/movil-personalizacion.png)
- Imagen y nombre del producto
- Lista de ingredientes con opciones:
  - Agregar más
  - Quitar
  - Sustituir
- Campo para notas especiales
- Botones para cancelar o confirmar personalización

### 7. Proceso de Pago
![Pago](../imagenes/wireframes/movil-pago.png)
- Resumen de la cuenta
- Desglose de productos
- Cálculo de impuestos y total
- Opciones de división de cuenta
- Selección de método de pago
- Botón para procesar pago
- Opción para generar comprobante

### 8. Dashboard Principal - Cocinero
![Dashboard Cocinero](../imagenes/wireframes/movil-dashboard-cocinero.png)
- Cola de comandas pendientes
- Comandas en preparación
- Indicadores de tiempo
- Filtros por tipo de producto
- Notificaciones de nuevas comandas

### 9. Detalle de Comanda - Cocina
![Detalle Comanda Cocina](../imagenes/wireframes/movil-detalle-comanda-cocina.png)
- Lista de productos a preparar
- Tiempos estimados
- Prioridad visual
- Notas especiales y personalizaciones
- Botones para marcar estados (iniciado, terminado)
- Opción para notificar problemas

## Portal Web (Gerentes y Administradores)

### 1. Dashboard Principal - Gerente
![Dashboard Web](../imagenes/wireframes/web-dashboard.png)
- Indicadores clave de rendimiento (KPIs):
  - Ventas del día
  - Mesas ocupadas
  - Tiempo promedio de servicio
  - Productos más vendidos
- Gráficos de rendimiento
- Alertas de inventario
- Comandas activas
- Menú de navegación principal

### 2. Gestión de Productos
![Gestión Productos](../imagenes/wireframes/web-productos.png)
- Tabla de productos con filtros y búsqueda
- Columnas para información principal:
  - Nombre
  - Categoría
  - Precio
  - Disponibilidad
- Botones de acción:
  - Nuevo producto
  - Editar
  - Desactivar
- Vista previa de imagen

### 3. Formulario de Producto
![Formulario Producto](../imagenes/wireframes/web-formulario-producto.png)
- Campos para toda la información del producto
- Carga de imágenes con vista previa
- Selector de categoría
- Gestión de ingredientes
- Opciones de personalización
- Botones para guardar o cancelar

### 4. Gestión de Inventario
![Gestión Inventario](../imagenes/wireframes/web-inventario.png)
- Tabla de ingredientes
- Indicadores visuales de stock (normal, bajo, agotado)
- Filtros por categoría y estado
- Botones para registrar movimientos
- Historial de movimientos recientes
- Alertas de stock mínimo

### 5. Reportes
![Reportes](../imagenes/wireframes/web-reportes.png)
- Selector de tipo de reporte
- Filtros de fecha y parámetros
- Vista previa de reporte
- Opciones de exportación y compartir
- Gráficos interactivos
- Tablas de datos con opción de ordenar

### 6. Configuración de Mesas
![Configuración Mesas](../imagenes/wireframes/web-configuracion-mesas.png)
- Editor visual del plano del restaurante
- Herramientas para agregar, mover o eliminar mesas
- Propiedades de mesa (número, capacidad, forma)
- Definición de secciones o áreas
- Vista previa del plano final

### 7. Gestión de Usuarios
![Gestión Usuarios](../imagenes/wireframes/web-usuarios.png)
- Tabla de usuarios con filtros
- Información principal (nombre, rol, estado)
- Botones para crear, editar o desactivar usuarios
- Asignación de permisos
- Historial de actividad

### 8. Calendario de Reservaciones
![Reservaciones](../imagenes/wireframes/web-reservaciones.png)
- Vista de calendario con reservaciones
- Vista alternativa de lista
- Formulario de nueva reservación
- Filtros por fecha, mesa o estado
- Código de colores por estado
- Opción para confirmaciones y recordatorios

## Notas sobre el Diseño

### Principios de Diseño
- **Consistencia**: Mantener patrones de interfaz coherentes en toda la aplicación
- **Accesibilidad**: Asegurar que la interfaz sea usable por personas con diferentes capacidades
- **Responsividad**: Adaptar la interfaz a diferentes tamaños de pantalla
- **Simplicidad**: Minimizar la complejidad, enfocándose en las tareas principales
- **Feedback**: Proporcionar retroalimentación clara para todas las acciones

### Paleta de Colores
- **Principal**: Tonos de azul (#1a73e8, #4285f4)
- **Secundario**: Tonos de verde (#34a853)
- **Acento**: Naranja (#f9ab00)
- **Estados**:
  - Libre: Verde (#34a853)
  - Ocupado: Rojo (#ea4335)
  - Reservado: Amarillo (#fbbc04)
  - Mantenimiento: Gris (#9aa0a6)

### Tipografía
- **Familia principal**: Roboto
- **Títulos**: Roboto Bold
- **Texto**: Roboto Regular
- **Jerarquía clara** con diferentes tamaños y pesos para mejorar la legibilidad

### Iconografía
- Set de iconos consistente (Material Design)
- Iconos significativos que comunican claramente su función
- Tamaño adecuado para interacción táctil en dispositivos móviles

## Próximos Pasos

1. Validar wireframes con stakeholders
2. Crear prototipos interactivos de alta fidelidad
3. Realizar pruebas de usabilidad
4. Refinar el diseño basado en feedback
5. Documentar guía de estilo detallada
6. Implementar componentes UI reutilizables 