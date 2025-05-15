# Casos de Uso Expandidos - RestaurantePro

## Índice de Casos de Uso
1. [CU-001: Iniciar Sesión](#cu-001-iniciar-sesión)
2. [CU-002: Gestionar Mesas](#cu-002-gestionar-mesas)
3. [CU-003: Tomar Comanda](#cu-003-tomar-comanda)
4. [CU-004: Preparar Comanda en Cocina](#cu-004-preparar-comanda-en-cocina)
5. [CU-005: Procesar Pago](#cu-005-procesar-pago)
6. [CU-006: Gestionar Productos](#cu-006-gestionar-productos)
7. [CU-007: Gestionar Inventario](#cu-007-gestionar-inventario)
8. [CU-008: Generar Reportes](#cu-008-generar-reportes)
9. [CU-009: Gestionar Reservaciones](#cu-009-gestionar-reservaciones)
10. [CU-010: Administrar Usuarios](#cu-010-administrar-usuarios)

---

## CU-001: Iniciar Sesión

### Descripción
Este caso de uso describe el proceso mediante el cual un usuario accede al sistema con sus credenciales.

### Actores
- Usuario (Mesero, Cocinero, Gerente, Administrador)

### Precondiciones
- El usuario debe estar registrado en el sistema
- El usuario debe tener credenciales válidas
- El dispositivo debe tener conectividad a internet o a la red local

### Flujo Básico
1. El usuario abre la aplicación (móvil o web)
2. El sistema muestra la pantalla de inicio de sesión
3. El usuario ingresa su correo electrónico y contraseña
4. El usuario presiona el botón "Iniciar Sesión"
5. El sistema valida las credenciales
6. El sistema autentica al usuario
7. El sistema redirige al usuario a la pantalla principal correspondiente a su rol

### Flujos Alternativos

#### A. Credenciales Inválidas
1. En el paso 5, si las credenciales son inválidas:
   - El sistema muestra un mensaje de error
   - El sistema permite al usuario intentar nuevamente

#### B. Cuenta Bloqueada
1. En el paso 5, si la cuenta está bloqueada:
   - El sistema muestra un mensaje informando que la cuenta está bloqueada
   - El sistema proporciona información de contacto para soporte

#### C. Recordar Contraseña
1. En la pantalla de inicio de sesión:
   - El usuario hace clic en "Olvidé mi contraseña"
   - El sistema muestra un formulario para ingresar el correo electrónico
   - El usuario ingresa su correo y envía la solicitud
   - El sistema envía un enlace de restablecimiento a su correo

### Postcondiciones
- El usuario queda autenticado en el sistema
- El sistema registra la fecha y hora del inicio de sesión
- El sistema muestra las funcionalidades correspondientes al rol del usuario

### Requisitos Especiales
- El tiempo de respuesta para la validación no debe superar los 2 segundos
- La contraseña debe transmitirse de forma segura (HTTPS)
- Después de 5 intentos fallidos, la cuenta debe bloquearse temporalmente

---

## CU-002: Gestionar Mesas

### Descripción
Este caso de uso describe cómo los usuarios visualizan y gestionan el estado de las mesas del restaurante.

### Actores
- Mesero
- Gerente

### Precondiciones
- El usuario debe estar autenticado en el sistema
- El usuario debe tener permisos para gestionar mesas

### Flujo Básico
1. El usuario selecciona la opción "Mesas" en el menú principal
2. El sistema muestra un plano con todas las mesas del restaurante
3. Las mesas se muestran con colores diferentes según su estado:
   - Verde: Libre
   - Rojo: Ocupada
   - Amarillo: Reservada
   - Gris: En mantenimiento
4. El usuario selecciona una mesa
5. El sistema muestra información detallada sobre la mesa:
   - Número de mesa
   - Capacidad
   - Estado actual
   - Comandas activas (si está ocupada)
   - Reservaciones futuras (si las hay)

### Flujos Alternativos

#### A. Cambiar Estado de Mesa
1. En el paso 5, el usuario puede cambiar el estado de la mesa
2. El usuario selecciona "Cambiar Estado"
3. El sistema muestra las opciones disponibles
4. El usuario selecciona el nuevo estado
5. El sistema actualiza el estado de la mesa

#### B. Ver Historial de Mesa
1. En el paso 5, el usuario puede seleccionar "Ver Historial"
2. El sistema muestra un registro de las comandas y reservaciones pasadas
3. El usuario puede filtrar por fechas

#### C. Filtrar Mesas
1. En el paso 2, el usuario puede aplicar filtros:
   - Por estado
   - Por capacidad
   - Por ubicación

### Postcondiciones
- Los cambios en el estado de las mesas quedan registrados
- El sistema actualiza la visualización del plano de mesas

### Requisitos Especiales
- La visualización del plano de mesas debe actualizarse en tiempo real
- La interfaz debe ser intuitiva y táctil para dispositivos móviles

---

## CU-003: Tomar Comanda

### Descripción
Este caso de uso describe el proceso de tomar una comanda (orden) para una mesa.

### Actores
- Mesero

### Precondiciones
- El usuario debe estar autenticado con rol de Mesero
- La mesa debe existir y estar ocupada o cambiarse a ocupada

### Flujo Básico
1. El mesero selecciona una mesa en el plano
2. El sistema verifica el estado de la mesa
3. Si la mesa está libre, el sistema pregunta si desea ocuparla
4. El mesero confirma y la mesa cambia a estado "Ocupada"
5. El mesero selecciona "Nueva Comanda"
6. El sistema muestra el menú de productos organizados por categorías
7. El mesero selecciona los productos solicitados por el cliente
8. Para cada producto, el mesero indica:
   - Cantidad
   - Notas especiales (opcional)
   - Personalizaciones (si el producto es personalizable)
9. El sistema calcula el subtotal
10. El mesero confirma la comanda
11. El sistema guarda la comanda y la envía a cocina
12. El sistema muestra una confirmación

### Flujos Alternativos

#### A. Mesa ya tiene comanda activa
1. Si la mesa ya tiene una comanda activa:
   - El sistema muestra las comandas existentes
   - El mesero puede agregar productos a una comanda existente o crear una nueva

#### B. Personalización de Productos
1. En el paso 8, si el producto es personalizable:
   - El sistema muestra los ingredientes del producto
   - El mesero puede agregar, quitar o modificar ingredientes
   - El sistema actualiza el precio si es necesario

#### C. Cancelar Comanda
1. En cualquier momento antes del paso 10:
   - El mesero puede cancelar la comanda
   - El sistema solicita confirmación
   - El sistema descarta los cambios

### Postcondiciones
- La comanda queda registrada en el sistema
- La comanda se envía a la cocina para su preparación
- La mesa queda asociada a la comanda

### Requisitos Especiales
- La notificación a cocina debe ser inmediata
- El sistema debe permitir la toma de comandas sin conexión y sincronizarlas cuando se recupere la conexión

---

## CU-004: Preparar Comanda en Cocina

### Descripción
Este caso de uso describe cómo el personal de cocina visualiza y actualiza el estado de las comandas durante su preparación.

### Actores
- Cocinero
- Jefe de Cocina

### Precondiciones
- El usuario debe estar autenticado con rol de Cocinero o Jefe de Cocina
- Deben existir comandas en estado "Pendiente"

### Flujo Básico
1. El cocinero accede a la pantalla de "Comandas Pendientes"
2. El sistema muestra la lista de comandas pendientes ordenadas por tiempo
3. El cocinero selecciona una comanda
4. El sistema muestra los detalles de los productos a preparar
5. El cocinero marca la comanda como "En Preparación"
6. El cocinero prepara los productos
7. El cocinero marca cada producto como "Listo" cuando termina su preparación
8. Cuando todos los productos están listos, el cocinero marca la comanda como "Completada"
9. El sistema notifica al mesero que la comanda está lista

### Flujos Alternativos

#### A. Producto No Disponible
1. En el paso 6, si un producto no puede prepararse:
   - El cocinero marca el producto como "No Disponible"
   - El sistema solicita un motivo
   - El sistema notifica al mesero

#### B. Priorizar Comanda
1. El Jefe de Cocina puede cambiar la prioridad de las comandas
2. Las comandas de alta prioridad se muestran destacadas en la lista

#### C. Ver Historial de Comandas
1. El cocinero puede acceder al historial de comandas completadas
2. El sistema permite filtrar por fecha, mesa o estado

### Postcondiciones
- El estado de la comanda y sus productos se actualiza en el sistema
- El sistema notifica a los meseros sobre cambios importantes
- Se registran los tiempos de preparación

### Requisitos Especiales
- La interfaz debe ser optimizada para pantallas táctiles resistentes al calor y humedad
- Los cambios de estado deben reflejarse en tiempo real

---

## CU-005: Procesar Pago

### Descripción
Este caso de uso describe cómo se procesa el pago de una comanda.

### Actores
- Mesero
- Cliente (actor secundario)

### Precondiciones
- El usuario debe estar autenticado con rol de Mesero
- La comanda debe existir y estar en estado "Entregada"
- Todos los productos de la comanda deben estar entregados

### Flujo Básico
1. El mesero selecciona la mesa que desea cobrar
2. El sistema muestra las comandas asociadas a la mesa
3. El mesero selecciona "Generar Cuenta"
4. El sistema calcula el subtotal, impuestos y total
5. El mesero confirma los detalles de la cuenta
6. El sistema muestra la cuenta con opciones de pago
7. El mesero selecciona el método de pago (efectivo, tarjeta, etc.)
8. El sistema procesa el pago
9. El sistema genera un comprobante
10. El mesero entrega el comprobante al cliente
11. El mesero marca la mesa como "Libre"

### Flujos Alternativos

#### A. Pago Dividido
1. En el paso 6, el cliente puede solicitar dividir la cuenta:
   - Por porcentajes
   - Por productos específicos
   - En partes iguales
2. El mesero selecciona "Dividir Cuenta"
3. El sistema permite configurar la división
4. El sistema procesa cada pago por separado

#### B. Descuento o Propina
1. En el paso 5, el mesero puede aplicar:
   - Descuentos (requiere autorización de gerente)
   - Propina sugerida

#### C. Pago Parcial
1. En el paso 8, si el pago es parcial:
   - El sistema registra el pago parcial
   - La cuenta queda pendiente por el monto restante

### Postcondiciones
- El pago queda registrado en el sistema
- La comanda cambia a estado "Pagada"
- El comprobante se genera correctamente
- La mesa queda disponible para nuevos clientes (si se libera)

### Requisitos Especiales
- Integración con sistemas de pago (TPV, pasarelas de pago)
- Capacidad de funcionamiento offline con sincronización posterior
- Generación de comprobantes fiscales válidos

---

## CU-006: Gestionar Productos

### Descripción
Este caso de uso describe cómo se administran los productos ofrecidos en el menú.

### Actores
- Gerente
- Administrador

### Precondiciones
- El usuario debe estar autenticado con rol de Gerente o Administrador

### Flujo Básico
1. El usuario selecciona "Gestión de Productos" en el menú administrativo
2. El sistema muestra la lista de productos existentes
3. El usuario puede filtrar y buscar productos
4. El usuario selecciona "Nuevo Producto"
5. El sistema muestra el formulario de creación con campos:
   - Nombre
   - Descripción
   - Categoría
   - Precio
   - Imagen
   - Tiempo de preparación
   - Ingredientes
   - Disponibilidad
6. El usuario completa la información
7. El sistema valida los datos
8. El usuario guarda el nuevo producto
9. El sistema confirma la creación exitosa

### Flujos Alternativos

#### A. Editar Producto Existente
1. En el paso 2, el usuario selecciona un producto existente
2. El usuario selecciona "Editar"
3. El sistema muestra el formulario con los datos actuales
4. El usuario modifica los campos necesarios
5. El sistema valida y guarda los cambios

#### B. Cambiar Disponibilidad
1. El usuario puede marcar/desmarcar productos como disponibles
2. El sistema actualiza la disponibilidad
3. Los productos no disponibles se muestran como tal en el menú

#### C. Gestionar Categorías
1. El usuario puede crear, editar o eliminar categorías
2. El sistema permite ordenar las categorías para su visualización

### Postcondiciones
- Los cambios en productos se reflejan inmediatamente en el menú
- Se mantiene un historial de cambios en productos

### Requisitos Especiales
- Soporte para múltiples imágenes por producto
- Capacidad para importar/exportar catálogos de productos en formato Excel
- Control de versiones de menú (para cambios estacionales)

---

## CU-007: Gestionar Inventario

### Descripción
Este caso de uso describe cómo se gestiona el inventario de ingredientes y su relación con los productos.

### Actores
- Gerente
- Jefe de Cocina
- Administrador

### Precondiciones
- El usuario debe estar autenticado con permisos de gestión de inventario

### Flujo Básico
1. El usuario accede a "Gestión de Inventario"
2. El sistema muestra la lista de ingredientes con:
   - Nombre
   - Stock actual
   - Stock mínimo
   - Unidad de medida
   - Estado (Normal, Bajo, Agotado)
3. El usuario selecciona "Nuevo Ingrediente"
4. El sistema muestra el formulario de registro
5. El usuario completa la información
6. El sistema guarda el nuevo ingrediente

### Flujos Alternativos

#### A. Registrar Movimiento de Inventario
1. El usuario selecciona un ingrediente
2. El usuario selecciona "Registrar Movimiento"
3. El sistema muestra opciones:
   - Entrada (compra, devolución)
   - Salida (uso, merma, caducidad)
4. El usuario registra la cantidad y motivo
5. El sistema actualiza el stock

#### B. Alerta de Stock Bajo
1. El sistema identifica ingredientes con stock bajo
2. El sistema genera alertas
3. El usuario puede generar órdenes de compra desde las alertas

#### C. Ajuste de Inventario
1. El usuario realiza un conteo físico
2. El usuario selecciona "Ajuste de Inventario"
3. El usuario registra las cantidades reales
4. El sistema ajusta el stock y registra la discrepancia

### Postcondiciones
- El inventario se actualiza en tiempo real
- Se mantiene un registro de todos los movimientos
- El sistema alerta sobre ingredientes en niveles críticos

### Requisitos Especiales
- Integración con el módulo de productos para actualización automática
- Capacidad para escanear códigos de barras
- Generación de reportes de consumo y proyecciones

---

## CU-008: Generar Reportes

### Descripción
Este caso de uso describe cómo los usuarios generan reportes para análisis y toma de decisiones.

### Actores
- Gerente
- Administrador
- Propietario

### Precondiciones
- El usuario debe estar autenticado con permisos para generar reportes

### Flujo Básico
1. El usuario accede a "Reportes" en el portal web
2. El sistema muestra las categorías de reportes disponibles:
   - Ventas
   - Inventario
   - Productividad
   - Clientes
   - Financieros
3. El usuario selecciona el tipo de reporte
4. El sistema muestra opciones de configuración:
   - Rango de fechas
   - Filtros específicos
   - Formato de salida (PDF, Excel, CSV)
5. El usuario configura los parámetros
6. El usuario selecciona "Generar Reporte"
7. El sistema procesa los datos
8. El sistema muestra una vista previa del reporte
9. El usuario puede descargar, imprimir o compartir el reporte

### Flujos Alternativos

#### A. Reportes Programados
1. El usuario puede programar la generación automática
2. El sistema solicita:
   - Frecuencia (diario, semanal, mensual)
   - Destinatarios
   - Formato
3. El sistema genera y envía los reportes según lo programado

#### B. Reportes Personalizados
1. El usuario selecciona "Reporte Personalizado"
2. El usuario selecciona las métricas y dimensiones
3. El sistema genera el reporte según las especificaciones

#### C. Dashboard Interactivo
1. El usuario accede al "Dashboard"
2. El sistema muestra gráficos interactivos
3. El usuario puede modificar la visualización

### Postcondiciones
- El reporte se genera correctamente
- El sistema registra la generación del reporte
- El formato de salida cumple con los requisitos del usuario

### Requisitos Especiales
- Capacidad para procesar grandes volúmenes de datos
- Visualizaciones gráficas interactivas
- Exportación en múltiples formatos

---

## CU-009: Gestionar Reservaciones

### Descripción
Este caso de uso describe el proceso de creación y gestión de reservaciones de mesas.

### Actores
- Mesero
- Gerente
- Cliente (actor secundario)

### Precondiciones
- El usuario debe estar autenticado con permisos para gestionar reservaciones

### Flujo Básico
1. El usuario accede a "Reservaciones"
2. El sistema muestra el calendario de reservaciones
3. El usuario selecciona "Nueva Reservación"
4. El sistema muestra el formulario con campos:
   - Fecha y hora
   - Número de personas
   - Duración estimada
   - Datos del cliente (nombre, teléfono, email)
   - Mesas disponibles para la fecha y hora
5. El usuario completa la información
6. El sistema verifica la disponibilidad
7. El usuario confirma la reservación
8. El sistema registra la reservación
9. El sistema envía confirmación al cliente

### Flujos Alternativos

#### A. Editar Reservación
1. El usuario busca una reservación existente
2. El usuario selecciona "Editar"
3. El usuario modifica los datos necesarios
4. El sistema verifica la disponibilidad
5. El sistema actualiza la reservación

#### B. Cancelar Reservación
1. El usuario selecciona una reservación
2. El usuario selecciona "Cancelar"
3. El sistema solicita un motivo
4. El sistema actualiza el estado a "Cancelada"
5. El sistema notifica al cliente

#### C. Gestionar Llegada
1. Cuando el cliente llega, el usuario busca la reservación
2. El usuario confirma la llegada
3. El sistema asigna la mesa
4. El estado de la mesa cambia a "Ocupada"

### Postcondiciones
- La reservación queda registrada en el sistema
- La mesa queda bloqueada para el horario reservado
- Se envían notificaciones al cliente

### Requisitos Especiales
- Sincronización con sistemas externos de reservas
- Notificaciones por email y SMS
- Visualización clara de disponibilidad

---

## CU-010: Administrar Usuarios

### Descripción
Este caso de uso describe cómo se gestionan los usuarios del sistema y sus permisos.

### Actores
- Administrador

### Precondiciones
- El usuario debe estar autenticado con rol de Administrador

### Flujo Básico
1. El administrador accede a "Gestión de Usuarios"
2. El sistema muestra la lista de usuarios registrados
3. El administrador selecciona "Nuevo Usuario"
4. El sistema muestra el formulario con campos:
   - Nombre y apellido
   - Email
   - Teléfono
   - Rol (Mesero, Cocinero, Gerente, etc.)
   - Estado (Activo, Inactivo)
5. El administrador completa la información
6. El sistema valida los datos
7. El sistema crea el usuario
8. El sistema envía credenciales temporales al email

### Flujos Alternativos

#### A. Editar Usuario
1. El administrador selecciona un usuario existente
2. El administrador selecciona "Editar"
3. El administrador modifica los datos necesarios
4. El sistema actualiza la información

#### B. Cambiar Estado de Usuario
1. El administrador puede activar/desactivar usuarios
2. El sistema actualiza el estado
3. Los usuarios inactivos no pueden acceder al sistema

#### C. Restablecer Contraseña
1. El administrador selecciona "Restablecer Contraseña"
2. El sistema genera una contraseña temporal
3. El sistema envía la contraseña al email del usuario

### Postcondiciones
- Los cambios en usuarios se registran en el sistema
- Los usuarios pueden acceder según sus permisos
- Se mantiene un registro de cambios en usuarios

### Requisitos Especiales
- Gestión de permisos granular
- Autenticación de doble factor para administradores
- Cumplimiento con normativas de protección de datos 