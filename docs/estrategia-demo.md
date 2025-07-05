# Estrategia de Demo y Distribución - RestaurantePro

## 🎯 Objetivo
Crear una demo pública que permita a potenciales clientes experimentar el sistema completo antes de comprar.

## 📊 Estructura de Ramas

### Ramas Principales
- **`main`**: Versión estable, base para clientes
- **`develop`**: Desarrollo activo, integración de features
- **`demo`**: Versión pública con datos ficticios

### Ramas de Clientes
- **`cliente-{nombre}`**: Personalizaciones específicas por cliente
- Ejemplo: `cliente-restaurante-mama`, `cliente-pizzeria-roma`

## 🚀 Estrategia de Demo

### Opción Recomendada: Demo con API Remota

#### Configuración
- **API Demo**: `https://demo.restaurantepro.com` (SmarterASP.NET)
- **Base de Datos**: SQL Server con datos ficticios
- **App Demo**: Conectada a la API remota

#### Datos Ficticios
```
Restaurante: "La Buena Mesa"
- 15 mesas
- 25 productos (entradas, platos principales, postres)
- 3 usuarios demo (admin, mesero, cocinero)
- Inventario pre-cargado
- Historial de comandas de ejemplo
```

#### Protecciones
- **Reset automático**: Cada 24 horas
- **Límites de uso**: Máximo 10 comandas por sesión
- **Monitoreo**: Alertas de uso excesivo
- **Backup**: Restauración automática de datos

### Implementación Técnica

#### 1. Configuración de Entornos
```json
// appsettings.Demo.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=demo-server;Database=RestaurantePro_Demo;..."
  },
  "DemoSettings": {
    "MaxComandasPerSession": 10,
    "AutoResetHours": 24,
    "RestaurantName": "La Buena Mesa Demo"
  }
}
```

#### 2. Scripts de Reset
- Script SQL para restaurar datos demo
- Tarea programada para reset automático
- Logs de actividad para monitoreo

#### 3. Personalización por Cliente
```csharp
// ClienteConfiguration.cs
public class ClienteConfiguration
{
    public string NombreRestaurante { get; set; }
    public string LogoUrl { get; set; }
    public string ColorPrimario { get; set; }
    public string ColorSecundario { get; set; }
    public List<string> CaracteristicasEspeciales { get; set; }
}
```

## 💰 Modelo de Negocio

### Precio y Estrategia
- **Precio**: 350,000 CLP (venta única)
- **Estrategia**: Venta en masa, precio accesible
- **Beneficio**: Volumen de ventas vs. precio unitario alto

### Proceso de Venta
1. Cliente descarga demo desde app stores
2. Experimenta funcionalidad completa
3. Contacta para compra
4. Se crea rama específica del cliente
5. Personalización mínima (logo, colores, nombre)
6. Entrega e instalación

## 🔧 Consideraciones Técnicas

### Para la Demo
- **Base de datos aislada**: Sin afectar otros clientes
- **Monitoreo de uso**: Métricas de engagement
- **Backup automático**: Protección de datos demo
- **Escalabilidad**: Preparado para múltiples usuarios demo

### Para Clientes
- **Personalización mínima**: Logo, colores, nombre
- **Configuración rápida**: Setup en 1-2 días
- **Soporte técnico**: Documentación y asistencia
- **Actualizaciones**: Nuevas features para todos los clientes

## 📱 Distribución

### App Stores
- **Google Play Store**: Versión demo pública
- **App Store**: Versión demo pública
- **Descripción**: "Demo del sistema de gestión para restaurantes"

### Página Web
- Landing page con información del producto
- Términos y condiciones
- Preguntas frecuentes
- Formulario de contacto
- Enlaces a app stores

## 🎨 Personalización por Cliente

### Elementos Personalizables
- Logo del restaurante
- Nombre del restaurante
- Colores de la aplicación
- Textos específicos
- Configuración inicial (mesas, productos)

### Elementos No Personalizables
- Funcionalidad core
- Estructura de base de datos
- APIs y servicios
- Actualizaciones de seguridad

## 📈 Métricas de Éxito

### Demo
- Descargas de la app demo
- Tiempo de uso promedio
- Conversión a contactos
- Engagement con features

### Ventas
- Número de clientes adquiridos
- Tiempo promedio de conversión
- Satisfacción del cliente
- Referencias generadas 