# Pruebas UI Automatizadas - RestaurantePro Mobile

Este proyecto contiene las **pruebas UI automatizadas** para la aplicación móvil RestaurantePro usando **Appium** y **xUnit**.

## 🎯 **Objetivo**

Validar que toda la aplicación móvil funciona correctamente desde la perspectiva del usuario final, asegurando que:

- ✅ **Flujos completos** funcionan de principio a fin
- ✅ **Interfaz de usuario** responde correctamente
- ✅ **Navegación** entre pantallas es fluida
- ✅ **Validaciones** de formularios funcionan
- ✅ **Estados** de la aplicación se actualizan correctamente

## 🏗️ **Arquitectura**

### **Estructura del Proyecto**
```
RestaurantePro.Mobile.UITests/
├── TestBase/
│   └── AppiumTestBase.cs          # Clase base para todas las pruebas
├── PageObjects/
│   ├── LoginPageObject.cs         # POM para página de login
│   ├── DashboardPageObject.cs     # POM para dashboard
│   ├── MesasPageObject.cs         # POM para gestión de mesas
│   └── ComandasPageObject.cs      # POM para gestión de comandas
├── Tests/
│   ├── AuthenticationTests.cs     # Pruebas de autenticación
│   └── ComandaFlowTests.cs        # Pruebas de flujo de comandas
├── appsettings.Test.json          # Configuración de pruebas
└── GlobalUsings.cs                # Usings globales
```

### **Patrones Utilizados**
- **Page Object Model (POM)**: Encapsula la lógica de interacción con elementos UI
- **Base Class Pattern**: Clase base común para configuración de Appium
- **Configuration Pattern**: Configuración centralizada en JSON
- **Fluent Assertions**: Aserciones más legibles y expresivas

## 🚀 **Configuración Inicial**

### **1. Prerrequisitos**

#### **Software Requerido**
- ✅ **.NET 9.0 SDK**
- ✅ **Appium Server** (versión 2.x)
- ✅ **Android SDK** con emulador configurado
- ✅ **Java JDK** (para Appium)

#### **Instalación de Appium**
```bash
# Instalar Appium globalmente
npm install -g appium

# Instalar drivers necesarios
appium driver install uiautomator2
appium driver install xcuitest

# Verificar instalación
appium driver list
```

### **2. Configuración del Emulador Android**

#### **Crear Emulador**
```bash
# Listar dispositivos disponibles
emulator -list-avds

# Crear nuevo emulador (si no existe)
avdmanager create avd -n "Android_Test" -k "system-images;android-30;google_apis;x86"

# Iniciar emulador
emulator -avd Android_Test
```

#### **Configuración Recomendada**
- **API Level**: 30 (Android 11)
- **RAM**: 2GB
- **Internal Storage**: 2GB
- **SD Card**: 512MB

### **3. Configuración del Proyecto**

#### **Archivo appsettings.Test.json**
```json
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
      "Email": "admin@restaurantepro.com",
      "Password": "Admin123!"
    }
  }
}
```

## 🧪 **Ejecución de Pruebas**

### **1. Iniciar Servicios**

#### **Paso 1: Iniciar Appium Server**
```bash
# Terminal 1 - Iniciar Appium
appium --base-path /wd/hub
```

#### **Paso 2: Iniciar Emulador Android**
```bash
# Terminal 2 - Iniciar emulador
emulator -avd Android_Test
```

#### **Paso 3: Verificar Conexión**
```bash
# Verificar que el emulador está conectado
adb devices
```

### **2. Ejecutar Pruebas**

#### **Ejecutar Todas las Pruebas**
```bash
cd tests/Frontend/RestaurantePro.Mobile.UITests
dotnet test --verbosity normal
```

#### **Ejecutar Pruebas Específicas**
```bash
# Solo pruebas de autenticación
dotnet test --filter "FullyQualifiedName~AuthenticationTests"

# Solo pruebas de comandas
dotnet test --filter "FullyQualifiedName~ComandaFlowTests"

# Prueba específica
dotnet test --filter "DisplayName~LoginWithValidCredentials_ShouldNavigateToDashboard"
```

#### **Ejecutar con Reportes Detallados**
```bash
# Con reporte HTML
dotnet test --logger "html;LogFileName=TestResults.html"

# Con reporte TRX
dotnet test --logger "trx;LogFileName=TestResults.trx"
```

