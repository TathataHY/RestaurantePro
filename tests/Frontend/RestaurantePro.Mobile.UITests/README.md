# 🧪 Tests UI para RestaurantePro Mobile

## 🎯 **Arquitectura de Tests**

Este proyecto implementa una **arquitectura híbrida** que combina lo mejor de ambos mundos:

### 📱 **Appium Real** 
- **Interactúa con la UI móvil real** en el emulador/dispositivo
- **Tests de UI verdaderos** que validan la experiencia del usuario
- **Detección de problemas reales** de interfaz y usabilidad

### 🌐 **API en Memoria**
- **Tests ultra rápidos** (sin latencia de red)
- **Datos consistentes** entre ejecuciones
- **Sin dependencias externas** (BD, servicios web, etc.)

### 🔗 **Conexión UI-API**
- **La UI móvil se conecta a la API en memoria** durante los tests
- **Validación end-to-end** del flujo completo
- **Tests de integración reales** sin mocks

## 🚀 **Cómo Funciona**

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Test Runner   │───▶│  Appium Driver   │───▶│  App Móvil      │
│                 │    │                  │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│ WebAppFactory   │───▶│  API en Memoria  │◀───│  HTTP Client    │
│ (In-Memory)     │    │                  │    │  (App Móvil)    │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## 📋 **Requisitos Previos**

### 1. **Appium Server**
```bash
# Instalar Appium globalmente
npm install -g appium

# Iniciar servidor Appium
appium
```

### 2. **Android SDK y Emulador**
```bash
# Verificar dispositivos disponibles
adb devices

# Crear emulador (si no existe)
avdmanager create avd -n RestaurantePro_Test -k "system-images;android-34;google_apis;x86_64"
```

### 3. **Compilar la App Móvil**
```bash
# Compilar y generar APK
dotnet build src/Frontend/RestaurantePro.Mobile/RestaurantePro.Mobile.csproj
```

## 🧪 **Ejecutar Tests**

### **Test Simple de Verificación**
```bash
dotnet test --filter "FullyQualifiedName~SimpleUITest"
```

### **Tests de Autenticación**
```bash
dotnet test --filter "FullyQualifiedName~AuthenticationTests"
```

### **Todos los Tests UI**
```bash
dotnet test
```

## ⚙️ **Configuración**

### **appsettings.Test.json**
```json
{
  "AppiumConfig": {
    "ServerUrl": "http://localhost:4723",
    "PlatformName": "Android",
    "PlatformVersion": "14.0",
    "DeviceName": "emulator-5554",
    "AppPath": "com.companyname.restaurantepro.mobile-Signed.apk"
  },
  "EmulatorConfig": {
    "Enabled": true,
    "AvdName": "RestaurantePro_Test"
  }
}
```

## 🔍 **Estructura de Tests**

### **TestBase/AppiumTestBase.cs**
- **Clase base** para todos los tests UI
- **Inicialización automática** de Appium y API en memoria
- **Helpers** para screenshots, esperas inteligentes, etc.

### **PageObjects/**
- **LoginPageObject.cs** - Interacción con pantalla de login
- **DashboardPageObject.cs** - Interacción con dashboard
- **ComandasPageObject.cs** - Gestión de comandas
- **MesasPageObject.cs** - Gestión de mesas
- **ProductosPageObject.cs** - Gestión de productos

### **Tests/**
- **SimpleUITest.cs** - Verificación básica de la app
- **AuthenticationTests.cs** - Tests de login y autenticación
- **ComandaFlowTests.cs** - Flujo completo de comandas
- **UIWithApiTest.cs** - Integración UI-API

## 💡 **Ventajas de esta Arquitectura**

### ✅ **Rapidez**
- Tests UI completos en segundos (no minutos)
- API en memoria sin latencia de red
- Sin tiempo de espera por servicios externos

### ✅ **Confiabilidad**
- Sin dependencias de red o servicios externos
- Datos consistentes entre ejecuciones
- Tests determinísticos y predecibles

### ✅ **Cobertura Real**
- **UI real** (no mocks)
- **Integración real** entre UI y API
- **Flujos completos** de usuario

### ✅ **Mantenibilidad**
- Tests fáciles de entender y modificar
- Page Objects reutilizables
- Configuración centralizada

## 🚨 **Solución de Problemas**

### **Error: App no encontrada**
```bash
# Verificar que el APK esté en la carpeta de salida
ls tests/Frontend/RestaurantePro.Mobile.UITests/bin/Debug/net9.0/

# Verificar que el emulador esté funcionando
adb devices

# Verificar que Appium esté ejecutándose
curl http://localhost:4723/status
```

### **Error: Emulador no disponible**
```bash
# Listar emuladores disponibles
emulator -list-avds

# Iniciar emulador específico
emulator -avd RestaurantePro_Test
```

### **Error: API en memoria no responde**
```bash
# Verificar que la API compile correctamente
dotnet build src/Backend/RestaurantePro.Api/RestaurantePro.Api.csproj

# Verificar dependencias del proyecto de tests
dotnet restore tests/Frontend/RestaurantePro.Mobile.UITests/RestaurantePro.Mobile.UITests.csproj
```

## 🎉 **Resultado Final**

Con esta arquitectura obtienes:

1. **Tests UI verdaderos** que validan la experiencia real del usuario
2. **Velocidad de tests unitarios** gracias a la API en memoria
3. **Cobertura completa** del flujo UI-API
4. **Mantenimiento simple** y tests fáciles de entender
5. **Integración continua** confiable y rápida

¡Los tests UI ahora son **rápidos, confiables y reales**! 🚀 