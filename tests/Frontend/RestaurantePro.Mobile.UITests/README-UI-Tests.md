# 🧪 Pruebas UI Automatizadas - RestaurantePro Mobile

Este documento describe cómo configurar y ejecutar las pruebas UI automatizadas para la aplicación móvil RestaurantePro usando Appium y xUnit.

## 📋 **Requisitos Previos**

### **1. Herramientas Necesarias**
- ✅ **Node.js** (v18 o superior)
- ✅ **Android SDK** con ADB configurado
- ✅ **.NET 9.0** SDK
- ✅ **Appium** (se instala automáticamente)
- ✅ **Emulador Android** o dispositivo físico

### **2. Verificación de Instalación**
```powershell
# Verificar Node.js
node --version

# Verificar ADB
adb version

# Verificar .NET
dotnet --version

# Verificar Appium
appium --version
```

## 🚀 **Configuración Rápida**

### **Opción 1: Script Automatizado (Recomendado)**
```powershell
# Ejecutar el script de configuración
.\setup-ui-tests.ps1
```

### **Opción 2: Configuración Manual**

#### **Paso 1: Instalar Appium**
```powershell
npm install -g appium
appium driver install uiautomator2
```

#### **Paso 2: Crear Emulador Android**
```powershell
# Listar imágenes disponibles
C:\Users\andre\AppData\Local\Android\Sdk\cmdline-tools\2.1\bin\avdmanager.bat list target

# Crear emulador
C:\Users\andre\AppData\Local\Android\Sdk\cmdline-tools\2.1\bin\avdmanager.bat create avd -n "RestaurantePro_Test" -k "system-images;android-30;google_apis_playstore;x86"
```

#### **Paso 3: Compilar Aplicación**
```powershell
cd src\Frontend\RestaurantePro.Mobile
dotnet build -c Debug
```

#### **Paso 4: Iniciar Servicios**
```powershell
# Terminal 1: Iniciar Appium Server
appium

# Terminal 2: Iniciar Emulador
C:\Users\andre\AppData\Local\Android\Sdk\emulator\emulator.exe -avd RestaurantePro_Test
```

#### **Paso 5: Ejecutar Pruebas**
```powershell
cd tests\Frontend\RestaurantePro.Mobile.UITests
dotnet test --verbosity normal
```

## 📱 **Configuración de Dispositivos**

### **Emulador Android**
- **Nombre**: RestaurantePro_Test
- **API Level**: 30 (Android 11)
- **ABI**: x86
- **Google Play**: Sí

### **Dispositivo Físico**
1. Habilitar **Opciones de desarrollador**
2. Habilitar **Depuración USB**
3. Conectar dispositivo via USB
4. Verificar con `adb devices`

## 🧪 **Estructura de Pruebas**

### **Pruebas Implementadas (15 tests)**

#### **🔐 AuthenticationTests (10 tests)**
- ✅ Login con credenciales válidas
- ✅ Login con credenciales inválidas
- ✅ Login con campos vacíos
- ✅ Validación de formato de email
- ✅ Verificación de elementos de página
- ✅ Funcionalidad del checkbox "Recordarme"
- ✅ Manejo del estado de carga
- ✅ Funcionalidad de logout
- ✅ Manejo de caracteres especiales
- ✅ Manejo de credenciales largas

#### **📋 ComandaFlowTests (5 tests)**
- ✅ Flujo completo de creación de comanda
- ✅ Creación con un solo producto
- ✅ Creación con múltiples cantidades
- ✅ Creación con observaciones
- ✅ Cancelación de comanda
- ✅ Filtrado de mesas

### **Page Object Models**
```
PageObjects/
├── LoginPageObject.cs          # Página de login
├── DashboardPageObject.cs      # Dashboard principal
├── MesasPageObject.cs          # Gestión de mesas
└── ComandasPageObject.cs       # Gestión de comandas
```

### **Test Base**
```
TestBase/
└── AppiumTestBase.cs           # Clase base para todas las pruebas
```

## ⚙️ **Configuración**