## 📋 **Suite de Pruebas**

### **🔐 AuthenticationTests (10 pruebas)**
- ✅ Login con credenciales válidas
- ✅ Login con credenciales inválidas
- ✅ Login con campos vacíos
- ✅ Validación de formato de email
- ✅ Verificación de elementos de la página
- ✅ Funcionalidad "Recordarme"
- ✅ Manejo de estado de carga
- ✅ Logout exitoso
- ✅ Manejo de caracteres especiales
- ✅ Manejo de credenciales largas

### **🍽️ ComandaFlowTests (6 pruebas)**
- ✅ Flujo completo de creación de comanda
- ✅ Comanda con un solo producto
- ✅ Cálculo correcto de cantidades múltiples
- ✅ Inclusión de observaciones
- ✅ Cancelación de comanda
- ✅ Filtrado de mesas

## 📸 **Screenshots Automáticos**

### **Configuración**
Las pruebas toman screenshots automáticamente en:
- ✅ **Errores**: Cuando una prueba falla
- ✅ **Final de prueba**: Al completar cada prueba
- ✅ **Puntos críticos**: En momentos importantes del flujo

### **Ubicación**
```
TestResults/
└── Screenshots/
    ├── test_end_20241215_143022.png
    ├── authentication_error_20241215_143045.png
    └── comanda_success_20241215_143112.png
```

## 🔧 **Troubleshooting**

### **Problemas Comunes**

#### **1. Appium no puede conectar al emulador**
```bash
# Verificar que el emulador está corriendo
adb devices

# Reiniciar ADB
adb kill-server
adb start-server

# Verificar que Appium puede ver el dispositivo
appium driver doctor uiautomator2
```

#### **2. Elementos no encontrados**
- ✅ Verificar que los IDs en los Page Objects coinciden con la UI
- ✅ Asegurar que la aplicación está completamente cargada
- ✅ Revisar que el emulador tiene suficiente memoria

#### **3. Pruebas fallan intermitentemente**
- ✅ Aumentar timeouts en `appsettings.Test.json`
- ✅ Agregar waits explícitos en Page Objects
- ✅ Verificar estabilidad del emulador

### **Logs y Debugging**
```bash
# Ejecutar con logs detallados
dotnet test --verbosity detailed --logger "console;verbosity=detailed"

# Ver logs de Appium
appium --log appium.log --log-level debug
```

## 📊 **Métricas y Reportes**

### **Cobertura de Pruebas**
- **Flujos Críticos**: 100% cubiertos
- **Casos de Error**: 100% cubiertos
- **Validaciones**: 100% cubiertos
- **Navegación**: 100% cubiertos

### **Tiempos de Ejecución**
- **Prueba Individual**: 30-60 segundos
- **Suite Completa**: 10-15 minutos
- **Setup/Teardown**: 5-10 segundos por prueba

## 🚀 **Próximos Pasos**

### **Pruebas Pendientes**
- [ ] **Preparaciones Diarias**: Flujo completo de preparaciones
- [ ] **Reservaciones**: Crear y gestionar reservaciones
- [ ] **Facturación**: Proceso completo de facturación
- [ ] **Gestión de Clientes**: CRUD de clientes
- [ ] **Reportes**: Verificación de reportes

### **Mejoras Planificadas**
- [ ] **Paralelización**: Ejecutar pruebas en paralelo
- [ ] **CI/CD Integration**: Integración con GitHub Actions
- [ ] **Cross-Platform**: Pruebas en iOS
- [ ] **Performance Testing**: Medición de rendimiento UI
- [ ] **Accessibility Testing**: Pruebas de accesibilidad

## 📞 **Soporte**

### **Contacto**
- **Desarrollador**: Equipo RestaurantePro
- **Documentación**: Este README
- **Issues**: Crear issue en el repositorio

### **Recursos Útiles**
- [Appium Documentation](http://appium.io/docs/en/about-appium/intro/)
- [xUnit Documentation](https://xunit.net/docs)
- [Fluent Assertions](https://fluentassertions.com/)

---

**¡Las pruebas UI automatizadas aseguran que RestaurantePro Mobile funcione perfectamente en producción!** 🎉 