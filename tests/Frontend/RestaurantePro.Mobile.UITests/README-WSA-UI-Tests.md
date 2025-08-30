# 🧪 Pruebas UI con WSA - RestaurantePro Mobile

Este documento describe cómo configurar y ejecutar las pruebas UI automatizadas para la aplicación móvil RestaurantePro usando **WSA (Windows Subsystem for Android)** y Appium.

## 🚀 **¿Por qué WSA?**

### **Ventajas de WSA para Pruebas UI:**
- ✅ **Más rápido** que emuladores tradicionales
- ✅ **Más estable** y confiable
- ✅ **Integración nativa** con Windows
- ✅ **Mejor rendimiento** para pruebas automatizadas
- ✅ **Configuración más simple**
- ✅ **Sin problemas de variables de entorno**

## 📋 **Requisitos Previos**

### **1. WSA (Windows Subsystem for Android)**
- ✅ **Windows 11** (WSA incluido por defecto)
- ✅ **WSA habilitado** en Windows Features
- ✅ **Modo desarrollador** habilitado en WSA

### **2. Herramientas Necesarias**
- ✅ **Node.js** (v18 o superior)
- ✅ **Android SDK** con ADB configurado
- ✅ **.NET 9.0** SDK
- ✅ **Appium** (se instala automáticamente)

## 🔧 **Configuración de WSA**

### **Paso 1: Verificar WSA Instalado**
```powershell
# Verificar si WSA está instalado
Get-AppxPackage -Name "MicrosoftCorporationII.WindowsSubsystemForAndroid"
```

### **Paso 2: Habilitar Modo Desarrollador**
1. **Abrir WSA Settings:**
   ```powershell
   Start-Process "ms-windows-store://pdp/?ProductId=9P3395VX91NR"
   ```

2. **Configurar WSA:**
   - ✅ Habilitar **"Modo desarrollador"**
   - ✅ Habilitar **"Depuración USB"**
   - ✅ Anotar el **puerto IP** mostrado (ej: 127.0.0.1:58526)

### **Paso 3: Conectar WSA via ADB**
```powershell
# Conectar a WSA (usar el puerto mostrado en la configuración)
adb connect 127.0.0.1:58526

# Verificar conexión
adb devices
```

## 🚀 **Configuración Rápida con Script**

### **Script Automatizado (Recomendado)**
```powershell
# Ejecutar el script de configuración WSA
.\setup-wsa-ui-tests.ps1
```

### **Configuración Manual**

#### **Paso 1: Instalar Appium**
```powershell
npm install -g appium
appium driver install uiautomator2
```

#### **Paso 2: Compilar Aplicación**
```powershell
cd src\Frontend\RestaurantePro.Mobile
dotnet build -c Debug
```

#### **Paso 3: Instalar APK en WSA**
```powershell
# Instalar APK en WSA
adb install -r "bin\Debug\net9.0-android\com.companyname.restaurantepro.mobile-Signed.apk"
```

#### **Paso 4: Iniciar Servicios**
```powershell
# Terminal 1: Iniciar Appium Server
appium

# Terminal 2: Verificar WSA conectado
adb devices
```

#### **Paso 5: Ejecutar Pruebas**
```powershell
cd tests\Frontend\RestaurantePro.Mobile.UITests
dotnet test --verbosity normal
```

## 📱 **Configuración de WSA**

### **Configuración Óptima para Pruebas**
- **Plataforma**: Android 13.0
- **Dispositivo**: Windows Subsystem for Android
- **Automation**: UiAutomator2
- **Puerto ADB**: 58526 (por defecto)
- **Modo**: Desarrollador habilitado

### **Configuración de Rendimiento**
```json
{
  "WSAConfig": {
    "Enabled": true,
    "DefaultPort": "58526",
    "ConnectionTimeout": 30,
    "RetryAttempts": 3
  }
}
```

## 🧪 **Estructura de Pruebas WSA**

### **Pruebas Optimizadas para WSA (15 tests)**

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

### **Configuración Específica WSA**
```csharp
// AppiumTestBase.cs optimizado para WSA
Options.AddAdditionalAppiumOption("wsaEnabled", true);
Options.AddAdditionalAppiumOption("wsaConnectionTimeout", 30);
Options.AddAdditionalAppiumOption("uiautomator2ServerLaunchTimeout", 60000);
Options.AddAdditionalAppiumOption("androidInstallTimeout", 90000);
```

## ⚙️ **Configuración Avanzada**