### **Archivo de Configuración**
```json
// appsettings.Test.json
{
  "AppiumConfig": {
    "ServerUrl": "http://localhost:4723/wd/hub",
    "PlatformName": "Android",
    "PlatformVersion": "11.0",
    "DeviceName": "Android Emulator",
    "AutomationName": "UiAutomator2",
    "ImplicitWaitSeconds": 10,
    "PageLoadTimeoutSeconds": 30
  },
  "TestData": {
    "ValidCredentials": {
      "Email": "test@example.com",
      "Password": "password123"
    },
    "InvalidCredentials": {
      "Email": "invalid@example.com",
      "Password": "wrongpassword"
    }
  },
  "Screenshots": {
    "Enabled": true,
    "Directory": "Screenshots"
  }
}
```

## 🎯 **Ejecución de Pruebas**

### **Ejecutar Todas las Pruebas**
```powershell
dotnet test --verbosity normal
```

### **Ejecutar Pruebas Específicas**
```powershell
# Solo pruebas de autenticación
dotnet test --filter "FullyQualifiedName~AuthenticationTests"

# Solo pruebas de comandas
dotnet test --filter "FullyQualifiedName~ComandaFlowTests"
```

### **Ejecutar con Reportes**
```powershell
dotnet test --logger "html;LogFileName=test-results.html"
```

## 📊 **Resultados y Reportes**

### **Screenshots Automáticos**
- Se capturan automáticamente en caso de error
- Ubicación: `Screenshots/` en el directorio de pruebas
- Formato: `{test-name}_{timestamp}.png`

### **Logs Detallados**
- Logs de Appium en la consola
- Logs de xUnit con información de pruebas
- Logs de Page Objects para debugging

## 🔧 **Troubleshooting**

### **Problemas Comunes**

#### **1. "No se puede establecer una conexión"**
```powershell
# Verificar que Appium Server esté ejecutándose
netstat -an | findstr 4723

# Reiniciar Appium Server
taskkill /f /im node.exe
appium
```

#### **2. "No se encuentra el dispositivo"**
```powershell
# Verificar dispositivos conectados
adb devices

# Reiniciar ADB
adb kill-server
adb start-server
```

#### **3. "Emulador no responde"**
```powershell
# Verificar procesos del emulador
Get-Process | Where-Object {$_.ProcessName -like "*emulator*"}

# Reiniciar emulador
taskkill /f /im emulator.exe
C:\Users\andre\AppData\Local\Android\Sdk\emulator\emulator.exe -avd RestaurantePro_Test
```

#### **4. "Error de compilación"**
```powershell
# Limpiar y reconstruir
dotnet clean
dotnet build -c Debug
```

### **Logs de Debug**
```powershell
# Ejecutar con logs detallados
dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

## 📈 **Métricas de Pruebas**

### **Cobertura Actual**
- **Total de Pruebas**: 15
- **Pruebas de Autenticación**: 10
- **Pruebas de Flujo**: 5
- **Page Objects**: 4
- **Cobertura de Funcionalidades**: ~80%

### **Tiempos de Ejecución**
- **Tiempo promedio por prueba**: 30-60 segundos
- **Tiempo total estimado**: 15-20 minutos
- **Setup inicial**: 2-3 minutos

## 🚀 **Próximos Pasos**

### **Expansión de Pruebas**
- [ ] Pruebas de Preparaciones Diarias
- [ ] Pruebas de Reservaciones
- [ ] Pruebas de Facturación
- [ ] Pruebas de Gestión de Clientes
- [ ] Pruebas de Reportes

### **Mejoras Técnicas**
- [ ] Ejecución paralela de pruebas
- [ ] Integración con CI/CD
- [ ] Pruebas cross-platform (iOS)
- [ ] Pruebas de rendimiento
- [ ] Pruebas de accesibilidad

## 📞 **Soporte**

### **Recursos Útiles**
- [Documentación de Appium](https://appium.io/docs/en/about-appium/intro/)
- [Documentación de xUnit](https://xunit.net/)
- [Documentación de .NET MAUI](https://docs.microsoft.com/en-us/dotnet/maui/)

### **Contacto**
- **Proyecto**: RestaurantePro Mobile
- **Versión**: V2 - Conceptos Avanzados
- **Fecha**: Diciembre 2024

---

*Este framework de pruebas UI proporciona una base sólida para asegurar la calidad de la aplicación móvil RestaurantePro.* 