### **Archivo de Configuración WSA**
```json
{
  "AppiumConfig": {
    "ServerUrl": "http://localhost:4723/wd/hub",
    "PlatformName": "Android",
    "PlatformVersion": "13.0",
    "DeviceName": "Windows Subsystem for Android",
    "AutomationName": "UiAutomator2",
    "ImplicitWaitSeconds": 15,
    "PageLoadTimeoutSeconds": 30,
    "AppPackage": "com.companyname.restaurantepro.mobile",
    "AppActivity": "crc64e1fb321c08285b90.MainActivity",
    "NoReset": true,
    "AutoGrantPermissions": true,
    "NewCommandTimeout": 60,
    "UnicodeKeyboard": true,
    "ResetKeyboard": true
  },
  "WSAConfig": {
    "Enabled": true,
    "DefaultPort": "58526",
    "ConnectionTimeout": 30,
    "RetryAttempts": 3
  }
}
```

## 🎯 **Ejecución de Pruebas WSA**

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

### **Ejecutar con Logs Detallados**
```powershell
dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

## 📊 **Resultados y Métricas WSA**

### **Rendimiento Esperado**
- **Tiempo de inicio**: 5-10 segundos (vs 30-60 segundos emulador)
- **Tiempo por prueba**: 15-30 segundos (vs 30-60 segundos emulador)
- **Tiempo total**: 8-12 minutos (vs 15-20 minutos emulador)
- **Estabilidad**: 95%+ (vs 80-85% emulador)

### **Screenshots Automáticos**
- Se capturan automáticamente en caso de error
- Ubicación: `Screenshots/` en el directorio de pruebas
- Formato: `{test-name}_{timestamp}.png`
- Optimizados para resolución WSA

## 🔧 **Troubleshooting WSA**

### **Problemas Comunes**

#### **1. "No se puede conectar a WSA"**
```powershell
# Verificar que WSA esté ejecutándose
Get-Process | Where-Object {$_.ProcessName -like "*WsaClient*"}

# Reiniciar WSA
Get-AppxPackage -Name "MicrosoftCorporationII.WindowsSubsystemForAndroid" | 
    ForEach-Object { Add-AppxPackage -DisableDevelopmentMode -Register "$($_.InstallLocation)\AppXManifest.xml" }
```

#### **2. "ADB no encuentra WSA"**
```powershell
# Verificar puertos WSA
netstat -an | findstr 58526

# Intentar diferentes puertos
adb connect 127.0.0.1:58526
adb connect 127.0.0.1:58527
adb connect 127.0.0.1:58528
```

#### **3. "APK no se instala en WSA"**
```powershell
# Verificar que el APK existe
Test-Path "src\Frontend\RestaurantePro.Mobile\bin\Debug\net9.0-android\com.companyname.restaurantepro.mobile-Signed.apk"

# Forzar reinstalación
adb install -r -d "path\to\app.apk"
```

#### **4. "Appium no puede iniciar sesión"**
```powershell
# Verificar que Appium Server esté ejecutándose
netstat -an | findstr 4723

# Reiniciar Appium Server
taskkill /f /im node.exe
appium
```

### **Logs de Debug WSA**
```powershell
# Ejecutar con logs detallados
dotnet test --verbosity detailed --logger "console;verbosity=detailed"

# Ver logs de WSA
adb logcat
```

## 📈 **Ventajas de WSA vs Emulador**

| Característica | WSA | Emulador Tradicional |
|----------------|-----|---------------------|
| **Velocidad de inicio** | ⚡ 5-10 segundos | 🐌 30-60 segundos |
| **Rendimiento** | ⚡ Excelente | 🐌 Lento |
| **Estabilidad** | ⚡ 95%+ | 🐌 80-85% |
| **Configuración** | ⚡ Simple | 🐌 Compleja |
| **Recursos** | ⚡ Bajos | 🐌 Altos |
| **Integración Windows** | ⚡ Nativa | 🐌 Limitada |

## 🚀 **Próximos Pasos**

### **Expansión de Pruebas WSA**
- [ ] Pruebas de Preparaciones Diarias
- [ ] Pruebas de Reservaciones
- [ ] Pruebas de Facturación
- [ ] Pruebas de Gestión de Clientes
- [ ] Pruebas de Reportes

### **Optimizaciones WSA**
- [ ] Ejecución paralela de pruebas
- [ ] Cache de APK en WSA
- [ ] Configuración automática de WSA
- [ ] Integración con CI/CD
- [ ] Reportes de rendimiento WSA

## 📞 **Soporte WSA**

### **Recursos Útiles**
- [Documentación oficial de WSA](https://docs.microsoft.com/en-us/windows/android/wsa/)
- [Guía de desarrollo WSA](https://docs.microsoft.com/en-us/windows/android/wsa/develop)
- [Troubleshooting WSA](https://docs.microsoft.com/en-us/windows/android/wsa/troubleshooting)

### **Contacto**
- **Proyecto**: RestaurantePro Mobile
- **Versión**: V2 - Conceptos Avanzados
- **Plataforma**: WSA (Windows Subsystem for Android)
- **Fecha**: Diciembre 2024

---

*Este framework de pruebas UI con WSA proporciona una base sólida y de alto rendimiento para asegurar la calidad de la aplicación móvil RestaurantePro.* 